#!/usr/bin/env python3
"""Test/build a disposable source copy (P01.2 default; --part P01.3/P01.4/P02.1/P02.2/P02.3 includes short/match/board tests)."""
import argparse
import fcntl
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
ARTIFACTS = ROOT / 'build/P01.2'
EDITOR = Path.home() / 'Unity/Hub/Editor/6000.6.7f1/Editor/Unity'


def prepare():
    project = ARTIFACTS / 'unity'
    project.mkdir(parents=True, exist_ok=True)
    # Copy source only. Keep this disposable project's imported cache across retries.
    for name in ['Assets', 'Packages', 'ProjectSettings']:
        destination = project / name
        if destination.exists():
            shutil.rmtree(destination)  # Only the ignored disposable copy, never user sources.
        shutil.copytree(ROOT / 'unity' / name, destination,
                        ignore=shutil.ignore_patterns('_Recovery', '_Recovery.meta'))
    return project


def run(step, editor, part='P01.2'):
    ARTIFACTS.mkdir(parents=True, exist_ok=True)
    if step == 'android':
        previous = ARTIFACTS / ('NARD-' + part + '.apk')
        if previous.exists():
            stamp = datetime.now(timezone.utc).strftime('%Y%m%dT%H%M%S%f')
            previous.rename(ARTIFACTS / ('NARD-' + part + '.' + stamp + '.previous.apk'))
    project = prepare()
    log = ARTIFACTS / (step + '.raw.log')
    xml = ARTIFACTS / (step + '.xml')
    xml.unlink(missing_ok=True)
    command = [str(editor), '-batchmode', '-nographics', '-projectPath', str(project), '-logFile', str(log)]
    if part in ['P02.1', 'P02.2', 'P02.3', 'P02.4'] and step == 'playmode':
        command.remove('-nographics')  # Real Editor camera render, not a fabricated screenshot.
    if step in ['editmode', 'playmode']:
        command += ['-runTests', '-testPlatform', 'EditMode' if step == 'editmode' else 'PlayMode', '-testResults', str(xml)]
    else:
        command += ['-buildTarget', 'Android', '-executeMethod', 'Nard.Build.P024Build.BuildAndroid' if part == 'P02.4' else 'Nard.Build.P023Build.BuildAndroid' if part == 'P02.3' else 'Nard.Build.P022Build.BuildAndroid' if part == 'P02.2' else 'Nard.Build.P021Build.BuildAndroid' if part == 'P02.1' else 'Nard.Build.P003Build.BuildAndroid', '-quit']
    started = datetime.now(timezone.utc).isoformat(timespec='seconds')
    env = dict(os.environ, NARD_RULE_FIXTURES=str(ROOT / 'docs/rules/fixtures.json'),
               NARD_BUILD_OUTPUT=str(ARTIFACTS / ('NARD-' + part + '.apk')),
               NARD_BOARD_CAPTURE_DIR=str(ARTIFACTS / 'captures'))
    try:
        process = subprocess.run(command, env=env, cwd=ROOT, capture_output=True, text=True, timeout=1800)
        code = process.returncode
        (ARTIFACTS / (step + '.process.raw.log')).write_text(process.stdout + process.stderr)
    except subprocess.TimeoutExpired:
        code = 124
    text = log.read_text(errors='replace') if log.exists() else ''
    source_hashes = {}
    for name in ['Assets/Game', 'Packages', 'ProjectSettings']:
        for file in sorted((project / name).rglob('*')):
            if file.is_file():
                source_hashes['unity/' + file.relative_to(project).as_posix()] = hashlib.sha256(file.read_bytes()).hexdigest()
    source_hashes['docs/rules/fixtures.json'] = hashlib.sha256((ROOT / 'docs/rules/fixtures.json').read_bytes()).hexdigest()
    report = {'part': part, 'step': step, 'started_at': started, 'exit_code': code,
              'editor': '6000.6.7f1', 'project': f'build/{part}/unity (source copy)',
              'source_sha256': source_hashes,
              'status': 'PASS' if code == 0 else 'FAIL',
              'compiler_errors': sorted(set(re.findall(r'^.*error CS[^\n]*', text, re.M)))}
    if report['compiler_errors']:
        report['status'] = 'FAIL'
    if step != 'android':
        if not xml.exists():
            report.update(status='FAIL', reason='No NUnit XML produced')
        else:
            root = ET.parse(xml).getroot()
            report['tests'] = root.attrib
            report['cases'] = [{k: c.attrib[k] for k in ['name', 'fullname', 'result', 'label'] if k in c.attrib}
                               for c in root.iter('test-case')]
            report['failures'] = [c.findtext('failure/message') for c in root.iter('test-case') if c.attrib.get('result') == 'Failed']
            corpus = json.loads((ROOT / 'docs/rules/fixtures.json').read_text())['cases']
            allowed_skips = {'Fixture_' + c['id'] for c in corpus if c['mode'] != 'long' or c['action']['kind'] == 'undo'} if step == 'editmode' else set()
            actual_skips = {c['name'] for c in report['cases'] if c['result'] == 'Skipped'}
            report['expected_out_of_scope_skips'] = len(allowed_skips)
            expected_fixtures = {'Fixture_' + c['id'] for c in corpus if c['mode'] == 'long' and c['action']['kind'] != 'undo'} if step == 'editmode' else set()
            actual_fixtures = {c['name'] for c in report['cases'] if c['name'].startswith('Fixture_') and c['result'] == 'Passed'}
            report['executed_long_fixtures'] = len(actual_fixtures)
            expected_short = {'ShortFixture_' + c['id'] for c in corpus if c['mode'] == 'short' and c['action']['kind'] in ['start', 'opening', 'move', 'turn', 'classify_result']} if step == 'editmode' and part in ['P01.3', 'P01.4', 'P02.1', 'P02.2', 'P02.3', 'P02.4'] else set()
            actual_short = {c['name'] for c in report['cases'] if c['name'].startswith('ShortFixture_') and c['result'] == 'Passed'}
            report['executed_short_fixtures'] = len(actual_short)
            expected_match = {'MatchFixture_' + c['id'] for c in corpus if c['action']['kind'] in ['series', 'rating_policy']} if step == 'editmode' and part in ['P01.4', 'P02.1', 'P02.2', 'P02.3', 'P02.4'] else set()
            actual_match = {c['name'] for c in report['cases'] if c['name'].startswith('MatchFixture_') and c['result'] == 'Passed'}
            report['executed_match_fixtures'] = len(actual_match)
            if part in ['P02.1', 'P02.2', 'P02.3', 'P02.4']:
                prefix = '.BoardProjectionTests.' if step == 'editmode' else '.BoardSceneTests.'
                board_cases = [c for c in report['cases'] if prefix in c.get('fullname', '') and c['result'] == 'Passed']
                report['executed_board_cases'] = len(board_cases)
                if len(board_cases) != (38 if step == 'editmode' else 1):
                    report.update(status='FAIL', reason='Required board test suite missing or incomplete')
            if part in ['P02.2', 'P02.3', 'P02.4']:
                prefix = '.TapSelectionTests.' if step == 'editmode' else '.TapSceneTests.'
                tap_cases = [c for c in report['cases'] if prefix in c.get('fullname', '') and c['result'] == 'Passed']
                report['executed_tap_cases'] = len(tap_cases)
                expected_tap = {'TapFixture_' + c['id'] for c in corpus if c['mode'] in ['long', 'short'] and c['before']['phase'] == 'moving' and c['action']['kind'] in ['move', 'turn']} if step == 'editmode' else set()
                actual_tap = {c['name'] for c in report['cases'] if c['name'].startswith('TapFixture_') and c['result'] == 'Passed'}
                report['executed_tap_fixtures'] = len(actual_tap)
                if len(tap_cases) != (55 if step == 'editmode' else 5) or actual_tap != expected_tap:
                    report.update(status='FAIL', reason='Required tap suites missing or incomplete')
            if part in ['P02.3', 'P02.4']:
                prefix = '.LocalTurnTests.' if step == 'editmode' else '.TurnSceneTests.'
                turn_cases = [c for c in report['cases'] if prefix in c.get('fullname', '') and c['result'] == 'Passed']
                report['executed_turn_cases'] = len(turn_cases)
                expected_rolls = {f'TurnRoll_{mode}_P{actor}_{a}_{b}' for mode in ['Long', 'Short'] for actor in range(2) for a in range(1, 7) for b in range(1, 7)} if step == 'editmode' else set()
                actual_rolls = {c['name'] for c in report['cases'] if c['name'].startswith('TurnRoll_') and c['result'] == 'Passed'}
                report['executed_turn_roll_cases'] = len(actual_rolls)
                if len(turn_cases) != (168 if step == 'editmode' else 5) or actual_rolls != expected_rolls:
                    report.update(status='FAIL', reason='Required turn suites missing or incomplete')
            if part == 'P02.4':
                expected_undo = {'UndoFixture_' + c['id'] for c in corpus if c['action']['kind'] == 'undo'} if step == 'editmode' else set()
                actual_undo = {c['name'] for c in report['cases'] if c['name'].startswith('UndoFixture_') and c['result'] == 'Passed'}
                report['executed_undo_fixtures'] = len(actual_undo)
                prefix = '.UndoRegressionTests.' if step == 'editmode' else '.UndoSceneTests.'
                actual_undo_tests = [c for c in report['cases'] if prefix in c.get('fullname', '') and c['result'] == 'Passed']
                report['executed_undo_tests'] = len(actual_undo_tests)
                if step == 'editmode':
                    skin_cases = [c for c in report['cases'] if '.ReferenceSkinTests.' in c.get('fullname', '') and c['result'] == 'Passed']
                    report['executed_reference_skin_cases'] = len(skin_cases)
                    if len(skin_cases) != 27:
                        report.update(status='FAIL', reason='Reference skin coordinate tests missing')
                if actual_undo != expected_undo or len(actual_undo_tests) != (194 if step == 'editmode' else 5):
                    report.update(status='FAIL', reason='Required undo suites missing or incomplete')
            if (int(root.attrib.get('failed', '0')) != 0 or int(root.attrib.get('inconclusive', '0')) != 0
                    or int(root.attrib.get('passed', '0')) == 0 or actual_skips != allowed_skips
                    or actual_fixtures != expected_fixtures
                    or (part in ['P01.3', 'P01.4', 'P02.1', 'P02.2', 'P02.3', 'P02.4'] and actual_short != expected_short)
                    or (part in ['P01.4', 'P02.1', 'P02.2', 'P02.3', 'P02.4'] and actual_match != expected_match)
                    or any(c['result'] not in ['Passed', 'Skipped'] for c in report['cases'])):
                report['status'] = 'FAIL'
    else:
        apk = ARTIFACTS / ('NARD-' + part + '.apk')
        report['apk_exists'] = apk.exists()
        if apk.exists():
            report['apk_bytes'] = apk.stat().st_size
            report['apk_sha256'] = hashlib.sha256(apk.read_bytes()).hexdigest()
        else:
            report['status'] = 'FAIL'
    (ARTIFACTS / (step + '.json')).write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n')
    print(json.dumps({k: v for k, v in report.items() if k != 'cases'}, ensure_ascii=False, indent=2))
    return int(report['status'] != 'PASS')


def run_exclusive(step, editor, part):
    """Linux runner: acquire before copying sources so two runs cannot damage each other's evidence."""
    ARTIFACTS.mkdir(parents=True, exist_ok=True)
    with (ARTIFACTS / '.runner.lock').open('a') as lock:
        try:
            fcntl.flock(lock, fcntl.LOCK_EX | fcntl.LOCK_NB)
        except BlockingIOError:
            print('Another test/build is using this disposable project; wait for it to finish.')
            return 1
        return run(step, editor, part)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('step', choices=['editmode', 'playmode', 'android'])
    parser.add_argument('--editor', type=Path, default=EDITOR)
    parser.add_argument('--part', choices=['P01.2', 'P01.3', 'P01.4', 'P02.1', 'P02.2', 'P02.3', 'P02.4'], default='P01.2')
    args = parser.parse_args()
    ARTIFACTS = ROOT / 'build' / args.part
    raise SystemExit(run_exclusive(args.step, args.editor, args.part))
