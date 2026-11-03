using System;
using System.Collections.ObjectModel;

namespace Nard.Match
{
    /// <summary>Immutable best-of-games ledger. A mars/koks is exactly one game win.</summary>
    public sealed class MatchSeries
    {
        private readonly int[] wins, gamePoints;
        public GameMode Mode { get; }
        public string RuleSetVersion { get; }
        public int BestOf { get; }
        public int WinsRequired => BestOf / 2 + 1;
        public int Winner { get; }
        public ReadOnlyCollection<GameResult> Games { get; }
        private static void CheckPlayer(int player)
        {
            if (player < 0 || player > 1) throw new ArgumentOutOfRangeException(nameof(player));
        }
        public int Wins(int player) { CheckPlayer(player); return wins[player]; }
        public int GamePoints(int player) { CheckPlayer(player); return gamePoints[player]; }
        public MatchSeries(GameMode mode, string version, int bestOf)
            : this(mode, version, bestOf, Array.Empty<GameResult>()) { }
        private MatchSeries(GameMode mode, string version, int bestOf, GameResult[] games)
        {
            RuleContract.Require(mode, version);
            if (bestOf != 1 && bestOf != 3 && bestOf != 5) throw new ArgumentException("Only single, best of 3 or best of 5", nameof(bestOf));
            Mode = mode; RuleSetVersion = version; BestOf = bestOf;
            Games = Array.AsReadOnly((GameResult[])games.Clone()); wins = new int[2]; gamePoints = new int[2];
            foreach (var result in games) { wins[result.Winner]++; gamePoints[result.Winner] += result.Points; }
            Winner = wins[0] == WinsRequired ? 0 : wins[1] == WinsRequired ? 1 : -1;
        }
        public MatchSeries Record(GameResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (Winner != -1) throw new InvalidOperationException("Series has finished");
            if (result.Mode != Mode || result.RuleSetVersion != RuleSetVersion) throw new ArgumentException("Result mode/version does not match series", nameof(result));
            foreach (var previous in Games)
                if (previous.GameId == result.GameId) throw new ArgumentException("Duplicate game result", nameof(result));
            var games = new GameResult[Games.Count + 1]; Games.CopyTo(games, 0); games[Games.Count] = result;
            return new MatchSeries(Mode, RuleSetVersion, BestOf, games);
        }
    }
}
