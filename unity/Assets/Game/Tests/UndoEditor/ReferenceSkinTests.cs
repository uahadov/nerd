using System.Collections.Generic;
using Nard.Presentation;
using NUnit.Framework;
using UnityEngine;
namespace Nard.Tests.Undo
{
 public sealed class ReferenceSkinTests
 {
  public static IEnumerable<TestCaseData> Points(){for(int i=0;i<24;i++)yield return new TestCaseData(i);}
  [TestCaseSource(nameof(Points))] public void EveryPaintedPointMapsBackToTheCanonicalHitTarget(int index)
  {
   var p=BoardGeometry.PointBase(index);p.y=index<12?-4:4;var local=ReferenceBoardSkin.ToLocal(p);var inverse=ReferenceBoardSkin.ToBoard(local);
   Assert.That(Vector2.Distance(p,inverse),Is.LessThan(.001f));Assert.AreEqual(index,BoardHitMap.PointAt(inverse,0));
  }
  [TestCase(0)] [TestCase(1)] public void PaintedBarAndOffTraysAreClickableForEachOwner(int player)
  {foreach(float x in new[]{-.25f,4.53f}){var p=new Vector2(x,player==0?-4:4);var local=ReferenceBoardSkin.ToLocal(p);Assert.AreEqual(x<0?-1:24,BoardHitMap.PointAt(ReferenceBoardSkin.ToBoard(local),player));}}
  [Test] public void RelocatedButtonsKeepDistinctIntentAndBoardBounds()
  {
   Assert.IsTrue(BoardHitMap.IsUndoButton(ReferenceBoardSkin.ToBoard(ReferenceBoardSkin.ToLocal(new Vector2(0,9.45f)))));
   Assert.IsTrue(BoardHitMap.IsRollButton(ReferenceBoardSkin.ToBoard(ReferenceBoardSkin.ToLocal(new Vector2(0,-9.35f)))));
   Assert.AreEqual(BoardHitMap.Outside,BoardHitMap.PointAt(ReferenceBoardSkin.ToBoard(new Vector2(9.1f,4)),0));
  }
 }
}
