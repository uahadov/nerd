using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Nard.Rules.Long;
using NUnit.Framework;
using UnityEngine;

namespace Nard.Tests.Rules
{
    public sealed class LongRulesTests
    {
        [Serializable] public sealed class Corpus { public Fixture[] cases; }
        [Serializable] public sealed class Snapshot
        {
            public int[] points, bar, off, roll, used, head_departures;
            public int to_move;
            public string phase;
            public bool[] first_turn;
            public bool committed;
            public LongPosition Position() => new LongPosition(points, off, roll, to_move,
                (LongPhase)Enum.Parse(typeof(LongPhase), phase.Replace("await_roll", "AwaitRoll"), true),
                first_turn, head_departures, used.Aggregate(0, (mask, slot) => mask | (1 << slot)));
        }
        [Serializable] public sealed class MoveData
        {
            public int source, target, slot;
            public LongMove Move() => new LongMove(source, target, slot);
        }
        [Serializable] public sealed class ActionData { public string kind; public MoveData[] moves; public int[] opening_dice; }
        [Serializable] public sealed class Expectation
        {
            public string status, reason, win_kind;
            public Snapshot after;
            public int max_used, starter, winner, points;
        }
        [Serializable] public sealed class Fixture
        {
            public string id, mode, ruleset_version;
            public Snapshot before;
            public ActionData action;
            public Expectation expected;
        }
        public static IEnumerable FixtureCases()
        {
            string path = Environment.GetEnvironmentVariable("NARD_RULE_FIXTURES") ??
                Path.GetFullPath(Path.Combine(Application.dataPath, "../../docs/rules/fixtures.json"));
            string json = File.ReadAllText(path);
            json = Regex.Replace(json, "\"target\"\\s*:\\s*\"off\"", "\"target\":24");
            json = Regex.Replace(json, "\"source\"\\s*:\\s*\"bar\"", "\"source\":-1");
            foreach (var f in JsonUtility.FromJson<Corpus>(json).cases)
            {
                var test = new TestCaseData(f).SetName("Fixture_" + f.id);
                if (f.mode != "long") test.Ignore("P01.3/P01.4: short/series/rating engine not authorized");
                else if (f.action.kind == "undo") test.Ignore("P02.4: undo application is not a P01.2 engine action");
                yield return test;
            }
        }
        private static void Same(LongPosition a, LongPosition b)
        {
            CollectionAssert.AreEqual(a.CopyPoints(), b.CopyPoints());
            CollectionAssert.AreEqual(a.CopyOff(), b.CopyOff());
            CollectionAssert.AreEqual(a.CopyRoll(), b.CopyRoll());
            CollectionAssert.AreEqual(a.CopyHeadDepartures(), b.CopyHeadDepartures());
            CollectionAssert.AreEqual(a.CopyFirstTurns(), b.CopyFirstTurns());
            Assert.AreEqual(a.Player, b.Player); Assert.AreEqual(a.Phase, b.Phase); Assert.AreEqual(a.UsedMask, b.UsedMask);
        }
        [TestCaseSource(nameof(FixtureCases))]
        public void CanonicalFixture(Fixture f)
        {
            Assert.AreEqual(LongNardeRules.RulesetVersion, f.ruleset_version);
            CollectionAssert.AreEqual(new int[2], f.before.bar);
            var before = f.before.Position(); var after = before;
            bool accepted = true; string reason = "OK";
            switch (f.action.kind)
            {
                case "start": after = LongNardeRules.Initial(); break;
                case "opening":
                    after = LongNardeRules.ResolveOpening(before, f.action.opening_dice[0], f.action.opening_dice[1]);
                    Assert.AreEqual(f.expected.starter, after.Player); break;
                case "move": accepted = LongNardeRules.TryApplyLocalStep(before, f.action.moves[0].Move(), out after, out reason); break;
                case "turn":
                    accepted = LongNardeRules.TryApplyTurn(before, f.action.moves.Select(m => m.Move()).ToArray(), out after, out reason);
                    Assert.AreEqual(f.expected.max_used, LongNardeRules.LegalTurns(before)[0].Count); break;
                case "classify_result":
                    Assert.AreEqual(f.expected.points, LongNardeRules.ResultPoints(before, out int winner));
                    Assert.AreEqual(f.expected.winner, winner); break;
                default: Assert.Fail("Unsupported authorized fixture action: " + f.action.kind); break;
            }
            Assert.AreEqual(f.expected.status == "ACCEPT", accepted, reason);
            Assert.AreEqual(f.expected.reason, reason);
            Same(f.expected.after.Position(), after);
            Same(f.before.Position(), before);
        }

        private static LongPosition Moving(int player, int d0, int d1, bool first = true)
        {
            var initial = LongNardeRules.Initial();
            return new LongPosition(initial.CopyPoints(), new int[2], new[] { d0, d1 }, player,
                LongPhase.Moving, new[] { first, first }, new int[2]);
        }
        private static LongPosition Position(int[] board, int player = 0, int d0 = 2, int d1 = 1,
            int off0 = 0, int off1 = 0, bool first = false, int mask = 0, int head = 0)
        {
            var heads = new int[2]; heads[player] = head;
            return new LongPosition(board, new[] { off0, off1 }, new[] { d0, d1 }, player,
                LongPhase.Moving, new[] { first, first }, heads, mask);
        }
        private static int[] Board(params int[] pairs)
        {
            var board = new int[24];
            for (int i = 0; i < pairs.Length; i += 2) board[pairs[i]] = pairs[i+1];
            return board;
        }
        private static string Choices(LongPosition p) => string.Join("|", LongNardeRules.LegalTurns(p)
            .Select(path => string.Join(",", path.Select(m => m.ToString()))));

        [TestCase(0, 3, 4)] [TestCase(0, 4, 4)] [TestCase(0, 6, 2)]
        [TestCase(1, 3, 4)] [TestCase(1, 4, 4)] [TestCase(1, 6, 2)]
        public void BothPlayersFirstSpecialDoubles(int player, int die, int max)
        {
            var p = Moving(player, die, die);
            var choices = LongNardeRules.LegalTurns(p);
            Assert.IsNotEmpty(choices);
            foreach (var path in choices)
            {
                Assert.AreEqual(max, path.Count);
                Assert.IsTrue(LongNardeRules.TryApplyTurn(p, path, out var after, out _));
                Assert.AreEqual(2, after.HeadDepartures(player));
            }
        }
        [TestCase(0,3,4)] [TestCase(0,4,4)] [TestCase(0,6,2)]
        [TestCase(1,3,4)] [TestCase(1,4,4)] [TestCase(1,6,2)]
        public void OwnFirstDoubleWorksAfterOpponentHasAlreadyPlayed(int actor,int die,int max)
        {
            int sign=actor==0?1:-1;int head=actor*12,enemyHead=(1-actor)*12;
            var board=Board(head,15*sign,enemyHead,-14*sign,(enemyHead+3)%24,-sign);
            var first=new[]{false,false};first[actor]=true;
            var p=new LongPosition(board,new int[2],new[]{die,die},actor,LongPhase.Moving,first,new int[2]);
            foreach(var path in LongNardeRules.LegalTurns(p))
            {
                Assert.AreEqual(max,path.Count);
                Assert.IsTrue(LongNardeRules.TryApplyTurn(p,path,out var after,out _));
                Assert.AreEqual(2,after.HeadDepartures(actor));
                Assert.IsFalse(after.IsFirstTurn(1-actor));
            }
        }
        [TestCase(0, 1)] [TestCase(0, 2)] [TestCase(0, 5)]
        [TestCase(1, 1)] [TestCase(1, 2)] [TestCase(1, 5)]
        public void OtherFirstDoublesUseOneHead(int player, int die)
        {
            var p = Moving(player, die, die);
            foreach (var path in LongNardeRules.LegalTurns(p))
            {
                Assert.AreEqual(4, path.Count);
                Assert.IsTrue(LongNardeRules.TryApplyTurn(p, path, out var after, out _));
                Assert.AreEqual(1, after.HeadDepartures(player));
            }
        }
        [TestCase(0)] [TestCase(1)]
        public void FirstThreeThreeIncludesUnevenDistribution(int player)
        {
            int h = player * 12;
            var expected = new[] { new LongMove(h,(h+3)%24,0), new LongMove((h+3)%24,(h+6)%24,1),
                new LongMove((h+6)%24,(h+9)%24,2), new LongMove(h,(h+3)%24,3) };
            var p = Moving(player,3,3);
            Assert.IsTrue(LongNardeRules.TryApplyTurn(p, expected, out var result, out _));
            Assert.AreEqual(13, Math.Abs(result.Point(h)));
            Assert.AreEqual(1, Math.Abs(result.Point((h+9)%24)));
            Assert.IsTrue(LongNardeRules.LegalTurns(p).Any(path => path.Select(m => m.ToString()).SequenceEqual(expected.Select(m => m.ToString()))));
        }
        [TestCase(0,3)] [TestCase(0,4)] [TestCase(0,6)]
        [TestCase(1,3)] [TestCase(1,4)] [TestCase(1,6)]
        public void LaterSpecialDoublesDoNotGetSecondHead(int player, int die)
        {
            var p = Moving(player,die,die,false); int h = player*12;
            Assert.IsFalse(LongNardeRules.TryApplyTurn(p,new[] {new LongMove(h,(h+die)%24,0),new LongMove(h,(h+die)%24,1)},out var after,out string reason));
            Assert.AreEqual("HEAD_LIMIT",reason); Assert.AreSame(p,after);
        }
        [TestCase(0)] [TestCase(1)]
        public void OpeningWinnerNeedsFreshRoll(int winner)
        {
            var p = LongNardeRules.ResolveOpening(LongNardeRules.Initial(), winner == 0 ? 6 : 1,winner == 0 ? 1 : 6);
            Assert.AreEqual(winner,p.Player); Assert.AreEqual(0,p.TokenCount);
            Assert.IsEmpty(LongNardeRules.LegalTurns(p));
            var moving = LongNardeRules.SupplyOpeningRoll(p,3,3);
            Assert.AreEqual(4,LongNardeRules.LegalTurns(moving)[0].Count);
        }
        [Test]
        public void LowerDieMustComeFirstAndPrematurePassRejected()
        {
            var p = Position(Board(0,15,2,-1,12,-14));
            var turns = LongNardeRules.LegalTurns(p);
            Assert.AreEqual(1,turns.Count); Assert.AreEqual(1,turns[0][0].Slot);
            Assert.AreEqual(2,turns[0].Count);
            Assert.IsFalse(LongNardeRules.TryApplyTurn(p,Array.Empty<LongMove>(),out var after,out var reason));
            Assert.AreEqual("MAX_DICE_REQUIRED",reason); Assert.AreSame(p,after);
        }
        [Test]
        public void MiddleBoardHigherDieWinsWhenBothAreAlternatives()
        {
            var p = Position(Board(5,1,8,-1,12,-14),off0:14);
            var choices = LongNardeRules.LegalTurns(p);
            Assert.IsTrue(choices.All(t => t.Count == 1 && t[0].Slot == 0));
            Assert.IsFalse(LongNardeRules.TryApplyTurn(p,new[]{new LongMove(5,6,1)},out var after,out string reason));
            Assert.AreEqual("HIGHER_DIE_REQUIRED",reason); Assert.AreSame(p,after);
        }
        [Test]
        public void LowerOnlyIsPlayedWhenHigherBlocked()
        {
            var p = Position(Board(5,1,7,-1,8,-1,12,-13),off0:14);
            var choices = LongNardeRules.LegalTurns(p);
            Assert.IsTrue(choices.All(t => t.Count == 1 && t[0].Slot == 1));
        }
        [Test]
        public void AllBlockedAllowsEmptyTurn()
        {
            var p = Position(Board(0,15,1,-1,2,-1,12,-13));
            Assert.AreEqual(1,LongNardeRules.LegalTurns(p).Count);
            Assert.IsTrue(LongNardeRules.TryApplyTurn(p,Array.Empty<LongMove>(),out var after,out _));
            Assert.AreSame(p,after);
        }
        [TestCase(0,false)] [TestCase(0,true)] [TestCase(1,false)] [TestCase(1,true)]
        public void SixPrimeAcrossGlobalOrPlayerRotatedCorner(int actor,bool ahead)
        {
            // Rotate the canonical global-corner fixture 12 points and invert ownership for P1.
            var b = Board(0,7,1,2,2,2,22,2,23,2,12,-15);
            if (ahead) { b[12] = -14; b[4] = -1; }
            if (actor == 1) { var rotated = new int[24]; for(int i=0;i<24;i++)rotated[(i+12)%24]=-b[i]; b=rotated; }
            var p = Position(b,actor,3,1);
            bool ok = LongNardeRules.TryApplyLocalStep(p,new LongMove(actor*12,(actor*12+3)%24,0),out var after,out var reason);
            Assert.AreEqual(ahead,ok);
            Assert.AreEqual(ahead?"OK":"SIX_BLOCK_TRAPS_ALL",reason);
            if (!ok) Assert.AreSame(p,after);
        }
        [TestCase(0)] [TestCase(1)]
        public void BearingOffAndGameEndAreSymmetric(int actor)
        {
            var b = new int[24]; int index = (actor*12+23)%24; b[index]=actor==0?1:-1;b[(1-actor)*12]=actor==0?-15:15;
            var p=Position(b,actor,1,2,actor==0?14:0,actor==1?14:0);
            Assert.IsTrue(LongNardeRules.TryApplyTurn(p,new[]{new LongMove(index,LongMove.Off,1)},out var after,out _));
            Assert.AreEqual(LongPhase.Finished,after.Phase); Assert.IsEmpty(LongNardeRules.LegalTurns(after));
            Assert.AreEqual(2,LongNardeRules.ResultPoints(after,out int winner)); Assert.AreEqual(actor,winner);
            Assert.IsFalse(LongNardeRules.TryApplyTurn(after,Array.Empty<LongMove>(),out _,out _));
        }
        [Test]
        public void EnteringHomeDuringTurnAllowsBearingOff()
        {
            var p=Position(Board(17,1,12,-15),0,1,6,14);
            var path=new[]{new LongMove(17,18,0),new LongMove(18,LongMove.Off,1)};
            Assert.IsTrue(LongNardeRules.TryApplyTurn(p,path,out var after,out _)); Assert.AreEqual(15,after.Off(0));
        }
        [Test]
        public void FailedLateHopIsAtomicAndDoesNotConsumeDice()
        {
            var p=Moving(0,2,1,false);
            Assert.IsFalse(LongNardeRules.TryApplyTurn(p,new[]{new LongMove(0,2,0),new LongMove(2,3,0)},out var after,out string reason));
            Assert.AreEqual("DIE_ALREADY_USED",reason); Assert.AreSame(p,after); Assert.AreEqual(0,p.UsedMask);
        }
        [Test]
        public void ConstructorAndExportArraysCannotMutateState()
        {
            var board=Board(0,15,12,-15);var off=new int[2];var dice=new[]{1,2};var first=new[]{true,true};var head=new int[2];
            var p=new LongPosition(board,off,dice,0,LongPhase.Moving,first,head);
            board[0]=0;off[0]=15;dice[0]=6;first[0]=false;head[0]=2;
            p.CopyPoints()[0]=0;p.CopyOff()[0]=15;p.CopyRoll()[0]=6;p.CopyFirstTurns()[0]=false;p.CopyHeadDepartures()[0]=2;
            Assert.AreEqual(15,p.Point(0));Assert.AreEqual(0,p.Off(0));Assert.AreEqual(1,p.Die(0));Assert.IsTrue(p.IsFirstTurn(0));Assert.AreEqual(0,p.HeadDepartures(0));
        }
        [Test]
        public void InvalidInputAndUnsupportedShortRulesetAreRejected()
        {
            Assert.Throws<ArgumentException>(()=>Position(Board(0,14,12,-15)));
            Assert.Throws<ArgumentException>(()=>Position(Board(0,15,12,-15),d0:0));
            Assert.Throws<ArgumentException>(()=>Position(Board(0,15,12,-15),mask:16));
            Assert.Throws<ArgumentException>(()=>new LongPosition(Board(0,15,12,-15),new int[2],new[]{1,2},0,LongPhase.Moving,new bool[2],new int[2],0,"short-gdd1-draft.1"));
            Assert.IsFalse(LongNardeRules.TryApplyTurn(null,Array.Empty<LongMove>(),out _,out _));
            Assert.IsFalse(LongNardeRules.TryApplyTurn(Moving(0,1,2),null,out _,out _));
        }
        [TestCase(-1,2,0)] [TestCase(24,2,0)] [TestCase(0,25,0)] [TestCase(0,2,-1)] [TestCase(0,2,4)]
        [TestCase(12,14,0)] [TestCase(0,1,0)]
        public void InvalidLocalMovesAreRejectedWithoutMutation(int source,int target,int slot)
        {
            var p=Moving(0,2,1);Assert.IsFalse(LongNardeRules.TryApplyLocalStep(p,new LongMove(source,target,slot),out var after,out _));Assert.AreSame(p,after);
        }
        [Test]
        public void EqualDieTokenPermutationRemainsValid()
        {
            var p=Moving(0,1,1);
            var moves=new[]{new LongMove(0,1,3),new LongMove(1,2,1),new LongMove(2,3,2),new LongMove(3,4,0)};
            Assert.IsTrue(LongNardeRules.TryApplyTurn(p,moves,out var after,out _));Assert.AreEqual(15,after.UsedMask);
        }
        [Test]
        public void ExhaustiveInitialRollMatrixIsDeterministicAndSymmetric()
        {
            for(int d0=1;d0<=6;d0++)for(int d1=1;d1<=6;d1++)for(int actor=0;actor<2;actor++)
            {
                var p=Moving(actor,d0,d1);string fingerprint=Choices(p);
                Assert.AreEqual(fingerprint,Choices(p));
                var turns=LongNardeRules.LegalTurns(p);Assert.IsNotEmpty(turns);
                var other=LongNardeRules.LegalTurns(Moving(1-actor,d0,d1));Assert.AreEqual(turns.Count,other.Count);
                foreach(var path in turns)
                {
                    Assert.IsTrue(LongNardeRules.TryApplyTurn(p,path,out var after,out _));
                    for(int pl=0;pl<2;pl++)Assert.AreEqual(15,after.CopyPoints().Where(n=>n*(pl==0?1:-1)>0).Sum(n=>Math.Abs(n))+after.Off(pl));
                }
            }
        }
        [Test]
        public void MovingCheckerOutOfOldBlockDoesNotInventSixPrime()
        {
            var p=Position(Board(5,6,18,1,19,2,20,2,21,2,22,2,12,-15),d0:5);
            Assert.IsTrue(LongNardeRules.TryApplyLocalStep(p,new LongMove(18,23,0),out _,out _));
        }
        [Test]
        public void AlreadyBorneOffOpponentCheckerHasPassedTheBlock()
        {
            var p=Position(Board(0,4,17,1,18,2,19,2,20,2,21,2,22,2,12,-14),d0:6,off1:1);
            Assert.IsTrue(LongNardeRules.TryApplyLocalStep(p,new LongMove(17,23,0),out _,out _));
        }
        // Independent test oracle: relative coordinates, raw arrays, die-value sequences.
        // Never calls the production step validator or search while calculating expectations.
        private static HashSet<string> ReferenceTurns(LongPosition p)
        {
            int actor=p.Player;int[] own=new int[24],enemy=new int[24];
            for(int q=0;q<24;q++){int n=p.Point((actor*12+q)%24)*(actor==0?1:-1);own[q]=Math.Max(n,0);enemy[q]=Math.Max(-n,0);}
            var remaining=new List<int>();for(int s=0;s<p.TokenCount;s++)if(!p.IsUsed(s))remaining.Add(p.Die(s));
            var leaves=new List<List<string>>();
            Action<int[],List<int>,int,List<string>> walk=null;
            walk=(board,dice,heads,path)=>
            {
                bool extended=false;
                if(board.Sum()>0)foreach(int d in dice.Distinct().ToArray())for(int from=0;from<24;from++)
                {
                    if(board[from]==0)continue;
                    int cap=p.IsFirstTurn(actor)&&p.Die(0)==p.Die(1)&&new[]{3,4,6}.Contains(d)?2:1;
                    if(from==0&&heads>=cap)continue;
                    int to=from+d;
                    if(to<24){if(enemy[to]>0)continue;}
                    else
                    {
                        if(board.Take(18).Sum()!=0)continue;
                        if(to>24&&board.Take(from).Sum()>0)continue;
                        to=24;
                    }
                    var next=(int[])board.Clone();next[from]--;if(to<24)next[to]++;
                    bool banned=false;
                    if(p.Off(1-actor)==0)for(int start=0;start<=18;start++)
                    {
                        // Opponent progress r maps to our relative (r+12)%24.
                        bool full=Enumerable.Range(start,6).All(r=>next[(r+12)%24]>0);
                        bool beyond=Enumerable.Range(start+6,24-start-6).Any(r=>enemy[(r+12)%24]>0);
                        if(full&&!beyond){banned=true;break;}
                    }
                    if(banned)continue;
                    extended=true;var left=new List<int>(dice);left.Remove(d);
                    var moves=new List<string>(path){$"{(actor*12+from)%24}>{(to==24?24:(actor*12+to)%24)}@{d}"};
                    walk(next,left,heads+(from==0?1:0),moves);
                }
                if(!extended)leaves.Add(path);
            };
            walk(own,remaining,p.HeadDepartures(actor),new List<string>());
            int max=leaves.Max(l=>l.Count);var selected=leaves.Where(l=>l.Count==max).ToList();
            if(max==1){int high=selected.Max(l=>int.Parse(l[0].Split('@')[1]));selected=selected.Where(l=>l[0].EndsWith("@"+high)).ToList();}
            return new HashSet<string>(selected.Select(l=>string.Join(",",l)));
        }
        private static void CompareReference(LongPosition p)
        {
            var actual=new HashSet<string>(LongNardeRules.LegalTurns(p).Select(path=>string.Join(",",path.Select(m=>$"{m.Source}>{m.Target}@{p.Die(m.Slot)}"))));
            CollectionAssert.AreEquivalent(ReferenceTurns(p),actual);
        }
        [Test]
        public void IndependentOracleAgreesOnEveryInitialRollAndPlayer()
        {
            for(int a=1;a<=6;a++)for(int b=1;b<=6;b++)for(int player=0;player<2;player++)CompareReference(Moving(player,a,b));
        }
        [TestCase(0)] [TestCase(1)]
        public void SeededSyntheticPositionsAgreeWithIndependentOracle(int actor)
        {
            const int seed=1203;var rng=new System.Random(seed+actor);
            for(int sample=0;sample<500;sample++)
            {
                var b=new int[24];int[] off={sample%2==0?12:0,sample%3==0?12:0};
                for(int pl=0;pl<2;pl++)
                {
                    var candidates=Enumerable.Range(off[pl]>0?18:0,off[pl]>0?6:24)
                        .Select(q=>(pl*12+q)%24).Where(i=>b[i]==0).OrderBy(i=>rng.Next()).Take(3).ToArray();
                    int left=15-off[pl];
                    for(int j=0;j<3;j++){int n=j==2?left:1;b[candidates[j]]=n*(pl==0?1:-1);left-=n;}
                }
                var p=Position(b,actor,rng.Next(1,7),rng.Next(1,7),off[0],off[1]);
                CompareReference(p);Assert.AreEqual(Choices(p),Choices(p));
                var legal=LongNardeRules.LegalTurns(p);
                foreach(var path in legal.Take(3))Assert.IsTrue(LongNardeRules.TryApplyTurn(p,path,out _,out _),$"seed={seed+actor},sample={sample}");
            }
            TestContext.WriteLine($"Synthetic differential samples: seed={seed+actor}, count=500; reachability not asserted");
        }
        [Test]
        public void SeededReachablePositionsRemainImmutableAndRepeatable()
        {
            const int seed=1202;var rng=new System.Random(seed);var p=LongNardeRules.Initial();bool[] first={true,true};int turns=0;
            for(int i=0;i<240;i++)
            {
                int player=i%2;
                p=new LongPosition(p.CopyPoints(),p.CopyOff(),new[]{rng.Next(1,7),rng.Next(1,7)},player,LongPhase.Moving,first,new int[2]);
                if(i%5==0)CompareReference(p);
                var snapshot=p.CopyPoints();var choices=LongNardeRules.LegalTurns(p);
                Assert.AreEqual(Choices(p),Choices(p),$"seed={seed},turn={i}");
                var path=choices[rng.Next(choices.Count)];
                Assert.IsTrue(LongNardeRules.TryApplyTurn(p,path,out var after,out _));
                CollectionAssert.AreEqual(snapshot,p.CopyPoints());
                foreach(int pl in new[]{0,1})Assert.AreEqual(15,after.CopyPoints().Where(n=>n*(pl==0?1:-1)>0).Sum(n=>Math.Abs(n))+after.Off(pl));
                first[player]=false;p=after;turns++;
                if(after.Phase==LongPhase.Finished)break;
            }
            Assert.Greater(turns,20);TestContext.WriteLine($"Reachable position run: seed={seed}, turns={turns}");
        }
    }
}
