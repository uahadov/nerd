using System;
using System.Collections;
using System.IO;
using System.Linq;
using Nard.Interaction;
using Nard.Presentation;
using Nard.Rules.Short;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Nard.Tests.Tap
{
    public sealed class TapSceneTests
    {
        private InputSettings previousSettings;
        private void RouteSyntheticDevicesToTheIsolatedPlayModeTest()
        {
            // InputManager destroys its old HideAndDontSave defaults when settings are replaced.
            previousSettings=UnityEngine.Object.Instantiate(InputSystem.settings);
            previousSettings.hideFlags=HideFlags.HideAndDontSave;
            var isolated=UnityEngine.Object.Instantiate(previousSettings);
            isolated.hideFlags=HideFlags.HideAndDontSave;
            isolated.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            isolated.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings=isolated;
        }
        [TearDown] public void RestoreInputSettings()
        {
            var isolated=InputSystem.settings;InputSystem.settings=previousSettings;
            if(isolated!=null)UnityEngine.Object.Destroy(isolated);
        }
        private IEnumerator Load()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Game/Scenes/TapBoard.unity",new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("TapBoard",LoadSceneMode.Single);
#endif
            yield return null;yield return null;
            RouteSyntheticDevicesToTheIsolatedPlayModeTest();
        }
        private static BoardTapInput Input()=>UnityEngine.Object.FindAnyObjectByType<BoardTapInput>();
        private static Vector2 Screen(float x,float y)=>Camera.main.WorldToScreenPoint(new Vector3(x,y,0));
        private static IEnumerator Click(Mouse mouse,Vector2 at)
        {
            InputSystem.QueueStateEvent(mouse,new MouseState{position=at,buttons=1});yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=at,buttons=0});yield return null;
        }
        [UnityTest] public IEnumerator MousePressMovesOnceAndCapturesFeedback()
        {
            yield return Load();var input=Input();var view=input.GetComponent<BoardView>();var mouse=InputSystem.AddDevice<Mouse>();
            try
            {
                yield return Click(mouse,Screen(3.75f,-6.6f));Assert.AreEqual(0,input.Selection.Selected);Assert.Contains(3,input.Selection.Targets);
                Assert.AreEqual(1,view.GetComponentsInChildren<SpriteRenderer>().Count(r=>r.name=="Target_3"));Capture("tap-selected");
                var before=input.Selection.LongPosition;yield return Click(mouse,Screen(-4.25f,4));
                Assert.AreSame(before,input.Selection.LongPosition);Assert.AreEqual(0,input.Selection.Selected);
                Assert.That(view.GetComponentsInChildren<TextMesh>().Single(t=>t.name=="Unavailable").text,Does.Contain("mümkün deyil"));Capture("tap-rejected");
                yield return Click(mouse,Screen(1.8f,-1.65f));Assert.AreEqual(1,input.Selection.Prefix.Count);
                Assert.AreEqual(14,input.Selection.LongPosition.Point(0));Assert.AreEqual(1,input.Selection.LongPosition.Point(3));
                Assert.AreEqual(30,view.Checkers.Count);Assert.AreEqual(14,view.Checkers.Count(c=>c.Point==0));Assert.AreEqual(1,view.Checkers.Count(c=>c.Point==3));
                Assert.AreEqual(0,view.GetComponentsInChildren<SpriteRenderer>().Count(r=>r.name.StartsWith("Target_")));Capture("tap-moved");
                // Two independent rapid clicks at the same destination select then deselect; they do not repeat a move.
                yield return Click(mouse,Screen(1.8f,-4));yield return Click(mouse,Screen(1.8f,-4));Assert.AreEqual(1,input.Selection.Prefix.Count);
            }
            finally{InputSystem.RemoveDevice(mouse);}
        }
        [UnityTest] public IEnumerator FiveManualStyleMovesAcrossExplicitExampleReset()
        {
            yield return Load();var input=Input();var mouse=InputSystem.AddDevice<Mouse>();
            try
            {
                for(int repeat=0;repeat<5;repeat++)
                {
                    yield return Click(mouse,Screen(0,-9.35f));Assert.AreEqual(0,input.Selection.Prefix.Count);
                    yield return Click(mouse,Screen(3.75f,-6.6f));yield return Click(mouse,Screen(1.8f,-1.65f));Assert.AreEqual(1,input.Selection.Prefix.Count);
                }
                Assert.AreEqual(30,input.GetComponent<BoardView>().Checkers.Count);
            }
            finally{InputSystem.RemoveDevice(mouse);}
        }
        [UnityTest] public IEnumerator DuplicateAndOutOfOrderPressCannotMoveTwice()
        {
            yield return Load();var input=Input();long id=Time.frameCount+100;
            Assert.IsTrue(input.ProcessPress(Screen(3.75f,-6.6f),id));Assert.IsFalse(input.ProcessPress(Screen(1.8f,-1.65f),id));
            Assert.IsTrue(input.ProcessPress(Screen(1.8f,-1.65f),id+1));Assert.IsFalse(input.ProcessPress(Screen(1.8f,-4),id));
            Assert.IsFalse(input.ProcessPress(Screen(1.8f,-4),id+1));Assert.AreEqual(1,input.Selection.Prefix.Count);
        }
        [UnityTest] public IEnumerator TouchPressMovesButDragAndSecondFingerDoNot()
        {
            yield return Load();var input=Input();var touch=InputSystem.AddDevice<Touchscreen>();
            try
            {
                var source=Screen(3.75f,-6.6f);var target=Screen(1.8f,-1.65f);
                InputSystem.QueueStateEvent(touch,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Began,position=source});yield return null;
                Assert.AreEqual(0,input.Selection.Selected);
                InputSystem.QueueStateEvent(touch,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Moved,position=target});yield return null;Assert.AreEqual(0,input.Selection.Prefix.Count);
                InputSystem.QueueStateEvent(touch,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=target});yield return null;
                InputSystem.QueueStateEvent(touch,new TouchState{touchId=2,phase=UnityEngine.InputSystem.TouchPhase.Began,position=target});yield return null;
                Assert.AreEqual(1,input.Selection.Prefix.Count);
                InputSystem.QueueStateEvent(touch,new TouchState{touchId=2,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=target});yield return null;
                input.RestartExample();
                InputSystem.QueueStateEvent(touch,new TouchState{touchId=3,phase=UnityEngine.InputSystem.TouchPhase.Began,position=source});
                InputSystem.QueueStateEvent(touch,new TouchState{touchId=4,phase=UnityEngine.InputSystem.TouchPhase.Began,position=target});yield return null;
                Assert.AreEqual(TapSelection.None,input.Selection.Selected);Assert.AreEqual(0,input.Selection.Prefix.Count);
            }
            finally{InputSystem.RemoveDevice(touch);}
        }
        [UnityTest] public IEnumerator ShortBarHitOffAndWrongOwnerStayInSync()
        {
            yield return Load();var input=Input();var view=input.GetComponent<BoardView>();
            var b=new int[24];b[7]=1;b[5]=14;b[4]=-1;b[0]=-14;
            var p=new ShortPosition(b,new int[2],new int[2],new[]{3,4},0,ShortPhase.Moving);
            input.Bind(new TapSelection(p,1));long id=Time.frameCount+100;
            Assert.IsFalse(input.ProcessPress(Screen(1.15f,-4),id++));Assert.AreSame(p,input.Selection.ShortPosition);
            input.Bind(new TapSelection(p,0));
            var from=BoardGeometry.PointBase(7);var to=BoardGeometry.PointBase(4);
            input.ProcessPress(Screen(from.x,-4),id++);input.ProcessPress(Screen(to.x,-1.65f),id++);
            Assert.AreEqual(1,input.Selection.ShortPosition.Bar(1));Assert.AreEqual(1,view.Checkers.Count(c=>c.Point==-1&&c.Player==1));Capture("tap-short-hit");
            b=new int[24];b[5]=14;b[12]=-15;
            input.Bind(new TapSelection(new ShortPosition(b,new[]{1,0},new int[2],new[]{1,2},0,ShortPhase.Moving),0));
            input.ProcessPress(Screen(-.25f,-4),id++);Assert.Contains(23,input.Selection.Targets);
            to=BoardGeometry.PointBase(23);input.ProcessPress(Screen(to.x,1.65f),id++);Assert.AreEqual(0,input.Selection.ShortPosition.Bar(0));
            b=new int[24];b[0]=1;b[12]=-15;
            input.Bind(new TapSelection(new ShortPosition(b,new int[2],new[]{14,0},new[]{6,5},0,ShortPhase.Moving),0));
            input.ProcessPress(Screen(3.75f,-4),id++);input.ProcessPress(Screen(4.53f,-1.42f),id++);
            Assert.AreEqual(15,input.Selection.ShortPosition.Off(0));Assert.AreEqual(15,view.Checkers.Count(c=>c.Player==0&&c.Point==24));Capture("tap-short-off");
        }
        private static void Capture(string name)
        {
            string dir=Environment.GetEnvironmentVariable("NARD_BOARD_CAPTURE_DIR");if(string.IsNullOrEmpty(dir))return;
            var camera=Camera.main;var rt=new RenderTexture(540,960,24);var image=new Texture2D(540,960,TextureFormat.RGB24,false);
            var active=RenderTexture.active;var target=camera.targetTexture;
            try
            {
                camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,540,960),0,0);image.Apply();
                Directory.CreateDirectory(dir);File.WriteAllBytes(Path.Combine(dir,name+".png"),image.EncodeToPNG());
            }
            finally{camera.targetTexture=target;RenderTexture.active=active;rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(image);}
        }
    }
}
