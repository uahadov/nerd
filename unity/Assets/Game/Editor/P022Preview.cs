using System;
using System.IO;
using Nard.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Nard.Build
{
    public static class P022Preview
    {
        private const string Request="Nard.P022.Preview";
        private static double beginAfter,deadline;
        private static int startFrame=-1,stage;
        private static Mouse mouse;
        private static InputSettings previousSettings;
        private static string dir;
        [MenuItem("NARD/P02.2/Open tap example")]
        public static void Open()=>EditorSceneManager.OpenScene(P022Build.ScenePath);
        public static void Capture()
        {
            Open();SessionState.SetBool(Request,true);beginAfter=EditorApplication.timeSinceStartup+3;
            EditorApplication.update+=Begin;
        }
        private static void Begin()
        {
            if(EditorApplication.timeSinceStartup<beginAfter||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            EditorApplication.update-=Begin;EditorApplication.isPlaying=true;
        }
        [InitializeOnLoadMethod] private static void Resume()
        {
            if(!SessionState.GetBool(Request,false))return;
            dir=Environment.GetEnvironmentVariable("NARD_GUI_CAPTURE_DIR");deadline=EditorApplication.timeSinceStartup+120;
            EditorApplication.update+=Observe;
        }
        private static void Press(float x,float y,bool down)
        {
            var at=(Vector2)Camera.main.WorldToScreenPoint(new Vector3(x,y,0));
            InputSystem.QueueStateEvent(mouse,new MouseState{position=at,buttons=(ushort)(down?1:0)});
        }
        private static void Shot(string name)=>ScreenCapture.CaptureScreenshot(Path.Combine(dir,name+".png"));
        private static void Observe()
        {
            if(EditorApplication.timeSinceStartup>deadline){Debug.LogError("NARD_P022_PREVIEW_TIMEOUT");SessionState.SetBool(Request,false);EditorApplication.Exit(1);return;}
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
            if(startFrame<0)
            {
                Application.runInBackground=true;EditorApplication.ExecuteMenuItem("Window/General/Game");
                if(UnityEngine.Object.FindAnyObjectByType<BoardTapInput>()?.Selection==null)return;
                Directory.CreateDirectory(dir);previousSettings=UnityEngine.Object.Instantiate(InputSystem.settings);
                previousSettings.hideFlags=HideFlags.HideAndDontSave;
                var settings=UnityEngine.Object.Instantiate(previousSettings);settings.hideFlags=HideFlags.HideAndDontSave;
                settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings=settings;
                mouse=InputSystem.AddDevice<Mouse>();startFrame=Time.frameCount;
            }
            int frame=Time.frameCount-startFrame;
            if(stage==0&&frame>=20){Press(3.75f,-6.6f,true);stage++;}
            else if(stage==1&&frame>=22){Press(3.75f,-6.6f,false);stage++;}
            else if(stage==2&&frame>=25){Shot("editor-selected");stage++;}
            else if(stage==3&&frame>=28){Press(-4.25f,4,true);stage++;}
            else if(stage==4&&frame>=30){Press(-4.25f,4,false);stage++;}
            else if(stage==5&&frame>=33){Shot("editor-rejected");stage++;}
            else if(stage==6&&frame>=36){Press(1.8f,-1.65f,true);stage++;}
            else if(stage==7&&frame>=38){Press(1.8f,-1.65f,false);stage++;}
            else if(stage==8&&frame>=41){Shot("editor-moved");stage++;}
            else if(stage==9&&frame>=46&&File.Exists(Path.Combine(dir,"editor-moved.png")))
            {
                var input=UnityEngine.Object.FindAnyObjectByType<BoardTapInput>();
                if(input.Selection.Prefix.Count!=1){Debug.LogError("NARD_P022_GUI_INPUT_FAILED");EditorApplication.Exit(1);return;}
                InputSystem.RemoveDevice(mouse);var settings=InputSystem.settings;InputSystem.settings=previousSettings;if(settings!=null)UnityEngine.Object.Destroy(settings);
                Debug.Log("NARD_P022_GUI_SELECTED_REJECTED_MOVED_CAPTURED");SessionState.SetBool(Request,false);
                EditorApplication.update-=Observe;EditorApplication.Exit(0);
            }
        }
    }
}
