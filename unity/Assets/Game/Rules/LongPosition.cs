using System;

namespace Nard.Rules.Long
{
    public enum LongPhase { Opening, AwaitRoll, Moving, Committed, Finished }

    /// <summary>Immutable long-narde position in board-global-v1 coordinates. No bar exists.</summary>
    public sealed class LongPosition
    {
        private readonly int[] points, off, roll, head;
        private readonly bool[] first;
        public int Player { get; }
        public LongPhase Phase { get; }
        public int UsedMask { get; }
        public int TokenCount => roll.Length == 0 ? 0 : roll[0] == roll[1] ? 4 : 2;
        public int Point(int index) => points[index];
        public int Off(int player) => off[player];
        public int HeadDepartures(int player) => head[player];
        public bool IsFirstTurn(int player) => first[player];
        public int[] CopyPoints() => (int[])points.Clone();
        public int[] CopyOff() => (int[])off.Clone();
        public int[] CopyRoll() => (int[])roll.Clone();
        public int[] CopyHeadDepartures() => (int[])head.Clone();
        public bool[] CopyFirstTurns() => (bool[])first.Clone();
        public bool IsUsed(int slot) => (UsedMask & (1 << slot)) != 0;
        public int Die(int slot)
        {
            if (slot < 0 || slot >= TokenCount) throw new ArgumentOutOfRangeException(nameof(slot));
            return roll[slot % 2];
        }

        public LongPosition(int[] points, int[] off, int[] roll, int player, LongPhase phase,
            bool[] firstTurns, int[] headDepartures, int usedMask = 0,
            string rulesetVersion = LongNardeRules.RulesetVersion)
        {
            if (rulesetVersion != LongNardeRules.RulesetVersion) throw new ArgumentException("Unsupported long ruleset");
            if (points == null || points.Length != 24 || off == null || off.Length != 2 ||
                roll == null || (roll.Length != 0 && roll.Length != 2) || firstTurns == null ||
                firstTurns.Length != 2 || headDepartures == null || headDepartures.Length != 2)
                throw new ArgumentException("Invalid position dimensions");
            this.points = (int[])points.Clone(); this.off = (int[])off.Clone();
            this.roll = (int[])roll.Clone(); first = (bool[])firstTurns.Clone(); head = (int[])headDepartures.Clone();
            Player = player; Phase = phase; UsedMask = usedMask;
            if (!Enum.IsDefined(typeof(LongPhase), phase) || player < -1 || player > 1 ||
                ((player == -1) != (phase == LongPhase.Opening))) throw new ArgumentException("Invalid phase/player");
            foreach (int d in this.roll) if (d < 1 || d > 6) throw new ArgumentException("Invalid die");
            if (usedMask < 0 || (usedMask >> TokenCount) != 0) throw new ArgumentException("Invalid used mask");
            if (phase == LongPhase.Moving && roll.Length != 2) throw new ArgumentException("Moving requires dice");
            for (int p = 0; p < 2; p++)
            {
                if (this.off[p] < 0 || this.off[p] > 15 || head[p] < 0 || head[p] > 2) throw new ArgumentException("Invalid checker count");
                int total = this.off[p], sign = p == 0 ? 1 : -1;
                foreach (int n in this.points)
                {
                    if (n < -15 || n > 15) throw new ArgumentException("Invalid point count");
                    if (n * sign > 0) total += n * sign;
                }
                if (total != 15) throw new ArgumentException("Each player must have exactly 15 checkers");
            }
            bool winner = this.off[0] == 15 || this.off[1] == 15;
            if ((phase == LongPhase.Finished) != winner || (this.off[0] == 15 && this.off[1] == 15))
                throw new ArgumentException("Winner/phase mismatch");
        }
    }

    public readonly struct LongMove
    {
        public const int Off = 24;
        public int Source { get; }
        public int Target { get; }
        public int Slot { get; }
        public LongMove(int source, int target, int slot) { Source = source; Target = target; Slot = slot; }
        public override string ToString() => $"{Source}>{Target}@{Slot}";
    }
}
