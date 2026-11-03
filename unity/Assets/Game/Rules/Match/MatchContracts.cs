using System;
using Nard.Rules.Long;
using Nard.Rules.Short;

namespace Nard.Match
{
    public enum GameMode { Long, Short }
    public enum MatchPhase { Created, Rolling, Moving, TurnCommitted, Finished }
    public enum WinKind { Normal, Mars, Koks }
    public enum MatchChannel { Bot, Private, Random }

    public static class RuleContract
    {
        public static string VersionFor(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.Long: return LongNardeRules.RulesetVersion;
                case GameMode.Short: return ShortNardeRules.RulesetVersion;
                default: throw new ArgumentException("Unknown game mode", nameof(mode));
            }
        }
        public static void Require(GameMode mode, string version)
        {
            if (version != VersionFor(mode)) throw new ArgumentException("Ruleset version does not match game mode", nameof(version));
        }
    }

    /// <summary>Derived only from a finished engine position. Game points are separate from series wins.</summary>
    public sealed class GameResult
    {
        public string GameId { get; }
        public GameMode Mode { get; }
        public string RuleSetVersion { get; }
        public int Winner { get; }
        public WinKind Kind { get; }
        public int Points { get; }
        private GameResult(string id, GameMode mode, string version, int winner, int points)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Game ID is required", nameof(id));
            RuleContract.Require(mode, version);
            if (winner < 0 || points < 1) throw new InvalidOperationException("Game has not finished");
            GameId = id; Mode = mode; RuleSetVersion = version; Winner = winner; Points = points;
            Kind = points == 1 ? WinKind.Normal : points == 2 ? WinKind.Mars : WinKind.Koks;
        }
        public static GameResult FromLong(string id, string version, LongPosition position)
        {
            RuleContract.Require(GameMode.Long, version);
            int points = LongNardeRules.ResultPoints(position, out int winner);
            return new GameResult(id, GameMode.Long, version, winner, points);
        }
        public static GameResult FromShort(string id, string version, ShortPosition position)
        {
            RuleContract.Require(GameMode.Short, version);
            int points = ShortNardeRules.ResultPoints(position, out int winner);
            return new GameResult(id, GameMode.Short, version, winner, points);
        }
    }

    /// <summary>Eligibility only. The confirmation flag must come from the future trusted authority,
    /// not a client assertion. No rating calculation or persistence exists here.</summary>
    public static class RatingPolicy
    {
        public static bool IsEligible(MatchChannel channel, bool ranked, bool authoritativeResultConfirmed)
        {
            if (!Enum.IsDefined(typeof(MatchChannel), channel)) throw new ArgumentException("Unknown channel", nameof(channel));
            return channel == MatchChannel.Random && ranked && authoritativeResultConfirmed;
        }
    }

    /// <summary>board-global-v1 request: source -1 is short bar, target 24 is off; slot is a unique die token.</summary>
    public readonly struct TurnMove
    {
        public int Source { get; }
        public int Target { get; }
        public int Slot { get; }
        public TurnMove(int source, int target, int slot) { Source = source; Target = target; Slot = slot; }
    }
}
