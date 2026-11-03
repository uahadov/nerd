"""Mutation tests for specification checks, not gameplay-engine tests."""
import copy
import json
from pathlib import Path
import unittest
import validate_rule_fixtures as rules


class RuleSpecificationTests(unittest.TestCase):
    def setUp(self):
        root = rules.ROOT
        self.data = json.loads((root / 'docs/rules/fixtures.json').read_text())
        self.schema = json.loads((root / 'docs/rules/fixtures.schema.json').read_text())
        self.matrix = (root / 'docs/RULE_MATRIX.md').read_text()
        self.docs = '\n'.join((root / ('docs/' + n)).read_text() for n in
                              ['RULES_LONG.md', 'RULES_SHORT.md', 'RULESET_VARIANTS.md'])

    def case(self, name):
        return next(c for c in self.data['cases'] if c['id'] == name)

    def run_audit(self, sync_matrix=False):
        matrix = '\n'.join(rules.matrix_row(c) for c in self.data['cases']) if sync_matrix else self.matrix
        return rules.audit(self.data, self.schema, matrix, self.docs)

    def rejected(self, sync_matrix=True):
        with self.assertRaises(ValueError):
            self.run_audit(sync_matrix)

    def test_canonical_corpus_passes(self):
        result = self.run_audit()
        self.assertEqual(result['fixture_count'], 67)
        self.assertEqual(result['engine_tests'], 'NOT_RUN')
        self.assertEqual(result['human_rule_review'], 'PENDING')

    def test_boolean_is_not_integer(self):
        self.data['format_version'] = True
        self.rejected()

    def test_boolean_is_not_enum_integer(self):
        self.case('L_OPEN_HIGH')['expected']['starter'] = False
        self.rejected()

    def test_unknown_keyword_fails_closed(self):
        self.schema['$defs']['state']['oneOf'] = []
        self.rejected()

    def test_unknown_field_rejected(self):
        self.case('L_START')['before']['unrecognized'] = 0
        self.rejected()

    def test_missing_field_rejected(self):
        del self.case('L_START')['before']['off']
        self.rejected()

    def test_twenty_three_points_rejected(self):
        self.case('L_START')['before']['points'].pop()
        self.rejected()

    def test_checker_loss_rejected(self):
        self.case('L_START')['before']['points'][0] = 14
        self.rejected()

    def test_duplicate_id_rejected(self):
        self.data['cases'].append(copy.deepcopy(self.data['cases'][0]))
        self.rejected()

    def test_rejected_state_mutation_rejected(self):
        self.case('L_SECOND_HEAD_REJECT')['expected']['after']['head_departures'][0] = 2
        self.rejected()

    def test_accepted_transition_drift_rejected(self):
        self.case('L_STEP_OVER_OCCUPIED')['expected']['after']['head_departures'][0] = 0
        self.rejected()

    def test_accepted_reused_token_rejected(self):
        self.case('L_FIRST_33')['action']['moves'][1]['slot'] = 0
        self.rejected()

    def test_invalid_used_slot_rejected(self):
        self.case('L_STEP_OVER_OCCUPIED')['before']['used'] = [3]
        self.rejected()

    def test_one_die_roll_rejected(self):
        self.case('L_START')['before']['roll'] = [2]
        self.rejected()

    def test_long_bar_rejected(self):
        c = self.case('L_START')['before']; c['points'][0] -= 1; c['bar'][0] += 1
        self.rejected()

    def test_provisional_tag_loss_rejected(self):
        self.case('L_FIRST_33')['provisional_rules'] = []
        self.rejected()

    def test_ruleset_mode_mismatch_rejected(self):
        self.case('L_START')['ruleset_version'] = 'short-gdd1-draft.1'
        self.rejected()

    def test_matrix_drift_rejected(self):
        self.matrix = self.matrix.replace('| L_START |', '| WRONG_ID |')
        self.rejected(False)

    def test_missing_coverage_rejected(self):
        self.data['cases'] = [c for c in self.data['cases'] if 'C-RATING' not in c['covers']]
        self.rejected()

    def test_undocumented_requirement_rejected(self):
        self.docs = self.docs.replace('S-HIT', 'REMOVED')
        self.rejected()

    def test_wrong_undo_reference_rejected(self):
        self.case('L_UNDO_LOCAL')['action']['snapshot_fixture'] = 'MISSING'
        self.rejected()

    def test_wrong_opening_values_rejected(self):
        self.case('S_OPEN_HIGH')['expected']['next_roll'] = 'FRESH_TWO_DICE'
        self.rejected()

    def test_score_drift_rejected(self):
        self.case('S_MARS')['expected']['points'] = 1
        self.rejected()

    def test_rating_private_ranked_rejected(self):
        c = self.case('C_RATING_PRIVATE_CASUAL'); c['action']['ranked'] = True
        c['expected']['rating_eligible'] = True
        self.rejected()

    def test_best_of_count_drift_rejected(self):
        self.case('C_BO3_FINISHES')['expected']['win_counts'] = [1, 2]
        self.rejected()

    def test_kind_specific_fields_rejected(self):
        self.case('L_START')['action']['opening_dice'] = [6, 2]
        self.rejected()

    def test_score_label_condition_rejected(self):
        c = self.case('S_KOKS_BAR')['expected']; c['win_kind'] = 'MARS'; c['points'] = 2
        self.rejected()

    def test_accepted_move_without_roll_rejected(self):
        c = self.case('S_HIT_SINGLE'); c['before']['roll'] = []; c['before']['used'] = []
        self.rejected()

    def test_overshoot_example_really_exceeds_distance(self):
        c = self.case('S_OVERSHOOT_FARTHEST')
        move = c['action']['moves'][0]
        self.assertGreater(c['before']['roll'][move['slot']], move['source'] + 1)


if __name__ == '__main__':
    unittest.main()
