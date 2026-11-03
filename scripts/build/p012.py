#!/usr/bin/env python3
"""Test/build a disposable source copy (P01.2 default; --part P01.3/P01.4 includes short/match rules)."""
import argparse
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
    if step in ['editmode', 'playmode']:
        command += ['-runTests', '-testPlatform', 'EditMode' if step == 'editmode' else 'PlayMode', '-testResults', str(xml)]
    else:
        command += ['-buildTarget', 'Android', '-executeMethod', 'Nard.Build.P003Build.BuildAndroid', '-quit']
    started = datetime.now(timezone.utc).isoformat(timespec='seconds')
    env = dict(os.environ, NARD_RULE_FIXTURES=str(ROOT / 'docs/rules/fixtures.json'),
               NARD_BUILD_OUTPUT=str(ARTIFACTS / ('NARD-' + part + '.apk')))
    try:
        process = subprocess.run(command, env=env, cwd=ROOT, capture_output=True, text=True, timeout=1800)
        code = process.returncode
        (ARTIFACTS / (step + '.process.raw.log')).write_text(process.stdout + process.stderr)
    except subprocess.TimeoutExpired:
        code = 124
    text = log.read_text(errors='replace') if log.exists() else ''
    source_hashes = {}
    for name in ['Assets/Game/Rules', 'Assets/Game/Tests/Rules', 'Assets/Game/Tests/ShortRules', 'Assets/Game/Tests/Match', 'Packages', 'ProjectSettings']:
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
            report['cases'] = [{k: c.attrib[k] for k in ['name', 'result', 'label'] if k in c.attrib}
                               for c in root.iter('test-case')]
            report['failures'] = [c.findtext('failure/message') for c in root.iter('test-case') if c.attrib.get('result') == 'Failed']
            corpus = json.loads((ROOT / 'docs/rules/fixtures.json').read_text())['cases']
            allowed_skips = {'Fixture_' + c['id'] for c in corpus if c['mode'] != 'long' or c['action']['kind'] == 'undo'} if step == 'editmode' else set()
            actual_skips = {c['name'] for c in report['cases'] if c['result'] == 'Skipped'}
            report['expected_out_of_scope_skips'] = len(allowed_skips)
            expected_fixtures = {'Fixture_' + c['id'] for c in corpus if c['mode'] == 'long' and c['action']['kind'] != 'undo'} if step == 'editmode' else set()
            actual_fixtures = {c['name'] for c in report['cases'] if c['name'].startswith('Fixture_') and c['result'] == 'Passed'}
            report['executed_long_fixtures'] = len(actual_fixtures)
            expected_short = {'ShortFixture_' + c['id'] for c in corpus if c['mode'] == 'short' and c['action']['kind'] in ['start', 'opening', 'move', 'turn', 'classify_result']} if step == 'editmode' and part in ['P01.3', 'P01.4'] else set()
            actual_short = {c['name'] for c in report['cases'] if c['name'].startswith('ShortFixture_') and c['result'] == 'Passed'}
            report['executed_short_fixtures'] = len(actual_short)
            expected_match = {'MatchFixture_' + c['id'] for c in corpus if c['action']['kind'] in ['series', 'rating_policy']} if step == 'editmode' and part == 'P01.4' else set()
            actual_match = {c['name'] for c in report['cases'] if c['name'].startswith('MatchFixture_') and c['result'] == 'Passed'}
            report['executed_match_fixtures'] = len(actual_match)
            if (int(root.attrib.get('failed', '0')) != 0 or int(root.attrib.get('inconclusive', '0')) != 0
                    or int(root.attrib.get('passed', '0')) == 0 or actual_skips != allowed_skips
                    or actual_fixtures != expected_fixtures
                    or (part in ['P01.3', 'P01.4'] and actual_short != expected_short)
                    or (part == 'P01.4' and actual_match != expected_match)
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


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('step', choices=['editmode', 'playmode', 'android'])
    parser.add_argument('--editor', type=Path, default=EDITOR)
    parser.add_argument('--part', choices=['P01.2', 'P01.3', 'P01.4'], default='P01.2')
    args = parser.parse_args()
    ARTIFACTS = ROOT / 'build' / args.part
    raise SystemExit(run(args.step, args.editor, args.part))
