"""Failure-path tests for the runner; simulated reports are never gameplay evidence."""
import contextlib
import importlib.util
import io
import json
from pathlib import Path
import shutil
import tempfile
from types import SimpleNamespace
import unittest
from unittest.mock import patch
import xml.etree.ElementTree as ET

SPEC = importlib.util.spec_from_file_location('p012', Path(__file__).parents[1] / 'build/p012.py')
runner = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(runner)


class RunnerTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix='nard-runner-')
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.artifacts = self.root / 'build/P01.2'
        self.project = self.artifacts / 'unity'
        for name in ['Assets/Game/Rules', 'Assets/Game/Tests/Rules', 'Packages', 'ProjectSettings']:
            (self.project / name).mkdir(parents=True, exist_ok=True)
        target = self.root / 'docs/rules/fixtures.json'
        target.parent.mkdir(parents=True)
        shutil.copyfile(runner.ROOT / 'docs/rules/fixtures.json', target)
        self.cases = json.loads(target.read_text())['cases']
        self.xml = ET.Element('test-run', result='Skipped:Ignored', total='67', passed='26',
                              failed='0', inconclusive='0', skipped='41')
        for case in self.cases:
            skipped = case['mode'] != 'long' or case['action']['kind'] == 'undo'
            ET.SubElement(self.xml, 'test-case', name='Fixture_' + case['id'],
                          result='Skipped' if skipped else 'Passed', label='Ignored' if skipped else '')
        self.code = 0
        self.write_xml = True
        self.error_log = ''

    def fake_process(self, command, **kwargs):
        if self.write_xml and '-testResults' in command:
            ET.ElementTree(self.xml).write(command[command.index('-testResults') + 1])
        Path(command[command.index('-logFile') + 1]).write_text(self.error_log)
        return SimpleNamespace(returncode=self.code, stdout='', stderr='')

    def run_step(self, step='editmode', part='P01.2'):
        with patch.object(runner, 'ROOT', self.root), patch.object(runner, 'ARTIFACTS', self.artifacts), \
                patch.object(runner, 'prepare', return_value=self.project), \
                patch.object(runner.subprocess, 'run', side_effect=self.fake_process), contextlib.redirect_stdout(io.StringIO()):
            return runner.run(step, Path('/synthetic/editor'), part)

    def test_only_documented_skips_accepted(self):
        self.assertEqual(self.run_step(), 0)

    def test_unexpected_skip_rejected(self):
        next(c for c in self.xml if c.get('result') == 'Passed').set('result', 'Skipped')
        self.assertEqual(self.run_step(), 1)

    def test_missing_authorized_fixture_rejected(self):
        self.xml.remove(next(c for c in self.xml if c.get('name') == 'Fixture_L_START'))
        self.assertEqual(self.run_step(), 1)

    def test_failed_nunit_report_rejected(self):
        self.xml.set('failed', '1')
        self.assertEqual(self.run_step(), 1)

    def test_success_exit_without_xml_rejected(self):
        self.write_xml = False
        self.assertEqual(self.run_step(), 1)

    def test_failed_process_with_passing_xml_rejected(self):
        self.code = 2
        self.assertEqual(self.run_step(), 1)

    def test_compilation_error_with_zero_exit_rejected(self):
        self.error_log = 'Assets/Test.cs(1): error CS1002: synthetic failure\n'
        self.assertEqual(self.run_step(), 1)

    def test_android_without_apk_rejected(self):
        self.assertEqual(self.run_step('android'), 1)

    def test_stale_apk_cannot_mask_missing_build(self):
        (self.artifacts / 'NARD-P01.2.apk').write_bytes(b'synthetic old build')
        self.assertEqual(self.run_step('android'), 1)
        self.assertEqual(len(list(self.artifacts.glob('*.previous.apk'))), 1)

    def include_short_cases(self):
        for case in self.cases:
            if case['mode'] == 'short' and case['action']['kind'] in ['start', 'opening', 'move', 'turn', 'classify_result']:
                ET.SubElement(self.xml, 'test-case', name='ShortFixture_' + case['id'], result='Passed')
        self.xml.set('total', '97')
        self.xml.set('passed', '56')

    def test_short_part_requires_short_fixtures(self):
        self.assertEqual(self.run_step(part='P01.3'), 1)

    def test_short_part_with_complete_fixtures_passes(self):
        self.include_short_cases()
        self.assertEqual(self.run_step(part='P01.3'), 0)

    def test_missing_short_fixture_rejected(self):
        self.include_short_cases()
        self.xml.remove(next(c for c in self.xml if c.get('name') == 'ShortFixture_S_START'))
        self.assertEqual(self.run_step(part='P01.3'), 1)

    def test_skipped_short_fixture_is_not_a_pass(self):
        self.include_short_cases()
        next(c for c in self.xml if c.get('name') == 'ShortFixture_S_START').set('result', 'Skipped')
        self.assertEqual(self.run_step(part='P01.3'), 1)

    def include_match_cases(self):
        for case in self.cases:
            if case['action']['kind'] in ['series', 'rating_policy']:
                ET.SubElement(self.xml, 'test-case', name='MatchFixture_' + case['id'], result='Passed')
        self.xml.set('total', '104')
        self.xml.set('passed', '63')

    def test_match_part_requires_common_fixtures(self):
        self.include_short_cases()
        self.assertEqual(self.run_step(part='P01.4'), 1)

    def test_match_part_complete_fixtures_pass(self):
        self.include_short_cases()
        self.include_match_cases()
        self.assertEqual(self.run_step(part='P01.4'), 0)

    def test_missing_series_fixture_rejected(self):
        self.include_short_cases()
        self.include_match_cases()
        self.xml.remove(next(c for c in self.xml if c.get('name') == 'MatchFixture_C_BO3_FINISHES'))
        self.assertEqual(self.run_step(part='P01.4'), 1)

    def test_skipped_rating_fixture_cannot_be_counted_as_pass(self):
        self.include_short_cases()
        self.include_match_cases()
        next(c for c in self.xml if c.get('name') == 'MatchFixture_C_RATING_PRIVATE_CASUAL').set('result', 'Skipped')
        self.assertEqual(self.run_step(part='P01.4'), 1)

    def test_board_part_cannot_pass_with_only_legacy_suites(self):
        self.include_short_cases()
        self.include_match_cases()
        self.assertEqual(self.run_step(part='P02.1'), 1)

    def test_board_part_requires_all_projection_cases(self):
        self.include_short_cases()
        self.include_match_cases()
        for i in range(38):
            ET.SubElement(self.xml, 'test-case', name=f'Projection{i}', fullname=f'Nard.Tests.Board.BoardProjectionTests.Projection{i}', result='Passed')
        self.assertEqual(self.run_step(part='P02.1'), 0)
        self.xml.remove(self.xml[-1])
        self.assertEqual(self.run_step(part='P02.1'), 1)

    def include_board_cases(self):
        for i in range(38):
            ET.SubElement(self.xml, 'test-case', name=f'Projection{i}', fullname=f'Nard.Tests.Board.BoardProjectionTests.Projection{i}', result='Passed')

    def test_tap_part_cannot_pass_with_only_previous_suites(self):
        self.include_short_cases(); self.include_match_cases(); self.include_board_cases()
        self.assertEqual(self.run_step(part='P02.2'), 1)

    def test_tap_part_requires_every_tap_fixture_and_case(self):
        self.include_short_cases(); self.include_match_cases(); self.include_board_cases()
        count = 0
        for case in self.cases:
            if case['mode'] in ['long', 'short'] and case['before']['phase'] == 'moving' and case['action']['kind'] in ['move', 'turn']:
                name = 'TapFixture_' + case['id']
                ET.SubElement(self.xml, 'test-case', name=name, fullname='Nard.Tests.Tap.TapSelectionTests.' + name, result='Passed')
                count += 1
        for i in range(55-count):
            ET.SubElement(self.xml, 'test-case', name=f'ExtraTap{i}', fullname=f'Nard.Tests.Tap.TapSelectionTests.ExtraTap{i}', result='Passed')
        self.assertEqual(self.run_step(part='P02.2'), 0)
        self.xml.remove(next(c for c in self.xml if c.get('name') == 'TapFixture_S_HIGH_REJECT'))
        self.assertEqual(self.run_step(part='P02.2'), 1)

    def test_turn_part_rejects_incomplete_previous_suites(self):
        self.include_short_cases(); self.include_match_cases(); self.include_board_cases()
        self.assertEqual(self.run_step(part='P02.3'), 1)

    def include_turn_cases(self):
        self.include_short_cases(); self.include_match_cases(); self.include_board_cases()
        tap_count = 0
        for case in self.cases:
            if case['mode'] in ['long', 'short'] and case['before']['phase'] == 'moving' and case['action']['kind'] in ['move', 'turn']:
                name = 'TapFixture_' + case['id']
                ET.SubElement(self.xml, 'test-case', name=name, fullname='Nard.Tests.Tap.TapSelectionTests.' + name, result='Passed')
                tap_count += 1
        for i in range(55-tap_count):
            ET.SubElement(self.xml, 'test-case', name=f'ExtraTap{i}', fullname=f'Nard.Tests.Tap.TapSelectionTests.ExtraTap{i}', result='Passed')
        for mode in ['Long', 'Short']:
            for actor in range(2):
                for a in range(1, 7):
                    for b in range(1, 7):
                        name = f'TurnRoll_{mode}_P{actor}_{a}_{b}'
                        ET.SubElement(self.xml, 'test-case', name=name, fullname='Nard.Tests.Turn.LocalTurnTests.' + name, result='Passed')
        for i in range(24):
            ET.SubElement(self.xml, 'test-case', name=f'TurnExtra{i}', fullname=f'Nard.Tests.Turn.LocalTurnTests.TurnExtra{i}', result='Passed')
    def test_turn_part_requires_every_roll_and_turn_case(self):
        self.include_turn_cases()
        self.assertEqual(self.run_step(part='P02.3'), 0)
        self.xml.remove(next(c for c in self.xml if c.get('name') == 'TurnRoll_Short_P1_6_6'))
        self.assertEqual(self.run_step(part='P02.3'), 1)

    def test_undo_part_requires_previous_and_undo_suites(self):
        self.include_turn_cases()
        self.assertEqual(self.run_step(part='P02.4'), 1)
        for case in self.cases:
            if case['action']['kind'] == 'undo':
                ET.SubElement(self.xml, 'test-case', name='UndoFixture_' + case['id'], result='Passed')
        for i in range(194):
            ET.SubElement(self.xml, 'test-case', name=f'Undo{i}', fullname=f'Nard.Tests.Undo.UndoRegressionTests.Undo{i}', result='Passed')
        for i in range(27):
            ET.SubElement(self.xml, 'test-case', name=f'Skin{i}', fullname=f'Nard.Tests.Undo.ReferenceSkinTests.Skin{i}', result='Passed')
        self.assertEqual(self.run_step(part='P02.4'), 0)
        case = next(c for c in self.xml if c.get('name') == 'UndoFixture_S_UNDO_COMMITTED')
        self.xml.remove(case)
        self.assertEqual(self.run_step(part='P02.4'), 1)

    def test_undo_part_cannot_accept_only_canonical_four_without_regressions(self):
        self.include_turn_cases()
        for case in self.cases:
            if case['action']['kind'] == 'undo':
                ET.SubElement(self.xml, 'test-case', name='UndoFixture_' + case['id'], result='Passed')
        self.assertEqual(self.run_step(part='P02.4'), 1)

    def test_concurrent_run_is_rejected_before_source_copy(self):
        with patch.object(runner, 'ARTIFACTS', self.artifacts), patch.object(runner, 'run') as work:
            self.artifacts.mkdir(parents=True, exist_ok=True)
            with (self.artifacts / '.runner.lock').open('a') as lock:
                runner.fcntl.flock(lock, runner.fcntl.LOCK_EX | runner.fcntl.LOCK_NB)
                with contextlib.redirect_stdout(io.StringIO()):
                    self.assertEqual(runner.run_exclusive('editmode', Path('/synthetic/editor'), 'P02.4'), 1)
            work.assert_not_called()

    def test_exclusive_lock_is_released_after_run(self):
        with patch.object(runner, 'ARTIFACTS', self.artifacts), patch.object(runner, 'run', return_value=0) as work:
            for _ in range(2):
                self.assertEqual(runner.run_exclusive('editmode', Path('/synthetic/editor'), 'P02.4'), 0)
            self.assertEqual(work.call_count, 2)
