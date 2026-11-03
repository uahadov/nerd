using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Nard.Rules.Short;
using NUnit.Framework;
using UnityEngine;

namespace Nard.Tests.ShortRules
{
    public sealed class ShortRulesTests
    {
        [Serializable] public sealed class Corpus { public Fixture[] cases; }
        [Serializable] public sealed class Snapshot
        {
            public int[] points, bar, off, roll, used;
            public int to_move;
            public string phase;
            public ShortPosition Position() => new ShortPosition(points, bar, off, roll, to_move,
                (ShortPhase)Enum.Parse(typeof(ShortPhase),phase,true),used.Aggregate(0,(mask,slot)=>mask|(1<<slot)));
        }
        [Serializable] public sealed class MoveData
        {
            public int source,target,slot;
            public ShortMove Move()=>new ShortMove(source,target,slot);
        }
        [Serializable] public sealed class ActionData { public string kind;public MoveData[] moves;public int[] opening_dice; }
        [Serializable] public sealed class Expectation
        {
            public string status,reason,win_kind;public Snapshot after;public int max_used,starter,winner,points;
        }
        [Serializable] public sealed class Fixture
        {
            public string id,mode,ruleset_version;public Snapshot before;public ActionData action;public Expectation expected;
        }
        private static IEnumerable<Fixture> LoadFixtures()
        {
            string path=Environment.GetEnvironmentVariable("NARD_RULE_FIXTURES")??Path.GetFullPath(Path.Combine(Application.dataPath,"../../docs/rules/fixtures.json"));
            string json=File.ReadAllText(path);
            json=Regex.Replace(json,"\"target\"\\s*:\\s*\"off\"","\"target\":24");
            json=Regex.Replace(json,"\"source\"\\s*:\\s*\"bar\"","\"source\":-1");
            return JsonUtility.FromJson<Corpus>(json).cases.Where(f=>f.mode=="short"&&new[]{"start","opening","move","turn","classify_result"}.Contains(f.action.kind));
        }
        public static IEnumerable FixtureCases()=>LoadFixtures().Select(f=>new TestCaseData(f).SetName("ShortFixture_"+f.id));
        public static IEnumerable MirrorCases()
        {
            foreach(var original in LoadFixtures().Where(f=>f.action.kind!="start"&&f.action.kind!="opening"))
            {
                var f=JsonUtility.FromJson<Fixture>(JsonUtility.ToJson(original));
                foreach(var b in new[]{f.before,f.expected.after})
                {
                    b.points=b.points.Reverse().Select(n=>-n).ToArray();b.bar=b.bar.Reverse().ToArray();b.off=b.off.Reverse().ToArray();b.to_move=1-b.to_move;
                }
                foreach(var m in f.action.moves){if(m.source!=-1)m.source=23-m.source;if(m.target!=24)m.target=23-m.target;}
                if(f.action.kind=="classify_result")f.expected.winner=1-f.expected.winner;
                yield return new TestCaseData(f).SetName("ShortMirror_"+f.id);
            }
        }
        private static void Same(ShortPosition a,ShortPosition b)
        {
            CollectionAssert.AreEqual(a.CopyPoints(),b.CopyPoints());CollectionAssert.AreEqual(a.CopyBar(),b.CopyBar());
            CollectionAssert.AreEqual(a.CopyOff(),b.CopyOff());CollectionAssert.AreEqual(a.CopyRoll(),b.CopyRoll());
            Assert.AreEqual(a.Player,b.Player);Assert.AreEqual(a.Phase,b.Phase);Assert.AreEqual(a.UsedMask,b.UsedMask);
        }
        [TestCaseSource(nameof(FixtureCases))]
        [TestCaseSource(nameof(MirrorCases))]
        public void SpecificationFixture(Fixture f)
        {
            Assert.AreEqual(ShortNardeRules.RulesetVersion,f.ruleset_version);
            var before=f.before.Position();var after=before;bool accepted=true;string reason="OK";
            switch(f.action.kind)
            {
                case "start":after=ShortNardeRules.Initial();break;
                case "opening":after=ShortNardeRules.ResolveOpening(before,f.action.opening_dice[0],f.action.opening_dice[1]);Assert.AreEqual(f.expected.starter,after.Player);break;
                case "move":accepted=ShortNardeRules.TryApplyLocalStep(before,f.action.moves[0].Move(),out after,out reason);break;
                case "turn":accepted=ShortNardeRules.TryApplyTurn(before,f.action.moves.Select(m=>m.Move()).ToArray(),out after,out reason);Assert.AreEqual(f.expected.max_used,ShortNardeRules.LegalTurns(before)[0].Count);break;
                case "classify_result":Assert.AreEqual(f.expected.points,ShortNardeRules.ResultPoints(before,out int winner));Assert.AreEqual(f.expected.winner,winner);break;
                default:Assert.Fail("Unauthorized fixture action");break;
            }
            Assert.AreEqual(f.expected.status=="ACCEPT",accepted,reason);Assert.AreEqual(f.expected.reason,reason);
            Same(f.expected.after.Position(),after);Same(f.before.Position(),before);
        }
        private static int[] Board(params int[] pairs)
        {
            var board=new int[24];for(int i=0;i<pairs.Length;i+=2)board[pairs[i]]=pairs[i+1];return board;
        }
        private static ShortPosition Position(int[] board,int actor=0,int d0=2,int d1=1,int bar0=0,int bar1=0,int off0=0,int off1=0,int mask=0)
            =>new ShortPosition(board,new[]{bar0,bar1},new[]{off0,off1},new[]{d0,d1},actor,ShortPhase.Moving,mask);
        private static ShortPosition Mirror(ShortPosition p)=>new ShortPosition(p.CopyPoints().Reverse().Select(n=>-n).ToArray(),p.CopyBar().Reverse().ToArray(),p.CopyOff().Reverse().ToArray(),p.CopyRoll(),1-p.Player,p.Phase,p.UsedMask);
        private static ShortPosition Start(int actor,int a,int b)
        {
            var initial=ShortNardeRules.Initial();return new ShortPosition(initial.CopyPoints(),new int[2],new int[2],new[]{a,b},actor,ShortPhase.Moving);
        }
        private static string Choices(ShortPosition p)=>string.Join("|",ShortNardeRules.LegalTurns(p).Select(t=>string.Join(",",t.Select(m=>m.ToString()))));

        [TestCase(0)] [TestCase(1)]
        public void OpeningUsesOriginalDiceWithoutFreshRoll(int winner)
        {
            int a=winner==0?6:2,b=winner==0?2:6;
            var p=ShortNardeRules.ResolveOpening(ShortNardeRules.Initial(),a,b);
            Assert.AreEqual(winner,p.Player);CollectionAssert.AreEqual(new[]{a,b},p.CopyRoll());Assert.AreEqual(2,p.TokenCount);
            Assert.AreEqual(2,ShortNardeRules.LegalTurns(p)[0].Count);
        }
        [TestCase(0)] [TestCase(1)]
        public void LastBarEntryAllowsOtherCheckerToUseRemainingDie(int actor)
        {
            var p=Position(Board(5,14,23,-1,21,-2,0,-12),d0:1,d1:2,bar0:1);if(actor==1)p=Mirror(p);
            int target=actor==0?23:0,source=actor==0?5:18,to=actor==0?3:20;
            Assert.IsTrue(ShortNardeRules.TryApplyTurn(p,new[]{new ShortMove(-1,target,0),new ShortMove(source,to,1)},out var after,out _));
            Assert.AreEqual(0,after.Bar(actor));Assert.AreEqual(1,after.Bar(1-actor));Assert.AreEqual(3,after.UsedMask);
        }
        [TestCase(0)] [TestCase(1)]
        public void LowerBarEntryFirstIsRequiredToPlayBothDice(int actor)
        {
            var p=Position(Board(22,-2,0,-13),bar0:1,off0:14);if(actor==1)p=Mirror(p);
            var paths=ShortNardeRules.LegalTurns(p);Assert.IsNotEmpty(paths);
            Assert.IsTrue(paths.All(t=>t.Count==2&&t[0].Source==-1&&t[0].Slot==1));
        }
        [TestCase(0)] [TestCase(1)]
        public void HigherDieIsRequiredWhenOnlyOneBarEntryCanBePlayed(int actor)
        {
            var p=Position(Board(21,-2,0,-13),bar0:1,off0:14);if(actor==1)p=Mirror(p);
            Assert.IsTrue(ShortNardeRules.LegalTurns(p).All(t=>t.Count==1&&t[0].Slot==0));
            Assert.IsFalse(ShortNardeRules.TryApplyTurn(p,new[]{new ShortMove(-1,actor==0?23:0,1)},out var after,out var reason));
            Assert.AreEqual("HIGHER_DIE_REQUIRED",reason);Assert.AreSame(p,after);
        }
        [TestCase(0)] [TestCase(1)]
        public void FullyClosedHomeBlocksEveryRollAndOtherCheckers(int actor)
        {
            var board=Board(5,14,0,-3);for(int i=18;i<24;i++)board[i]=-2;
            for(int a=1;a<=6;a++)for(int b=1;b<=6;b++)
            {
                var p=Position(board,d0:a,d1:b,bar0:1);if(actor==1)p=Mirror(p);
                var paths=ShortNardeRules.LegalTurns(p);Assert.AreEqual(1,paths.Count);Assert.IsEmpty(paths[0]);
                Assert.IsTrue(ShortNardeRules.TryApplyTurn(p,Array.Empty<ShortMove>(),out var after,out _));Assert.AreSame(p,after);
            }
        }
        [TestCase(0,1)] [TestCase(0,2)] [TestCase(0,3)] [TestCase(0,4)] [TestCase(0,5)] [TestCase(0,6)]
        [TestCase(1,1)] [TestCase(1,2)] [TestCase(1,3)] [TestCase(1,4)] [TestCase(1,5)] [TestCase(1,6)]
        public void DoublesEnterFourBarCheckersWithoutHeadLimit(int actor,int die)
        {
            var p=Position(Board(0,-15),d0:die,d1:die,bar0:4,off0:11);if(actor==1)p=Mirror(p);
            var paths=ShortNardeRules.LegalTurns(p);Assert.AreEqual(1,paths.Count);Assert.AreEqual(4,paths[0].Count);
            Assert.IsTrue(paths[0].All(m=>m.Source==-1));
            Assert.IsTrue(ShortNardeRules.TryApplyTurn(p,paths[0],out var after,out _));Assert.AreEqual(0,after.Bar(actor));
        }
        [Test]
        public void BearingOffCannotBypassOwnBar()
        {
            var p=Position(Board(0,1,23,-15),d0:1,d1:2,bar0:1,off0:13);
            Assert.IsFalse(ShortNardeRules.TryApplyLocalStep(p,new ShortMove(0,24,0),out var after,out var reason));
            Assert.AreEqual("BAR_REQUIRED",reason);Assert.AreSame(p,after);
        }
        [TestCase(0)] [TestCase(1)]
        public void ExactBearOffIsAllowedEvenWithFartherChecker(int actor)
        {
            var p=Position(Board(2,1,5,1,23,-15),d0:3,d1:1,off0:13,mask:2);if(actor==1)p=Mirror(p);
            Assert.IsTrue(ShortNardeRules.TryApplyTurn(p,new[]{new ShortMove(actor==0?2:21,24,0)},out var after,out _));Assert.AreEqual(14,after.Off(actor));
        }
        [TestCase(0)] [TestCase(1)]
        public void FinalCheckerUsesHigherDieAndStopsImmediately(int actor)
        {
            var p=Position(Board(0,1,12,-15),d0:1,d1:6,off0:14);if(actor==1)p=Mirror(p);
            Assert.IsTrue(ShortNardeRules.TryApplyTurn(p,new[]{new ShortMove(actor==0?0:23,24,1)},out var after,out _));
            Assert.AreEqual(ShortPhase.Finished,after.Phase);Assert.IsEmpty(ShortNardeRules.LegalTurns(after));
            Assert.AreEqual(2,ShortNardeRules.ResultPoints(after,out int winner));Assert.AreEqual(actor,winner);Assert.AreEqual(2,after.UsedMask);
            Assert.IsFalse(ShortNardeRules.TryApplyTurn(after,Array.Empty<ShortMove>(),out _,out _));
        }
        [TestCase(0,0,1)] [TestCase(0,1,2)] [TestCase(0,2,3)] [TestCase(0,3,3)] [TestCase(0,4,1)]
        [TestCase(1,0,1)] [TestCase(1,1,2)] [TestCase(1,2,3)] [TestCase(1,3,3)] [TestCase(1,4,1)]
        public void ResultClassificationIsSymmetricAndNormalOverridesKoks(int winner,int scenario,int points)
        {
            int loser=1-winner,sign=loser==0?1:-1;var off=new int[2];off[winner]=15;
            if(scenario==0||scenario==4)off[loser]=1;
            var bar=new int[2];if(scenario==2||scenario==4)bar[loser]=1;
            var board=new int[24];board[scenario==3?(winner==0?0:23):12]=sign*(15-off[loser]-bar[loser]);
            var p=new ShortPosition(board,bar,off,Array.Empty<int>(),winner,ShortPhase.Finished);
            Assert.AreEqual(points,ShortNardeRules.ResultPoints(p,out int actualWinner));Assert.AreEqual(winner,actualWinner);
        }
        [Test]
        public void ShortRulesHaveNeitherHeadLimitNorSixPrimeBan()
        {
            var p=Start(0,1,2);
            Assert.IsTrue(ShortNardeRules.TryApplyTurn(p,new[]{new ShortMove(23,22,0),new ShortMove(23,21,1)},out _,out _));
            p=Position(Board(0,5,1,2,2,2,3,2,4,2,6,2,23,-15),d0:1,d1:2);
            Assert.IsTrue(ShortNardeRules.TryApplyTurn(p,new[]{new ShortMove(6,5,0),new ShortMove(6,4,1)},out _,out _));
        }
        [Test]
        public void LateInvalidMoveIsAtomicIncludingHitAndBar()
        {
            var p=Position(Board(7,1,5,-1,0,-14),off0:14);
            Assert.IsFalse(ShortNardeRules.TryApplyTurn(p,new[]{new ShortMove(7,5,0),new ShortMove(5,3,0)},out var after,out var reason));
            Assert.AreEqual("DIE_ALREADY_USED",reason);Assert.AreSame(p,after);Assert.AreEqual(0,p.Bar(1));Assert.AreEqual(-1,p.Point(5));
        }
        [Test]
        public void EqualDiceAcceptDistinctTokenPermutations()
        {
            var p=Position(Board(12,1,23,-15),d0:2,d1:2,off0:14);
            Assert.IsTrue(ShortNardeRules.TryApplyTurn(p,new[]{new ShortMove(12,10,3),new ShortMove(10,8,1),new ShortMove(8,6,2),new ShortMove(6,4,0)},out var after,out _));Assert.AreEqual(15,after.UsedMask);
        }
        [Test]
        public void InputAndExportArraysCannotMutatePosition()
        {
            var board=Board(5,14,0,-15);var bar=new[]{1,0};var off=new int[2];var dice=new[]{2,1};
            var p=new ShortPosition(board,bar,off,dice,0,ShortPhase.Moving);
            board[5]=0;bar[0]=0;off[0]=15;dice[0]=6;
            p.CopyPoints()[5]=0;p.CopyBar()[0]=0;p.CopyOff()[0]=15;p.CopyRoll()[0]=6;
            Assert.AreEqual(14,p.Point(5));Assert.AreEqual(1,p.Bar(0));Assert.AreEqual(0,p.Off(0));Assert.AreEqual(2,p.Die(0));
        }
        [Test]
        public void InvalidStateVersionAndOpeningInputsAreRejected()
        {
            Assert.Throws<ArgumentException>(()=>Position(Board(0,14,23,-15)));
            Assert.Throws<ArgumentException>(()=>Position(Board(0,15,23,-15),d0:7));
            Assert.Throws<ArgumentException>(()=>Position(Board(0,15,23,-15),mask:4));
            Assert.Throws<ArgumentException>(()=>new ShortPosition(Board(0,15,23,-15),new int[2],new int[2],new[]{1,2},0,ShortPhase.Moving,0,"long-az-gdd1-provisional.1"));
            Assert.Throws<ArgumentException>(()=>ShortNardeRules.ResolveOpening(ShortNardeRules.Initial(),0,6));
            Assert.Throws<ArgumentException>(()=>ShortNardeRules.ResolveOpening(Start(0,1,2),1,2));
            Assert.IsFalse(ShortNardeRules.TryApplyTurn(null,Array.Empty<ShortMove>(),out _,out _));
            Assert.IsFalse(ShortNardeRules.TryApplyTurn(Start(0,1,2),null,out _,out _));
            Assert.AreEqual(0,ShortNardeRules.ResultPoints(Start(0,1,2),out int winner));Assert.AreEqual(-1,winner);
        }
        [TestCase(-2,0,0)] [TestCase(24,0,0)] [TestCase(5,-1,0)] [TestCase(5,25,0)]
        [TestCase(5,3,-1)] [TestCase(5,3,2)] [TestCase(23,21,0)] [TestCase(-1,23,0)]
        public void InvalidMovesReturnOriginalPosition(int source,int target,int slot)
        {
            var p=Position(Board(5,15,23,-15));Assert.IsFalse(ShortNardeRules.TryApplyLocalStep(p,new ShortMove(source,target,slot),out var after,out _));Assert.AreSame(p,after);
        }

        // Test-only independent relative-distance oracle. No production hop/search calls.
        private static HashSet<string> ReferenceTurns(ShortPosition p)
        {
            int actor=p.Player;var own=new int[25];var enemy=new int[25];
            for(int i=0;i<24;i++){int q=actor==0?i+1:24-i,n=p.Point(i)*(actor==0?1:-1);own[q]=Math.Max(n,0);enemy[q]=Math.Max(-n,0);}
            var dice=new List<int>();for(int slot=0;slot<p.TokenCount;slot++)if(!p.IsUsed(slot))dice.Add(p.Die(slot));
            var leaves=new List<List<string>>();
            Action<int[],int[],int,int,List<int>,List<string>> walk=null;
            walk=(board,opponent,bar,borne,left,path)=>
            {
                bool extended=false;
                if(borne<15)foreach(int d in left.Distinct().ToArray())
                {
                    var sources=bar>0?new[]{25}:Enumerable.Range(1,24).Where(q=>board[q]>0).ToArray();
                    foreach(int from in sources)
                    {
                        int to=from-d;
                        if(to>0){if(opponent[to]>=2)continue;}
                        else
                        {
                            if(bar>0||board.Skip(7).Sum()!=0)continue;
                            if(to<0&&board.Skip(from+1).Sum()>0)continue;
                            to=0;
                        }
                        var next=(int[])board.Clone();var enemyNext=(int[])opponent.Clone();
                        if(from!=25)next[from]--;if(to>0){next[to]++;enemyNext[to]=0;}
                        int source=from==25?-1:actor==0?from-1:24-from;
                        int target=to==0?24:actor==0?to-1:24-to;
                        var remaining=new List<int>(left);remaining.Remove(d);
                        var moves=new List<string>(path){$"{source}>{target}@{d}"};
                        extended=true;walk(next,enemyNext,bar-(from==25?1:0),borne+(to==0?1:0),remaining,moves);
                    }
                }
                if(!extended)leaves.Add(path);
            };
            walk(own,enemy,p.Bar(actor),p.Off(actor),dice,new List<string>());
            int max=leaves.Max(l=>l.Count);var selected=leaves.Where(l=>l.Count==max).ToList();
            if(max==1){int high=selected.Max(l=>int.Parse(l[0].Split('@')[1]));selected=selected.Where(l=>l[0].EndsWith("@"+high)).ToList();}
            return new HashSet<string>(selected.Select(l=>string.Join(",",l)));
        }
        private static void CompareReference(ShortPosition p)
        {
            var actual=new HashSet<string>(ShortNardeRules.LegalTurns(p).Select(path=>string.Join(",",path.Select(m=>$"{m.Source}>{m.Target}@{p.Die(m.Slot)}"))));
            CollectionAssert.AreEquivalent(ReferenceTurns(p),actual);
        }
        [Test]
        public void EveryInitialRollPlayerAndDiceOrderIsDeterministic()
        {
            for(int a=1;a<=6;a++)for(int b=1;b<=6;b++)for(int actor=0;actor<2;actor++)
            {
                var p=Start(actor,a,b);CompareReference(p);Assert.AreEqual(Choices(p),Choices(p));
                foreach(var path in ShortNardeRules.LegalTurns(p).Take(3))Assert.IsTrue(ShortNardeRules.TryApplyTurn(p,path,out _,out _));
            }
        }
        [TestCase(0)] [TestCase(1)]
        public void ThousandSyntheticBarOffPositionsMatchReference(int actor)
        {
            int seed=1303+actor;var rng=new System.Random(seed);
            for(int sample=0;sample<500;sample++)
            {
                var board=new int[24];int[] bar={rng.Next(3),rng.Next(3)},off={rng.Next(13),rng.Next(13)};
                for(int pl=0;pl<2;pl++)
                {
                    int active=15-bar[pl]-off[pl];var slots=Enumerable.Range(0,24).Where(i=>board[i]==0).OrderBy(i=>rng.Next()).Take(Math.Min(3,active)).ToArray();
                    for(int j=0;j<slots.Length;j++){int n=j==slots.Length-1?active:1;board[slots[j]]=n*(pl==0?1:-1);active-=n;}
                }
                int a=rng.Next(1,7),b=rng.Next(1,7),tokens=a==b?4:2,mask=sample%2==0?0:rng.Next(1<<tokens);
                var p=new ShortPosition(board,bar,off,new[]{a,b},actor,ShortPhase.Moving,mask);
                CompareReference(p);Assert.AreEqual(Choices(p),Choices(p),$"seed={seed},sample={sample}");
                foreach(var path in ShortNardeRules.LegalTurns(p).Take(3))Assert.IsTrue(ShortNardeRules.TryApplyTurn(p,path,out _,out _));
            }
            TestContext.WriteLine($"Short synthetic differential: seed={seed}, count=500; reachability not asserted");
        }
        [Test]
        public void SeededReachablePlayPreservesCheckersAndExercisesHitting()
        {
            const int seed=1313;var rng=new System.Random(seed);var p=ShortNardeRules.Initial();int turns=0,hits=0;
            for(int i=0;i<800;i++)
            {
                int actor=i%2;p=new ShortPosition(p.CopyPoints(),p.CopyBar(),p.CopyOff(),new[]{rng.Next(1,7),rng.Next(1,7)},actor,ShortPhase.Moving);
                if(i%7==0)CompareReference(p);var before=p.CopyPoints();var choices=ShortNardeRules.LegalTurns(p);
                Assert.AreEqual(Choices(p),Choices(p));var path=choices[rng.Next(choices.Count)];
                Assert.IsTrue(ShortNardeRules.TryApplyTurn(p,path,out var after,out _));
                if(after.Bar(1-actor)>p.Bar(1-actor))hits+=after.Bar(1-actor)-p.Bar(1-actor);
                CollectionAssert.AreEqual(before,p.CopyPoints());
                for(int pl=0;pl<2;pl++)Assert.AreEqual(15,after.CopyPoints().Where(n=>n*(pl==0?1:-1)>0).Sum(n=>Math.Abs(n))+after.Bar(pl)+after.Off(pl));
                p=after;turns++;if(p.Phase==ShortPhase.Finished)break;
            }
            Assert.Greater(hits,0);Assert.AreEqual(ShortPhase.Finished,p.Phase);
            TestContext.WriteLine($"Short reachable run: seed={seed}, turns={turns}, hits={hits}");
        }
    }
}
