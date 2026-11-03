using System;

namespace Nard.Rules.Short
{
    public enum ShortPhase { Opening, Moving, Committed, Finished }

    /// <summary>Immutable short-backgammon state in board-global-v1 coordinates.</summary>
    public sealed class ShortPosition
    {
        private readonly int[] points, bar, off, roll;
        public int Player { get; }
        public ShortPhase Phase { get; }
        public int UsedMask { get; }
        public int TokenCount => roll.Length == 0 ? 0 : roll[0] == roll[1] ? 4 : 2;
        public int Point(int index) => points[index];
        public int Bar(int player) => bar[player];
        public int Off(int player) => off[player];
        public int[] CopyPoints() => (int[])points.Clone();
        public int[] CopyBar() => (int[])bar.Clone();
        public int[] CopyOff() => (int[])off.Clone();
        public int[] CopyRoll() => (int[])roll.Clone();
        public bool IsUsed(int slot) => (UsedMask & (1 << slot)) != 0;
        public int Die(int slot)
        {
            if (slot < 0 || slot >= TokenCount) throw new ArgumentOutOfRangeException(nameof(slot));
            return roll[slot % 2];
        }

        public ShortPosition(int[] points, int[] bar, int[] off, int[] roll, int player,
            ShortPhase phase, int usedMask = 0, string rulesetVersion = ShortNardeRules.RulesetVersion)
        {
            if (rulesetVersion != ShortNardeRules.RulesetVersion) throw new ArgumentException("Unsupported short ruleset");
            if (points == null || points.Length != 24 || bar == null || bar.Length != 2 ||
                off == null || off.Length != 2 || roll == null || (roll.Length != 0 && roll.Length != 2))
                throw new ArgumentException("Invalid position dimensions");
            this.points = (int[])points.Clone(); this.bar = (int[])bar.Clone();
            this.off = (int[])off.Clone(); this.roll = (int[])roll.Clone();
            Player = player; Phase = phase; UsedMask = usedMask;
            if (!Enum.IsDefined(typeof(ShortPhase), phase) || player < -1 || player > 1 ||
                ((player == -1) != (phase == ShortPhase.Opening))) throw new ArgumentException("Invalid phase/player");
            foreach (int die in this.roll) if (die < 1 || die > 6) throw new ArgumentException("Invalid die");
            if (usedMask < 0 || (usedMask >> TokenCount) != 0) throw new ArgumentException("Invalid used mask");
            if (phase == ShortPhase.Moving && this.roll.Length != 2) throw new ArgumentException("Moving requires dice");
            for (int p = 0; p < 2; p++)
            {
                if (this.off[p] < 0 || this.off[p] > 15 || this.bar[p] < 0 || this.bar[p] > 15)
                    throw new ArgumentException("Invalid checker count");
                int total = this.off[p] + this.bar[p], sign = p == 0 ? 1 : -1;
                foreach (int n in this.points)
                {
                    if (n < -15 || n > 15) throw new ArgumentException("Invalid point count");
                    if (n * sign > 0) total += n * sign;
                }
                if (total != 15) throw new ArgumentException("Each player must have exactly 15 checkers");
            }
            bool winner = this.off[0] == 15 || this.off[1] == 15;
            if ((phase == ShortPhase.Finished) != winner || (this.off[0] == 15 && this.off[1] == 15))
                throw new ArgumentException("Winner/phase mismatch");
        }
    }

    public readonly struct ShortMove
    {
        public const int Bar = -1;
        public const int Off = 24;
        public int Source { get; }
        public int Target { get; }
        public int Slot { get; }
        public ShortMove(int source, int target, int slot) { Source = source; Target = target; Slot = slot; }
        public override string ToString() => $"{Source}>{Target}@{Slot}";
    }
}
