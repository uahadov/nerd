using System;
using System.Collections.Generic;
using Nard.Rules.Long;
using Nard.Rules.Short;

namespace Nard.Match
{
    /// <summary>Immutable single-game coordinator. Explicit dice in, validated whole-turn commit out.
    /// No preview/undo, timers, save, UI, RNG, network or series auto-start.</summary>
    public sealed class MatchSession
    {
        public string GameId { get; }
        public GameMode Mode { get; }
        public string RuleSetVersion { get; }
        public MatchPhase Phase { get; }
        public int Player { get; }
        public int TurnNumber { get; }
        public LongPosition LongPosition { get; }
        public ShortPosition ShortPosition { get; }
        public GameResult Result { get; }

        private MatchSession(string id, GameMode mode, string version, MatchPhase phase, int player,
            int turnNumber, LongPosition longPosition, ShortPosition shortPosition, GameResult result = null)
        {
            GameId = id; Mode = mode; RuleSetVersion = version; Phase = phase; Player = player;
            TurnNumber = turnNumber; LongPosition = longPosition; ShortPosition = shortPosition; Result = result;
        }
        public static MatchSession Create(string gameId, GameMode mode, string version)
        {
            if (string.IsNullOrWhiteSpace(gameId)) throw new ArgumentException("Game ID is required", nameof(gameId));
            RuleContract.Require(mode, version);
            return new MatchSession(gameId, mode, version, MatchPhase.Created, -1, 0,
                mode == GameMode.Long ? LongNardeRules.Initial() : null,
                mode == GameMode.Short ? ShortNardeRules.Initial() : null);
        }
        private MatchSession With(MatchPhase phase, int player, int turnNumber,
            LongPosition longPosition, ShortPosition shortPosition, GameResult result = null)
            => new MatchSession(GameId, Mode, RuleSetVersion, phase, player, turnNumber, longPosition, shortPosition, result);

        public MatchSession Begin()
        {
            if (Phase != MatchPhase.Created) throw new InvalidOperationException("Only a created match can begin");
            return With(MatchPhase.Rolling, -1, 0, LongPosition, ShortPosition);
        }
        public MatchSession ResolveOpening(int player0Die, int player1Die)
        {
            if (Phase != MatchPhase.Rolling || Player != -1) throw new InvalidOperationException("Opening is not pending");
            if (Mode == GameMode.Long)
            {
                var position = LongNardeRules.ResolveOpening(LongPosition, player0Die, player1Die);
                return With(MatchPhase.Rolling, position.Player, position.Player == -1 ? 0 : 1, position, null);
            }
            var shortPosition = ShortNardeRules.ResolveOpening(ShortPosition, player0Die, player1Die);
            return With(shortPosition.Player == -1 ? MatchPhase.Rolling : MatchPhase.Moving,
                shortPosition.Player, shortPosition.Player == -1 ? 0 : 1, null, shortPosition);
        }
        public MatchSession SupplyRoll(int player, string version, int die0, int die1)
        {
            RuleContract.Require(Mode, version);
            if (Phase != MatchPhase.Rolling || Player < 0) throw new InvalidOperationException("A normal roll is not pending");
            if (player != Player) throw new ArgumentException("Wrong player", nameof(player));
            if (Mode == GameMode.Long)
            {
                var position = new LongPosition(LongPosition.CopyPoints(), LongPosition.CopyOff(), new[] { die0, die1 },
                    Player, LongPhase.Moving, LongPosition.CopyFirstTurns(), new int[2], 0, version);
                return With(MatchPhase.Moving, Player, TurnNumber, position, null);
            }
            var shortPosition = new ShortPosition(ShortPosition.CopyPoints(), ShortPosition.CopyBar(), ShortPosition.CopyOff(),
                new[] { die0, die1 }, Player, ShortPhase.Moving, 0, version);
            return With(MatchPhase.Moving, Player, TurnNumber, null, shortPosition);
        }

        /// <summary>Atomically validates and commits the complete remaining turn. Result is published only on a winning commit.</summary>
        public bool TryCommitTurn(int player, string version, IReadOnlyList<TurnMove> proposal,
            out MatchSession after, out string reason)
        {
            after = this; reason = "OK";
            if (version != RuleSetVersion) { reason = "RULESET_MISMATCH"; return false; }
            if (Phase != MatchPhase.Moving) { reason = "NOT_MOVING"; return false; }
            if (player != Player) { reason = "WRONG_PLAYER"; return false; }
            if (proposal == null) { reason = "INVALID_PROPOSAL"; return false; }
            if (proposal.Count > 4) { reason = "TOO_MANY_MOVES"; return false; }
            // Copy the request before adapting it; caller-owned arrays are never retained.
            var moves = new TurnMove[proposal.Count]; for (int i = 0; i < moves.Length; i++) moves[i] = proposal[i];
            if (Mode == GameMode.Long)
            {
                var typed = new LongMove[moves.Length];
                for (int i = 0; i < typed.Length; i++) typed[i] = new LongMove(moves[i].Source, moves[i].Target, moves[i].Slot);
                if (!LongNardeRules.TryApplyTurn(LongPosition, typed, out var position, out reason)) return false;
                bool finished = position.Phase == LongPhase.Finished;
                var first = position.CopyFirstTurns(); first[Player] = false;
                var committed = new LongPosition(position.CopyPoints(), position.CopyOff(), position.CopyRoll(), Player,
                    finished ? LongPhase.Finished : LongPhase.Committed, first, position.CopyHeadDepartures(), position.UsedMask);
                var result = finished ? GameResult.FromLong(GameId, version, committed) : null;
                after = With(finished ? MatchPhase.Finished : MatchPhase.TurnCommitted, Player, TurnNumber, committed, null, result);
                return true;
            }
            var shortMoves = new ShortMove[moves.Length];
            for (int i = 0; i < shortMoves.Length; i++) shortMoves[i] = new ShortMove(moves[i].Source, moves[i].Target, moves[i].Slot);
            if (!ShortNardeRules.TryApplyTurn(ShortPosition, shortMoves, out var shortPosition, out reason)) return false;
            bool shortFinished = shortPosition.Phase == ShortPhase.Finished;
            var shortCommitted = new ShortPosition(shortPosition.CopyPoints(), shortPosition.CopyBar(), shortPosition.CopyOff(),
                shortPosition.CopyRoll(), Player, shortFinished ? ShortPhase.Finished : ShortPhase.Committed, shortPosition.UsedMask);
            var shortResult = shortFinished ? GameResult.FromShort(GameId, version, shortCommitted) : null;
            after = With(shortFinished ? MatchPhase.Finished : MatchPhase.TurnCommitted, Player, TurnNumber, null, shortCommitted, shortResult);
            return true;
        }
        public MatchSession AdvanceTurn()
        {
            if (Phase != MatchPhase.TurnCommitted) throw new InvalidOperationException("Only a committed unfinished turn can advance");
            // The engine position remains the last committed snapshot until SupplyRoll creates the next active position.
            return With(MatchPhase.Rolling, 1 - Player, checked(TurnNumber + 1), LongPosition, ShortPosition);
        }
    }
}
