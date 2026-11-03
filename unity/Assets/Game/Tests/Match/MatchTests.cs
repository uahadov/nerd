using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nard.Match;
using Nard.Rules.Long;
using Nard.Rules.Short;
using NUnit.Framework;
using UnityEngine;

namespace Nard.Tests.Match
{
    public sealed class MatchTests
    {
        [Serializable] public sealed class Corpus { public Fixture[] cases; }
        [Serializable] public sealed class Fixture { public string id; public ActionData action; public Expected expected; }
        [Serializable] public sealed class ActionData
        {
            public string kind,channel; public int series_length; public int[] winners; public string[] win_kinds; public bool ranked;
        }
        [Serializable] public sealed class Expected { public string status; public int winner; public int[] win_counts; public bool rating_eligible; }
        public static IEnumerable FixtureCases()
        {
            string path=Environment.GetEnvironmentVariable("NARD_RULE_FIXTURES")??Path.GetFullPath(Path.Combine(Application.dataPath,"../../docs/rules/fixtures.json"));
            foreach(var f in JsonUtility.FromJson<Corpus>(File.ReadAllText(path)).cases)
                if(f.action.kind=="series"||f.action.kind=="rating_policy")yield return new TestCaseData(f).SetName("MatchFixture_"+f.id);
        }
        private static GameResult Result(GameMode mode,WinKind kind,int winner,string id,int[] roll=null,int usedMask=0)
        {
            int loser=1-winner,sign=loser==0?1:-1;var board=new int[24];var off=new int[2];off[winner]=15;
            off[loser]=kind==WinKind.Normal?1:0;
            if(mode==GameMode.Long)
            {
                if(kind==WinKind.Koks)throw new ArgumentException("Test cannot manufacture long koks");
                board[loser*12]=sign*(15-off[loser]);
                var p=new LongPosition(board,off,roll??Array.Empty<int>(),winner,LongPhase.Finished,new bool[2],new int[2],usedMask);
                return GameResult.FromLong(id,RuleContract.VersionFor(mode),p);
            }
            var bar=new int[2];if(kind==WinKind.Koks)bar[loser]=1;board[12]=sign*(15-off[loser]-bar[loser]);
            return GameResult.FromShort(id,RuleContract.VersionFor(mode),new ShortPosition(board,bar,off,roll??Array.Empty<int>(),winner,ShortPhase.Finished,usedMask));
        }
        [TestCaseSource(nameof(FixtureCases))]
        public void CommonSpecificationFixture(Fixture f)
        {
            Assert.AreEqual("ACCEPT",f.expected.status);
            if(f.action.kind=="series")
            {
                var series=new MatchSeries(GameMode.Short,ShortNardeRules.RulesetVersion,f.action.series_length);
                for(int i=0;i<f.action.winners.Length;i++)
                    series=series.Record(Result(GameMode.Short,(WinKind)Enum.Parse(typeof(WinKind),f.action.win_kinds[i],true),f.action.winners[i],f.id+"-"+i));
                Assert.AreEqual(f.expected.winner,series.Winner);Assert.AreEqual(f.expected.win_counts[0],series.Wins(0));Assert.AreEqual(f.expected.win_counts[1],series.Wins(1));
                Assert.AreEqual(f.action.winners.Length,series.Games.Count);
                TestContext.WriteLine($"{f.id}: wins={series.Wins(0)}/{series.Wins(1)}, points={series.GamePoints(0)}/{series.GamePoints(1)}, winner={series.Winner}");
            }
            else
            {
                bool eligible=RatingPolicy.IsEligible((MatchChannel)Enum.Parse(typeof(MatchChannel),f.action.channel,true),f.action.ranked,true);
                Assert.AreEqual(f.expected.rating_eligible,eligible);TestContext.WriteLine($"{f.id}: rating_eligible={eligible}");
            }
        }
        private static MatchSession Moving(GameMode mode,int player=0)
        {
            var state=MatchSession.Create("test-"+mode,mode,RuleContract.VersionFor(mode)).Begin().ResolveOpening(player==0?6:2,player==0?2:6);
            return mode==GameMode.Long?state.SupplyRoll(player,state.RuleSetVersion,3,3):state;
        }
        private static TurnMove[] Proposal(MatchSession state,int choice=0)
        {
            return state.Mode==GameMode.Long
                ?LongNardeRules.LegalTurns(state.LongPosition)[choice].Select(m=>new TurnMove(m.Source,m.Target,m.Slot)).ToArray()
                :ShortNardeRules.LegalTurns(state.ShortPosition)[choice].Select(m=>new TurnMove(m.Source,m.Target,m.Slot)).ToArray();
        }
        [TestCase(GameMode.Long)] [TestCase(GameMode.Short)]
        public void CreatedOpeningTieAndModeSpecificFirstRoll(GameMode mode)
        {
            var created=MatchSession.Create("opening",mode,RuleContract.VersionFor(mode));Assert.AreEqual(MatchPhase.Created,created.Phase);Assert.AreEqual(-1,created.Player);Assert.IsNull(created.Result);
            var rolling=created.Begin();var tie=rolling.ResolveOpening(4,4);Assert.AreEqual(MatchPhase.Rolling,tie.Phase);Assert.AreEqual(-1,tie.Player);Assert.AreEqual(0,tie.TurnNumber);
            var winner=tie.ResolveOpening(2,6);Assert.AreEqual(1,winner.Player);Assert.AreEqual(1,winner.TurnNumber);
            Assert.AreEqual(mode==GameMode.Long?MatchPhase.Rolling:MatchPhase.Moving,winner.Phase);
            if(mode==GameMode.Long){Assert.AreEqual(0,winner.LongPosition.TokenCount);Assert.IsNull(winner.ShortPosition);}
            else{CollectionAssert.AreEqual(new[]{2,6},winner.ShortPosition.CopyRoll());Assert.IsNull(winner.LongPosition);}
            Assert.AreEqual(MatchPhase.Created,created.Phase);Assert.AreEqual(MatchPhase.Rolling,rolling.Phase);
        }
        [TestCase(GameMode.Long)] [TestCase(GameMode.Short)]
        public void CommitAdvanceAndNextRollResetTokensWithoutChangingOldSnapshot(GameMode mode)
        {
            var moving=Moving(mode);var before=mode==GameMode.Long?moving.LongPosition.CopyPoints():moving.ShortPosition.CopyPoints();
            Assert.IsTrue(moving.TryCommitTurn(0,moving.RuleSetVersion,Proposal(moving),out var committed,out string reason),reason);
            Assert.AreEqual(MatchPhase.TurnCommitted,committed.Phase);Assert.AreEqual(0,committed.Player);Assert.IsNull(committed.Result);
            Assert.AreEqual(MatchPhase.Moving,moving.Phase);CollectionAssert.AreEqual(before,mode==GameMode.Long?moving.LongPosition.CopyPoints():moving.ShortPosition.CopyPoints());
            var rolling=committed.AdvanceTurn();Assert.AreEqual(1,rolling.Player);Assert.AreEqual(2,rolling.TurnNumber);Assert.AreEqual(MatchPhase.Rolling,rolling.Phase);
            var next=rolling.SupplyRoll(1,rolling.RuleSetVersion,6,6);Assert.AreEqual(MatchPhase.Moving,next.Phase);
            if(mode==GameMode.Long)
            {
                Assert.AreEqual(0,next.LongPosition.UsedMask);Assert.AreEqual(0,next.LongPosition.HeadDepartures(0));Assert.AreEqual(0,next.LongPosition.HeadDepartures(1));
                Assert.IsFalse(next.LongPosition.IsFirstTurn(0));Assert.IsTrue(next.LongPosition.IsFirstTurn(1));Assert.IsTrue(moving.LongPosition.IsFirstTurn(0));
                Assert.AreEqual(2,LongNardeRules.LegalTurns(next.LongPosition)[0].Count);
            }
            else{Assert.AreEqual(0,next.ShortPosition.UsedMask);Assert.AreEqual(4,next.ShortPosition.TokenCount);}
        }
        [TestCase(GameMode.Long)] [TestCase(GameMode.Short)]
        public void WrongPhaseOperationsThrowWithoutAdvancing(GameMode mode)
        {
            string v=RuleContract.VersionFor(mode);var created=MatchSession.Create("phases",mode,v);
            Assert.Throws<InvalidOperationException>(()=>created.ResolveOpening(6,1));Assert.Throws<InvalidOperationException>(()=>created.SupplyRoll(0,v,1,2));Assert.Throws<InvalidOperationException>(()=>created.AdvanceTurn());
            Assert.IsFalse(created.TryCommitTurn(0,v,Array.Empty<TurnMove>(),out var unchanged,out _));Assert.AreSame(created,unchanged);
            var opening=created.Begin();Assert.Throws<InvalidOperationException>(()=>opening.Begin());Assert.Throws<InvalidOperationException>(()=>opening.SupplyRoll(0,v,1,2));Assert.Throws<ArgumentException>(()=>opening.ResolveOpening(0,6));
            var moving=Moving(mode);Assert.Throws<InvalidOperationException>(()=>moving.ResolveOpening(6,1));Assert.Throws<InvalidOperationException>(()=>moving.SupplyRoll(0,v,1,2));Assert.Throws<InvalidOperationException>(()=>moving.AdvanceTurn());
            Assert.IsTrue(moving.TryCommitTurn(0,v,Proposal(moving),out var committed,out _));Assert.Throws<InvalidOperationException>(()=>committed.Begin());Assert.Throws<InvalidOperationException>(()=>committed.ResolveOpening(6,1));Assert.Throws<InvalidOperationException>(()=>committed.SupplyRoll(0,v,1,2));
            Assert.IsFalse(committed.TryCommitTurn(0,v,Proposal(moving),out var same,out string reason));Assert.AreEqual("NOT_MOVING",reason);Assert.AreSame(committed,same);
        }
        [TestCase(GameMode.Long)] [TestCase(GameMode.Short)]
        public void WrongPlayerVersionNullAndOversizedProposalAreRejected(GameMode mode)
        {
            var s=Moving(mode);var moves=Proposal(s);
            Assert.IsFalse(s.TryCommitTurn(1,s.RuleSetVersion,moves,out var same,out var reason));Assert.AreEqual("WRONG_PLAYER",reason);Assert.AreSame(s,same);
            Assert.IsFalse(s.TryCommitTurn(0,"unknown-version",moves,out same,out reason));Assert.AreEqual("RULESET_MISMATCH",reason);Assert.AreSame(s,same);
            Assert.IsFalse(s.TryCommitTurn(0,s.RuleSetVersion,null,out same,out reason));Assert.AreEqual("INVALID_PROPOSAL",reason);Assert.AreSame(s,same);
            Assert.IsFalse(s.TryCommitTurn(0,s.RuleSetVersion,new TurnMove[5],out same,out reason));Assert.AreEqual("TOO_MANY_MOVES",reason);Assert.AreSame(s,same);
            Assert.IsFalse(s.TryCommitTurn(0,s.RuleSetVersion,Array.Empty<TurnMove>(),out same,out reason));Assert.AreEqual("MAX_DICE_REQUIRED",reason);Assert.AreSame(s,same);
        }
        [TestCase(GameMode.Long)] [TestCase(GameMode.Short)]
        public void EngineRejectionIsAtomicAndDoesNotClearFirstTurn(GameMode mode)
        {
            var s=Moving(mode);var moves=Proposal(s);Assert.Greater(moves.Length,1);
            moves[moves.Length-1]=new TurnMove(moves[moves.Length-1].Source,moves[moves.Length-1].Target,moves[0].Slot);
            Assert.IsFalse(s.TryCommitTurn(0,s.RuleSetVersion,moves,out var same,out string reason));Assert.AreEqual("DIE_ALREADY_USED",reason);Assert.AreSame(s,same);
            Assert.AreEqual(MatchPhase.Moving,s.Phase);Assert.IsNull(s.Result);
            if(mode==GameMode.Long){Assert.IsTrue(s.LongPosition.IsFirstTurn(0));Assert.AreEqual(0,s.LongPosition.UsedMask);}else Assert.AreEqual(0,s.ShortPosition.UsedMask);
        }
        [TestCase(GameMode.Long)] [TestCase(GameMode.Short)]
        public void ExplicitRollValidatesVersionPlayerAndDice(GameMode mode)
        {
            var moving=Moving(mode);Assert.IsTrue(moving.TryCommitTurn(0,moving.RuleSetVersion,Proposal(moving),out var committed,out _));var rolling=committed.AdvanceTurn();
            Assert.Throws<ArgumentException>(()=>rolling.SupplyRoll(0,rolling.RuleSetVersion,1,2));
            Assert.Throws<ArgumentException>(()=>rolling.SupplyRoll(1,"wrong",1,2));Assert.Throws<ArgumentException>(()=>rolling.SupplyRoll(1,rolling.RuleSetVersion,7,1));
            Assert.AreEqual(MatchPhase.Rolling,rolling.Phase);Assert.AreEqual(1,rolling.Player);
        }
        [TestCase(GameMode.Long)] [TestCase(GameMode.Short)]
        public void IdenticalInputsAreRepeatableAndRequestArraysAreNotRetained(GameMode mode)
        {
            var state=Moving(mode);var request=Proposal(state);
            Assert.IsTrue(state.TryCommitTurn(0,state.RuleSetVersion,request,out var a,out _));Assert.IsTrue(state.TryCommitTurn(0,state.RuleSetVersion,request,out var b,out _));
            request[0]=new TurnMove(-100,-100,-100);
            CollectionAssert.AreEqual(mode==GameMode.Long?a.LongPosition.CopyPoints():a.ShortPosition.CopyPoints(),mode==GameMode.Long?b.LongPosition.CopyPoints():b.ShortPosition.CopyPoints());
            Assert.AreEqual(a.Phase,b.Phase);Assert.AreEqual(a.TurnNumber,b.TurnNumber);Assert.AreEqual(MatchPhase.Moving,state.Phase);
        }
        [TestCase(GameMode.Long,0,WinKind.Normal,1)] [TestCase(GameMode.Long,1,WinKind.Normal,1)]
        [TestCase(GameMode.Long,0,WinKind.Mars,2)] [TestCase(GameMode.Long,1,WinKind.Mars,2)]
        [TestCase(GameMode.Short,0,WinKind.Normal,1)] [TestCase(GameMode.Short,1,WinKind.Normal,1)]
        [TestCase(GameMode.Short,0,WinKind.Mars,2)] [TestCase(GameMode.Short,1,WinKind.Mars,2)]
        [TestCase(GameMode.Short,0,WinKind.Koks,3)] [TestCase(GameMode.Short,1,WinKind.Koks,3)]
        public void ResultHasSeparateTypePointsModeAndVersion(GameMode mode,int winner,WinKind kind,int points)
        {
            var r=Result(mode,kind,winner,"result");Assert.AreEqual(mode,r.Mode);Assert.AreEqual(kind,r.Kind);Assert.AreEqual(points,r.Points);Assert.AreEqual(winner,r.Winner);Assert.AreEqual(RuleContract.VersionFor(mode),r.RuleSetVersion);
        }
        [TestCase(GameMode.Long)] [TestCase(GameMode.Short)]
        public void UnfinishedResultsAndVersionMismatchCannotBeRecorded(GameMode mode)
        {
            if(mode==GameMode.Long)
            {
                Assert.Throws<InvalidOperationException>(()=>GameResult.FromLong("x",LongNardeRules.RulesetVersion,LongNardeRules.Initial()));
                Assert.Throws<ArgumentNullException>(()=>GameResult.FromLong("x",LongNardeRules.RulesetVersion,null));
                Assert.Throws<ArgumentException>(()=>GameResult.FromLong("x",ShortNardeRules.RulesetVersion,LongNardeRules.Initial()));
            }
            else
            {
                Assert.Throws<InvalidOperationException>(()=>GameResult.FromShort("x",ShortNardeRules.RulesetVersion,ShortNardeRules.Initial()));
                Assert.Throws<ArgumentNullException>(()=>GameResult.FromShort("x",ShortNardeRules.RulesetVersion,null));
                Assert.Throws<ArgumentException>(()=>GameResult.FromShort("x",LongNardeRules.RulesetVersion,ShortNardeRules.Initial()));
            }
            Assert.Throws<ArgumentException>(()=>Result(mode,WinKind.Normal,0," "));
        }
        [TestCase(GameMode.Long,1)] [TestCase(GameMode.Long,3)] [TestCase(GameMode.Long,5)]
        [TestCase(GameMode.Short,1)] [TestCase(GameMode.Short,3)] [TestCase(GameMode.Short,5)]
        public void EveryWinnerSequenceClinchesByGamesNotPoints(GameMode mode,int bestOf)
        {
            for(int mask=0;mask<(1<<bestOf);mask++)
            {
                var series=new MatchSeries(mode,RuleContract.VersionFor(mode),bestOf);int[] wins={0,0},points={0,0};int expectedWinner=-1;
                for(int i=0;i<bestOf&&expectedWinner==-1;i++)
                {
                    int winner=(mask>>i)&1;WinKind kind=mode==GameMode.Short&&i%3==0?WinKind.Koks:i%2==0?WinKind.Mars:WinKind.Normal;
                    var before=series;series=series.Record(Result(mode,kind,winner,"game-"+i));wins[winner]++;points[winner]+=kind==WinKind.Koks?3:kind==WinKind.Mars?2:1;
                    if(wins[winner]==bestOf/2+1)expectedWinner=winner;
                    Assert.AreEqual(expectedWinner,series.Winner);for(int p=0;p<2;p++){Assert.AreEqual(wins[p],series.Wins(p));Assert.AreEqual(points[p],series.GamePoints(p));}
                    Assert.AreEqual(i,before.Games.Count);Assert.AreEqual(i+1,series.Games.Count);
                }
                Assert.AreNotEqual(-1,series.Winner);
            }
        }
        [TestCase(GameMode.Long)] [TestCase(GameMode.Short)]
        public void MarsOrKoksDoesNotFinishBestOfThreeAfterOneGame(GameMode mode)
        {
            var kind=mode==GameMode.Long?WinKind.Mars:WinKind.Koks;
            var series=new MatchSeries(mode,RuleContract.VersionFor(mode),3).Record(Result(mode,kind,0,"one"));
            Assert.AreEqual(-1,series.Winner);Assert.AreEqual(1,series.Wins(0));Assert.AreEqual(mode==GameMode.Long?2:3,series.GamePoints(0));
            series=series.Record(Result(mode,kind,0,"two"));Assert.AreEqual(0,series.Winner);Assert.AreEqual(2,series.Wins(0));
            Assert.Throws<InvalidOperationException>(()=>series.Record(Result(mode,WinKind.Normal,1,"three")));
        }
        [TestCase(GameMode.Long)] [TestCase(GameMode.Short)]
        public void DuplicateResultWrongModeAndNullAreRejected(GameMode mode)
        {
            var series=new MatchSeries(mode,RuleContract.VersionFor(mode),5).Record(Result(mode,WinKind.Normal,0,"same-id"));
            Assert.Throws<ArgumentException>(()=>series.Record(Result(mode,WinKind.Mars,1,"same-id")));
            var other=mode==GameMode.Long?GameMode.Short:GameMode.Long;
            Assert.Throws<ArgumentException>(()=>series.Record(Result(other,WinKind.Normal,1,"other")));Assert.Throws<ArgumentNullException>(()=>series.Record(null));
            Assert.AreEqual(1,series.Games.Count);Assert.AreEqual(1,series.Wins(0));Assert.AreEqual(0,series.Wins(1));
            Assert.Throws<NotSupportedException>(()=>((IList<GameResult>)series.Games).Add(Result(mode,WinKind.Normal,1,"mutate")));
        }
        [TestCase(0)] [TestCase(2)] [TestCase(4)] [TestCase(7)] [TestCase(-1)]
        public void UnsupportedSeriesLengthsAreRejected(int bestOf)
            =>Assert.Throws<ArgumentException>(()=>new MatchSeries(GameMode.Long,LongNardeRules.RulesetVersion,bestOf));
        [Test]
        public void InvalidModeVersionIdsAndPlayerQueriesAreRejected()
        {
            Assert.Throws<ArgumentException>(()=>RuleContract.VersionFor((GameMode)99));
            Assert.Throws<ArgumentException>(()=>MatchSession.Create("x",GameMode.Long,ShortNardeRules.RulesetVersion));
            Assert.Throws<ArgumentException>(()=>MatchSession.Create("",GameMode.Short,ShortNardeRules.RulesetVersion));
            Assert.Throws<ArgumentException>(()=>new MatchSeries(GameMode.Short,"unknown",3));
            var series=new MatchSeries(GameMode.Short,ShortNardeRules.RulesetVersion,1);
            Assert.Throws<ArgumentOutOfRangeException>(()=>series.Wins(-1));Assert.Throws<ArgumentOutOfRangeException>(()=>series.GamePoints(2));
        }
        [TestCase(MatchChannel.Bot)] [TestCase(MatchChannel.Private)] [TestCase(MatchChannel.Random)]
        public void RatingEligibilityRequiresRankedRandomAndTrustedConfirmation(MatchChannel channel)
        {
            foreach(bool ranked in new[]{false,true})foreach(bool confirmed in new[]{false,true})
                Assert.AreEqual(channel==MatchChannel.Random&&ranked&&confirmed,RatingPolicy.IsEligible(channel,ranked,confirmed));
        }
        [Test]
        public void InvalidRatingChannelIsRejected()=>Assert.Throws<ArgumentException>(()=>RatingPolicy.IsEligible((MatchChannel)99,true,true));

        [TestCase(GameMode.Long,WinKind.Normal,1)] [TestCase(GameMode.Long,WinKind.Mars,2)]
        [TestCase(GameMode.Short,WinKind.Normal,1)] [TestCase(GameMode.Short,WinKind.Mars,2)] [TestCase(GameMode.Short,WinKind.Koks,3)]
        public void DoubleSixVisualValueDoesNotMultiplyResultPoints(GameMode mode,WinKind kind,int expected)
        {
            var result=Result(mode,kind,0,"double-six",new[]{6,6},15);
            Assert.AreEqual(expected,result.Points);Assert.AreEqual(kind,result.Kind);
            var series=new MatchSeries(mode,result.RuleSetVersion,3).Record(result);
            Assert.AreEqual(1,series.Wins(0));Assert.AreEqual(-1,series.Winner);
        }
        [TestCase(GameMode.Long)] [TestCase(GameMode.Short)]
        public void OpeningWinnerOneCommitsBeforeZeroGetsTheNextTurn(GameMode mode)
        {
            var state=Moving(mode,1);Assert.AreEqual(1,state.Player);
            Assert.IsTrue(state.TryCommitTurn(1,state.RuleSetVersion,Proposal(state),out var committed,out _));
            Assert.AreEqual(1,committed.Player);var next=committed.AdvanceTurn();Assert.AreEqual(0,next.Player);
            next=next.SupplyRoll(0,next.RuleSetVersion,3,3);
            if(mode==GameMode.Long){Assert.IsFalse(next.LongPosition.IsFirstTurn(1));Assert.IsTrue(next.LongPosition.IsFirstTurn(0));}
            Assert.AreEqual(MatchPhase.Moving,next.Phase);
        }
        [TestCase(GameMode.Long,1401)] [TestCase(GameMode.Short,1402)]
        public void CompleteGameExercisesAllPhasesAndPublishesOneEngineDerivedResult(GameMode mode,int seed)
        {
            var state=MatchSession.Create("complete-"+mode,mode,RuleContract.VersionFor(mode));var phases=new HashSet<MatchPhase>{state.Phase};
            state=state.Begin();phases.Add(state.Phase);state=state.ResolveOpening(6,2);phases.Add(state.Phase);
            var random=new System.Random(seed);int turns=0;
            for(int i=0;i<1000;i++)
            {
                if(state.Phase==MatchPhase.Rolling){state=state.SupplyRoll(state.Player,state.RuleSetVersion,random.Next(1,7),random.Next(1,7));phases.Add(state.Phase);}
                Assert.AreEqual(MatchPhase.Moving,state.Phase);Assert.IsNull(state.Result);
                int count=mode==GameMode.Long?LongNardeRules.LegalTurns(state.LongPosition).Count:ShortNardeRules.LegalTurns(state.ShortPosition).Count;
                var old=state;Assert.IsTrue(state.TryCommitTurn(state.Player,state.RuleSetVersion,Proposal(state,random.Next(count)),out state,out string reason),reason);
                Assert.AreEqual(MatchPhase.Moving,old.Phase);Assert.IsNull(old.Result);turns++;phases.Add(state.Phase);
                if(state.Phase==MatchPhase.Finished)break;
                Assert.AreEqual(MatchPhase.TurnCommitted,state.Phase);state=state.AdvanceTurn();phases.Add(state.Phase);
            }
            Assert.AreEqual(MatchPhase.Finished,state.Phase);CollectionAssert.AreEquivalent(Enum.GetValues(typeof(MatchPhase)),phases);
            Assert.AreEqual("complete-"+mode,state.Result.GameId);Assert.AreEqual(mode,state.Result.Mode);Assert.AreEqual(state.Player,state.Result.Winner);
            int points=mode==GameMode.Long?LongNardeRules.ResultPoints(state.LongPosition,out _):ShortNardeRules.ResultPoints(state.ShortPosition,out _);
            Assert.AreEqual(points,state.Result.Points);
            var single=new MatchSeries(mode,state.RuleSetVersion,1).Record(state.Result);Assert.AreEqual(state.Result.Winner,single.Winner);Assert.AreEqual(1,single.Games.Count);
            Assert.Throws<InvalidOperationException>(()=>state.Begin());Assert.Throws<InvalidOperationException>(()=>state.AdvanceTurn());Assert.Throws<InvalidOperationException>(()=>state.ResolveOpening(6,1));Assert.Throws<InvalidOperationException>(()=>state.SupplyRoll(state.Player,state.RuleSetVersion,1,2));
            Assert.IsFalse(state.TryCommitTurn(state.Player,state.RuleSetVersion,Array.Empty<TurnMove>(),out var unchanged,out var finalReason));Assert.AreEqual("NOT_MOVING",finalReason);Assert.AreSame(state,unchanged);
            TestContext.WriteLine($"Match lifecycle: mode={mode}, seed={seed}, turns={turns}, winner={state.Result.Winner}, kind={state.Result.Kind}, points={points}");
        }
    }
}
