using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nard.Interaction;
using Nard.Match;
using Nard.Presentation;
using Nard.Rules.Long;
using Nard.Rules.Short;
using NUnit.Framework;
using LongFixture = Nard.Tests.Rules.LongRulesTests.Fixture;
using ShortFixture = Nard.Tests.ShortRules.ShortRulesTests.Fixture;

namespace Nard.Tests.Tap
{
    public sealed class TapSelectionTests
    {
        public static IEnumerable Cases()
        {
            foreach(TestCaseData data in Nard.Tests.Rules.LongRulesTests.FixtureCases())
            {
                var f=(LongFixture)data.Arguments[0];
                if(f.mode=="long"&&f.before.phase=="moving"&&(f.action.kind=="move"||f.action.kind=="turn"))
                    yield return new TestCaseData(f).SetName("TapFixture_"+f.id);
            }
            foreach(TestCaseData data in Nard.Tests.ShortRules.ShortRulesTests.FixtureCases())
            {
                var f=(ShortFixture)data.Arguments[0];
                if(f.before.phase=="moving"&&(f.action.kind=="move"||f.action.kind=="turn"))
                    yield return new TestCaseData(f).SetName("TapFixture_"+f.id);
            }
        }
        [TestCaseSource(nameof(Cases))] public void MarkersAndEveryFirstHopComeOnlyFromCompleteEnginePaths(object fixture)
        {
            Func<TapSelection> create;List<TurnMove[]> paths;
            if(fixture is LongFixture l)
            {var pos=l.before.Position();create=()=>new TapSelection(pos,pos.Player);paths=LongNardeRules.LegalTurns(pos).Select(p=>p.Select(m=>new TurnMove(m.Source,m.Target,m.Slot)).ToArray()).ToList();}
            else
            {var s=(ShortFixture)fixture;var pos=s.before.Position();create=()=>new TapSelection(pos,pos.Player);paths=ShortNardeRules.LegalTurns(pos).Select(p=>p.Select(m=>new TurnMove(m.Source,m.Target,m.Slot)).ToArray()).ToList();}
            var sources=paths.Where(p=>p.Length>0).Select(p=>p[0].Source).Distinct().ToArray();
            for(int source=-1;source<24;source++)
            {
                var tap=create();var outcome=tap.Tap(source);
                if(!sources.Contains(source)){Assert.AreEqual(TapOutcome.Rejected,outcome);Assert.AreEqual(0,tap.Prefix.Count);continue;}
                Assert.AreEqual(TapOutcome.Selected,outcome);
                var expected=paths.Where(p=>p.Length>0&&p[0].Source==source).Select(p=>p[0].Target).Distinct().OrderBy(x=>x);
                CollectionAssert.AreEqual(expected,tap.Targets);
                foreach(int target in tap.Targets)
                {
                    var test=create();test.Tap(source);Assert.AreEqual(TapOutcome.Moved,test.Tap(target));Assert.AreEqual(1,test.Prefix.Count);
                    var move=test.Prefix[0];
                    if(test.Mode==GameMode.Long)
                    {
                        Assert.IsTrue(LongNardeRules.TryApplyLocalStep(create().LongPosition,new LongMove(source,target,move.Slot),out var after,out _));
                        CollectionAssert.AreEqual(after.CopyPoints(),test.LongPosition.CopyPoints());CollectionAssert.AreEqual(after.CopyOff(),test.LongPosition.CopyOff());Assert.AreEqual(after.UsedMask,test.LongPosition.UsedMask);
                    }
                    else
                    {
                        Assert.IsTrue(ShortNardeRules.TryApplyLocalStep(create().ShortPosition,new ShortMove(source,target,move.Slot),out var after,out _));
                        CollectionAssert.AreEqual(after.CopyPoints(),test.ShortPosition.CopyPoints());CollectionAssert.AreEqual(after.CopyBar(),test.ShortPosition.CopyBar());CollectionAssert.AreEqual(after.CopyOff(),test.ShortPosition.CopyOff());Assert.AreEqual(after.UsedMask,test.ShortPosition.UsedMask);
                    }
                }
            }
        }
        private static LongPosition Long(int player=0,int a=3,int b=3)=>LongNardeRules.SupplyOpeningRoll(LongNardeRules.ResolveOpening(LongNardeRules.Initial(),player==0?6:1,player==0?1:6),a,b);
        private static ShortPosition Short(int player=0)=>ShortNardeRules.ResolveOpening(ShortNardeRules.Initial(),player==0?6:1,player==0?1:6);
        [Test] public void HigherDieIsUsedWhenBothDiceReachSameTarget()
        {
            var f=Cases().Cast<TestCaseData>().Select(t=>t.Arguments[0]).OfType<ShortFixture>().First(x=>x.id=="S_HIGH_REJECT");
            var move=f.action.moves[0];Assert.IsTrue(ShortNardeRules.TryApplyLocalStep(f.before.Position(),move.Move(),out _,out _));
            var tap=new TapSelection(f.before.Position(),f.before.to_move);tap.Tap(move.source);
            Assert.IsTrue(tap.Targets.Contains(move.target));Assert.AreEqual(TapOutcome.Moved,tap.Tap(move.target));
            Assert.AreNotEqual(move.slot,tap.Prefix[0].Slot);Assert.AreEqual(2,tap.ShortPosition.Die(tap.Prefix[0].Slot));
        }
        [TestCase(GameMode.Long)] [TestCase(GameMode.Short)] public void WrongOwnerCannotSelectOrChangeState(GameMode mode)
        {
            var tap=mode==GameMode.Long?new TapSelection(Long(),1):new TapSelection(Short(),1);
            for(int p=-2;p<=25;p++)Assert.AreEqual(TapOutcome.Rejected,tap.Tap(p));Assert.AreEqual(0,tap.Prefix.Count);Assert.IsEmpty(tap.Targets);
        }
        [TestCase(GameMode.Long)] [TestCase(GameMode.Short)] public void OpeningAndCommittedPositionsAreReadOnly(GameMode mode)
        {
            var tap=mode==GameMode.Long?new TapSelection(LongNardeRules.Initial(),0):new TapSelection(ShortNardeRules.Initial(),0);
            Assert.AreEqual(TapOutcome.Rejected,tap.Tap(0));Assert.AreEqual("NOT_YOUR_MOVING_TURN",tap.Reason);
            if(mode==GameMode.Long)
            {var p=Long();tap=new TapSelection(new LongPosition(p.CopyPoints(),p.CopyOff(),p.CopyRoll(),0,LongPhase.Committed,p.CopyFirstTurns(),p.CopyHeadDepartures()),0);}
            else{var p=Short();tap=new TapSelection(new ShortPosition(p.CopyPoints(),p.CopyBar(),p.CopyOff(),p.CopyRoll(),0,ShortPhase.Committed),0);}
            Assert.AreEqual(TapOutcome.Rejected,tap.Tap(0));
        }
        [Test] public void RepeatedSourceTapDeselectsAndInvalidTargetKeepsSelection()
        {
            var p=Long();var tap=new TapSelection(p,0);Assert.AreEqual(TapOutcome.Selected,tap.Tap(0));
            Assert.AreEqual(TapOutcome.Rejected,tap.Tap(12));Assert.AreEqual(0,tap.Selected);Assert.AreSame(p,tap.LongPosition);
            Assert.AreEqual(TapOutcome.Deselected,tap.Tap(0));Assert.IsEmpty(tap.Targets);Assert.AreEqual(0,tap.Prefix.Count);
        }
        [Test] public void AnotherOwnCheckerCanBeSelectedWithoutMoving()
        {
            var p=Short();var tap=new TapSelection(p,0);tap.Tap(23);Assert.AreEqual(TapOutcome.Selected,tap.Tap(12));
            Assert.AreEqual(12,tap.Selected);Assert.AreSame(p,tap.ShortPosition);Assert.AreEqual(0,tap.Prefix.Count);
        }
        [Test] public void EveryInitialRollPrefixCompletesOneEngineLegalTurnWithoutAdvancing()
        {
            for(int player=0;player<2;player++)for(int a=1;a<=6;a++)for(int b=1;b<=6;b++)
            {
                var p=Long(player,a,b);var tap=new TapSelection(p,player);var path=LongNardeRules.LegalTurns(p)[0];
                foreach(var m in path){Assert.AreEqual(TapOutcome.Selected,tap.Tap(m.Source));Assert.AreEqual(TapOutcome.Moved,tap.Tap(m.Target));}
                Assert.IsTrue(LongNardeRules.TryApplyTurn(p,tap.Prefix.Select(m=>new LongMove(m.Source,m.Target,m.Slot)).ToArray(),out var expected,out _));
                CollectionAssert.AreEqual(expected.CopyPoints(),tap.LongPosition.CopyPoints());Assert.AreEqual(player,tap.LongPosition.Player);Assert.IsTrue(tap.LongPosition.IsFirstTurn(player));Assert.IsTrue(tap.IsExhausted);
                Assert.AreEqual(TapOutcome.Rejected,tap.Tap(0));Assert.AreEqual(path.Count,tap.Prefix.Count);
                var s=new ShortPosition(ShortNardeRules.Initial().CopyPoints(),new int[2],new int[2],new[]{a,b},player,ShortPhase.Moving);
                var shortTap=new TapSelection(s,player);var shortPath=ShortNardeRules.LegalTurns(s)[0];
                foreach(var m in shortPath){shortTap.Tap(m.Source);Assert.AreEqual(TapOutcome.Moved,shortTap.Tap(m.Target));}
                Assert.IsTrue(ShortNardeRules.TryApplyTurn(s,shortTap.Prefix.Select(m=>new ShortMove(m.Source,m.Target,m.Slot)).ToArray(),out var after,out _));
                CollectionAssert.AreEqual(after.CopyPoints(),shortTap.ShortPosition.CopyPoints());Assert.AreEqual(player,shortTap.ShortPosition.Player);Assert.IsTrue(shortTap.IsExhausted);
            }
        }
        [Test] public void BlockedBarDoesNotAllowAnotherCheckerOrInventAPath()
        {
            var board=new int[24];board[5]=14;board[23]=-2;board[22]=-2;board[12]=-11;
            var p=new ShortPosition(board,new[]{1,0},new int[2],new[]{1,2},0,ShortPhase.Moving);var tap=new TapSelection(p,0);
            Assert.IsTrue(tap.IsExhausted);Assert.AreEqual(TapOutcome.Rejected,tap.Tap(-1));Assert.AreEqual(TapOutcome.Rejected,tap.Tap(5));Assert.AreSame(p,tap.ShortPosition);
        }
        [Test] public void ShortHitAndWinningBearOffUseEngineState()
        {
            var board=new int[24];board[7]=1;board[5]=14;board[4]=-1;board[0]=-14;
            var tap=new TapSelection(new ShortPosition(board,new int[2],new int[2],new[]{3,4},0,ShortPhase.Moving),0);
            tap.Tap(7);Assert.Contains(4,tap.Targets);Assert.AreEqual(TapOutcome.Moved,tap.Tap(4));Assert.AreEqual(1,tap.ShortPosition.Bar(1));
            board=new int[24];board[0]=1;board[12]=-15;
            tap=new TapSelection(new ShortPosition(board,new int[2],new[]{14,0},new[]{6,5},0,ShortPhase.Moving),0);
            tap.Tap(0);Assert.AreEqual(TapOutcome.Moved,tap.Tap(24));Assert.AreEqual(15,tap.ShortPosition.Off(0));Assert.AreEqual(ShortPhase.Finished,tap.ShortPosition.Phase);Assert.IsTrue(tap.IsExhausted);
        }
        [Test] public void NullPlayersAndReadOnlyPrefixRejectInvalidUse()
        {
            Assert.Throws<ArgumentNullException>(()=>new TapSelection((LongPosition)null,0));Assert.Throws<ArgumentNullException>(()=>new TapSelection((ShortPosition)null,0));
            Assert.Throws<ArgumentOutOfRangeException>(()=>new TapSelection(Long(),-1));Assert.Throws<ArgumentOutOfRangeException>(()=>new TapSelection(Short(),2));
            var tap=new TapSelection(Long(),0);Assert.AreEqual(TapOutcome.Rejected,tap.Tap(25));Assert.AreEqual(TapOutcome.Rejected,tap.Tap(-3));
            Assert.Throws<NotSupportedException>(()=>((IList<TurnMove>)tap.Prefix).Add(new TurnMove(0,3,0)));
        }
        [Test] public void HitMapMapsAllIndicesButNotGapsOrWrongPlayerTrays()
        {
            for(int i=0;i<24;i++){var b=BoardGeometry.PointBase(i);Assert.AreEqual(i,BoardHitMap.PointAt(new UnityEngine.Vector2(b.x,b.y>0?4:-4),0));}
            Assert.AreEqual(-1,BoardHitMap.PointAt(new UnityEngine.Vector2(-.25f,-4),0));Assert.AreEqual(-2,BoardHitMap.PointAt(new UnityEngine.Vector2(-.25f,4),0));
            Assert.AreEqual(24,BoardHitMap.PointAt(new UnityEngine.Vector2(4.53f,4),1));Assert.AreEqual(-2,BoardHitMap.PointAt(UnityEngine.Vector2.zero,0));
            Assert.AreEqual(-2,BoardHitMap.PointAt(new UnityEngine.Vector2(float.NaN,0),0));Assert.AreEqual(-2,BoardHitMap.PointAt(UnityEngine.Vector2.zero,2));
        }
    }
}
