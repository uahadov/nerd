using System;
using System.Linq;
using Nard.Match;
using Nard.Presentation;
using Nard.Rules.Long;
using Nard.Rules.Short;
using NUnit.Framework;
using UnityEngine;

namespace Nard.Tests.Board
{
    public sealed class BoardProjectionTests
    {
        [Test] public void CanonicalIndexMapHasTwentyFourUniqueColumnsAndRows()
        {
            var positions=Enumerable.Range(0,24).Select(BoardGeometry.PointBase).ToArray();
            Assert.AreEqual(24,positions.Distinct().Count());
            Assert.Less(positions[0].y,0);Assert.Greater(positions[12].y,0);
            for(int i=0;i<11;i++)Assert.Greater(positions[i].x,positions[i+1].x);
            for(int i=12;i<23;i++)Assert.Less(positions[i].x,positions[i+1].x);
            for(int i=0;i<12;i++)Assert.AreEqual(positions[i].x,positions[23-i].x,.001f);
        }
        [TestCase(-1)] [TestCase(24)] public void InvalidPointIndexIsRejected(int index)
            =>Assert.Throws<ArgumentOutOfRangeException>(()=>BoardGeometry.PointBase(index));
        [TestCase(0)] [TestCase(16)] public void InvalidStackSizeIsRejected(int count)
            =>Assert.Throws<ArgumentOutOfRangeException>(()=>BoardGeometry.Diameter(count));
        [Test] public void InvalidOrdinalIsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()=>BoardGeometry.CheckerCenter(Vector2.zero,false,-1,15));
            Assert.Throws<ArgumentOutOfRangeException>(()=>BoardGeometry.CheckerCenter(Vector2.zero,false,15,15));
        }
        [Test] public void NullInputsAreRejected()
        {
            Assert.Throws<ArgumentNullException>(()=>BoardSnapshot.From((LongPosition)null));
            Assert.Throws<ArgumentNullException>(()=>BoardSnapshot.From((ShortPosition)null));
            Assert.Throws<ArgumentNullException>(()=>BoardSnapshot.From((MatchSession)null));
        }
        [TestCase(GameMode.Long)] [TestCase(GameMode.Short)] public void InitialMatchProjectionHasExactStateCounts(GameMode mode)
        {
            var match=MatchSession.Create("projection",mode,RuleContract.VersionFor(mode));var frame=BoardSnapshot.From(match);
            for(int i=0;i<24;i++)Assert.AreEqual(mode==GameMode.Long?match.LongPosition.Point(i):match.ShortPosition.Point(i),frame.Point(i));
            for(int p=0;p<2;p++){Assert.AreEqual(0,frame.Bar(p));Assert.AreEqual(0,frame.Off(p));}
            Assert.AreEqual(MatchPhase.Created,match.Phase);
        }
        [TestCaseSource(nameof(Indices))] public void AllFifteenCheckersRemainVisibleAtEveryPoint(int index)
        {
            var points=new int[24];points[index]=15;points[(index+12)%24]=-15;
            var frame=BoardSnapshot.From(new LongPosition(points,new int[2],Array.Empty<int>(),-1,LongPhase.Opening,new[]{true,true},new int[2]));
            CheckView(frame);
        }
        public static int[] Indices()=>Enumerable.Range(0,24).ToArray();
        [TestCase(0)] [TestCase(1)] public void FullShortBarAndBorneOffAreNotHidden(int player)
        {
            var points=new int[24];points[12]=player==0?-15:15;var bar=new int[2];bar[player]=15;
            CheckView(BoardSnapshot.From(new ShortPosition(points,bar,new int[2],new[]{2,3},player,ShortPhase.Moving)));
            var off=new int[2];off[player]=15;
            CheckView(BoardSnapshot.From(new ShortPosition(points,new int[2],off,new[]{2,3},player,ShortPhase.Finished)));
        }
        [Test] public void SwitchingModesAndRepeatedDisplayCannotLeaveOldActiveCheckers()
        {
            var go=new GameObject("view");try
            {
                var view=go.AddComponent<BoardView>();
                for(int i=0;i<5;i++)view.Display(BoardSnapshot.From(i%2==0?MatchSession.Create("long",GameMode.Long,RuleContract.VersionFor(GameMode.Long)):MatchSession.Create("short",GameMode.Short,RuleContract.VersionFor(GameMode.Short))));
                Assert.AreEqual(30,view.Checkers.Count);Assert.AreEqual(24,view.Points.Count);
                Assert.AreEqual(30,go.GetComponentsInChildren<SpriteRenderer>().Count(r=>r.name.StartsWith("Checker_")));
                Assert.Throws<ArgumentNullException>(()=>view.Display(null));Assert.AreEqual(30,view.Checkers.Count);
            }finally{UnityEngine.Object.DestroyImmediate(go);}
        }
        [Test] public void BuiltinFontContainsAzerbaijaniNoticeGlyphs()
        {
            var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            foreach(char c in "əğıİöşçüƏĞIÖŞÇÜ")Assert.IsTrue(font.HasCharacter(c),"Missing glyph "+c);
        }
        [Test] public void ProjectionChangesOnlyAfterExplicitNewEngineSnapshot()
        {
            var initial=LongNardeRules.SupplyOpeningRoll(LongNardeRules.ResolveOpening(LongNardeRules.Initial(),6,1),2,5);
            var frame=BoardSnapshot.From(initial);Assert.IsTrue(LongNardeRules.TryApplyTurn(initial,LongNardeRules.LegalTurns(initial)[0],out var after,out _));
            for(int i=0;i<24;i++)Assert.AreEqual(initial.Point(i),frame.Point(i));
            var next=BoardSnapshot.From(after);Assert.IsTrue(Enumerable.Range(0,24).Any(i=>next.Point(i)!=frame.Point(i)));CheckView(next);
        }
        private static void CheckView(BoardSnapshot frame)
        {
            var go=new GameObject("projection-test");try
            {
                var view=go.AddComponent<BoardView>();view.Display(frame);Assert.AreEqual(24,view.Points.Count);Assert.AreEqual(30,view.Checkers.Count);
                for(int p=0;p<2;p++)
                {
                    Assert.AreEqual(15,view.Checkers.Count(c=>c.Player==p));
                    Assert.AreEqual(frame.Bar(p),view.Checkers.Count(c=>c.Player==p&&c.Point==-1));
                    Assert.AreEqual(frame.Off(p),view.Checkers.Count(c=>c.Player==p&&c.Point==24));
                }
                for(int i=0;i<24;i++)Assert.AreEqual(Math.Abs(frame.Point(i)),view.Checkers.Count(c=>c.Point==i));
                foreach(var c in view.Checkers)
                {
                    Assert.IsTrue(c.Renderer.enabled&&c.Renderer.gameObject.activeInHierarchy);
                    var b=c.Renderer.bounds;Assert.Greater(b.size.x,.2f);Assert.Greater(b.size.y,.2f);
                    Assert.Less(b.max.y,7.01f);Assert.Greater(b.min.y,-7.01f);
                    foreach(var other in view.Checkers.Where(o=>o!=c))Assert.IsFalse(b.Intersects(other.Renderer.bounds),"Overlapping checker faces");
                }
            }finally{UnityEngine.Object.DestroyImmediate(go);}
        }
    }
}
