using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nard.Rules.Long
{
    /// <summary>Pure deterministic long-narde rules. PROVISIONAL_RULE: local expert review remains pending.</summary>
    public static class LongNardeRules
    {
        public const string RulesetVersion = "long-az-gdd1-provisional.1";
        public static LongPosition Initial()
        {
            var points = new int[24]; points[0] = 15; points[12] = -15;
            return new LongPosition(points, new int[2], Array.Empty<int>(), -1, LongPhase.Opening,
                new[] { true, true }, new int[2]);
        }

        public static LongPosition ResolveOpening(LongPosition state, int die0, int die1)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Phase != LongPhase.Opening || die0 < 1 || die0 > 6 || die1 < 1 || die1 > 6)
                throw new ArgumentException("Invalid opening input");
            if (die0 == die1) return state;
            return new LongPosition(state.CopyPoints(), state.CopyOff(), Array.Empty<int>(), die0 > die1 ? 0 : 1,
                LongPhase.AwaitRoll, state.CopyFirstTurns(), new int[2]);
        }

        public static LongPosition SupplyOpeningRoll(LongPosition state, int die0, int die1)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Phase != LongPhase.AwaitRoll) throw new ArgumentException("Opening winner must await fresh dice");
            return new LongPosition(state.CopyPoints(), state.CopyOff(), new[] { die0, die1 }, state.Player,
                LongPhase.Moving, state.CopyFirstTurns(), new int[2]);
        }

        private static int Progress(int point, int player) => (point - player * 12 + 24) % 24;

        /// <summary>Local hop analysis ONLY. Does not enforce full-turn maximum/higher-die policy.
        /// Use LegalTurns/TryApplyTurn to accept a player's complete proposal.</summary>
        public static bool TryApplyLocalStep(LongPosition state, LongMove move, out LongPosition after, out string reason)
        {
            after = state; reason = "OK";
            if (state == null) { reason = "NULL_STATE"; return false; }
            if (state.Phase != LongPhase.Moving) { reason = "NOT_MOVING"; return false; }
            if (move.Source < 0 || move.Source >= 24 || move.Target < 0 || move.Target > 24 ||
                move.Slot < 0 || move.Slot >= state.TokenCount) { reason = "INVALID_MOVE"; return false; }
            if (state.IsUsed(move.Slot)) { reason = "DIE_ALREADY_USED"; return false; }
            int p = state.Player, sign = p == 0 ? 1 : -1, die = state.Die(move.Slot);
            if (state.Point(move.Source) * sign <= 0) { reason = "NOT_OWN_CHECKER"; return false; }
            int q = Progress(move.Source, p);
            int headLimit = state.IsFirstTurn(p) && state.Die(0) == state.Die(1) &&
                (die == 3 || die == 4 || die == 6) ? 2 : 1;
            if (q == 0 && state.HeadDepartures(p) >= headLimit) { reason = "HEAD_LIMIT"; return false; }
            if (move.Target == LongMove.Off)
            {
                for (int i = 0; i < 24; i++)
                    if (state.Point(i) * sign > 0 && Progress(i, p) < 18) { reason = "NOT_ALL_HOME"; return false; }
                if (q + die < 24) { reason = "DIE_TOO_SMALL"; return false; }
                if (q + die > 24)
                    for (int i = 0; i < 24; i++)
                        if (state.Point(i) * sign > 0 && Progress(i, p) < q)
                        { reason = "OVERSHOOT_NOT_FARTHEST"; return false; }
            }
            else
            {
                if (q + die >= 24) { reason = "SECOND_LAP"; return false; }
                if (move.Target != (move.Source + die) % 24) { reason = "WRONG_DISTANCE"; return false; }
                if (state.Point(move.Target) * sign < 0) { reason = "DESTINATION_OCCUPIED"; return false; }
            }
            var points = state.CopyPoints(); var off = state.CopyOff(); var head = state.CopyHeadDepartures();
            points[move.Source] -= sign;
            if (move.Target == LongMove.Off) off[p]++; else points[move.Target] += sign;
            if (q == 0) head[p]++;
            if (ForbiddenPrime(points, p, off[1-p])) { reason = "SIX_BLOCK_TRAPS_ALL"; return false; }
            after = new LongPosition(points, off, state.CopyRoll(), p,
                off[p] == 15 ? LongPhase.Finished : LongPhase.Moving, state.CopyFirstTurns(), head,
                state.UsedMask | (1 << move.Slot));
            return true;
        }

        private static bool ForbiddenPrime(int[] board, int actor, int opponentOff)
        {
            int sign = actor == 0 ? 1 : -1, opponent = 1-actor;
            // Windows follow the opponent's finite route, including the global 23/0 corner.
            // A checker already borne off has also passed every such window.
            for (int start = 0; start <= 18; start++)
            {
                bool full = true;
                for (int q = start; q < start + 6; q++)
                    if (board[(opponent * 12 + q) % 24] * sign <= 0) { full = false; break; }
                if (!full || opponentOff > 0) continue;
                bool ahead = false;
                for (int q = start + 6; q < 24; q++)
                    if (board[(opponent * 12 + q) % 24] * sign < 0) { ahead = true; break; }
                if (!ahead) return true;
            }
            return false;
        }

        /// <summary>All maximal legal sequences, stable slot/source order. Equal dice use the
        /// lowest remaining slot: equivalent token permutations are not separate game choices.</summary>
        public static ReadOnlyCollection<ReadOnlyCollection<LongMove>> LegalTurns(LongPosition state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            var leaves = new List<LongMove[]>();
            if (state.Phase != LongPhase.Moving) return new List<ReadOnlyCollection<LongMove>>().AsReadOnly();
            Search(state, new List<LongMove>(), leaves);
            int max = 0, highest = 0;
            foreach (var path in leaves) max = Math.Max(max, path.Length);
            if (max == 1) foreach (var path in leaves) if (path.Length == 1) highest = Math.Max(highest, state.Die(path[0].Slot));
            var result = new List<ReadOnlyCollection<LongMove>>();
            foreach (var path in leaves)
                if (path.Length == max && (max != 1 || state.Die(path[0].Slot) == highest))
                    result.Add(Array.AsReadOnly(path));
            return result.AsReadOnly();
        }

        private static void Search(LongPosition state, List<LongMove> path, List<LongMove[]> leaves)
        {
            bool extended = false; int previousDie = 0;
            if (state.Phase == LongPhase.Moving)
                for (int slot = 0; slot < state.TokenCount; slot++)
                {
                    if (state.IsUsed(slot)) continue;
                    int die = state.Die(slot);
                    if (die == previousDie) continue;
                    previousDie = die;
                    for (int source = 0; source < 24; source++)
                    {
                        int target = Progress(source, state.Player) + die >= 24 ? LongMove.Off : (source + die) % 24;
                        var move = new LongMove(source, target, slot);
                        if (!TryApplyLocalStep(state, move, out var next, out _)) continue;
                        extended = true; path.Add(move); Search(next, path, leaves); path.RemoveAt(path.Count - 1);
                    }
                }
            if (!extended) leaves.Add(path.ToArray());
        }

        public static bool TryApplyTurn(LongPosition state, IReadOnlyList<LongMove> proposal,
            out LongPosition after, out string reason)
        {
            after = state; reason = "OK";
            if (state == null || proposal == null) { reason = "INVALID_PROPOSAL"; return false; }
            if (state.Phase != LongPhase.Moving) { reason = "NOT_MOVING"; return false; }
            if (proposal.Count > state.TokenCount) { reason = "TOO_MANY_MOVES"; return false; }
            var current = state;
            for (int i = 0; i < proposal.Count; i++)
                if (!TryApplyLocalStep(current, proposal[i], out current, out reason)) return false;
            var legal = LegalTurns(state);
            int max = legal[0].Count;
            if (proposal.Count != max) { reason = "MAX_DICE_REQUIRED"; return false; }
            if (max == 1 && state.Die(proposal[0].Slot) != state.Die(legal[0][0].Slot))
            { reason = "HIGHER_DIE_REQUIRED"; return false; }
            after = current; return true;
        }

        /// <summary>0 = unfinished, 1 = normal, 2 = mars. No series, rating or rewards.</summary>
        public static int ResultPoints(LongPosition state, out int winner)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            winner = state.Off(0) == 15 ? 0 : state.Off(1) == 15 ? 1 : -1;
            return winner < 0 ? 0 : state.Off(1-winner) == 0 ? 2 : 1;
        }
    }
}
