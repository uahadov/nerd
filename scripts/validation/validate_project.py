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
AUTHORIZED_PART = 'P02.1'
ENGINE_BASELINE = {'unity/Assets/Game/Rules/LongNardeRules.cs': '48e29e9ac9231d198a04bea9026e434fb377763a7632ed3627bcfa5d08b96a07',
 'unity/Assets/Game/Rules/LongNardeRules.cs.meta': 'c57e59400cd9b27753e425648c29590da7c29597ff06ef5a4d4721f128d47796',
 'unity/Assets/Game/Rules/LongPosition.cs': 'e5e084c9ec79481adbc1e36d6314fc5ddef19f08a37eb6924d10a24ee2587c5c',
 'unity/Assets/Game/Rules/LongPosition.cs.meta': 'eec70ed4adca23273a4f9074065777e7087dc08dcc49f2aa7a38a4e09b1fbf0a',
 'unity/Assets/Game/Rules/Match.meta': '6ae0340b000cef09d0906a22aaacd6876667a1fa40ef3ab5f4af22c7866af5fb',
 'unity/Assets/Game/Rules/Match/MatchContracts.cs': '3605bac9cdfcea0f570a06a27a1924e202a63196b0a5abc08ddf6cb1ba03193c',
 'unity/Assets/Game/Rules/Match/MatchContracts.cs.meta': 'ac1dde11fe14a0fe57f1b49634bfd40808d7b471ce1a6709e19ead3dccad48e0',
 'unity/Assets/Game/Rules/Match/MatchSeries.cs': '73c6f7e10c987a628d0cb34bcdb2a5efaa0cefa39c72e5ef3f0686cce9fd577c',
 'unity/Assets/Game/Rules/Match/MatchSeries.cs.meta': '00f4a4977aeb2509f3d87666527e88fe233344ede7f370400ba9d0ef58953101',
 'unity/Assets/Game/Rules/Match/MatchSession.cs': '9046ffdb3c4e592eada4fc793cbfd8458089afa31c50da8c25fdcbeb071a971e',
 'unity/Assets/Game/Rules/Match/MatchSession.cs.meta': '0cbec4e196ec200e31b0cadef5be578e6436b8b99795b48da8adeccbb95c6a92',
 'unity/Assets/Game/Rules/Match/Nard.Match.asmdef': '344f85f556396c5649367ac5f7f1a84a7572082c52afa8bf020f42ccdcf86a60',
 'unity/Assets/Game/Rules/Match/Nard.Match.asmdef.meta': '21c266ef918c310d865c8a915c62a5e465f92376b6e700795b95d357d8cc26d9',
 'unity/Assets/Game/Rules/Nard.Rules.Long.asmdef': '5a679457f5465cde54ed4495b7a5b7c7d92249187b5b7b2ceff3aa61cfb66b0d',
 'unity/Assets/Game/Rules/Nard.Rules.Long.asmdef.meta': 'c75cbae2d8afecb19d0e42e6593bc7582b10eb77266e55f6db374682ccfcc589',
 'unity/Assets/Game/Rules/Short.meta': '71983c16886d3d1f678e8a73ba7dee6e012002b0d4785cdfae11625de65641a8',
 'unity/Assets/Game/Rules/Short/Nard.Rules.Short.asmdef': '56301d3301c4a749004be25e515afdfdc9c52bcb99bcfcdc761972d3af49819a',
 'unity/Assets/Game/Rules/Short/Nard.Rules.Short.asmdef.meta': 'a2c68bac09bce3ee0cf779a6552888e30e65a354e944594928ea08c83f621ad4',
 'unity/Assets/Game/Rules/Short/ShortNardeRules.cs': '8eea41858f56fc1d53c56077edf95c224e091a37c0c81fe3f5e1b64ec0b03904',
 'unity/Assets/Game/Rules/Short/ShortNardeRules.cs.meta': 'b4b1488bd3ac37b203962614ccc82d99331ee6295eb40e2760cf653bf2b0f84b',
 'unity/Assets/Game/Rules/Short/ShortPosition.cs': '8b59b463c250a73874dd1e2a32159e4d8d0b839172aeffb057b76a10a886dd9d',
 'unity/Assets/Game/Rules/Short/ShortPosition.cs.meta': '2f05a5cf1e0b651af0ea91cbeb5a49533e1a4708b1d998eefbd53db4f87ddbb2',
 'unity/Assets/Game/Tests/Match/MatchTests.cs': '7fde7bf4fe77108cd4c2a7858072bbf9765fae3f220f0bcca2ec9ced6fc33215',
 'unity/Assets/Game/Tests/Match/MatchTests.cs.meta': 'fa9628ad76b958f20374f8ff015559f754aab9daed2a6e48628f119d08b62668',
 'unity/Assets/Game/Tests/Match/Nard.Match.Tests.asmdef': '02bbff6a52a4e275e816a30bf557d2e74eee53956241d4ec020b69be736033e2',
 'unity/Assets/Game/Tests/Match/Nard.Match.Tests.asmdef.meta': '5ba545b5680503d9b3761b45770851af2a1375a7a9dcb948299c038023f5f42f',
 'unity/Assets/Game/Tests/Rules/LongRulesTests.cs': '14c4dbdc39f4cc3c0fb965c934afc747e7c89b146ee6e01eba36cec4841da362',
 'unity/Assets/Game/Tests/Rules/LongRulesTests.cs.meta': '10bb0adfc3b5632f948cbe731f4fecbcc1a64f28cfba7755d6530f195ab504d8',
 'unity/Assets/Game/Tests/Rules/Nard.Rules.Long.Tests.asmdef': '4ce440ca99c8abfbbf8a227ce574081d9a1866d8c0887a83759edba2cdaa460b',
 'unity/Assets/Game/Tests/Rules/Nard.Rules.Long.Tests.asmdef.meta': 'bc897bb9f6c148aa933d9bf1f3572d3d09c154ec4844e4813cf1b7a24b205076',
 'unity/Assets/Game/Tests/ShortRules/Nard.Rules.Short.Tests.asmdef': '670f77d2cf346383a470ee8f77fb8cbb3aade54898e32ccb4d9ee014e71ae837',
 'unity/Assets/Game/Tests/ShortRules/Nard.Rules.Short.Tests.asmdef.meta': 'd1d5912208f481e650861b66095c490f107ef6749918154a3cf6ef0d034d8ed0',
 'unity/Assets/Game/Tests/ShortRules/ShortRulesTests.cs': 'd8f26c34249fd0aa5b0c897a38bd3edd988639298058311ff4a6205becae50d1',
 'unity/Assets/Game/Tests/ShortRules/ShortRulesTests.cs.meta': '10897e52aee127d3906266fad97890189fbf49b4c1310d5d0049c6c656ad688f'}

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
    'unity/Assets/Game/Rules/Short/ShortPosition.cs', 'unity/Assets/Game/Rules/Short/ShortNardeRules.cs',
    'unity/Assets/Game/Rules/Short/Nard.Rules.Short.asmdef',
    'unity/Assets/Game/Tests/ShortRules/ShortRulesTests.cs',
    'unity/Assets/Game/Tests/ShortRules/Nard.Rules.Short.Tests.asmdef',
    'docs/reports/P01.3.md', 'docs/reports/P01.3.execution.json',
    'docs/reports/P01.3.validation.json', 'docs/reports/P01.3.tests.txt',
    'docs/reports/P01.3.editmode.xml', 'docs/reports/P01.3.playmode.xml',
    'unity/Assets/Game/Rules/Match/MatchContracts.cs', 'unity/Assets/Game/Rules/Match/MatchSession.cs',
    'unity/Assets/Game/Rules/Match/MatchSeries.cs', 'unity/Assets/Game/Rules/Match/Nard.Match.asmdef',
    'unity/Assets/Game/Tests/Match/MatchTests.cs', 'unity/Assets/Game/Tests/Match/Nard.Match.Tests.asmdef',
    'docs/reports/P01.4.md', 'docs/reports/P01.4.execution.json', 'docs/reports/P01.4.validation.json',
    'docs/reports/P01.4.tests.txt', 'docs/reports/P01.4.editmode.xml', 'docs/reports/P01.4.playmode.xml',
    'unity/Assets/Game/Presentation/BoardSnapshot.cs', 'unity/Assets/Game/Presentation/BoardGeometry.cs',
    'unity/Assets/Game/Presentation/BoardView.cs', 'unity/Assets/Game/Scenes/TechnicalBoard.unity',
    'unity/Assets/Game/Editor/P021Build.cs', 'unity/Assets/Game/Tests/BoardEditor/BoardProjectionTests.cs',
    'unity/Assets/Game/Tests/BoardPlayMode/BoardSceneTests.cs', 'docs/reports/P02.1.md',
    'docs/reports/P02.1.execution.json', 'docs/reports/P02.1.validation.json', 'docs/reports/P02.1.tests.txt',
    'docs/reports/P02.1.editmode.xml', 'docs/reports/P02.1.playmode.xml',
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
            require(all(r[2] == ('IN_PROGRESS' if prefix == 'A' and r[0] in ['A01', 'A02', 'A18', 'A53'] else status) for r in rows), f'{file} implementation status invalid')
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
                    if name.startswith('unity/Assets/Game/') or name == 'docs/rules/fixtures.json':
                        path = (root / name).resolve()
                        require(path.is_relative_to(root) and hashlib.sha256(path.read_bytes()).hexdigest() == digest,
                                'Engine evidence source mismatch: ' + name)
                if step != 'android':
                    document = ET.parse(root / ('docs/reports/P02.1.' + step + '.xml')).getroot()
                    require(document.attrib['failed'] == '0' and int(document.attrib['passed']) > 0, 'Failed NUnit XML')
                    require(document.attrib['passed'] == report['tests']['passed'] and document.attrib['skipped'] == report['tests']['skipped'], 'NUnit summary mismatch')
            require(execution['coverage']['executed_long_fixtures'] == 26 and execution['coverage']['executed_short_fixtures'] == 30
                    and execution['coverage']['executed_match_fixtures'] == 7
                    and execution['coverage']['deferred_unique_fixtures'] == 4,
                    'Fixture execution coverage mismatch')
            require(execution['steps']['editmode']['executed_board_cases'] == 38 and execution['steps']['playmode']['executed_board_cases'] == 1, 'Board suites not executed')
            visual = execution['visual']
            require(visual['status'] == 'PASS' and visual['physical_device'] == 'NOT_RUN', 'Visual/device evidence mixed')
            require({row['kind'] for row in visual['images']} == {'long', 'short', 'bar-off', 'editor-gameview'}, 'Missing board screenshot')
            for row in visual['images']:
                path = (root / row['path']).resolve()
                require(path.is_relative_to(root) and path.is_file() and hashlib.sha256(path.read_bytes()).hexdigest() == row['sha256'], 'Screenshot missing or changed')
            corpus = json.loads(read(root, 'docs/rules/fixtures.json'))['cases']
            ledger = execution['fixture_execution']
            require([r['id'] for r in ledger] == [c['id'] for c in corpus], 'Fixture ledger IDs/order mismatch')
            xml = ET.parse(root / 'docs/reports/P02.1.editmode.xml').getroot()
            passed_names = {c.attrib['name'] for c in xml.iter('test-case') if c.attrib['result'] == 'Passed'}
            for case, row in zip(corpus, ledger):
                if case['action']['kind'] == 'undo':
                    require(row['status'] == 'NOT_RUN' and row['deferred_to'] == 'P02.4', 'Undo falsely claimed complete')
                else:
                    prefix = 'MatchFixture_' if case['action']['kind'] in ['series', 'rating_policy'] else 'Fixture_' if case['mode'] == 'long' else 'ShortFixture_'
                    name = prefix + case['id']
                    require(row['status'] == 'PASS' and row['test_name'] == name and name in passed_names, 'Fixture PASS lacks matching NUnit case')

        return 'Stored PASS evidence and runtime source hashes match; independent device/local reviews remain NOT_RUN'
    check('test_evidence', evidence)

    def scope():
        s = state(root)
        # Updated only after the human's explicit P02.1 authorization (D022).
        require(s['active_part'] == AUTHORIZED_PART, 'Only P02.1 is authorized')
        require(s['status'] == 'AWAITING_HUMAN_APPROVAL', 'P02.1 must await human approval')
        require(s['last_human_approved_part'] == 'P01.4', 'P01.4 acceptance missing')
        expected = {'unity_project_created', 'android_tools_installed', 'game_modules_implemented'}
        require(set(s['scope']) == expected, 'Scope fields missing or unexpected')
        require(all(type(value) is bool for value in s['scope'].values()), 'Scope flags must be booleans')
        require(s['scope']['unity_project_created'] is True and s['scope']['game_modules_implemented'] is True,
                'P02.1 permits only read-only board rendering on frozen engines')
        require(not any((root / p).exists() for p in ['nakama', 'art']), 'Premature game/backend directories')
        for module in ['Bots', 'Network', 'UI']:
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
        short_assembly = json.loads(read(root, 'unity/Assets/Game/Rules/Short/Nard.Rules.Short.asmdef'))
        require(short_assembly.get('noEngineReferences') is True and not short_assembly.get('references'), 'Short assembly must be independent pure C#')
        expected_sources = {'LongNardeRules.cs', 'LongPosition.cs', 'Short/ShortNardeRules.cs', 'Short/ShortPosition.cs', 'Match/MatchContracts.cs', 'Match/MatchSession.cs', 'Match/MatchSeries.cs'}
        actual_sources = {p.relative_to(root / 'unity/Assets/Game/Rules').as_posix() for p in (root / 'unity/Assets/Game/Rules').rglob('*.cs')}
        require(actual_sources == expected_sources, 'Unauthorized rules sources')
        for file in ['ShortNardeRules.cs', 'ShortPosition.cs']:
            code = read(root, 'unity/Assets/Game/Rules/Short/' + file)
            require(not re.search(r'UnityEngine|UnityEditor|System\.Random|System\.IO|DateTime|Nakama|Nard\.Rules\.Long', code), 'Short dependency violation')
        for name, digest in ENGINE_BASELINE.items():
            require(hashlib.sha256((root / name).read_bytes()).hexdigest() == digest, 'Accepted engine code/test changed: ' + name)
        match_assembly = json.loads(read(root, 'unity/Assets/Game/Rules/Match/Nard.Match.asmdef'))
        require(match_assembly.get('noEngineReferences') is True and
                match_assembly.get('references') == ['Nard.Rules.Long', 'Nard.Rules.Short'], 'Match dependency boundary violated')
        for file in ['MatchContracts.cs', 'MatchSession.cs', 'MatchSeries.cs']:
            code = read(root, 'unity/Assets/Game/Rules/Match/' + file)
            require(not re.search(r'UnityEngine|UnityEditor|System\.Random|System\.IO|DateTime|Nakama', code), 'Match dependency violation')
        presentation = root / 'unity/Assets/Game/Presentation'
        require({p.name for p in presentation.glob('*.cs')} == {'BoardSnapshot.cs', 'BoardGeometry.cs', 'BoardView.cs'}, 'Unauthorized presentation sources')
        for file in presentation.glob('*.cs'):
            code = file.read_text()
            require(not re.search(r'TryApply|LegalTurns|SupplyRoll|AdvanceTurn|Random\.|InputSystem|OnMouse|IPointer|Nakama', code), 'Presentation duplicates rules or future interaction')
        assembly = json.loads(read(root, 'unity/Assets/Game/Presentation/Nard.Presentation.asmdef'))
        require(assembly['references'] == ['Nard.Match', 'Nard.Rules.Long', 'Nard.Rules.Short'], 'Presentation dependency violation')
        require('Müvəqqəti qrafika' in read(root, 'unity/Assets/Game/Presentation/BoardView.cs'), 'Temporary graphic disclosure missing')
        review = s['rules_review']
        require(review['preliminary_standard'] == 'ACCEPTED_BY_HUMAN' and
                review['independent_azerbaijani_players'] == 'NOT_RUN' and
                review['provisional_rules_retained'] is True, 'Local review falsely completed')
        return 'P02.1: read-only technical board; engines/match/tests frozen; local review NOT_RUN'
    check('current_scope', scope)

    def policy():
        p = state(root)['tool_policy']
        for name in ['skills_allowed', 'antigravity_related_mcp_allowed', 'codex_gravity_mcp_allowed']:
            require(p[name] is False, f'Tool ban missing: {name}')
        require(p['standard_terminal_git_allowed'] is True, 'Standard tools must remain allowed')
        a = read(root, 'AGENTS.md')
        for term in ['Heç bir Skill', 'Antigravity MCP', 'Codex Gravity MCP', 'AWAITING_HUMAN_APPROVAL',
                     'P02.1', 'R01–R20']:
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
        return f"{result['fixture_count']} authored examples; engine results separately in P02.1.execution.json, independent local review NOT_RUN"
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
