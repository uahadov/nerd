using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nard.Interaction;
using Nard.Match;
using Nard.Presentation;
using Nard.Rules.Long;
using Nard.Rules.Short;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Nard.Tests.Layout
{
    public sealed class MatchLayoutSceneTests
    {
        private InputSettings previous;
        private BoardTurnInput input;private BoardView view;
        private sealed class Dice:IDiceSource{private readonly System.Random random;public int Calls;public Dice(int? seed=null){if(seed.HasValue)random=new System.Random(seed.Value);}public int NextDie(){Calls++;return random==null?3:random.Next(1,7);}}
        private IEnumerator Load()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Game/Scenes/LayoutBoard.unity",new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("LayoutBoard",LoadSceneMode.Single);
#endif
            yield return null;yield return null;input=UnityEngine.Object.FindAnyObjectByType<BoardTurnInput>();view=input.GetComponent<BoardView>();Assert.IsTrue(view.IsResponsive);
            previous=UnityEngine.Object.Instantiate(InputSystem.settings);previous.hideFlags=HideFlags.HideAndDontSave;var settings=UnityEngine.Object.Instantiate(previous);settings.hideFlags=HideFlags.HideAndDontSave;settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings=settings;
        }
        [TearDown] public void Restore(){if(previous==null)return;var current=InputSystem.settings;InputSystem.settings=previous;if(current!=null)UnityEngine.Object.Destroy(current);}
        private LocalTurnLoop Ready(GameMode mode,Dice dice)
        {var loop=new LocalTurnLoop(MatchSession.Create("layout",mode,RuleContract.VersionFor(mode)).Begin().ResolveOpening(6,1),dice);if(loop.CanRoll)loop.TryRoll(0);input.Bind(loop);return loop;}
        private void Viewport(int w,int h,Rect? safe=null)=>view.SetLayoutViewport(w,h,safe??new Rect(0,0,w,h));
        private Vector2 P(float x,float y)=>Camera.main.WorldToScreenPoint(view.BoardToLocal(new Vector2(x,y)));
        private static IEnumerator Mouse(Mouse m,Vector2 at,bool down){InputSystem.QueueStateEvent(m,new MouseState{position=at,buttons=(ushort)(down?1:0)});yield return null;}
        private IEnumerator Click(Mouse m,Vector2 at){yield return Mouse(m,at,true);yield return Mouse(m,at,false);}
        private static IEnumerator Touch(Touchscreen t,int id,UnityEngine.InputSystem.TouchPhase phase,Vector2 at){InputSystem.QueueStateEvent(t,new TouchState{touchId=id,phase=phase,position=at});yield return null;}
        private static TurnMove[] Path(LocalTurnLoop loop)=>loop.Session.Mode==GameMode.Long?LongNardeRules.LegalTurns(loop.LongPosition)[0].Select(m=>new TurnMove(m.Source,m.Target,m.Slot)).ToArray():ShortNardeRules.LegalTurns(loop.ShortPosition)[0].Select(m=>new TurnMove(m.Source,m.Target,m.Slot)).ToArray();
        private void CheckPieces()
        {
            view.GetComponent<BoardFeedback>()?.Finish();
            Assert.AreEqual(30,view.Checkers.Count);var l=view.CurrentLayout;
            foreach(var c in view.Checkers){Assert.IsTrue(c.Renderer.enabled);var at=l.WorldToPixel(c.Renderer.transform.localPosition);Assert.IsTrue(l.Safe.Contains(at),c.Renderer.name);Assert.IsTrue(l.Board.Contains(at),c.Renderer.name);Assert.AreEqual(c.Point,BoardHitMap.PointAt(l.WorldToCanonical(c.Renderer.transform.localPosition),c.Player),"Checker/hit-map mismatch: "+c.Renderer.name);
                var min=l.WorldToPixel(c.Renderer.bounds.min);var max=l.WorldToPixel(c.Renderer.bounds.max);var bounds=Rect.MinMaxRect(min.x,min.y,max.x,max.y);
                Assert.IsFalse(bounds.Overlaps(l.Dice),"Dice overlaps "+c.Renderer.name);
                Assert.That(bounds.xMin,Is.GreaterThanOrEqualTo(l.Safe.xMin-.1f));Assert.That(bounds.xMax,Is.LessThanOrEqualTo(l.Safe.xMax+.1f));Assert.That(bounds.yMin,Is.GreaterThanOrEqualTo(l.Safe.yMin-.1f));Assert.That(bounds.yMax,Is.LessThanOrEqualTo(l.Safe.yMax+.1f));}
            Assert.IsFalse(input.GetComponentsInChildren<SpriteRenderer>().Any(r=>r.name=="Off tray"&&r.enabled));
            Assert.IsFalse(input.GetComponentsInChildren<TextMesh>().Any(t=>t.GetComponent<Renderer>().enabled&&(t.text.Contains("P0")||t.text.Contains("P1")||t.text.Contains("Gedişi tamamlayın"))));
        }
        private void Capture(string name)
        {
            string dir=Environment.GetEnvironmentVariable("NARD_BOARD_CAPTURE_DIR");if(string.IsNullOrEmpty(dir))return;Directory.CreateDirectory(dir);var l=view.CurrentLayout;var camera=Camera.main;
            var rt=new RenderTexture(l.Width,l.Height,24);var texture=new Texture2D(l.Width,l.Height,TextureFormat.RGB24,false);var old=camera.targetTexture;var active=RenderTexture.active;
            try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,l.Width,l.Height),0,0);texture.Apply();File.WriteAllBytes(System.IO.Path.Combine(dir,name+".png"),texture.EncodeToPNG());}
            finally{camera.targetTexture=old;RenderTexture.active=active;rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(texture);}
        }
        [UnityTest] public IEnumerator PortraitCheckerEdgesSelectCorrectLaneAndHudShowsBothRealSides()
        {
            yield return Load();var dice=new Dice();var loop=Ready(GameMode.Long,dice);Viewport(540,960);yield return null;
            var mouse=InputSystem.AddDevice<Mouse>();
            try
            {
                var l=view.CurrentLayout;var cell=l.PointTouchRect(0);
                var world=l.PixelToWorld(new Vector2(cell.xMax-1,cell.center.y));
                yield return Click(mouse,Camera.main.WorldToScreenPoint(world));Assert.AreEqual(0,loop.Selected);Assert.AreEqual(0,loop.LongPosition.UsedMask);
                Assert.NotNull(GameObject.Find("SelectedChecker"));Assert.IsTrue(loop.Targets.Count>0);
                var texts=input.GetComponentsInChildren<TextMesh>().Where(t=>t.GetComponent<Renderer>().enabled).Select(t=>t.text).ToArray();
                CollectionAssert.Contains(texts,"Açıq tərəf");CollectionAssert.Contains(texts,"Qoz tərəf");CollectionAssert.Contains(texts,"4 gediş qalıb");
                Assert.IsFalse(texts.Any(t=>t.Contains("0:0")||t.Contains("BO3")));
                Capture("layout-portrait-selection");CheckPieces();
            }finally{InputSystem.RemoveDevice(mouse);}
        }
        [UnityTest] public IEnumerator OpeningAndRollControlsShowOnlyRealActions()
        {
            yield return Load();var dice=new Dice();var loop=new LocalTurnLoop(MatchSession.Create("opening-hud",GameMode.Long,RuleContract.VersionFor(GameMode.Long)),dice);input.Bind(loop);Viewport(540,960);yield return null;
            Assert.IsTrue(loop.CanRoll);Assert.IsFalse(loop.CanUndo);Assert.NotNull(GameObject.Find("RollLabel"));
            var mouse=InputSystem.AddDevice<Mouse>();
            try{yield return Click(mouse,P(0,-9.35f));Assert.AreEqual(2,dice.Calls);Assert.IsTrue(loop.CanRoll);Assert.IsFalse(loop.CanUndo);Assert.AreEqual(MatchPhase.Rolling,loop.Session.Phase);CollectionAssert.Contains(input.GetComponentsInChildren<TextMesh>().Where(t=>t.GetComponent<Renderer>().enabled).Select(t=>t.text).ToArray(),"Bərabərdir · yenidən atın");Capture("layout-portrait-opening");}
            finally{InputSystem.RemoveDevice(mouse);}
        }
        [UnityTest] public IEnumerator PortraitStackCanBeSelectedFromEveryVisibleLayerAndUndone()
        {
            yield return Load();var loop=Ready(GameMode.Long,new Dice());Viewport(540,960);yield return null;var original=loop.LongPosition;var mouse=InputSystem.AddDevice<Mouse>();
            try
            {
                Assert.NotNull(GameObject.Find("ResponsiveBoard"));Assert.NotNull(GameObject.Find("DiceTray"));Assert.IsNull(GameObject.Find("TrayTop_0"));
                foreach(int layer in Enumerable.Range(0,15))
                {
                    if(loop.Selected!=TapSelection.None)loop.Tap(0,loop.Selected);input.Bind(loop);var l=view.CurrentLayout;var pixel=l.CheckerPixel(0,0,layer,15);yield return Click(mouse,Camera.main.WorldToScreenPoint(l.PixelToWorld(pixel)));
                    Assert.AreEqual(0,loop.Selected);Assert.AreEqual(0,loop.LongPosition.UsedMask);
                    int target=loop.Targets[0];var at=Camera.main.WorldToScreenPoint(l.PixelToWorld(l.TargetPixel(target,0)));yield return Click(mouse,at);
                    Assert.AreEqual(1,loop.Tokens.Count(t=>t.State==TokenState.Used));Assert.IsTrue(loop.CanUndo);yield return Click(mouse,P(0,9.45f));Assert.AreEqual(0,loop.LongPosition.UsedMask);
                    CollectionAssert.AreEqual(original.CopyPoints(),loop.LongPosition.CopyPoints());
                }
                Capture("portrait-unified-long");CheckPieces();
            }finally{InputSystem.RemoveDevice(mouse);}
        }
        [UnityTest] public IEnumerator PortraitPhoneSizesKeepHudReadableAndLandscapeRendersUnchanged()
        {
            yield return Load();var loop=Ready(GameMode.Short,new Dice());var state=loop.Session;
            foreach(var size in new[]{new Vector2Int(320,568),new Vector2Int(390,844),new Vector2Int(540,960),new Vector2Int(720,1600)})
            {
                Viewport(size.x,size.y,new Rect(0,20,size.x,size.y-56));yield return null;CheckPieces();Assert.AreSame(state,loop.Session);
                var l=view.CurrentLayout;var labels=input.GetComponentsInChildren<TextMesh>().Where(t=>t.GetComponent<Renderer>().enabled);
                foreach(string name in new[]{"MoveFeedback","RightsRemaining","UndoLabel","PlayerName_0","PlayerName_1"})
                {var text=labels.Single(t=>t.name==name);Assert.GreaterOrEqual(text.characterSize*100*6.4f/l.Unit,12.9f);var bounds=text.GetComponent<Renderer>().bounds;Assert.IsTrue(l.Safe.Contains(l.WorldToPixel(bounds.min)),name);Assert.IsTrue(l.Safe.Contains(l.WorldToPixel(bounds.max)),name);}
                Capture("portrait-unified-"+size.x+"x"+size.y);
            }
            Viewport(960,540,new Rect(32,12,906,516));yield return null;Assert.IsNull(GameObject.Find("PortraitCentralBar"));CheckPieces();Assert.AreSame(state,loop.Session);Capture("portrait-landscape-regression");
        }
        [Serializable] private sealed class StackMetric
        {public int width,height,count;public float board_width,board_height,lane_width,sprite_footprint,min_renderer_clearance;}
        [Serializable] private sealed class StackMetrics {public List<StackMetric> rows=new List<StackMetric>();}
        private float CheckPortraitCells()
        {
            CheckPieces();float minimum=float.MaxValue;var l=view.CurrentLayout;
            foreach(var c in view.Checkers)
            {
                Rect cell=c.Point==-1?l.BarPocket(c.Player):c.Point==24?l.OffPocket(c.Player):l.PointVisualRect(c.Point);
                var min=l.WorldToPixel(c.Renderer.bounds.min);var max=l.WorldToPixel(c.Renderer.bounds.max);
                float clear=Mathf.Min(min.x-cell.xMin,cell.xMax-max.x,min.y-cell.yMin,cell.yMax-max.y);Assert.Greater(clear,0,"Checker touches its cell/frame boundary: "+c.Renderer.name);minimum=Mathf.Min(minimum,clear);
                foreach(var other in view.Checkers.Where(o=>o.Point!=c.Point))Assert.IsFalse(c.Renderer.bounds.Intersects(other.Renderer.bounds),"Different lanes/rows collide");
            }
            return minimum;
        }
        [UnityTest] public IEnumerator PortraitOneTwoFiveTenFifteenStacksFitRenderedCells()
        {
            yield return Load();var loop=Ready(GameMode.Long,new Dice());input.enabled=false;var metrics=new StackMetrics();
            foreach(var size in new[]{new Vector2Int(320,568),new Vector2Int(390,844),new Vector2Int(540,960),new Vector2Int(720,1600)})
            {
                Viewport(size.x,size.y,new Rect(0,20,size.x,size.y-56));yield return null;
                foreach(int count in new[]{1,2,5,10,15})
                {
                    var points=new int[24];points[0]=count;points[1]=15-count;points[12]=-count;points[13]=-(15-count);
                    var position=new LongPosition(points,new int[2],new[]{3,3},0,LongPhase.Moving,new[]{false,false},new int[2]);
                    view.Display(BoardSnapshot.From(position));view.SetTurnOverlay(loop,"");yield return null;float clearance=CheckPortraitCells();
                    metrics.rows.Add(new StackMetric{width=size.x,height=size.y,count=count,board_width=view.CurrentLayout.Board.width,board_height=view.CurrentLayout.Board.height,lane_width=view.CurrentLayout.PortraitLaneWidth,sprite_footprint=view.CurrentLayout.CheckerFootprint(0),min_renderer_clearance=clearance});
                    foreach(int point in new[]{0,12})Assert.AreEqual(count,view.Checkers.Where(c=>c.Point==point).Select(c=>(Vector2)c.Renderer.transform.localPosition).Distinct().Count());
                    if(size.x==320||size.x==540||count==15)Capture("portrait-stack-"+count+"-"+size.x+"x"+size.y);
                }
            }
            Viewport(540,960);yield return null;var shortLoop=Ready(GameMode.Short,new Dice());input.enabled=false;
            foreach(bool bar in new[]{true,false})
            {
                var points=new int[24];if(!bar)points[12]=-15;
                var position=new ShortPosition(points,bar?new[]{15,15}:new int[2],bar?new int[2]:new[]{15,0},new[]{6,1},0,bar?ShortPhase.Moving:ShortPhase.Finished);
                view.Display(BoardSnapshot.From(position));view.SetTurnOverlay(shortLoop,"");yield return null;CheckPortraitCells();Capture("portrait-stack-15-"+(bar?"bar":"off"));
            }
            string dir=Environment.GetEnvironmentVariable("NARD_BOARD_CAPTURE_DIR");if(!string.IsNullOrEmpty(dir))File.WriteAllText(System.IO.Path.Combine(dir,"portrait-measurements.json"),JsonUtility.ToJson(metrics,true));
        }
        [UnityTest] public IEnumerator FiveAspectSafeAreaRendersKeepTheSameMatch()
        {
            yield return Load();var dice=new Dice();var loop=Ready(GameMode.Short,dice);var session=loop.Session;var position=loop.ShortPosition;int i=0;
            foreach(var size in new[]{new Vector2Int(960,540),new Vector2Int(540,960),new Vector2Int(1280,720),new Vector2Int(720,1280),new Vector2Int(1024,768)})
            {Rect safe=size.y>size.x?new Rect(0,24,size.x,size.y-58):new Rect(32,12,size.x-54,size.y-24);Viewport(size.x,size.y,safe);yield return null;Assert.AreSame(session,loop.Session);Assert.AreSame(position,loop.ShortPosition);Assert.AreEqual(0,dice.Calls);CheckPieces();Capture("layout-"+size.x+"x"+size.y);i++;}Assert.AreEqual(5,i);
        }
        [UnityTest] public IEnumerator MouseUndoResizeAndDragContinueTheExistingTurn()
        {
            yield return Load();var dice=new Dice();var loop=Ready(GameMode.Long,dice);var root=loop.Session;Viewport(960,540);var mouse=InputSystem.AddDevice<Mouse>();
            try{
                yield return Click(mouse,P(3.75f,-4));yield return Click(mouse,P(1.8f,-2));Assert.AreEqual(1,loop.LongPosition.UsedMask);
                Viewport(540,960);yield return null;Assert.AreSame(root,loop.Session);Assert.IsTrue(loop.CanUndo);Capture("layout-portrait-moving");yield return Click(mouse,P(0,9.45f));Assert.AreEqual(0,loop.LongPosition.UsedMask);
                yield return Mouse(mouse,P(3.75f,-4),true);yield return Mouse(mouse,P(1.8f,-2),true);Assert.NotNull(GameObject.Find("DragPreview"));Viewport(960,540);yield return null;Assert.IsNull(GameObject.Find("DragPreview"));yield return Mouse(mouse,P(1.8f,-2),false);Assert.AreEqual(0,loop.LongPosition.UsedMask);
                yield return Mouse(mouse,P(3.75f,-4),true);yield return Mouse(mouse,P(1.8f,-2),true);yield return Mouse(mouse,P(1.8f,-2),false);Assert.AreEqual(1,loop.LongPosition.UsedMask);Assert.AreEqual(2,dice.Calls);
                foreach(var m in Path(loop)){loop.Tap(0,m.Source);Assert.AreEqual(TapOutcome.Moved,loop.Tap(0,m.Target));}input.Render();Assert.AreEqual(1,loop.Session.Player);Assert.IsFalse(loop.CanUndo);var committed=loop.Session;Viewport(540,960);yield return null;Assert.AreSame(committed,loop.Session);Assert.IsFalse(loop.TryUndo(0));Capture("layout-portrait-next-turn");CheckPieces();
            }finally{InputSystem.RemoveDevice(mouse);}
        }
        [UnityTest] public IEnumerator TouchWorksInBothLayoutsAndMultiTouchCannotSpendRights()
        {
            yield return Load();var dice=new Dice();var loop=Ready(GameMode.Long,dice);var touch=InputSystem.AddDevice<Touchscreen>();int id=0;
            try{
                foreach(bool portrait in new[]{false,true})
                {Viewport(portrait?540:960,portrait?960:540);yield return null;int finger=++id;yield return Touch(touch,finger,UnityEngine.InputSystem.TouchPhase.Began,P(3.75f,-4));yield return Touch(touch,finger,UnityEngine.InputSystem.TouchPhase.Moved,P(1.8f,-2));yield return Touch(touch,finger,UnityEngine.InputSystem.TouchPhase.Ended,P(1.8f,-2));Assert.AreEqual(1,loop.LongPosition.UsedMask);Assert.IsTrue(loop.TryUndo(0));input.Render();}
                yield return Touch(touch,++id,UnityEngine.InputSystem.TouchPhase.Began,P(3.75f,-4));yield return Touch(touch,++id,UnityEngine.InputSystem.TouchPhase.Began,P(1.8f,-2));yield return Touch(touch,id-1,UnityEngine.InputSystem.TouchPhase.Ended,P(1.8f,-2));yield return Touch(touch,id,UnityEngine.InputSystem.TouchPhase.Ended,P(1.8f,-2));Assert.AreEqual(0,loop.LongPosition.UsedMask);Assert.AreEqual(2,dice.Calls);CheckPieces();
            }finally{InputSystem.RemoveDevice(touch);}
        }
        [UnityTest] public IEnumerator EdgeAndControlDropsDoNotActAsButtons()
        {
            yield return Load();var loop=Ready(GameMode.Long,new Dice());Viewport(960,540,new Rect(40,15,880,510));var mouse=InputSystem.AddDevice<Mouse>();
            try{foreach(bool outside in new[]{false,true}){input.Bind(loop);yield return Mouse(mouse,P(3.75f,-4),true);var end=outside?new Vector2(-50,-50):P(0,9.45f);yield return Mouse(mouse,end,true);yield return Mouse(mouse,end,false);Assert.AreEqual(0,loop.LongPosition.UsedMask);Assert.IsNull(GameObject.Find("DragPreview"));}CheckPieces();}
            finally{InputSystem.RemoveDevice(mouse);}
        }
        [UnityTest] public IEnumerator BarOffAndFinishedMatchUseSmallPocketsAndSurviveRotation()
        {
            yield return Load();var dice=new Dice(2025);var loop=new LocalTurnLoop(MatchSession.Create("layout-finish",GameMode.Short,RuleContract.VersionFor(GameMode.Short)),dice);bool bar=false,off=false;int safety=2000;
            while(loop.Session.Phase!=MatchPhase.Finished&&safety-->0)
            {
                if(loop.CanRoll)loop.TryRoll(loop.Session.Player);
                if(loop.Session.Phase==MatchPhase.Moving){int actor=loop.Session.Player;foreach(var m in Path(loop)){loop.Tap(actor,m.Source);loop.Tap(actor,m.Target);}}
                if(!bar&&loop.ShortPosition.CopyBar().Sum()>0){bar=true;input.Bind(loop);Viewport(960,540);yield return null;CheckPieces();Capture("layout-short-bar");}
                if(!off&&loop.ShortPosition.CopyOff().Sum()>0){off=true;input.Bind(loop);Viewport(540,960);yield return null;CheckPieces();Capture("layout-short-off");}
            }
            Assert.Greater(safety,0);Assert.IsTrue(bar);Assert.IsTrue(off);input.Bind(loop);var ended=loop.Session;int calls=dice.Calls;Viewport(960,540);yield return null;Viewport(540,960);yield return null;Assert.AreSame(ended,loop.Session);Assert.AreEqual(calls,dice.Calls);Assert.IsFalse(loop.CanRoll);Assert.IsFalse(loop.CanUndo);CheckPieces();
        }
    }
}
