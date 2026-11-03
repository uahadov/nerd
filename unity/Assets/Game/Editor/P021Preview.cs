using System;
using System.IO;
using Nard.Match;
using Nard.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Nard.Build
{
    /// <summary>Explicit disposable Editor preview only; never runs in the user's primary Editor automatically.</summary>
    public static class P021Preview
    {
        private const string Request="Nard.P021.Preview";
        private static double startAt,deadline;
        private static int firstFrame=-1;
        private static bool captured;
        private static string capturePath;
        [MenuItem("NARD/P02.1/Open technical board")]
        public static void OpenBoard()=>EditorSceneManager.OpenScene(P021Build.ScenePath);
        public static void Capture()
        {
            OpenBoard();SessionState.SetBool(Request,true);startAt=EditorApplication.timeSinceStartup+3;
            EditorApplication.update+=Start;
        }
        private static void Start()
        {
            if(EditorApplication.timeSinceStartup<startAt||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            EditorApplication.update-=Start;EditorApplication.isPlaying=true;
        }
        [InitializeOnLoadMethod] private static void Resume()
        {
            if(!SessionState.GetBool(Request,false))return;
            capturePath=Environment.GetEnvironmentVariable("NARD_GUI_CAPTURE");
            deadline=EditorApplication.timeSinceStartup+120;EditorApplication.update+=Observe;
        }
        private static void Observe()
        {
            if(EditorApplication.timeSinceStartup>deadline){Debug.LogError("NARD_P021_PREVIEW_TIMEOUT");SessionState.SetBool(Request,false);EditorApplication.Exit(1);return;}
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
            if(firstFrame<0)
            {
                Application.runInBackground=true;
                EditorApplication.ExecuteMenuItem("Window/General/Game");
                var view=UnityEngine.Object.FindAnyObjectByType<BoardView>();if(view==null)return;
                var mode=Environment.GetEnvironmentVariable("NARD_GUI_MODE")=="short"?GameMode.Short:GameMode.Long;
                view.Display(BoardSnapshot.From(MatchSession.Create("gui-preview",mode,RuleContract.VersionFor(mode))));
                firstFrame=Time.frameCount;
            }
            if(!captured&&Time.frameCount-firstFrame>=20)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(capturePath));ScreenCapture.CaptureScreenshot(capturePath);captured=true;
            }
            if(captured&&File.Exists(capturePath)&&Time.frameCount-firstFrame>=25)
            {
                Debug.Log("NARD_P021_EDITOR_GAMEVIEW_CAPTURED frames="+(Time.frameCount-firstFrame));
                SessionState.SetBool(Request,false);EditorApplication.update-=Observe;EditorApplication.Exit(0);
            }
        }
    }
}
