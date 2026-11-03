using System;using System.Collections;using System.IO;using System.Linq;
using Nard.Match;using Nard.Interaction;using Nard.Presentation;using NUnit.Framework;using UnityEngine;using UnityEngine.InputSystem;using UnityEngine.InputSystem.LowLevel;using UnityEngine.SceneManagement;using UnityEngine.TestTools;
namespace Nard.Tests.Undo {public sealed class UndoSceneTests {
 private sealed class Dice:IDiceSource{public int NextDie()=>3;}
        private InputSettings previous;
        private IEnumerator Load()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Game/Scenes/UndoBoard.unity",new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("UndoBoard",LoadSceneMode.Single);
#endif
            yield return null;yield return null;
            previous=UnityEngine.Object.Instantiate(InputSystem.settings);previous.hideFlags=HideFlags.HideAndDontSave;
            var settings=UnityEngine.Object.Instantiate(previous);settings.hideFlags=HideFlags.HideAndDontSave;
            settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings=settings;
        }
        [TearDown] public void Restore(){var current=InputSystem.settings;InputSystem.settings=previous;if(current!=null)UnityEngine.Object.Destroy(current);}

 private static Vector2 P(float x,float y)=>Camera.main.WorldToScreenPoint(UnityEngine.Object.FindAnyObjectByType<BoardView>().BoardToLocal(new Vector2(x,y)));
 private static IEnumerator MouseEvent(Mouse m,Vector2 p,int button){InputSystem.QueueStateEvent(m,new MouseState{position=p,buttons=(ushort)button});yield return null;}

 private static void Capture(string name)
 {
    string dir=Environment.GetEnvironmentVariable("NARD_BOARD_CAPTURE_DIR");if(string.IsNullOrEmpty(dir))return;
    var camera=Camera.main;var rt=new RenderTexture(1440,1080,24);var image=new Texture2D(1440,1080,TextureFormat.RGB24,false);var active=RenderTexture.active;var target=camera.targetTexture;
    try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1440,1080),0,0);image.Apply();Directory.CreateDirectory(dir);File.WriteAllBytes(System.IO.Path.Combine(dir,name+".png"),image.EncodeToPNG());}
    finally{camera.targetTexture=target;RenderTexture.active=active;rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(image);}
 }
 private static LocalTurnLoop Ready(GameMode mode)
 {var p=MatchSession.Create("undo-gesture",mode,RuleContract.VersionFor(mode)).Begin().ResolveOpening(6,1);var loop=new LocalTurnLoop(p,new Dice());if(loop.CanRoll)loop.TryRoll(0);return loop;}
 private static IEnumerator Touch(Touchscreen t,int id,UnityEngine.InputSystem.TouchPhase phase,Vector2 p)
 {InputSystem.QueueStateEvent(t,new TouchState{touchId=id,phase=phase,position=p});yield return null;}
 [UnityTest] public IEnumerator TouchDragLong()=>TouchDrag(GameMode.Long);
 [UnityTest] public IEnumerator TouchDragShort()=>TouchDrag(GameMode.Short);
 private IEnumerator TouchDrag(GameMode mode)
 {
  yield return Load();var input=UnityEngine.Object.FindAnyObjectByType<BoardTurnInput>();input.Bind(Ready(mode));var touch=InputSystem.AddDevice<Touchscreen>();
  try{
   int source=mode==GameMode.Long?0:23;var from=BoardGeometry.PointBase(source);var point=P(from.x,source<12?-4:4);
   yield return Touch(touch,1,UnityEngine.InputSystem.TouchPhase.Began,point);Assert.AreEqual(source,input.Loop.Selected);int target=input.Loop.Targets[0];var to=BoardGeometry.PointBase(target);var end=P(to.x,target<12?-2:2);
   yield return Touch(touch,1,UnityEngine.InputSystem.TouchPhase.Moved,end);Assert.NotNull(GameObject.Find("DragPreview"));Assert.AreEqual(0,input.Loop.Tokens.Count(t=>t.State==TokenState.Used));Capture(mode==GameMode.Long?"undo-drag-long":"undo-drag-short");
   yield return Touch(touch,1,UnityEngine.InputSystem.TouchPhase.Ended,end);Assert.AreEqual(1,input.Loop.Tokens.Count(t=>t.State==TokenState.Used));Assert.IsNull(GameObject.Find("DragPreview"));Assert.IsTrue(input.Loop.CanUndo);Capture(mode==GameMode.Long?"undo-moved-long":"undo-moved-short");
   Assert.IsTrue(input.Loop.TryUndo(0));input.Render();Assert.AreEqual(0,input.Loop.Tokens.Count(t=>t.State==TokenState.Used));Capture(mode==GameMode.Long?"undo-restored-long":"undo-restored-short");
  }finally{InputSystem.RemoveDevice(touch);}
 }
 [UnityTest] public IEnumerator CancelMultiTouchBoundaryAndButtonDropCannotSpendRights()
 {
  yield return Load();var input=UnityEngine.Object.FindAnyObjectByType<BoardTurnInput>();var touch=InputSystem.AddDevice<Touchscreen>();
  try{
   foreach(int kind in new[]{0,1,2,3}){
    input.Bind(Ready(GameMode.Long));var source=P(3.75f,-4);yield return Touch(touch,10+kind*2,UnityEngine.InputSystem.TouchPhase.Began,source);
    var end=kind==2?new Vector2(-10,-10):kind==3?P(0,9.45f):P(1.8f,-2);
    yield return Touch(touch,10+kind*2,UnityEngine.InputSystem.TouchPhase.Moved,end);
    if(kind==0)yield return Touch(touch,11+kind*2,UnityEngine.InputSystem.TouchPhase.Began,P(-3,4));
    yield return Touch(touch,10+kind*2,kind==1?UnityEngine.InputSystem.TouchPhase.Canceled:UnityEngine.InputSystem.TouchPhase.Ended,end);
    if(kind==0)yield return Touch(touch,11+kind*2,UnityEngine.InputSystem.TouchPhase.Ended,P(-3,4));
    Assert.AreEqual(15,input.Loop.LongPosition.Point(0));Assert.AreEqual(0,input.Loop.LongPosition.UsedMask);Assert.IsFalse(input.Loop.CanUndo);Assert.IsNull(GameObject.Find("DragPreview"));
   }
  }finally{InputSystem.RemoveDevice(touch);}
 }
 [UnityTest] public IEnumerator SamePointJitterAndAmbiguousQuickTapDoNotInventMoves()
 {
  yield return Load();var input=UnityEngine.Object.FindAnyObjectByType<BoardTurnInput>();var mouse=InputSystem.AddDevice<Mouse>();
  try{
   input.Bind(Ready(GameMode.Long));var start=P(3.75f,-4);yield return MouseEvent(mouse,start,1);yield return MouseEvent(mouse,start+new Vector2(2,2),1);yield return MouseEvent(mouse,start+new Vector2(2,2),0);Assert.AreEqual(0,input.Loop.LongPosition.UsedMask);
   // Short opening 6/1 offers multiple targets; a second tap must not choose one arbitrarily.
   input.Bind(Ready(GameMode.Short));start=P(BoardGeometry.PointBase(23).x,4);yield return MouseEvent(mouse,start,1);yield return MouseEvent(mouse,start,0);Assert.That(input.Loop.Targets.Count,Is.GreaterThan(1));yield return MouseEvent(mouse,start,1);yield return MouseEvent(mouse,start,0);Assert.AreEqual(0,input.Loop.ShortPosition.UsedMask);
  }finally{InputSystem.RemoveDevice(mouse);}
 }
 [UnityTest] public IEnumerator DragUndoQuickMoveAndInvalidDropStayConsistent(){
 yield return Load();var input=UnityEngine.Object.FindAnyObjectByType<BoardTurnInput>();
 var root=MatchSession.Create("undo",GameMode.Long,RuleContract.VersionFor(GameMode.Long)).Begin().ResolveOpening(6,1);
 Assert.IsTrue(input.GetComponent<BoardView>().IsReferenceStyle);
 Assert.NotNull(GameObject.Find("ReferenceBoard"));
 input.Bind(new LocalTurnLoop(root,new Dice()));input.Loop.TryRoll(0);input.Render();var mouse=InputSystem.AddDevice<Mouse>();
 try{
 yield return MouseEvent(mouse,P(3.75f,-4),1);yield return MouseEvent(mouse,P(1.8f,-3),1);yield return MouseEvent(mouse,P(1.8f,-3),0);
 Assert.AreEqual(1,input.Loop.Tokens.Count(t=>t.State==TokenState.Used));Assert.IsTrue(input.Loop.CanUndo);Capture("undo-mouse-moved");
 yield return MouseEvent(mouse,P(0,9.45f),1);yield return MouseEvent(mouse,P(0,9.45f),0);
 Assert.AreEqual(15,input.Loop.LongPosition.Point(0));Assert.AreEqual(0,input.Loop.LongPosition.UsedMask);Assert.IsFalse(input.Loop.CanUndo);
 yield return MouseEvent(mouse,P(3.75f,-4),1);yield return MouseEvent(mouse,P(20,20),1);yield return MouseEvent(mouse,P(20,20),0);
 Assert.AreEqual(15,input.Loop.LongPosition.Point(0));Assert.AreEqual(0,input.Loop.LongPosition.UsedMask);
 // A selected source with one target supports a second short tap.
 yield return MouseEvent(mouse,P(3.75f,-4),1);yield return MouseEvent(mouse,P(3.75f,-4),0);
 Assert.AreEqual(14,input.Loop.LongPosition.Point(0));Assert.IsTrue(input.Loop.TryUndo(0));input.Render();
 Assert.AreEqual(15,input.Loop.LongPosition.Point(0));Assert.AreEqual(30,input.GetComponent<BoardView>().Checkers.Count);
 Capture("undo-restored");
 }finally{InputSystem.RemoveDevice(mouse);}
 }
}}
