using System;
using System.Linq;
using Nard.Interaction;
using Nard.Rules.Long;
using Nard.Rules.Short;
using NUnit.Framework;
using L=Nard.Tests.Rules.LongRulesTests;
namespace Nard.Tests.Undo
{
    public sealed class UndoTests
    {
        private static ShortPosition Short(L.Snapshot p)=>new ShortPosition(p.points,p.bar,p.off,p.roll,p.to_move,(ShortPhase)Enum.Parse(typeof(ShortPhase),p.phase,true),p.used.Aggregate(0,(a,b)=>a|(1<<b)));
        private static void Same(L.Snapshot expected,TapSelection actual)
        {
            if(actual.LongPosition!=null)
            {
                var p=actual.LongPosition;CollectionAssert.AreEqual(expected.points,p.CopyPoints());CollectionAssert.AreEqual(expected.off,p.CopyOff());CollectionAssert.AreEqual(expected.roll,p.CopyRoll());CollectionAssert.AreEqual(expected.head_departures,p.CopyHeadDepartures());CollectionAssert.AreEqual(expected.first_turn,p.CopyFirstTurns());Assert.AreEqual(expected.phase,p.Phase.ToString().ToLowerInvariant());Assert.AreEqual(expected.to_move,p.Player);Assert.AreEqual(expected.used.Aggregate(0,(a,b)=>a|(1<<b)),p.UsedMask);
            }
            else
            {
                var p=actual.ShortPosition;CollectionAssert.AreEqual(expected.points,p.CopyPoints());CollectionAssert.AreEqual(expected.bar,p.CopyBar());CollectionAssert.AreEqual(expected.off,p.CopyOff());CollectionAssert.AreEqual(expected.roll,p.CopyRoll());Assert.AreEqual(expected.phase,p.Phase.ToString().ToLowerInvariant());Assert.AreEqual(expected.to_move,p.Player);Assert.AreEqual(expected.used.Aggregate(0,(a,b)=>a|(1<<b)),p.UsedMask);
            }
        }
        [TestCase("L_UNDO_LOCAL",TestName="UndoFixture_L_UNDO_LOCAL")]
        [TestCase("L_UNDO_COMMITTED",TestName="UndoFixture_L_UNDO_COMMITTED")]
        [TestCase("S_UNDO_LOCAL",TestName="UndoFixture_S_UNDO_LOCAL")]
        [TestCase("S_UNDO_COMMITTED",TestName="UndoFixture_S_UNDO_COMMITTED")]
        public void UndoFixture(string id)
        {
            var all=L.FixtureCases().Cast<TestCaseData>().Select(x=>(L.Fixture)x.Arguments[0]).ToArray();var f=all.Single(x=>x.id==id);bool local=f.expected.status=="ACCEPT";
            var sample=all.Single(x=>x.id==(f.mode=="long"?"L_STEP_OVER_OCCUPIED":"S_HIT_SINGLE"));
            var root=local?sample.before:f.before;
            var preview=f.mode=="long"?new UndoPreview(root.Position(),root.to_move):new UndoPreview(Short(root),root.to_move);
            if(local)
            {
                var move=sample.action.moves[0];preview.Tap(move.source);Assert.AreEqual(TapOutcome.Moved,preview.Tap(move.target));Same(f.before,preview.Selection);
                Assert.IsFalse(preview.TryUndo(1-root.to_move));Same(f.before,preview.Selection);
            }
            Assert.AreEqual(local,preview.TryUndo(root.to_move));Same(f.expected.after,preview.Selection);Assert.IsFalse(preview.TryUndo(root.to_move));
        }
    }
}
