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
