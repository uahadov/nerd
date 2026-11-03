using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Nard.Build
{
    // Explicit local preview. SessionState preserves the request across Play mode's domain reload.
    public static class P003Preview
    {
        private const string Requested = "Nard.P003.PreviewRequested";
        private static double deadline;
        private static double beginAfter;
        private static int startFrame = -1;
        private static bool captureRequested;
        private static string capturePath;

        public static void Open()
        {
            EditorSceneManager.OpenScene(P003Build.ScenePath);
            SessionState.SetBool(Requested, true);
            // Search's first-use index must initialize before entering Play mode.
            // Starting Play synchronously can make its startup initialization skip.
            beginAfter = EditorApplication.timeSinceStartup + 3;
            EditorApplication.update += BeginAfterStartup;
        }

        private static void BeginAfterStartup()
        {
            if (EditorApplication.timeSinceStartup < beginAfter || EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            EditorApplication.update -= BeginAfterStartup;
            EditorApplication.isPlaying = true;
        }

        [InitializeOnLoadMethod]
        private static void ResumeRequestedPreview()
        {
            string closeRequest = Path.GetFullPath(Path.Combine(Application.dataPath, "../../build/P00.3/preview-close.request"));
            if (File.Exists(closeRequest))
            {
                File.Delete(closeRequest); // Consume only our explicit local control request.
                EditorApplication.delayCall += () => EditorApplication.Exit(0);
                return;
            }
            if (!SessionState.GetBool(Requested, false)) return;
            capturePath = Path.GetFullPath(Path.Combine(Application.dataPath, "../../build/P00.3/editor-gameview.png"));
            deadline = EditorApplication.timeSinceStartup + 120;
            EditorApplication.update += Observe;
        }

        private static void Observe()
        {
            if (EditorApplication.timeSinceStartup > deadline)
            {
                Debug.LogError("NARD_P003_PREVIEW_TIMEOUT");
                SessionState.SetBool(Requested, false);
                EditorApplication.update -= Observe;
                return;
            }
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
            if (startFrame < 0)
            {
                // Only this explicit Editor session; Android PlayerSettings are unaffected.
                Application.runInBackground = true;
                EditorApplication.ExecuteMenuItem("Window/General/Game");
                startFrame = Time.frameCount;
            }
            if (!captureRequested && Time.frameCount - startFrame >= 20)
            {
                ScreenCapture.CaptureScreenshot(capturePath);
                captureRequested = true;
            }
            if (captureRequested && File.Exists(capturePath) && Time.frameCount - startFrame >= 25)
            {
                Debug.Log("NARD_P003_GUI_PREVIEW_CAPTURED frames=" + (Time.frameCount - startFrame));
                SessionState.SetBool(Requested, false);
                EditorApplication.update -= Observe;
                EditorApplication.isPlaying = false;
            }
        }
    }
}
