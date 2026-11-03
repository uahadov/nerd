using System;
using System.Collections.ObjectModel;
using System.Linq;
using Nard.Match;

namespace Nard.Interaction
{
    public enum TokenState { Available, Used, Unavailable }
    public readonly struct DiceToken
    {
        public int Slot { get; }
        public int Value { get; }
        public TokenState State { get; }
        public DiceToken(int slot,int value,TokenState state){Slot=slot;Value=value;State=state;}
    }

    /// <summary>Local two-human turn application; composes the accepted match and tap engines.
    /// Dice are injected. No UI, animation, undo, bot, series, save or online authority.</summary>
    public sealed class LocalTurnLoop
    {
        private readonly IDiceSource dice;
        private TapSelection selection;
        private int[] displayedRoll=Array.Empty<int>();
        public MatchSession Session { get; private set; }
        public MatchSession LastCommitted { get; private set; }
        public string Reason { get; private set; }="ROLL_TO_START";
        public bool IsOpeningDisplay { get; private set; }
        public bool CanRoll => Session.Phase==MatchPhase.Rolling;
        public int Selected => selection?.Selected??TapSelection.None;
        public ReadOnlyCollection<int> Targets => selection?.Targets??Array.AsReadOnly(Array.Empty<int>());
        public int[] CopyDisplayedRoll()=>(int[])displayedRoll.Clone();
        public Nard.Rules.Long.LongPosition LongPosition => selection?.LongPosition??Session.LongPosition;
        public Nard.Rules.Short.ShortPosition ShortPosition => selection?.ShortPosition??Session.ShortPosition;
        public ReadOnlyCollection<DiceToken> Tokens
        {
            get
            {
                if(IsOpeningDisplay||displayedRoll.Length==0)return Array.AsReadOnly(Array.Empty<DiceToken>());
                int count=selection!=null?(Session.Mode==GameMode.Long?selection.LongPosition.TokenCount:selection.ShortPosition.TokenCount):
                    LastCommitted==null?0:Session.Mode==GameMode.Long?LastCommitted.LongPosition.TokenCount:LastCommitted.ShortPosition.TokenCount;
                int used=selection!=null?(Session.Mode==GameMode.Long?selection.LongPosition.UsedMask:selection.ShortPosition.UsedMask):
                    LastCommitted==null?0:Session.Mode==GameMode.Long?LastCommitted.LongPosition.UsedMask:LastCommitted.ShortPosition.UsedMask;
                var result=new DiceToken[count];
                for(int i=0;i<count;i++)result[i]=new DiceToken(i,displayedRoll[i%2],(used&(1<<i))!=0?TokenState.Used:selection!=null?TokenState.Available:TokenState.Unavailable);
                return Array.AsReadOnly(result);
            }
        }
        public LocalTurnLoop(MatchSession session,IDiceSource dice)
        {
            if(session==null)throw new ArgumentNullException(nameof(session));
            this.dice=dice??throw new ArgumentNullException(nameof(dice));
            Session=session.Phase==MatchPhase.Created?session.Begin():session;
            if(Session.Phase==MatchPhase.TurnCommitted){LastCommitted=Session;SetDisplayedFromSession();Session=Session.AdvanceTurn();Reason="TURN_COMPLETE";}
            else if(Session.Phase==MatchPhase.Finished){LastCommitted=Session;SetDisplayedFromSession();Reason="GAME_FINISHED";}
            else if(Session.Phase==MatchPhase.Moving){SetDisplayedFromSession();BeginMoving();}
            else Reason=Session.Player<0?"ROLL_TO_START":"ROLL_FIRST";
        }
        private void SetDisplayedFromSession()=>displayedRoll=Session.Mode==GameMode.Long?Session.LongPosition.CopyRoll():Session.ShortPosition.CopyRoll();
        public bool TryRoll(int actor)
        {
            if(!CanRoll){Reason="ROLL_NOT_ALLOWED";return false;}
            if(actor!=Session.Player){Reason="WRONG_PLAYER";return false;}
            int a,b;
            try{a=dice.NextDie();b=dice.NextDie();}
            catch(Exception){Reason="DICE_SOURCE_FAILED";return false;}
            if(a<1||a>6||b<1||b>6){Reason="INVALID_DICE";return false;}
            var after=Session.Player==-1?Session.ResolveOpening(a,b):Session.SupplyRoll(actor,Session.RuleSetVersion,a,b);
            displayedRoll=new[]{a,b};IsOpeningDisplay=Session.Player==-1;
            Session=after;
            if(Session.Phase==MatchPhase.Moving){IsOpeningDisplay=false;BeginMoving();}
            else Reason=Session.Player==-1?"OPENING_TIE":"OPENING_WINNER_NEEDS_FRESH_ROLL";
            return true;
        }
        private void BeginMoving()
        {
            selection=Session.Mode==GameMode.Long?new TapSelection(Session.LongPosition,Session.Player):new TapSelection(Session.ShortPosition,Session.Player);
            Reason="SELECT_CHECKER";
            if(selection.IsExhausted)Settle(true);
        }
        public TapOutcome Tap(int actor,int point)
        {
            if(Session.Phase!=MatchPhase.Moving||selection==null){Reason=Session.Phase==MatchPhase.Finished?"GAME_FINISHED":"ROLL_FIRST";return TapOutcome.Rejected;}
            if(actor!=Session.Player){Reason="WRONG_PLAYER";return TapOutcome.Rejected;}
            var outcome=selection.Tap(point);Reason=selection.Reason;
            if(outcome==TapOutcome.Moved&&selection.IsExhausted)Settle(false);
            return outcome;
        }
        private void Settle(bool blocked)
        {
            if(!Session.TryCommitTurn(Session.Player,Session.RuleSetVersion,selection.Prefix,out var committed,out var reason))
                throw new InvalidOperationException("Engine-approved preview could not commit: "+reason);
            LastCommitted=committed;selection=null;
            Session=committed.Phase==MatchPhase.Finished?committed:committed.AdvanceTurn();
            Reason=Session.Phase==MatchPhase.Finished?"GAME_FINISHED":blocked?"NO_LEGAL_MOVES_TURN_PASSED":"TURN_COMPLETE";
        }
    }
}
