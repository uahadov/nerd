"""Mutation tests run only in disposable repository copies, never the real project."""
import importlib.util
import json
from pathlib import Path
import shutil
import subprocess
import tempfile
import unittest

MODULE_PATH = Path(__file__).with_name('validate_project.py')
spec = importlib.util.spec_from_file_location('foundation', MODULE_PATH)
foundation = importlib.util.module_from_spec(spec)
spec.loader.exec_module(foundation)


class FoundationTests(unittest.TestCase):
    def setUp(self):
        self.tmp = tempfile.TemporaryDirectory(prefix='nard-foundation-')
        self.addCleanup(self.tmp.cleanup)
        self.root = Path(self.tmp.name) / 'repo'
        self.root.mkdir()
        listing = foundation.git(foundation.ROOT, 'ls-files', '--cached', '--others', '--exclude-standard', '-z')
        for name in sorted(set(filter(None, listing.stdout.split('\0')))):
            src = foundation.ROOT / name
            target = self.root / name
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(src, target)
        self.git('init', '-b', 'main')
        self.git('remote', 'add', 'origin', self.state()['repository']['remote_url'])

    def git(self, *args):
        return subprocess.run(['git', '-C', str(self.root), *args], capture_output=True, check=True)

    def state(self):
        return json.loads((self.root / 'docs/STATE.json').read_text())

    def change_state(self, mutate):
        value = self.state()
        mutate(value)
        (self.root / 'docs/STATE.json').write_text(json.dumps(value, ensure_ascii=False))

    def fails(self, check):
        result = foundation.validate(self.root)
        entry = next(c for c in result['checks'] if c['name'] == check)
        self.assertEqual(entry['status'], 'FAIL', entry)

    def test_valid_foundation_passes_from_different_path(self):
        result = foundation.validate(self.root)
        self.assertEqual(result['failed'], 0, result['checks'])

    def test_changed_source_rejected(self):
        path = self.root / 'NARD_Final_GDD_v1.0.md'
        path.write_text(path.read_text() + '\nchanged\n')
        self.fails('source_integrity')

    def test_reworded_decision_rejected(self):
        p = self.root / 'docs/APPROVED.md'
        p.write_text(p.read_text().replace('| A01 | Uzun nərd |', '| A01 | changed |'))
        self.fails('decision_inventory')

    def test_missing_rejection_rejected(self):
        p = self.root / 'docs/REJECTED.md'
        p.write_text('\n'.join(l for l in p.read_text().splitlines() if not l.startswith('| R20 |')))
        self.fails('decision_inventory')

    def test_unplanned_part_link_rejected(self):
        p = self.root / 'docs/APPROVED.md'
        p.write_text(p.read_text().replace('| P01.2 |', '| P99.9 |'))
        self.fails('decision_inventory')

    def test_self_approval_rejected(self):
        self.change_state(lambda s: s.update(current_part_human_approved=True))
        self.fails('state_contract')

    def test_next_part_authorization_rejected(self):
        self.change_state(lambda s: s.update(next_part_authorized=True))
        self.fails('state_contract')

    def test_string_boolean_rejected(self):
        self.change_state(lambda s: s.update(next_part_authorized='false'))
        self.fails('state_contract')

    def test_wrong_authorization_rejected(self):
        self.change_state(lambda s: s['authorization'].update(part='P01.3'))
        self.fails('state_contract')

    def test_future_human_approval_rejected(self):
        self.change_state(lambda s: s['human_approvals'].append(
            {'part': 'P01.2', 'by': 'human', 'quote': 'synthetic test', 'report': 'docs/reports/P00.1.md'}))
        self.fails('state_contract')

    def test_progress_drift_rejected(self):
        p = self.root / 'PROGRESS.md'
        p.write_text(p.read_text().replace('`AWAITING_HUMAN_APPROVAL`', '`HUMAN_APPROVED`'))
        self.fails('progress_consistency')

    def test_missing_link_rejected(self):
        p = self.root / 'README.md'
        p.write_text(p.read_text() + '\n[Broken](docs/missing.md)\n')
        self.fails('local_links')

    def test_path_escape_rejected(self):
        p = self.root / 'README.md'
        p.write_text(p.read_text() + '\n[Outside](../outside.md)\n')
        self.fails('local_links')

    def test_gameplay_directory_rejected(self):
        (self.root / 'unity/Assets/Game/Rules/Short').mkdir()
        self.fails('current_scope')

    def test_skill_policy_bypass_rejected(self):
        self.change_state(lambda s: s['tool_policy'].update(skills_allowed=True))
        self.fails('agent_policy')

    def test_missing_secret_ignore_rejected(self):
        p = self.root / '.gitignore'
        p.write_text(p.read_text().replace('\n.env\n', '\n'))
        self.fails('gitignore_behavior')

    def test_overbroad_ignore_rejected(self):
        p = self.root / '.gitignore'
        p.write_text(p.read_text() + '\n*.meta\n')
        self.fails('gitignore_behavior')

    def test_forced_tracked_secret_path_rejected(self):
        (self.root / '.env').write_text('SYNTHETIC_ONLY=yes\n')
        self.git('add', '-f', '.env')
        self.fails('candidate_and_index_safety')

    def test_secret_pattern_rejected(self):
        (self.root / 'leak.txt').write_text('gh' + 'p_' + 'Z' * 36)
        self.fails('candidate_and_index_safety')

    def test_staged_secret_hidden_by_clean_worktree_rejected(self):
        p = self.root / 'leak.txt'
        p.write_text('gh' + 'p_' + 'Z' * 36)
        self.git('add', 'leak.txt')
        p.write_text('clean working copy\n')
        self.fails('candidate_and_index_safety')

    def test_large_file_rejected(self):
        (self.root / 'large.txt').write_bytes(b'x' * (1024 * 1024 + 1))
        self.fails('candidate_and_index_safety')

    def test_asset_without_rights_rejected(self):
        p = self.root / 'docs/ASSET_MANIFEST.csv'
        p.write_text(p.read_text() + 'AR001,README.md,,someone,,,,,DRAFT\n')
        self.fails('asset_manifest')

    def test_public_push_record_rejected(self):
        self.change_state(lambda s: s['repository'].update(remote_visibility='PUBLIC', sync_status='PUSHED'))
        self.fails('remote_safety')

    def test_coordinated_next_part_change_rejected(self):
        self.change_state(lambda s: (s.update(active_part='P01.3', next_part='P01.4'),
                                     s['authorization'].update(part='P01.3')))
        p = self.root / 'PROGRESS.md'
        p.write_text(p.read_text().replace('- Aktiv part: `P01.2`', '- Aktiv part: `P01.3`')
                     .replace('- Növbəti part: `P01.3`', '- Növbəti part: `P01.4`'))
        self.fails('current_scope')

    def test_empty_scope_rejected(self):
        self.change_state(lambda s: s.update(scope={}))
        self.fails('current_scope')

    def test_numeric_scope_boolean_rejected(self):
        self.change_state(lambda s: s['scope'].update(unity_project_created=0))
        self.fails('current_scope')

    def test_false_evidence_reference_rejected(self):
        self.change_state(lambda s: s['checks'].update(validation='PASS', validation_evidence='README.md'))
        self.fails('test_evidence')

    def test_failed_test_log_with_pass_claim_rejected(self):
        self.change_state(lambda s: s['checks'].update(validator_tests='PASS'))
        (self.root / self.state()['checks']['tests_evidence']).write_text('FAILED\nExit code: 1\n')
        self.fails('test_evidence')

    def test_private_visibility_is_not_push_permission(self):
        self.change_state(lambda s: s['repository'].update(remote_visibility='PRIVATE', sync_status='PUSHED'))
        self.fails('remote_safety')

    def test_push_authorization_bypass_rejected(self):
        self.change_state(lambda s: s['repository'].update(push_authorized=True))
        self.fails('remote_safety')

    def test_missing_state_cli_returns_failure(self):
        (self.root / 'docs/STATE.json').write_text('{invalid')
        r = subprocess.run(['python3', str(MODULE_PATH), '--root', str(self.root)], capture_output=True, text=True)
        self.assertEqual(r.returncode, 1)
        self.assertGreater(json.loads(r.stdout)['failed'], 0)

    def test_unperformed_device_test_cannot_be_claimed(self):
        self.change_state(lambda s: s['build'].update(physical_device='PASS'))
        self.fails('rule_specification')

    def test_local_review_cannot_be_claimed_complete(self):
        self.change_state(lambda s: s['rules_review'].update(independent_azerbaijani_players='PASS'))
        self.fails('current_scope')

    def test_rules_unity_dependency_rejected(self):
        p = self.root / 'unity/Assets/Game/Rules/LongNardeRules.cs'
        p.write_text(p.read_text() + '\nusing UnityEngine;\n')
        self.fails('current_scope')

    def test_engine_evidence_source_drift_rejected(self):
        p = self.root / 'unity/Assets/Game/Rules/LongNardeRules.cs'
        p.write_text(p.read_text() + '\n// synthetic source drift\n')
        self.fails('test_evidence')

    def test_failed_engine_xml_rejected(self):
        p = self.root / 'docs/reports/P01.2.editmode.xml'
        p.write_text(p.read_text().replace('failed="0"', 'failed="1"', 1))
        self.fails('test_evidence')

    def test_full_long_game_cannot_be_claimed_completed(self):
        p = self.root / 'docs/APPROVED.md'
        p.write_text(p.read_text().replace('| A01 | Uzun nərd | IN_PROGRESS |', '| A01 | Uzun nərd | DONE |'))
        self.fails('decision_inventory')


if __name__ == '__main__':
    unittest.main()
