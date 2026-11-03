using System;
using System.Collections.Generic;
using System.Linq;
using Nard.Interaction;
using Nard.Match;
using Nard.Rules.Long;
using Nard.Rules.Short;
using NUnit.Framework;
using L=Nard.Tests.Rules.LongRulesTests;
using S=Nard.Tests.ShortRules.ShortRulesTests;
namespace Nard.Tests.Undo
{
    public sealed class UndoRegressionTests
    {
        private sealed class Dice:IDiceSource
        {private readonly int a,b;public int Calls;public Dice(int a,int b){this.a=a;this.b=b;}public int NextDie()=>++Calls%2==1?a:b;}
        private static TurnMove[] Path(LocalTurnLoop loop)=>loop.Session.Mode==GameMode.Long?
            LongNardeRules.LegalTurns(loop.LongPosition)[0].Select(m=>new TurnMove(m.Source,m.Target,m.Slot)).ToArray():
            ShortNardeRules.LegalTurns(loop.ShortPosition)[0].Select(m=>new TurnMove(m.Source,m.Target,m.Slot)).ToArray();
        private static string State(LongPosition p)=>string.Join(",",p.CopyPoints())+"|"+string.Join(",",p.CopyOff())+"|"+string.Join(",",p.CopyHeadDepartures())+"|"+p.UsedMask+"|"+p.Player+"|"+p.Phase+"|"+p.IsFirstTurn(0)+p.IsFirstTurn(1);
        private static string State(ShortPosition p)=>string.Join(",",p.CopyPoints())+"|"+string.Join(",",p.CopyOff())+"|"+string.Join(",",p.CopyBar())+"|"+p.UsedMask+"|"+p.Player+"|"+p.Phase;
        private static string State(LocalTurnLoop p)=>p.Session.Mode==GameMode.Long?State(p.LongPosition):State(p.ShortPosition);
        private static void Hop(LocalTurnLoop p,TurnMove m){p.Tap(p.Session.Player,m.Source);Assert.AreEqual(TapOutcome.Moved,p.Tap(p.Session.Player,m.Target));}
        public static IEnumerable<TestCaseData> Rolls(){foreach(GameMode mode in new[]{GameMode.Long,GameMode.Short})for(int actor=0;actor<2;actor++)for(int a=1;a<=6;a++)for(int b=1;b<=6;b++)yield return new TestCaseData(mode,actor,a,b).SetName($"UndoRoll_{mode}_P{actor}_{a}_{b}");}
        [TestCaseSource(nameof(Rolls))] public void UndoRestoresExactStateWithoutMintingDiceOrChangingTurns(GameMode mode,int actor,int a,int b)
        {
            var dice=new Dice(a,b);var loop=new LocalTurnLoop(Nard.Tests.Turn.LocalTurnTests.Ready(mode,actor),dice);
            Assert.IsFalse(loop.TryUndo(actor));Assert.IsTrue(loop.TryRoll(actor));var session=loop.Session;string before=State(loop);
            var path=Path(loop);Assert.That(path.Length,Is.GreaterThan(0));
            if(path.Length>1)
            {
                for(int repeat=0;repeat<3;repeat++)
                {
                    Hop(loop,path[0]);Assert.IsTrue(loop.CanUndo);string moved=State(loop);
                    Assert.IsFalse(loop.TryUndo(1-actor));Assert.AreEqual(moved,State(loop));Assert.AreSame(session,loop.Session);
                    Assert.AreEqual(TapOutcome.Rejected,loop.Tap(actor,100));Assert.AreEqual(moved,State(loop));
                    Assert.IsFalse(loop.TryRoll(actor));Assert.IsTrue(loop.TryUndo(actor));Assert.AreEqual(before,State(loop));
                    Assert.IsFalse(loop.CanUndo);Assert.IsFalse(loop.TryUndo(actor));Assert.AreEqual(2,dice.Calls);Assert.AreEqual(TapSelection.None,loop.Selected);
                    Assert.AreEqual(loop.Tokens.Count,loop.Tokens.Count(t=>t.State==TokenState.Available));
                }
            }
            foreach(var m in Path(loop))Hop(loop,m);
            Assert.AreEqual(MatchPhase.Rolling,loop.Session.Phase);Assert.AreEqual(1-actor,loop.Session.Player);Assert.AreEqual(session.TurnNumber+1,loop.Session.TurnNumber);
            string committed=State(loop);Assert.IsFalse(loop.TryUndo(actor));Assert.IsFalse(loop.TryUndo(1-actor));Assert.AreEqual(committed,State(loop));Assert.AreEqual(2,dice.Calls);
            Assert.IsTrue(loop.TryRoll(1-actor));string next=State(loop);Assert.IsFalse(loop.TryUndo(1-actor));Assert.AreEqual(next,State(loop));Assert.AreEqual(4,dice.Calls);
        }
        [TestCase(GameMode.Long)] [TestCase(GameMode.Short)] public void MultipleUndoRestoresPrefixesAndReplaysToIdenticalCommit(GameMode mode)
        {
            var loop=new LocalTurnLoop(Nard.Tests.Turn.LocalTurnTests.Ready(mode,0),new Dice(3,3));loop.TryRoll(0);var states=new List<string>{State(loop)};
            for(int i=0;i<2;i++){Hop(loop,Path(loop)[0]);states.Add(State(loop));}Assert.IsTrue(loop.TryUndo(0));Assert.AreEqual(states[1],State(loop));Assert.IsTrue(loop.TryUndo(0));Assert.AreEqual(states[0],State(loop));
            var path=Path(loop);foreach(var m in path)Hop(loop,m);Assert.AreEqual(path.Length,loop.Tokens.Count(t=>t.State==TokenState.Used));Assert.IsFalse(loop.CanUndo);
        }
        public static IEnumerable<TestCaseData> Samples()
        {
            foreach(var t in L.FixtureCases().Cast<TestCaseData>()){var f=(L.Fixture)t.Arguments[0];if(f.mode=="long"&&f.before.phase=="moving"&&(f.action.kind=="move"||f.action.kind=="turn"))yield return new TestCaseData(f,null).SetName("UndoReplay_"+f.id);}
            foreach(var t in S.FixtureCases().Cast<TestCaseData>()){var f=(S.Fixture)t.Arguments[0];if(f.before.phase=="moving"&&(f.action.kind=="move"||f.action.kind=="turn"))yield return new TestCaseData(null,f).SetName("UndoReplay_"+f.id);}
        }
        [TestCaseSource(nameof(Samples))] public void FixturePositionsRestoreBarOffHeadMaskAndKeepImmutableRoot(L.Fixture l,S.Fixture s)
        {
            var preview=l!=null?new UndoPreview(l.before.Position(),l.before.to_move):new UndoPreview(s.before.Position(),s.before.to_move);
            string root=l!=null?State(preview.Selection.LongPosition):State(preview.Selection.ShortPosition);
            var path=l!=null?LongNardeRules.LegalTurns(preview.Selection.LongPosition).FirstOrDefault()?.Select(m=>new TurnMove(m.Source,m.Target,m.Slot)).ToArray():ShortNardeRules.LegalTurns(preview.Selection.ShortPosition).FirstOrDefault()?.Select(m=>new TurnMove(m.Source,m.Target,m.Slot)).ToArray();
            if(path==null||path.Length==0){Assert.IsFalse(preview.CanUndo);Assert.IsFalse(preview.TryUndo(l!=null?l.before.to_move:s.before.to_move));return;}
            int actor=l!=null?l.before.to_move:s.before.to_move;
            preview.Tap(path[0].Source);Assert.AreEqual(TapOutcome.Moved,preview.Tap(path[0].Target));Assert.IsTrue(preview.TryUndo(actor));Assert.AreEqual(root,l!=null?State(preview.Selection.LongPosition):State(preview.Selection.ShortPosition));
            preview.Tap(path[0].Source);Assert.AreEqual(TapOutcome.Moved,preview.Tap(path[0].Target));Assert.AreEqual(1,preview.Selection.Prefix.Count);
        }
        [TestCase(GameMode.Long)] [TestCase(GameMode.Short)] public void ALoadedPartialPreviewCannotUndoRightsItDidNotOwn(GameMode mode)
        {
            var loop=new LocalTurnLoop(Nard.Tests.Turn.LocalTurnTests.Ready(mode,0),new Dice(3,3));loop.TryRoll(0);Hop(loop,Path(loop)[0]);
            var p=mode==GameMode.Long?new UndoPreview(loop.LongPosition,0):new UndoPreview(loop.ShortPosition,0);Assert.IsFalse(p.TryUndo(0));
            int mask=mode==GameMode.Long?p.Selection.LongPosition.UsedMask:p.Selection.ShortPosition.UsedMask;
            var next=Path(loop)[0];p.Tap(next.Source);Assert.AreEqual(TapOutcome.Moved,p.Tap(next.Target));Assert.IsTrue(p.TryUndo(0));Assert.AreEqual(mask,mode==GameMode.Long?p.Selection.LongPosition.UsedMask:p.Selection.ShortPosition.UsedMask);Assert.IsFalse(p.TryUndo(0));
        }
        [TestCase(GameMode.Long)] [TestCase(GameMode.Short)] public void FinishedMatchCannotBeUndoneAndWinnerRemainsImmutable(GameMode mode)
        {
            var loop=new LocalTurnLoop(Nard.Tests.Turn.LocalTurnTests.Ready(mode,0),new Dice(3,2));int limit=2000;
            while(loop.Session.Phase!=MatchPhase.Finished&&limit-->0){if(loop.CanRoll)loop.TryRoll(loop.Session.Player);if(loop.Session.Phase==MatchPhase.Moving)foreach(var m in Path(loop))Hop(loop,m);}
            Assert.Greater(limit,0);Assert.AreEqual(MatchPhase.Finished,loop.Session.Phase);var session=loop.Session;var result=session.Result;string before=State(loop);
            for(int actor=-1;actor<3;actor++)Assert.IsFalse(loop.TryUndo(actor));Assert.AreSame(session,loop.Session);Assert.AreSame(result,loop.Session.Result);Assert.AreEqual(before,State(loop));Assert.IsFalse(loop.CanUndo);
        }
        [Test] public void ConstructorsRejectNullAndInvalidActors()
        {
            Assert.Throws<ArgumentNullException>(()=>new UndoPreview((LongPosition)null,0));Assert.Throws<ArgumentNullException>(()=>new UndoPreview((ShortPosition)null,0));
            Assert.Throws<ArgumentOutOfRangeException>(()=>new UndoPreview(LongNardeRules.Initial(),2));Assert.Throws<ArgumentOutOfRangeException>(()=>new UndoPreview(ShortNardeRules.Initial(),-1));
        }
    }
}
