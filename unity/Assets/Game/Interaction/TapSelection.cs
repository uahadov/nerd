using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Nard.Match;
using Nard.Rules.Long;
using Nard.Rules.Short;

namespace Nard.Interaction
{
    public enum TapOutcome { Selected, Deselected, Moved, Rejected }

    /// <summary>A local, uncommitted turn preview. Only prefixes of the engine's maximal legal paths are selectable.
    /// No dice generation, turn advance, commit, undo, network or result publication.</summary>
    public sealed class TapSelection
    {
        public const int None = -2;
        private List<TurnMove[]> remaining;
        private readonly List<TurnMove> prefix = new List<TurnMove>();
        public GameMode Mode { get; }
        public int LocalPlayer { get; }
        public int Selected { get; private set; } = None;
        public string Reason { get; private set; } = "SELECT_CHECKER";
        public LongPosition LongPosition { get; private set; }
        public ShortPosition ShortPosition { get; private set; }
        public ReadOnlyCollection<TurnMove> Prefix => Array.AsReadOnly(prefix.ToArray());
        public bool CanInteract => Mode == GameMode.Long
            ? LongPosition.Phase == LongPhase.Moving && LongPosition.Player == LocalPlayer
            : ShortPosition.Phase == ShortPhase.Moving && ShortPosition.Player == LocalPlayer;
        public bool IsExhausted => !remaining.Any(path => path.Length > 0);
        public TapSelection(LongPosition position, int localPlayer)
        {
            if (position == null) throw new ArgumentNullException(nameof(position));
            CheckPlayer(localPlayer); Mode = GameMode.Long; LocalPlayer = localPlayer; LongPosition = position;
            remaining = LongNardeRules.LegalTurns(position).Select(path => path.Select(m => new TurnMove(m.Source,m.Target,m.Slot)).ToArray()).ToList();
        }
        public TapSelection(ShortPosition position, int localPlayer)
        {
            if (position == null) throw new ArgumentNullException(nameof(position));
            CheckPlayer(localPlayer); Mode = GameMode.Short; LocalPlayer = localPlayer; ShortPosition = position;
            remaining = ShortNardeRules.LegalTurns(position).Select(path => path.Select(m => new TurnMove(m.Source,m.Target,m.Slot)).ToArray()).ToList();
        }
        private static void CheckPlayer(int player)
        { if(player<0||player>1)throw new ArgumentOutOfRangeException(nameof(player)); }
        public ReadOnlyCollection<int> Targets => remaining.Where(path => path.Length>0 && path[0].Source==Selected)
            .Select(path=>path[0].Target).Distinct().OrderBy(x=>x).ToList().AsReadOnly();
        public TapOutcome Tap(int point)
        {
            if (!CanInteract) return Reject("NOT_YOUR_MOVING_TURN");
            if (point < -1 || point > 24) return Reject("OUTSIDE_BOARD");
            if (point == Selected) { Selected=None;Reason="SELECT_CHECKER";return TapOutcome.Deselected; }
            var candidates=remaining.Where(path=>path.Length>0 && path[0].Source==Selected && path[0].Target==point).ToList();
            if(candidates.Count>0)
            {
                var move=candidates[0][0];
                // Apply only a hop already present in an engine-approved complete path.
                if(Mode==GameMode.Long)
                {
                    if(!LongNardeRules.TryApplyLocalStep(LongPosition,new LongMove(move.Source,move.Target,move.Slot),out var after,out var reason))return Reject(reason);
                    LongPosition=after;
                }
                else
                {
                    if(!ShortNardeRules.TryApplyLocalStep(ShortPosition,new ShortMove(move.Source,move.Target,move.Slot),out var after,out var reason))return Reject(reason);
                    ShortPosition=after;
                }
                remaining=candidates.Where(path=>path[0].Slot==move.Slot).Select(path=>path.Skip(1).ToArray()).ToList();
                prefix.Add(move);Selected=None;Reason=IsExhausted?"EXAMPLE_COMPLETE":"MOVE_ACCEPTED";return TapOutcome.Moved;
            }
            if(remaining.Any(path=>path.Length>0 && path[0].Source==point))
            { Selected=point;Reason="CHOOSE_MARKED_TARGET";return TapOutcome.Selected; }
            return Reject(IsExhausted?"EXAMPLE_COMPLETE":"ILLEGAL_SELECTION_OR_TARGET");
        }
        private TapOutcome Reject(string reason) { Reason=reason;return TapOutcome.Rejected; }
    }
}
