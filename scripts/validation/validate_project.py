#!/usr/bin/env python3
"""Offline repository checks. Standard library only; never advances project state."""
import argparse
import csv
import hashlib
import json
import re
import subprocess
import xml.etree.ElementTree as ET
from datetime import datetime
from pathlib import Path
from urllib.parse import unquote, urlsplit
from zoneinfo import ZoneInfo

ROOT = Path(__file__).resolve().parents[2]
AUTHORIZED_PART = 'P01.2'
SOURCES = {
    'NARD_Final_GDD_v1.0.md': '2d2471282160bc10dc4eaec57ed90b8b2e6e7684cb4101c9bd5f2d76e90a8580',
    'NARD_Master_Icra_Plani_v2.md': '1e333827bf7c861c5fc2b279818c795c69ddeabbd1fa8c93e3b7daad2d6828bc',
}
REQUIRED = [
    '.gitignore', '.gitattributes', 'AGENTS.md', 'README.md', 'PROGRESS.md', 'CHANGELOG.md',
    'docs/STATE.json', 'docs/DECISIONS.md', 'docs/APPROVED.md', 'docs/REJECTED.md',
    'docs/ARCHITECTURE.md', 'docs/OPERATIONS.md', 'docs/TEST_MATRIX.md',
    'docs/ASSET_MANIFEST.csv', 'docs/reports/TEMPLATE.md', 'docs/reports/P00.1.md',
    'docs/reports/P00.2.md', 'scripts/validation/validate_project.py',
    'scripts/validation/test_foundation.py', 'docs/reports/P00.2.validation.json',
    'docs/reports/P00.2.tests.txt', 'docs/reports/P00.3.md',
    'docs/reports/P00.3.validation.json', 'docs/reports/P00.3.tests.txt',
    'unity/Packages/manifest.json', 'unity/Packages/packages-lock.json',
    'unity/ProjectSettings/ProjectVersion.txt', 'scripts/build/p003.py',
    'docs/reports/P00.3.execution.json', 'docs/screenshots/P00.3-editor.png',
    'docs/screenshots/P00.3-gameview.png',
    'docs/RULES_LONG.md', 'docs/RULES_SHORT.md', 'docs/RULESET_VARIANTS.md',
    'docs/RULE_MATRIX.md', 'docs/rules/fixtures.json', 'docs/rules/fixtures.schema.json',
    'scripts/validation/validate_rule_fixtures.py', 'scripts/validation/test_rule_fixtures.py',
    'docs/reports/P01.1.md', 'docs/reports/P01.1.validation.json',
    'docs/reports/P01.1.tests.txt', 'docs/reports/P01.1.rules.json',
    'docs/reports/P01.2.md', 'docs/reports/P01.2.execution.json',
    'docs/reports/P01.2.validation.json', 'docs/reports/P01.2.tests.txt',
    'scripts/build/p012.py', 'scripts/validation/test_p012_runner.py',
    'docs/reports/P01.2.editmode.xml', 'docs/reports/P01.2.playmode.xml', 'unity/Assets/Game/Rules/LongNardeRules.cs',
    'unity/Assets/Game/Rules/LongPosition.cs', 'unity/Assets/Game/Rules/Nard.Rules.Long.asmdef',
    'unity/Assets/Game/Tests/Rules/LongRulesTests.cs',
    'unity/Assets/Game/Tests/Rules/Nard.Rules.Long.Tests.asmdef',
]
ASSET_COLUMNS = ['asset_id', 'path', 'source', 'creator', 'license', 'license_evidence',
                 'modifications', 'approved_part', 'status']
SECRET_PATTERNS = [
    re.compile(rb'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----'),
    re.compile(rb'\bgh[pousr]_[A-Za-z0-9]{30,}\b'),
    re.compile(rb'\bgithub_pat_[A-Za-z0-9_]{30,}\b'),
    re.compile(rb'\bAKIA[A-Z0-9]{16}\b'),
]
IGNORE_PROBES = [
    'Library/cache.bin', 'unity/Library/cache.bin', 'unity/Temp/cache.bin',
    'unity/Obj/cache.bin', 'unity/Build/a.apk', 'unity/Builds/a.aab',
    'unity/Logs/editor.log', 'unity/UserSettings/layout.wlt', 'node_modules/a/index.js',
    '.env', '.env.production', 'nakama/.env', 'signing.p12', 'release.keystore',
    'signing.key', 'credentials.json', 'service-account-test.json',
    '.codex-gravity/private.json', 'scripts/__pycache__/file.pyc',
    'unity/.utmp/Debug/object.o', 'unity/.gradle/cache.bin', 'unity/.cxx/cache.bin',
    'unity/Assets/_Recovery/0.unity', 'unity/Assets/_Recovery.meta',
]
KEEP_PROBES = [
    'AGENTS.md', 'docs/STATE.json', '.env.example', 'unity/Assets/Game/Rules/Long.cs',
    'unity/Assets/Game/Art/board.png', 'unity/Assets/Game/Scenes/Main.unity',
    'unity/Assets/Game/Scenes/Main.unity.meta', 'unity/Packages/manifest.json',
    'unity/ProjectSettings/ProjectVersion.txt', 'nakama/server/main.ts',
    'docs/reports/P00.2.validation.json', 'scripts/build/build_android.py',
]


def git(root, *args, input_text=None):
    return subprocess.run(['git', '-C', str(root), *args], input=input_text,
                          text=True, capture_output=True, timeout=20)


def require(condition, message):
    if not condition:
        raise ValueError(message)


def read(root, name):
    return (root / name).read_text(encoding='utf-8')


def state(root):
    return json.loads(read(root, 'docs/STATE.json'))


def parts(root):
    return re.findall(r'^\*\*(P\d{2}\.\d+) —', read(root, 'NARD_Master_Icra_Plani_v2.md'), re.M)


def inventory(root, prefix):
    return re.findall(r'^\d+\. \*\*(' + prefix + r'\d{2})\*\* (.+)$',
                      read(root, 'NARD_Final_GDD_v1.0.md'), re.M)


def validate(root):
    root = Path(root).resolve()
    checks = []

    def check(name, action):
        try:
            detail = action() or 'OK'
            checks.append({'name': name, 'status': 'PASS', 'detail': detail})
        except (ValueError, KeyError, TypeError, IndexError, AttributeError, OSError, ET.ParseError, subprocess.SubprocessError) as exc:
            checks.append({'name': name, 'status': 'FAIL', 'detail': str(exc)})

    def required():
        missing = [n for n in [*SOURCES, *REQUIRED] if not (root / n).is_file()]
        require(not missing, 'Missing: ' + ', '.join(missing))
        return f'{len(SOURCES) + len(REQUIRED)} required files exist'
    check('required_files', required)

    def source_integrity():
        for name, expected in SOURCES.items():
            require(hashlib.sha256((root / name).read_bytes()).hexdigest() == expected,
                    f'Source changed: {name}')
            require(state(root)['sources'][name]['sha256'] == expected, f'Hash ledger mismatch: {name}')
        return 'Both original source files and recorded SHA-256 match baseline'
    check('source_integrity', source_integrity)

    def decisions():
        for prefix, count, file, status, width in [
            ('A', 87, 'docs/APPROVED.md', 'NOT_STARTED', 4),
            ('R', 20, 'docs/REJECTED.md', 'DO_NOT_BUILD', 3),
        ]:
            expected = inventory(root, prefix)
            require([i for i, _ in expected] == [f'{prefix}{i:02}' for i in range(1, count + 1)],
                    f'{prefix} source IDs invalid')
            rows = [[c.strip() for c in line.strip().strip('|').split('|')]
                    for line in read(root, file).splitlines()
                    if re.match(r'^\| ' + prefix + r'\d{2} \|', line)]
            require(len(rows) == count and all(len(row) == width for row in rows), f'{file} row count/shape')
            require([(r[0], r[1]) for r in rows] == expected, f'{file} differs from GDD wording/order')
            require(all(r[2] == ('IN_PROGRESS' if prefix == 'A' and r[0] == 'A01' else status) for r in rows), f'{file} implementation status invalid')
            if prefix == 'A':
                require(all(r[3] in parts(root) for r in rows), 'Unknown planned part in APPROVED')
            master = re.findall(r'^\d+\. \*\*(' + prefix + r'\d{2})\*\* (.+)$',
                                read(root, 'NARD_Master_Icra_Plani_v2.md'), re.M)
            require(master == expected, f'{prefix} GDD/Master disagreement')
        return '87 approved + 20 rejected: exact text, order, status and part links'
    check('decision_inventory', decisions)

    def state_contract():
        s = state(root)
        require(s['schema_version'] == 1, 'Unsupported state schema')
        require(s['timezone'] == 'Asia/Baku', 'Timezone mismatch')
        datetime.strptime(s['updated_on'], '%Y-%m-%d')
        known = parts(root)
        require(s['active_part'] in known, 'Unknown active part')
        active = s['active_part']
        require(s['authorization']['part'] == active and s['authorization']['by'] == 'human'
                and bool(s['authorization']['quote'].strip()), 'Missing human scope authorization')
        require(s['approved_feature_count'] == 87 and s['rejected_feature_count'] == 20, 'Feature count mismatch')
        for flag in ['current_part_human_approved', 'next_part_authorized']:
            require(type(s[flag]) is bool, f'{flag} must be boolean')
        approvals = s['human_approvals']
        approved_parts = [a['part'] for a in approvals]
        require(len(approved_parts) == len(set(approved_parts)), 'Duplicate human approval')
        for a in approvals:
            require(a['part'] in known and a['by'] == 'human' and a['quote'].strip(), 'Invalid approval record')
            require(known.index(a['part']) < known.index(active), 'Current/future part falsely human-approved')
            require((root / a['report']).is_file(), 'Approval report missing')
        require(bool(approvals) and approved_parts[-1] == s['last_human_approved_part'], 'Approval head mismatch')
        require(s['next_part'] == known[known.index(active) + 1], 'Next part out of plan order')
        require(s['status'] in ['PLANNED', 'IN_PROGRESS', 'SELF_TEST_FAILED', 'SELF_TEST_PASSED',
                               'AWAITING_HUMAN_APPROVAL', 'REWORK_REQUIRED', 'HUMAN_APPROVED'], 'Unknown status')
        if s['status'] == 'AWAITING_HUMAN_APPROVAL':
            require(not s['current_part_human_approved'] and not s['next_part_authorized'], 'Approval gate bypass')
        require(set(s['checks']) == {'validation', 'validator_tests', 'unity_build', 'visual_device',
                                     'validation_evidence', 'tests_evidence'}, 'Incomplete check ledger')
        for name, value in s['checks'].items():
            if not name.endswith('_evidence'):
                require(value in ['PASS', 'FAIL', 'BLOCKED', 'NOT_RUN'], f'Invalid check status: {name}')
        require((root / s['test_report']).is_file(), 'Current report missing')
        for item in s['open_items']:
            require(all(item[k] for k in ['id', 'status', 'impact', 'next_action']), 'Incomplete open item')
        require(len({i['id'] for i in s['open_items']}) == len(s['open_items']), 'Duplicate open item ID')
        return f'{active}: authorization and approval gate consistent'
    check('state_contract', state_contract)

    def evidence():
        s = state(root)
        paths = {}
        for key in ['validation_evidence', 'tests_evidence']:
            target = (root / s['checks'][key]).resolve()
            require(target.is_relative_to(root) and target.is_file(), f'Missing/unsafe evidence: {key}')
            paths[key] = target
        if s['checks']['validation'] == 'PASS':
            report = json.loads(paths['validation_evidence'].read_text(encoding='utf-8'))
            checks = report['checks']
            require(report['part'] == s['active_part'] and bool(checks), 'Evidence part/check list mismatch')
            require(report['failed'] == 0 and report['passed'] == len(checks)
                    and all(c['status'] == 'PASS' for c in checks), 'Validation PASS lacks passing evidence')
        if s['checks']['validator_tests'] == 'PASS':
            log = paths['tests_evidence'].read_text(encoding='utf-8')
            require(re.search(r'^Ran [1-9][0-9]* tests? in ', log, re.M) is not None
                    and '\nOK\n' in log and log.rstrip().endswith('Exit code: 0'),
                    'Test PASS lacks successful run evidence')
        if s.get('engine', {}).get('status') == 'PASS':
            import xml.etree.ElementTree as ET
            engine_path = (root / s['engine']['execution_evidence']).resolve()
            require(engine_path.is_relative_to(root), 'Unsafe engine evidence path')
            execution = json.loads(engine_path.read_text())
            require(execution['part'] == AUTHORIZED_PART and execution['physical_android'] == 'NOT_RUN'
                    and execution['independent_local_player_review'] == 'NOT_RUN', 'Engine evidence scope mismatch')
            for step in ['editmode', 'playmode', 'android']:
                report = execution['steps'][step]
                require(report['status'] == 'PASS' and report['exit_code'] == 0 and not report['compiler_errors'], 'Engine/build run failed')
                for name, digest in report['source_sha256'].items():
                    if '/Rules/' in name or name == 'docs/rules/fixtures.json':
                        path = (root / name).resolve()
                        require(path.is_relative_to(root) and hashlib.sha256(path.read_bytes()).hexdigest() == digest,
                                'Engine evidence source mismatch: ' + name)
                if step != 'android':
                    document = ET.parse(root / ('docs/reports/P01.2.' + step + '.xml')).getroot()
                    require(document.attrib['failed'] == '0' and int(document.attrib['passed']) > 0, 'Failed NUnit XML')
                    require(document.attrib['passed'] == report['tests']['passed'] and document.attrib['skipped'] == report['tests']['skipped'], 'NUnit summary mismatch')
            require(execution['coverage']['executed_long_fixtures'] == 26 and execution['coverage']['deferred_fixtures'] == 41,
                    'Fixture execution coverage mismatch')
        return 'Stored PASS evidence and runtime source hashes match; independent device/local reviews remain NOT_RUN'
    check('test_evidence', evidence)

    def scope():
        s = state(root)
        # Updated only after the human's explicit P01.2 authorization (D016).
        require(s['active_part'] == AUTHORIZED_PART, 'Only P01.2 is authorized')
        require(s['status'] == 'AWAITING_HUMAN_APPROVAL', 'P01.2 must await human approval')
        require(s['last_human_approved_part'] == 'P01.1', 'P01.1 acceptance missing')
        expected = {'unity_project_created', 'android_tools_installed', 'game_modules_implemented'}
        require(set(s['scope']) == expected, 'Scope fields missing or unexpected')
        require(all(type(value) is bool for value in s['scope'].values()), 'Scope flags must be booleans')
        require(s['scope']['unity_project_created'] is True and s['scope']['game_modules_implemented'] is True,
                'P01.2 permits only long rules and tests')
        require(not any((root / p).exists() for p in ['nakama', 'art']), 'Premature game/backend directories')
        for module in ['Bots', 'Network', 'UI', 'Rules/Short']:
            require(not (root / 'unity/Assets/Game' / module).exists(), 'Premature gameplay module: ' + module)
        version = read(root, 'unity/ProjectSettings/ProjectVersion.txt')
        require('m_EditorVersion: 6000.6.7f1\n' in version, 'Unity version not pinned')
        manifest = json.loads(read(root, 'unity/Packages/manifest.json'))['dependencies']
        require(manifest['com.unity.test-framework'] == '1.8.0', 'Test Framework version mismatch')
        require(manifest['com.unity.inputsystem'] == '1.20.1', 'Input system version mismatch')
        locked = json.loads(read(root, 'unity/Packages/packages-lock.json'))['dependencies']
        for name in ['com.unity.inputsystem', 'com.unity.test-framework']:
            require(locked[name]['version'] == manifest[name], 'Manifest/lock mismatch: ' + name)
        require((root / 'unity/Assets/Game/Scenes/Bootstrap.unity').is_file(), 'Bootstrap scene missing')
        assembly = json.loads(read(root, 'unity/Assets/Game/Rules/Nard.Rules.Long.asmdef'))
        require(assembly.get('noEngineReferences') is True and not assembly.get('references'), 'Rules assembly must be pure C#')
        files = sorted(p.name for p in (root / 'unity/Assets/Game/Rules').glob('*.cs'))
        require(files == ['LongNardeRules.cs', 'LongPosition.cs'], 'Only long-engine sources authorized')
        for file in files:
            code = read(root, 'unity/Assets/Game/Rules/' + file)
            require(not re.search(r'UnityEngine|UnityEditor|System\.Random|System\.IO|DateTime|Nakama', code), 'Rules dependency violation')
        review = s['rules_review']
        require(review['preliminary_standard'] == 'ACCEPTED_BY_HUMAN' and
                review['independent_azerbaijani_players'] == 'NOT_RUN' and
                review['provisional_rules_retained'] is True, 'Local review falsely completed')
        return 'P01.2: pure long C# engine only; local review remains NOT_RUN'
    check('current_scope', scope)

    def policy():
        p = state(root)['tool_policy']
        for name in ['skills_allowed', 'antigravity_related_mcp_allowed', 'codex_gravity_mcp_allowed']:
            require(p[name] is False, f'Tool ban missing: {name}')
        require(p['standard_terminal_git_allowed'] is True, 'Standard tools must remain allowed')
        a = read(root, 'AGENTS.md')
        for term in ['Heç bir Skill', 'Antigravity MCP', 'Codex Gravity MCP', 'AWAITING_HUMAN_APPROVAL',
                     'P01.2', 'R01–R20']:
            require(term in a, f'Agent rule missing: {term}')
        return 'Agent-neutral rules and tool bans present'
    check('agent_policy', policy)

    def progress():
        s = state(root)
        expected = {'Aktiv part': s['active_part'], 'Status': s['status'],
                    'Son insan təsdiqi': s['last_human_approved_part'],
                    'Cari partın insan təsdiqi': str(s['current_part_human_approved']).lower(),
                    'Növbəti part': s['next_part'],
                    'Növbəti parta icazə': str(s['next_part_authorized']).lower()}
        text = read(root, 'PROGRESS.md')
        for label, value in expected.items():
            require(f'- {label}: `{value}`' in text, f'PROGRESS mismatch: {label}')
        return 'Human progress view matches STATE'
    check('progress_consistency', progress)

    def links():
        count = 0
        names = git(root, 'ls-files', '--cached', '--others', '--exclude-standard', '-z')
        require(names.returncode == 0, 'Cannot enumerate documentation')
        for name in sorted(set(filter(None, names.stdout.split('\0')))):
            file = root / name
            if file.suffix != '.md' or file.name in SOURCES:
                continue  # Immutable originals contain illustrative future paths.
            text = re.sub(r'```.*?```', '', file.read_text(encoding='utf-8'), flags=re.S)
            for target in re.findall(r'\[[^\]]*\]\(([^\s)]+)\)', text):
                target = target.strip('<>')
                if urlsplit(target).scheme or target.startswith('#'):
                    continue
                path = unquote(target.split('#', 1)[0])
                dest = (file.parent / path).resolve()
                require(dest.is_relative_to(root), f'Link escapes repo: {file.relative_to(root)}')
                require(dest.exists(), f'Broken link: {file.relative_to(root)} -> {target}')
                count += 1
        return f'{count} local Markdown links resolve'
    check('local_links', links)

    def assets():
        with (root / 'docs/ASSET_MANIFEST.csv').open(encoding='utf-8', newline='') as f:
            reader = csv.DictReader(f)
            require(reader.fieldnames == ASSET_COLUMNS, 'Asset schema mismatch')
            rows = list(reader)
        seen = set()
        for row in rows:
            require(set(row) == set(ASSET_COLUMNS), 'Malformed CSV asset row')
            require(row['asset_id'] and row['asset_id'] not in seen, 'Duplicate/empty asset ID')
            seen.add(row['asset_id'])
            require(row['source'] and row['creator'] and row['license'] and row['license_evidence'], 'Missing asset rights')
            p = (root / row['path']).resolve()
            require(bool(row['path']) and p.is_relative_to(root) and p.is_file(), 'Invalid asset path')
        return f'{len(rows)} assets registered'
    check('asset_manifest', assets)

    def git_root():
        r = git(root, 'rev-parse', '--show-toplevel')
        require(r.returncode == 0 and Path(r.stdout.strip()).resolve() == root, 'Wrong/missing Git root')
        branch = git(root, 'symbolic-ref', '--short', 'HEAD')
        require(branch.returncode == 0 and branch.stdout.strip() == state(root)['repository']['branch'], 'Branch mismatch')
        return 'Git root and branch match STATE'
    check('git_root', git_root)

    def ignores():
        for path in IGNORE_PROBES:
            r = git(root, 'check-ignore', '--no-index', '-q', '--', path)
            require(r.returncode == 0, f'Sensitive/cache path not ignored: {path}')
        for path in KEEP_PROBES:
            r = git(root, 'check-ignore', '--no-index', '-q', '--', path)
            require(r.returncode == 1, f'Important source incorrectly ignored: {path}')
        return f'{len(IGNORE_PROBES)} excluded + {len(KEEP_PROBES)} retained paths checked'
    check('gitignore_behavior', ignores)

    def file_safety():
        listing = git(root, 'ls-files', '--cached', '--others', '--exclude-standard', '-z')
        require(listing.returncode == 0, 'Cannot enumerate candidate files')
        paths = sorted(set(filter(None, listing.stdout.split('\0'))))
        tracked = git(root, 'ls-files', '-z')
        require(tracked.returncode == 0, 'Cannot enumerate index')
        for name in filter(None, tracked.stdout.split('\0')):
            require(git(root, 'check-ignore', '--no-index', '-q', '--', name).returncode == 1,
                    f'Ignored file already tracked: {name}')
        for name in paths:
            p = root / name
            require(not p.is_symlink(), f'Symlink needs explicit review: {name}')
            require(p.resolve().is_relative_to(root), f'Path escapes repo: {name}')
            data = p.read_bytes()
            require(len(data) <= 1024 * 1024, f'File over 1 MiB needs review: {name}')
            require(not any(pattern.search(data) for pattern in SECRET_PATTERNS), f'Secret-like content: {name}')
            blob = subprocess.run(['git', '-C', str(root), 'show', ':' + name], capture_output=True, timeout=20)
            if blob.returncode == 0:
                require(len(blob.stdout) <= 1024 * 1024, f'Large staged file: {name}')
                require(not any(pat.search(blob.stdout) for pat in SECRET_PATTERNS), f'Staged secret-like content: {name}')
        return f'{len(paths)} candidate files and available index blobs checked; limited pattern scan'
    check('candidate_and_index_safety', file_safety)

    def remote_safety():
        repo = state(root)['repository']
        r = git(root, 'remote', 'get-url', 'origin')
        require(r.returncode == 0 and r.stdout.strip() == repo['remote_url'], 'Remote ledger mismatch')
        require(repo['push_authorized'] is False, 'Current human instruction forbids push')
        require(repo['sync_status'] in ['NOT_PUSHED', 'DEFERRED_PUBLIC_REMOTE'],
                'Push recorded despite explicit no-push instruction')
        return f"Remote {repo['remote_visibility']}; sync {repo['sync_status']}"
    check('remote_safety', remote_safety)

    def rule_specification():
        from validate_rule_fixtures import validate as validate_rules
        result = validate_rules(root)
        require(state(root)['build']['physical_device'] == 'NOT_RUN', 'Physical device test must remain open')
        return f"{result['fixture_count']} authored examples; engine results separately in P01.2.execution.json, independent local review NOT_RUN"
    check('rule_specification', rule_specification)

    return {'schema_version': 1, 'part': state(root)['active_part'],
            'executed_at': datetime.now(ZoneInfo('Asia/Baku')).isoformat(timespec='seconds'),
            'passed': sum(c['status'] == 'PASS' for c in checks),
            'failed': sum(c['status'] == 'FAIL' for c in checks), 'checks': checks}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--root', type=Path, default=ROOT)
    parser.add_argument('--output', type=Path)
    args = parser.parse_args()
    try:
        report = validate(args.root)
    except (ValueError, KeyError, TypeError, OSError) as exc:
        report = {'passed': 0, 'failed': 1, 'checks': [{'name': 'read_state', 'status': 'FAIL', 'detail': str(exc)}]}
    output = json.dumps(report, ensure_ascii=False, indent=2) + '\n'
    if args.output:
        # The caller must choose an existing parent; this is only an evidence file.
        destination = args.output if args.output.is_absolute() else args.root / args.output
        destination.write_text(output, encoding='utf-8')
    print(output, end='')
    return int(report['failed'] != 0)


if __name__ == '__main__':
    raise SystemExit(main())
