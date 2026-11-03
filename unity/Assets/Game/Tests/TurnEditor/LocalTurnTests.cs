using System;
using System.Collections.Generic;
using System.Linq;
using Nard.Interaction;
using Nard.Match;
using Nard.Rules.Long;
using Nard.Rules.Short;
using NUnit.Framework;

namespace Nard.Tests.Turn
{
    public sealed class LocalTurnTests
    {
        private sealed class Dice : IDiceSource
        {
            private readonly Queue<int> values; public int Calls;
            public Dice(params int[] values){this.values=new Queue<int>(values);}
            public int NextDie(){Calls++;return values.Dequeue();}
        }
        private sealed class SeedDice : IDiceSource
        { private readonly Random random;public int Calls;public SeedDice(int seed){random=new Random(seed);}public int NextDie(){Calls++;return random.Next(1,7);} }
        private static MatchSession Create(GameMode mode)=>MatchSession.Create("turn-test",mode,RuleContract.VersionFor(mode));
        public static MatchSession Ready(GameMode mode,int actor)
        {
            var session=Create(mode).Begin();
            int starter=mode==GameMode.Long?actor:1-actor;
            session=session.ResolveOpening(starter==0?6:1,starter==0?1:6);
            if(mode==GameMode.Short)
            {
                var path=ShortNardeRules.LegalTurns(session.ShortPosition)[0].Select(m=>new TurnMove(m.Source,m.Target,m.Slot)).ToArray();
                Assert.IsTrue(session.TryCommitTurn(starter,session.RuleSetVersion,path,out session,out _));session=session.AdvanceTurn();
            }
            return session;
        }
        private static TurnMove[] Path(LocalTurnLoop loop,int choice=0)=>loop.Session.Mode==GameMode.Long?
            LongNardeRules.LegalTurns(loop.LongPosition)[choice].Select(m=>new TurnMove(m.Source,m.Target,m.Slot)).ToArray():
            ShortNardeRules.LegalTurns(loop.ShortPosition)[choice].Select(m=>new TurnMove(m.Source,m.Target,m.Slot)).ToArray();
        private static void Play(LocalTurnLoop loop,TurnMove[] path)
        {
            int actor=loop.Session.Player;
            foreach(var move in path){Assert.AreEqual(TapOutcome.Selected,loop.Tap(actor,move.Source));Assert.Contains(move.Target,loop.Targets);Assert.AreEqual(TapOutcome.Moved,loop.Tap(actor,move.Target));}
        }
        public static IEnumerable<TestCaseData> Rolls()
        {
            foreach(GameMode mode in new[]{GameMode.Long,GameMode.Short})for(int actor=0;actor<2;actor++)for(int a=1;a<=6;a++)for(int b=1;b<=6;b++)
                yield return new TestCaseData(mode,actor,a,b).SetName($"TurnRoll_{mode}_P{actor}_{a}_{b}");
        }
        [TestCaseSource(nameof(Rolls))] public void EveryRollUsesUniqueTokensAndAutomaticallyChangesPlayer(GameMode mode,int actor,int a,int b)
        {
            var root=Ready(mode,actor);var dice=new Dice(a,b);var loop=new LocalTurnLoop(root,dice);
            Assert.IsTrue(loop.TryRoll(actor));CollectionAssert.AreEqual(new[]{a,b},loop.CopyDisplayedRoll());Assert.AreEqual(a==b?4:2,loop.Tokens.Count);Assert.AreEqual(2,dice.Calls);
            if(loop.Session.Phase==MatchPhase.Moving)
            {
                Assert.AreEqual(loop.Tokens.Count,loop.Tokens.Count(t=>t.State==TokenState.Available));var path=Path(loop);var slots=new HashSet<int>();
                foreach(var move in path)
                {
                    Assert.IsTrue(slots.Add(move.Slot));loop.Tap(actor,move.Source);Assert.AreEqual(TapOutcome.Moved,loop.Tap(actor,move.Target));
                    Assert.AreEqual(slots.Count,loop.Tokens.Count(t=>t.State==TokenState.Used));Assert.AreEqual(2,dice.Calls);
                    if(loop.Session.Phase==MatchPhase.Moving)Assert.IsFalse(loop.TryRoll(actor));
                }
                Assert.AreEqual(path.Length,loop.Tokens.Count(t=>t.State==TokenState.Used));
            }
            Assert.AreEqual(MatchPhase.Rolling,loop.Session.Phase);Assert.AreEqual(1-actor,loop.Session.Player);Assert.AreEqual(root.TurnNumber+1,loop.Session.TurnNumber);
            Assert.AreEqual(0,loop.Tokens.Count(t=>t.State==TokenState.Available));Assert.AreEqual(MatchPhase.TurnCommitted,loop.LastCommitted.Phase);
            Assert.IsFalse(loop.TryRoll(actor));Assert.AreEqual(2,dice.Calls);Assert.AreEqual(TapOutcome.Rejected,loop.Tap(actor,0));
            if(mode==GameMode.Long){Assert.IsTrue(root.LongPosition.IsFirstTurn(actor));Assert.IsFalse(loop.LastCommitted.LongPosition.IsFirstTurn(actor));}
            Assert.AreEqual(MatchPhase.Rolling,root.Phase);
        }
        [TestCase(GameMode.Long)] [TestCase(GameMode.Short)] public void OpeningTieAndFreshVersusReusedDiceFollowTheAcceptedRules(GameMode mode)
        {
            var dice=new Dice(4,4,2,5,6,6);var loop=new LocalTurnLoop(Create(mode),dice);
            Assert.IsFalse(loop.TryRoll(0));Assert.AreEqual(0,dice.Calls);Assert.IsTrue(loop.TryRoll(-1));Assert.AreEqual(-1,loop.Session.Player);Assert.IsTrue(loop.IsOpeningDisplay);Assert.IsEmpty(loop.Tokens);
            Assert.IsTrue(loop.TryRoll(-1));Assert.AreEqual(1,loop.Session.Player);
            if(mode==GameMode.Long)
            {Assert.IsTrue(loop.CanRoll);Assert.IsEmpty(loop.Tokens);Assert.IsTrue(loop.TryRoll(1));CollectionAssert.AreEqual(new[]{6,6},loop.CopyDisplayedRoll());Assert.AreEqual(4,loop.Tokens.Count);Assert.AreEqual(6,dice.Calls);}
            else{Assert.AreEqual(MatchPhase.Moving,loop.Session.Phase);CollectionAssert.AreEqual(new[]{2,5},loop.CopyDisplayedRoll());Assert.AreEqual(2,loop.Tokens.Count);Assert.AreEqual(4,dice.Calls);}
        }
        public static IEnumerable<TestCaseData> OpeningTies()
        {foreach(GameMode mode in new[]{GameMode.Long,GameMode.Short})for(int face=1;face<=6;face++)yield return new TestCaseData(mode,face).SetName($"OpeningTie_{mode}_{face}");}
        [TestCaseSource(nameof(OpeningTies))] public void EveryOpeningDoubleIsATieNotFourMoveRights(GameMode mode,int face)
        {
            var dice=new Dice(face,face);var loop=new LocalTurnLoop(Create(mode),dice);Assert.IsTrue(loop.TryRoll(-1));
            Assert.AreEqual(-1,loop.Session.Player);Assert.AreEqual(0,loop.Session.TurnNumber);Assert.IsTrue(loop.CanRoll);Assert.IsTrue(loop.IsOpeningDisplay);Assert.IsEmpty(loop.Tokens);Assert.AreEqual("OPENING_TIE",loop.Reason);
        }
        [TestCase(0)] [TestCase(7)] [TestCase(-1)] public void InvalidSourceValueDoesNotPublishHalfARoll(int bad)
        {
            var dice=new Dice(3,bad,6,1);var loop=new LocalTurnLoop(Create(GameMode.Long),dice);var before=loop.Session;
            Assert.IsFalse(loop.TryRoll(-1));Assert.AreSame(before,loop.Session);Assert.IsEmpty(loop.CopyDisplayedRoll());Assert.IsEmpty(loop.Tokens);
            Assert.IsTrue(loop.TryRoll(-1));Assert.AreEqual(0,loop.Session.Player);CollectionAssert.AreEqual(new[]{6,1},loop.CopyDisplayedRoll());
        }
        [TestCase(false)] [TestCase(true)] public void SourceExceptionIsReportedAndCannotMutateTheMatch(bool afterFirst)
        {
            var dice=afterFirst?new Dice(6):new Dice();var loop=new LocalTurnLoop(Create(GameMode.Long),dice);var before=loop.Session;
            Assert.IsFalse(loop.TryRoll(-1));Assert.AreEqual("DICE_SOURCE_FAILED",loop.Reason);Assert.AreSame(before,loop.Session);Assert.IsEmpty(loop.Tokens);
        }
        [Test] public void WrongActorPartialTurnAndReturnedCollectionsAreProtected()
        {
            var dice=new Dice(3,3);var loop=new LocalTurnLoop(Ready(GameMode.Long,0),dice);loop.TryRoll(0);var before=loop.LongPosition;
            Assert.AreEqual(TapOutcome.Rejected,loop.Tap(1,0));Assert.AreSame(before,loop.LongPosition);Assert.IsFalse(loop.TryRoll(0));Assert.AreEqual(2,dice.Calls);
            var copy=loop.CopyDisplayedRoll();copy[0]=99;Assert.AreEqual(3,loop.CopyDisplayedRoll()[0]);
            Assert.Throws<NotSupportedException>(()=>((IList<DiceToken>)loop.Tokens).Add(new DiceToken(0,1,TokenState.Available)));
            loop.Tap(0,0);loop.Tap(0,3);Assert.AreEqual(MatchPhase.Moving,loop.Session.Phase);Assert.IsNull(loop.LastCommitted);Assert.AreEqual(1,loop.Tokens.Count(t=>t.State==TokenState.Used));
        }
        [Test] public void ConstructorRejectsNullDependenciesAndLocalDiceStayWithinRange()
        {
            Assert.Throws<ArgumentNullException>(()=>new LocalTurnLoop(null,new Dice()));Assert.Throws<ArgumentNullException>(()=>new LocalTurnLoop(Create(GameMode.Long),null));
            var dice=new LocalDiceSource();for(int i=0;i<512;i++)Assert.That(dice.NextDie(),Is.InRange(1,6));
            // Range check only; this is not a statistical proof of fairness.
        }
        [TestCase(GameMode.Long)] [TestCase(GameMode.Short)] public void ReachableGamesCoverBlockedAndPartialTurnsThenStopAtTheWinner(GameMode mode)
        {
            int blocked=0,partial=0,totalTurns=0;
            for(int game=0;game<4;game++)
            {
                var dice=new SeedDice(2300+(int)mode*10+game);var choices=new Random(2323+game);
                var loop=new LocalTurnLoop(Create(mode),dice);int safety=0;
                while(loop.Session.Phase!=MatchPhase.Finished&&safety++<2000)
                {
                    if(loop.CanRoll)
                    {
                        var before=loop.Session;Assert.IsTrue(loop.TryRoll(before.Player));
                        if(loop.Reason=="NO_LEGAL_MOVES_TURN_PASSED")
                        {
                            blocked++;Assert.AreEqual(1-before.Player,loop.Session.Player);Assert.AreEqual(0,loop.Tokens.Count(t=>t.State==TokenState.Used));
                            if(mode==GameMode.Long)CollectionAssert.AreEqual(before.LongPosition.CopyPoints(),loop.LongPosition.CopyPoints());
                            else CollectionAssert.AreEqual(before.ShortPosition.CopyPoints(),loop.ShortPosition.CopyPoints());
                        }
                    }
                    if(loop.Session.Phase==MatchPhase.Moving)
                    {
                        int count=mode==GameMode.Long?LongNardeRules.LegalTurns(loop.LongPosition).Count:ShortNardeRules.LegalTurns(loop.ShortPosition).Count;
                        Play(loop,Path(loop,choices.Next(count)));
                    }
                    if(loop.LastCommitted!=null&&loop.Tokens.Any(t=>t.State==TokenState.Used)&&loop.Tokens.Any(t=>t.State==TokenState.Unavailable))partial++;
                }
                Assert.AreEqual(MatchPhase.Finished,loop.Session.Phase,"Safety limit reached");Assert.NotNull(loop.Session.Result);
                var last=loop.LastCommitted;int calls=dice.Calls;Assert.IsFalse(loop.TryRoll(loop.Session.Player));Assert.AreEqual(TapOutcome.Rejected,loop.Tap(loop.Session.Player,0));Assert.AreSame(last,loop.LastCommitted);Assert.AreEqual(calls,dice.Calls);
                var restored=new LocalTurnLoop(loop.Session,new Dice());Assert.IsFalse(restored.CanRoll);Assert.AreEqual(MatchPhase.Finished,restored.Session.Phase);
                totalTurns+=loop.Session.TurnNumber;
            }
            Assert.Greater(blocked,0,"No blocked-turn coverage");Assert.Greater(partial,0,"No partial-turn coverage");
            TestContext.WriteLine($"{mode}: games=4, turns={totalTurns}, blocked={blocked}, partial={partial}");
        }
        [Test] public void AlreadyCommittedSnapshotAdvancesOnceWithoutRollingAgain()
        {
            var root=Ready(GameMode.Long,0).SupplyRoll(0,RuleContract.VersionFor(GameMode.Long),3,3);
            var moves=LongNardeRules.LegalTurns(root.LongPosition)[0].Select(m=>new TurnMove(m.Source,m.Target,m.Slot)).ToArray();Assert.IsTrue(root.TryCommitTurn(0,root.RuleSetVersion,moves,out var committed,out _));
            var dice=new Dice();var loop=new LocalTurnLoop(committed,dice);Assert.AreEqual(1,loop.Session.Player);Assert.AreEqual(root.TurnNumber+1,loop.Session.TurnNumber);Assert.AreEqual(0,dice.Calls);Assert.AreEqual("TURN_COMPLETE",loop.Reason);
        }
    }
}
