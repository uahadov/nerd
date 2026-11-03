using System;
using Nard.Match;
using Nard.Rules.Long;
using Nard.Rules.Short;

namespace Nard.Presentation
{
    /// <summary>Read-only projection, not a second rules engine. No legal moves, dice generation or turn changes.</summary>
    public sealed class BoardSnapshot
    {
        private readonly int[] points, bar, off;
        public GameMode Mode { get; }
        private BoardSnapshot(GameMode mode, int[] points, int[] bar, int[] off)
        { Mode = mode; this.points = points; this.bar = bar; this.off = off; }
        public int Point(int index) => points[index];
        public int Bar(int player) => bar[player];
        public int Off(int player) => off[player];
        public static BoardSnapshot From(LongPosition position)
        {
            if (position == null) throw new ArgumentNullException(nameof(position));
            return new BoardSnapshot(GameMode.Long, position.CopyPoints(), new int[2], position.CopyOff());
        }
        public static BoardSnapshot From(ShortPosition position)
        {
            if (position == null) throw new ArgumentNullException(nameof(position));
            return new BoardSnapshot(GameMode.Short, position.CopyPoints(), position.CopyBar(), position.CopyOff());
        }
        public static BoardSnapshot From(MatchSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            return session.Mode == GameMode.Long ? From(session.LongPosition) : From(session.ShortPosition);
        }
    }
}
