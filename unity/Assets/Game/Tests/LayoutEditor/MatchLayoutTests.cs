using System;
using System.Collections.Generic;
using Nard.Presentation;
using NUnit.Framework;
using UnityEngine;
namespace Nard.Tests.Layout
{
    public sealed class MatchLayoutTests
    {
        public static IEnumerable<TestCaseData> Viewports()
        {
            var sizes=new[]{new Vector2Int(320,568),new Vector2Int(390,844),new Vector2Int(540,960),new Vector2Int(720,1600),new Vector2Int(768,1024),new Vector2Int(1024,768),new Vector2Int(960,540),new Vector2Int(1600,720),new Vector2Int(1920,1080),new Vector2Int(2340,1080),new Vector2Int(1080,2340),new Vector2Int(600,600)};
            foreach(var size in sizes)foreach(bool notch in new[]{false,true})yield return new TestCaseData(size.x,size.y,notch).SetName($"MatchLayout_{size.x}x{size.y}_{(notch?"Safe":"Full")}");
        }
        private static void Inside(Rect outer,Rect inner)
        {Assert.That(inner.xMin,Is.GreaterThanOrEqualTo(outer.xMin-.01f));Assert.That(inner.yMin,Is.GreaterThanOrEqualTo(outer.yMin-.01f));Assert.That(inner.xMax,Is.LessThanOrEqualTo(outer.xMax+.01f));Assert.That(inner.yMax,Is.LessThanOrEqualTo(outer.yMax+.01f));}
        [TestCaseSource(nameof(Viewports))] public void SafeAreaControlsBoardAndEveryHitTargetStayConsistent(int w,int h,bool notch)
        {
            var safe=notch?(h>w?new Rect(0,20,w,h-56):new Rect(36,12,w-66,h-24)):new Rect(0,0,w,h);
            var l=new MatchBoardLayout(w,h,safe);Inside(safe,l.Board);Inside(safe,l.Rail);Inside(safe,l.Header);Inside(l.Rail,l.Undo);Inside(l.Rail,l.Roll);Inside(l.Board,l.Dice);
            Assert.IsFalse(l.Undo.Overlaps(l.Roll));Assert.IsFalse(l.Undo.Overlaps(l.Dice));Assert.IsFalse(l.Roll.Overlaps(l.Dice));
            Assert.That(l.Undo.height/l.Unit,Is.GreaterThanOrEqualTo(43.99f));Assert.That(l.Roll.height/l.Unit,Is.GreaterThanOrEqualTo(43.99f));
            Assert.AreEqual(h>w,l.Portrait);if(l.Portrait)Assert.That(l.Board.height/l.Board.width,Is.EqualTo(4f/3f).Within(.001));else Assert.That(l.Board.width/l.Board.height,Is.EqualTo(4f/3f).Within(.001));
            Assert.That(l.Board.width*l.Board.height/(safe.width*safe.height),Is.GreaterThan(.39f));
            for(int i=0;i<24;i++)
            {var p=BoardGeometry.PointBase(i);p.y=i<12?-4:4;var world=l.CanonicalToWorld(p);Assert.IsTrue(l.Board.Contains(l.WorldToPixel(world)));Assert.AreEqual(i,BoardHitMap.PointAt(l.WorldToCanonical(world),0));}
            for(int player=0;player<2;player++)
            {Inside(l.Board,l.OffPocket(player));Assert.AreEqual(24,BoardHitMap.PointAt(l.WorldToCanonical(l.PixelToWorld(l.OffPocket(player).center)),player));var p=new Vector2(-.25f,player==0?-4:4);Assert.AreEqual(-1,BoardHitMap.PointAt(l.WorldToCanonical(l.CanonicalToWorld(p)),player));}
            Assert.IsTrue(BoardHitMap.IsUndoButton(l.WorldToCanonical(l.CanonicalToWorld(new Vector2(0,9.45f)))));
            Assert.IsTrue(BoardHitMap.IsRollButton(l.WorldToCanonical(l.CanonicalToWorld(new Vector2(0,-9.35f)))));
            Assert.AreEqual(BoardHitMap.Outside,BoardHitMap.PointAt(l.WorldToCanonical(l.PixelToWorld(new Vector2(-20,-20))),0));
            for(int i=0;i<24;i++)
            {
                var cell=l.PointTouchRect(i);Inside(l.Board,cell);
                foreach(var fraction in new[]{.02f,.5f,.98f})
                {var at=new Vector2(Mathf.Lerp(cell.xMin,cell.xMax,fraction),cell.center.y);Assert.AreEqual(i,BoardHitMap.PointAt(l.WorldToCanonical(l.PixelToWorld(at)),0));}
                for(int j=i+1;j<24;j++)Assert.IsFalse(cell.Overlaps(l.PointTouchRect(j)),"overlapping touch lanes");
            }
            var a=l.WorldToPixel(l.FromBoard(new Vector2(-6,3)));var b=l.WorldToPixel(l.FromBoard(new Vector2(-5,3)));Assert.Greater(Vector2.Distance(a,b),.82f*l.BoardScale);
        }
        [Test] public void PortraitHasSixUprightLanesPerTrayAndLargerCheckers()
        {
            var l=new MatchBoardLayout(540,960,new Rect(0,0,540,960));
            for(int i=0;i<6;i++)
            {Assert.AreEqual(l.PointTouchRect(12+i).center.y,l.PointTouchRect(11-i).center.y,.01f);Assert.Greater(l.PointTouchRect(12+i).center.x,l.PointTouchRect(11-i).center.x);if(i<5)Assert.Greater(l.PointTouchRect(12+i).center.y,l.PointTouchRect(13+i).center.y);}
            Assert.Greater(l.CheckerDiameter,24);Assert.Greater(l.PointTouchRect(0).height,l.CheckerDiameter);
        }
        [Test] public void InvalidPointIdsAreRejectedAndFrameDoesNotSelectAPoint()
        {
            var l=new MatchBoardLayout(390,844,new Rect(0,0,390,844));
            Assert.Throws<ArgumentOutOfRangeException>(()=>l.PointTouchRect(-1));Assert.Throws<ArgumentOutOfRangeException>(()=>l.PointTouchRect(24));
            Assert.AreEqual(BoardHitMap.Outside,BoardHitMap.PointAt(l.WorldToCanonical(l.PixelToWorld(new Vector2(l.Board.xMin+1,l.Board.center.y))),0));
        }
        [TestCase(320,568)] [TestCase(390,844)] [TestCase(540,960)] [TestCase(720,1600)] [TestCase(768,1024)]
        public void PortraitHasJoinedBoardAndOrderedControls(int w,int h)
        {
            var l=new MatchBoardLayout(w,h,new Rect(0,20,w,h-56));
            Assert.That(l.Tray(0).yMin-l.Tray(1).yMax,Is.EqualTo(l.PortraitBar.height).Within(.01f));
            Assert.That(l.Header.yMin,Is.GreaterThanOrEqualTo(l.Board.yMax-.01f));Assert.That(l.Board.yMin,Is.GreaterThanOrEqualTo(l.Rail.yMax-.01f));
            Assert.Greater(l.Dice.yMin,l.Undo.yMax);Assert.Greater(l.Dice.yMin,l.Roll.yMax);
            Assert.GreaterOrEqual(l.Undo.width/l.Unit,130);Assert.That(l.Undo.height/l.Unit,Is.GreaterThanOrEqualTo(44));
            for(int i=0;i<24;i++)
            {
                var at=l.CheckerPixel(i,i<12?0:1,14,15);Assert.AreEqual(i,BoardHitMap.PointAt(l.WorldToCanonical(l.PixelToWorld(at)),0));
                var centers=new HashSet<Vector2>();for(int n=0;n<15;n++)centers.Add(l.CheckerPixel(i,0,n,15));Assert.AreEqual(15,centers.Count);
                for(int n=0;n<5;n++)Assert.AreEqual(i,BoardHitMap.PointAt(l.WorldToCanonical(l.PixelToWorld(l.CheckerPixel(i,0,n,5))),0));
            }
        }
        [TestCase(1)] [TestCase(2)] [TestCase(5)] [TestCase(10)] [TestCase(15)]
        public void PortraitEveryStackFitsItsActualLaneAndNeverSharesAnotherTouchCell(int count)
        {
            foreach(var size in new[]{new Vector2Int(320,568),new Vector2Int(390,844),new Vector2Int(540,960),new Vector2Int(720,1600)})
            {
                var l=new MatchBoardLayout(size.x,size.y,new Rect(0,20,size.x,size.y-56));float lane=l.PortraitLaneWidth;
                Assert.That(l.CheckerFootprint(0)/lane,Is.EqualTo(.84f).Within(.001));Assert.Greater(l.PointTouchRect(0).height,l.CheckerFootprint(0));
                for(int point=-1;point<=24;point++)for(int player=0;player<2;player++)
                {
                    var cell=point==-1?l.BarPocket(player):point==24?l.OffPocket(player):l.PointVisualRect(point);float d=l.CheckerFootprint(point);var positions=new HashSet<Vector2>();
                    for(int i=0;i<count;i++)
                    {
                        var at=l.CheckerPixel(point,player,i,count);positions.Add(at);Inside(cell,new Rect(at.x-d/2,at.y-d/2,d,d));
                        Assert.AreEqual(point,BoardHitMap.PointAt(l.WorldToCanonical(l.PixelToWorld(at)),player));
                    }
                    Assert.AreEqual(count,positions.Count,"Every checker has its own regularly spaced center");
                    if(point>=0&&point<24)
                    {
                        var marker=l.TargetPixel(point,player);float m=lane*.24f;
                        for(int i=0;i<count;i++){var at=l.CheckerPixel(point,player,i,count);Assert.IsFalse(new Rect(marker.x-m/2,marker.y-m/2,m,m).Overlaps(new Rect(at.x-d/2,at.y-d/2,d,d)),"marker touches stack");}
                        var touch=l.PointTouchRect(point);Assert.AreEqual(point,BoardHitMap.PointAt(l.WorldToCanonical(l.PixelToWorld(new Vector2(touch.xMin+.1f,touch.center.y))),player));
                        for(int j=point+1;j<24;j++)Assert.IsFalse(touch.Overlaps(l.PointTouchRect(j)));
                    }
                }
            }
        }
        [TestCase(0,100)] [TestCase(100,0)] [TestCase(-1,100)] public void InvalidViewportIsRejected(int w,int h)=>Assert.Throws<ArgumentOutOfRangeException>(()=>new MatchBoardLayout(w,h,new Rect()));
        [Test] public void InvalidSafeAreaFallsBackAndOversizedSafeAreaIsClipped()
        {var full=new Rect(0,0,960,540);Assert.AreEqual(full,new MatchBoardLayout(960,540,new Rect()).Safe);Assert.AreEqual(full,new MatchBoardLayout(960,540,new Rect(-10,-10,1000,600)).Safe);}
    }
}
