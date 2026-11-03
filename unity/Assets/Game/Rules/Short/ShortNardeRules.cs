using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nard.Rules.Short
{
    /// <summary>Pure short-backgammon rules. Dice are explicit inputs; no RNG, UI or match lifecycle.</summary>
    public static class ShortNardeRules
    {
        public const string RulesetVersion = "short-gdd1-draft.1";
        public static ShortPosition Initial()
        {
            var board = new int[24];
            board[23] = 2; board[12] = 5; board[7] = 3; board[5] = 5;
            board[0] = -2; board[11] = -5; board[16] = -3; board[18] = -5;
            return new ShortPosition(board, new int[2], new int[2], Array.Empty<int>(), -1, ShortPhase.Opening);
        }

        public static ShortPosition ResolveOpening(ShortPosition state, int die0, int die1)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Phase != ShortPhase.Opening || die0 < 1 || die0 > 6 || die1 < 1 || die1 > 6)
                throw new ArgumentException("Invalid opening input");
            if (die0 == die1) return state;
            return new ShortPosition(state.CopyPoints(), state.CopyBar(), state.CopyOff(), new[] { die0, die1 },
                die0 > die1 ? 0 : 1, ShortPhase.Moving);
        }

        private static int Distance(int point, int player) => player == 0 ? point + 1 : 24 - point;
        private static int Entry(int die, int player) => player == 0 ? 24 - die : die - 1;

        /// <summary>Local hop analysis only. Use LegalTurns/TryApplyTurn for maximum/higher-die enforcement.</summary>
        public static bool TryApplyLocalStep(ShortPosition state, ShortMove move, out ShortPosition after, out string reason)
        {
            after = state; reason = "OK";
            if (state == null) { reason = "NULL_STATE"; return false; }
            if (state.Phase != ShortPhase.Moving) { reason = "NOT_MOVING"; return false; }
            if (move.Source < ShortMove.Bar || move.Source >= 24 || move.Target < 0 || move.Target > ShortMove.Off ||
                move.Slot < 0 || move.Slot >= state.TokenCount) { reason = "INVALID_MOVE"; return false; }
            if (state.IsUsed(move.Slot)) { reason = "DIE_ALREADY_USED"; return false; }
            int p = state.Player, sign = p == 0 ? 1 : -1, die = state.Die(move.Slot);
            if (state.Bar(p) > 0 && move.Source != ShortMove.Bar) { reason = "BAR_REQUIRED"; return false; }
            if (move.Source == ShortMove.Bar)
            {
                if (state.Bar(p) == 0) { reason = "EMPTY_BAR"; return false; }
                if (move.Target != Entry(die, p)) { reason = "INVALID_BAR_ENTRY"; return false; }
            }
            else
            {
                if (state.Point(move.Source) * sign <= 0) { reason = "NOT_OWN_CHECKER"; return false; }
                if (move.Target == ShortMove.Off)
                {
                    for (int i = 0; i < 24; i++)
                        if (state.Point(i) * sign > 0 && Distance(i, p) > 6) { reason = "NOT_ALL_HOME"; return false; }
                    int distance = Distance(move.Source, p);
                    if (die < distance) { reason = "DIE_TOO_SMALL"; return false; }
                    if (die > distance)
                        for (int i = 0; i < 24; i++)
                            if (state.Point(i) * sign > 0 && Distance(i, p) > distance)
                            { reason = "OVERSHOOT_NOT_FARTHEST"; return false; }
                }
                else
                {
                    int delta = (move.Source - move.Target) * sign;
                    if (delta <= 0) { reason = "WRONG_DIRECTION"; return false; }
                    if (delta != die)
                    {
                        reason = state.Die(0) != state.Die(1) && delta == state.Die(0) + state.Die(1)
                            ? "SUMMED_DICE_STEP" : "WRONG_DISTANCE";
                        return false;
                    }
                }
            }
            if (move.Target != ShortMove.Off && state.Point(move.Target) * sign <= -2)
            { reason = "DESTINATION_OCCUPIED"; return false; }
            var board = state.CopyPoints(); var bar = state.CopyBar(); var off = state.CopyOff();
            if (move.Source == ShortMove.Bar) bar[p]--; else board[move.Source] -= sign;
            if (move.Target == ShortMove.Off) off[p]++;
            else
            {
                if (board[move.Target] == -sign) { board[move.Target] = 0; bar[1-p]++; }
                board[move.Target] += sign;
            }
            after = new ShortPosition(board, bar, off, state.CopyRoll(), p,
                off[p] == 15 ? ShortPhase.Finished : ShortPhase.Moving, state.UsedMask | (1 << move.Slot));
            return true;
        }

        /// <summary>All legal maximal paths in stable slot/source order. Equal dice use the lowest free slot.</summary>
        public static ReadOnlyCollection<ReadOnlyCollection<ShortMove>> LegalTurns(ShortPosition state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            var leaves = new List<ShortMove[]>();
            if (state.Phase != ShortPhase.Moving) return new List<ReadOnlyCollection<ShortMove>>().AsReadOnly();
            Search(state, new List<ShortMove>(), leaves);
            int max = 0, high = 0;
            foreach (var path in leaves) max = Math.Max(max, path.Length);
            if (max == 1) foreach (var path in leaves) if (path.Length == 1) high = Math.Max(high, state.Die(path[0].Slot));
            var result = new List<ReadOnlyCollection<ShortMove>>();
            foreach (var path in leaves)
                if (path.Length == max && (max != 1 || state.Die(path[0].Slot) == high)) result.Add(Array.AsReadOnly(path));
            return result.AsReadOnly();
        }

        private static void Search(ShortPosition state, List<ShortMove> path, List<ShortMove[]> leaves)
        {
            bool extended = false; int previousDie = 0;
            if (state.Phase == ShortPhase.Moving)
                for (int slot = 0; slot < state.TokenCount; slot++)
                {
                    if (state.IsUsed(slot)) continue;
                    int die = state.Die(slot);
                    if (die == previousDie) continue;
                    previousDie = die;
                    int first = state.Bar(state.Player) > 0 ? ShortMove.Bar : 0;
                    int last = state.Bar(state.Player) > 0 ? ShortMove.Bar : 23;
                    for (int source = first; source <= last; source++)
                    {
                        int target = source == ShortMove.Bar ? Entry(die, state.Player) : source + (state.Player == 0 ? -die : die);
                        if (target < 0 || target > 23) target = ShortMove.Off;
                        var move = new ShortMove(source, target, slot);
                        if (!TryApplyLocalStep(state, move, out var next, out _)) continue;
                        extended = true; path.Add(move); Search(next, path, leaves); path.RemoveAt(path.Count - 1);
                    }
                }
            if (!extended) leaves.Add(path.ToArray());
        }

        public static bool TryApplyTurn(ShortPosition state, IReadOnlyList<ShortMove> proposal,
            out ShortPosition after, out string reason)
        {
            after = state; reason = "OK";
            if (state == null || proposal == null) { reason = "INVALID_PROPOSAL"; return false; }
            if (state.Phase != ShortPhase.Moving) { reason = "NOT_MOVING"; return false; }
            if (proposal.Count > state.TokenCount) { reason = "TOO_MANY_MOVES"; return false; }
            var current = state;
            for (int i = 0; i < proposal.Count; i++)
                if (!TryApplyLocalStep(current, proposal[i], out current, out reason)) return false;
            var legal = LegalTurns(state); int max = legal[0].Count;
            if (proposal.Count != max) { reason = "MAX_DICE_REQUIRED"; return false; }
            if (max == 1 && state.Die(proposal[0].Slot) != state.Die(legal[0][0].Slot))
            { reason = "HIGHER_DIE_REQUIRED"; return false; }
            after = current; return true;
        }

        /// <summary>0 unfinished, 1 normal, 2 mars, 3 koks. No doubling cube, series or rewards.</summary>
        public static int ResultPoints(ShortPosition state, out int winner)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            winner = state.Off(0) == 15 ? 0 : state.Off(1) == 15 ? 1 : -1;
            if (winner < 0) return 0;
            int loser = 1 - winner;
            if (state.Off(loser) > 0) return 1;
            if (state.Bar(loser) > 0) return 3;
            int sign = loser == 0 ? 1 : -1;
            for (int i = winner == 0 ? 0 : 18; i < (winner == 0 ? 6 : 24); i++)
                if (state.Point(i) * sign > 0) return 3;
            return 2;
        }
    }
}
