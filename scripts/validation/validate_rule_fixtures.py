#!/usr/bin/env python3
"""P01.1 specification audit only: no legal-move search or runtime rules engine.

Offline strict validator for the documented JSON Schema keyword subset. Unsupported
keywords fail closed. This is not a general draft-2020-12 implementation.
"""
import argparse
import copy
from datetime import datetime, timezone
import json
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[2]
PROVISIONAL = {'L-OPEN', 'L-HEAD-FIRST', 'L-BLOCK', 'L-BEAR', 'L-HIGH', 'L-MARS'}
KEYWORDS = {'$schema', '$id', 'description', '$defs', '$ref', 'type', 'properties',
            'required', 'additionalProperties', 'enum', 'const', 'items', 'minItems',
            'maxItems', 'uniqueItems', 'minimum', 'maximum', 'minLength', 'pattern'}


def need(condition, message):
    if not condition:
        raise ValueError(message)


def equal(a, b):
    return type(a) is type(b) and a == b


def schema_supported(s, root):
    need(isinstance(s, dict) and not set(s) - KEYWORDS, 'Unsupported schema keyword')
    if '$ref' in s:
        need(s['$ref'].startswith('#/$defs/') and s['$ref'][8:] in root['$defs'], 'Unknown schema reference')
    for key in ['properties', '$defs']:
        for child in s.get(key, {}).values():
            schema_supported(child, root)
    if 'items' in s:
        schema_supported(s['items'], root)


def schema_value(value, s, root, path='$'):
    if '$ref' in s:
        schema_value(value, root['$defs'][s['$ref'][8:]], root, path)
    types = {'object': dict, 'array': list, 'integer': int, 'string': str, 'boolean': bool}
    if 'type' in s:
        allowed = s['type'] if isinstance(s['type'], list) else [s['type']]
        need(all(t in types for t in allowed), 'Unsupported schema type')
        need(any(type(value) is types[t] for t in allowed), f'{path}: type mismatch')
    if 'enum' in s:
        need(any(equal(value, v) for v in s['enum']), f'{path}: invalid enum')
    if 'const' in s:
        need(equal(value, s['const']), f'{path}: invalid const')
    if isinstance(value, dict):
        need(set(s.get('required', [])) <= set(value), f'{path}: missing field')
        properties = s.get('properties', {})
        if s.get('additionalProperties') is False:
            need(set(value) <= set(properties), f'{path}: unexpected field')
        for k, v in value.items():
            if k in properties:
                schema_value(v, properties[k], root, path + '.' + k)
    if isinstance(value, list):
        need(len(value) >= s.get('minItems', 0) and len(value) <= s.get('maxItems', float('inf')), f'{path}: array length')
        if s.get('uniqueItems'):
            need(len({json.dumps(v, sort_keys=True) for v in value}) == len(value), f'{path}: duplicate value')
        for i, v in enumerate(value):
            if 'items' in s:
                schema_value(v, s['items'], root, f'{path}[{i}]')
    if type(value) is int:
        need(s.get('minimum', -float('inf')) <= value <= s.get('maximum', float('inf')), f'{path}: numeric bounds')
    if isinstance(value, str):
        need(len(value) >= s.get('minLength', 0), f'{path}: string length')
        if 'pattern' in s:
            need(re.search(s['pattern'], value) is not None, f'{path}: pattern')


def audit_state(s, mode):
    for player, sign in [(0, 1), (1, -1)]:
        total = sum(v * sign for v in s['points'] if v * sign > 0) + s['bar'][player] + s['off'][player]
        need(total == 15, 'Checker conservation: expected 15 per player')
    need(len(s['roll']) in [0, 2], 'Roll must be empty or two dice')
    tokens = s['roll'] * 2 if len(s['roll']) == 2 and s['roll'][0] == s['roll'][1] else s['roll']
    need(all(i < len(tokens) for i in s['used']), 'Used slot out of range')
    if mode == 'long':
        need(s['bar'] == [0, 0], 'Long mode cannot have bar checkers')
    else:
        need(s['head_departures'] == [0, 0], 'Short mode has no head counter')
    need(s['committed'] == (s['phase'] == 'committed'), 'Committed phase mismatch')
    need((s['to_move'] == -1) == (s['phase'] == 'opening'), 'Opening actor mismatch')
    if s['phase'] == 'finished':
        need(15 in s['off'], 'Finished state lacks winner')


def matrix_row(c):
    a, e = c['action'], c['expected']
    moves = ', '.join(f"{m['source']}→{m['target']}(slot {m['slot']})" for m in a['moves']) or a['kind']
    result = e['status'] + ': ' + e['reason']
    if 'max_used' in e:
        result += '; max=' + str(e['max_used'])
    if 'win_kind' in e:
        result += '; ' + e['win_kind'] + '=' + str(e['points'])
    return f"| {c['id']} | {c['title']} | {c['before']['roll']} | {moves} | {result} | {c['assurance']} |"


def audit(data, schema, matrix, documents):
    schema_supported(schema, schema)
    schema_value(data, schema, schema)
    cases = data['cases']
    ids = {c['id']: c for c in cases}
    need(len(ids) == len(cases), 'Duplicate fixture ID')
    tags = set(schema['$defs']['case']['properties']['covers']['items']['enum'])
    need(set().union(*(set(c['covers']) for c in cases)) == tags, 'Missing requirement coverage')
    need(all(tag in documents for tag in tags | PROVISIONAL), 'Undocumented requirement')
    need([line for line in matrix.splitlines() if re.match(r'^\| [LSC]_[A-Z0-9_]+ \|', line)] ==
         [matrix_row(c) for c in cases], 'Matrix drift from canonical JSON')
    for version, rs in data['rule_sets'].items():
        need(rs['mode'] == ('long' if version.startswith('long-') else 'short'), 'Ruleset mode mismatch')
        need(set(rs['provisional_rules']) == (PROVISIONAL if rs['mode'] == 'long' else set()), 'Ruleset provisional ledger mismatch')
    extra = {'start': set(), 'opening': {'opening_dice'}, 'move': set(), 'turn': set(),
             'undo': {'snapshot_fixture'}, 'classify_result': set(),
             'series': {'series_length', 'winners', 'win_kinds'}, 'rating_policy': {'channel', 'ranked'}}
    for c in cases:
        b, a, e, mode = c['before'], c['action'], c['expected'], c['mode']
        context = c['id'] + ': '
        need(data['rule_sets'][c['ruleset_version']]['mode'] == mode, context + 'Mode/version mismatch')
        provisional = (set(c['covers']) & PROVISIONAL) | ({'L-HIGH'} if mode == 'long' and 'C-HIGH' in c['covers'] else set())
        need(set(c['provisional_rules']) == provisional, context + 'Provisional ledger mismatch')
        need(c['assurance'] == ('PROVISIONAL_RULE' if provisional else 'GDD_ALIGNED'), context + 'Assurance mismatch')
        for s in [b, e['after']]:
            audit_state(s, mode)
        need(set(a) == {'kind', 'moves'} | extra[a['kind']], context + 'Kind-specific action fields')
        if a['kind'] not in ['move', 'turn']:
            need(not a['moves'], context + 'Unexpected moves')
        if a['kind'] == 'undo':
            need(a['snapshot_fixture'] in ids, context + 'Missing undo reference')
        if e['status'] == 'REJECT':
            need(e['after'] == b, context + 'Rejected action mutated state')
            continue
        if a['kind'] in ['move', 'turn']:
            need(b['to_move'] in [0, 1] and b['phase'] == 'moving', context + 'Invalid move context')
            need(len(b['roll']) == 2, context + 'Movement requires two dice')
            need(a['kind'] != 'move' or len(a['moves']) == 1, context + 'Move must contain one hop')
            after = copy.deepcopy(b)
            p = b['to_move']; sign = 1 if p == 0 else -1
            tokens = b['roll'] * 2 if b['roll'][0] == b['roll'][1] else b['roll']
            for m in a['moves']:
                slot, source, target = m['slot'], m['source'], m['target']
                need(slot < len(tokens) and slot not in after['used'], context + 'Invalid/reused token')
                need(after['off'][p] < 15, context + 'Move after final checker')
                if source == 'bar':
                    need(after['bar'][p] > 0, context + 'Empty bar source')
                    after['bar'][p] -= 1
                else:
                    need(after['points'][source] * sign > 0, context + 'Empty/wrong owner source')
                    after['points'][source] -= sign
                    if mode == 'long' and source == [0, 12][p]:
                        after['head_departures'][p] += 1
                if target == 'off':
                    after['off'][p] += 1
                else:
                    if after['points'][target] == -sign and mode == 'short':
                        after['points'][target] = 0
                        after['bar'][1-p] += 1
                    need(after['points'][target] * sign >= 0, context + 'Mixed owner target')
                    after['points'][target] += sign
                after['used'].append(slot)
                if after['off'][p] == 15:
                    after['phase'] = 'finished'
            need(after == e['after'], context + 'Authored transition arithmetic mismatch')
            if a['kind'] == 'turn':
                need(e['max_used'] == len(a['moves']), context + 'Declared max count mismatch')
        elif a['kind'] == 'undo':
            reference = ids.get(a['snapshot_fixture'])
            need(reference is not None and reference['mode'] == mode, context + 'Missing undo reference')
            need(b == reference['expected']['after'] and e['after'] == reference['before'], context + 'Undo snapshot drift')
        elif a['kind'] == 'opening':
            need(b['phase'] == 'opening', context + 'Opening phase required')
            dice = a['opening_dice']; winner = -1 if dice[0] == dice[1] else int(dice[1] > dice[0])
            need(e['starter'] == winner, context + 'Opening starter mismatch')
            after = copy.deepcopy(b)
            next_roll = 'REROLL_OPENING'
            if winner != -1:
                after.update(to_move=winner, phase='await_roll' if mode == 'long' else 'moving', roll=[] if mode == 'long' else dice)
                next_roll = 'FRESH_TWO_DICE' if mode == 'long' else 'OPENING_VALUES'
            need(e['after'] == after and e['next_roll'] == next_roll, context + 'Opening expectation mismatch')
        else:
            need(e['after'] == b, context + 'Read-only example mutated state')
            if a['kind'] == 'classify_result':
                need(e['winner'] in [0, 1] and b['off'][e['winner']] == 15, context + 'Winner mismatch')
                need(b['phase'] == 'finished', context + 'Result requires finished phase')
                loser = 1-e['winner']
                if e['win_kind'] == 'NORMAL':
                    need(b['off'][loser] > 0, context + 'Normal result needs loser off>0')
                else:
                    need(b['off'][loser] == 0, context + 'Mars/koks needs loser off=0')
                    if mode == 'short':
                        home = range(6) if e['winner'] == 0 else range(18, 24)
                        sign = 1 if loser == 0 else -1
                        exposed = b['bar'][loser] > 0 or any(b['points'][i] * sign > 0 for i in home)
                        need((e['win_kind'] == 'KOKS') == exposed, context + 'Declared koks condition mismatch')
                need(e['points'] == {'NORMAL': 1, 'MARS': 2, 'KOKS': 3}[e['win_kind']], context + 'Score mismatch')
                need(mode != 'long' or e['win_kind'] != 'KOKS', context + 'Long koks prohibited')
            elif a['kind'] == 'series':
                need(len(a['winners']) == len(a['win_kinds']), context + 'Series arrays mismatch')
                counts = [0, 0]; winner = -1; target = a['series_length'] // 2 + 1
                for p in a['winners']:
                    need(winner == -1, context + 'Series continues after winner')
                    counts[p] += 1
                    if counts[p] == target:
                        winner = p
                need(e['win_counts'] == counts and e['winner'] == winner, context + 'Series expectation mismatch')
            elif a['kind'] == 'rating_policy':
                need(e['rating_eligible'] == (a['channel'] == 'random' and a['ranked']), context + 'Rating boundary mismatch')
    return {'part': 'P01.1', 'executed_at': datetime.now(timezone.utc).isoformat(timespec='seconds'),
            'fixture_count': len(cases), 'provisional_fixture_count': sum(bool(c['provisional_rules']) for c in cases),
            'requirement_count': len(tags), 'status': 'PASS', 'engine_tests': 'NOT_RUN',
            'human_rule_review': 'PENDING', 'scope': 'schema, invariants, authored transition arithmetic, references and matrix consistency'}


def validate(root=ROOT):
    root = Path(root)
    load = lambda name: json.loads((root / name).read_text(encoding='utf-8'))
    return audit(load('docs/rules/fixtures.json'), load('docs/rules/fixtures.schema.json'),
                 (root / 'docs/RULE_MATRIX.md').read_text(encoding='utf-8'),
                 '\n'.join((root / ('docs/' + name)).read_text(encoding='utf-8') for name in
                           ['RULES_LONG.md', 'RULES_SHORT.md', 'RULESET_VARIANTS.md']))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--root', type=Path, default=ROOT)
    parser.add_argument('--output', type=Path)
    args = parser.parse_args()
    try:
        report = validate(args.root)
    except (ValueError, KeyError, IndexError, TypeError, OSError) as exc:
        report = {'part': 'P01.1', 'status': 'FAIL', 'detail': str(exc)}
    text = json.dumps(report, ensure_ascii=False, indent=2) + '\n'
    if args.output:
        args.output.write_text(text, encoding='utf-8')
    print(text, end='')
    return int(report['status'] != 'PASS')


if __name__ == '__main__':
    raise SystemExit(main())
