using System.Collections.Generic;
using System.Linq;
using Nard.Match;
using Nard.Rules.Long;
using Nard.Rules.Short;
namespace Nard.Interaction
{
    // Replay only engine-approved prefixes against the immutable root. Never changes a committed session.
    public sealed class UndoPreview
    {
        private readonly LongPosition longRoot;
        private readonly ShortPosition shortRoot;
        private readonly int actor;
        private readonly Stack<TurnMove[]> history=new Stack<TurnMove[]>();
        public TapSelection Selection {get;private set;}
        // Successful taps can finish an engine preview before the enclosing match commits it.
        // LocalTurnLoop destroys this history at commit; a root with no local moves has no history.
        public bool CanUndo=>history.Count>0;
        public UndoPreview(LongPosition root,int actor){longRoot=root;this.actor=actor;Selection=new TapSelection(root,actor);}
        public UndoPreview(ShortPosition root,int actor){shortRoot=root;this.actor=actor;Selection=new TapSelection(root,actor);}
        public TapOutcome Tap(int point)
        {
            var before=Selection.Prefix.ToArray();var outcome=Selection.Tap(point);
            if(outcome==TapOutcome.Moved)history.Push(before);
            return outcome;
        }
        public bool TryUndo(int player)
        {
            if(player!=actor||!CanUndo)return false;
            var prefix=history.Pop();
            Selection=longRoot!=null?new TapSelection(longRoot,actor):new TapSelection(shortRoot,actor);
            foreach(var move in prefix){Selection.Tap(move.Source);Selection.Tap(move.Target);}
            return true;
        }
    }
}
