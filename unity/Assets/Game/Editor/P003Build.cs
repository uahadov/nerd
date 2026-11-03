using System;
using System.IO;
using Nard.Bootstrap;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nard.Build
{
    public static class P003Build
    {
        public const string ScenePath = "Assets/Game/Scenes/Bootstrap.unity";

        public static void Configure()
        {
            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
            EditorSettings.serializationMode = SerializationMode.ForceText;
            VersionControlSettings.mode = "Visible Meta Files";
            PlayerSettings.companyName = "NARD";
            PlayerSettings.productName = "NƏRD. — P00.3";
            PlayerSettings.bundleVersion = "0.0.3";
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.nard.prototype");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.defaultScreenWidth = 540;
            PlayerSettings.defaultScreenHeight = 960;
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)26;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.useCustomKeystore = false;
            EditorUserBuildSettings.buildAppBundle = false;

            // New project template assets are moved into the shared Game tree.
            Directory.CreateDirectory("Assets/Game/Scenes");
            Directory.CreateDirectory("Assets/Game/Input");
            AssetDatabase.Refresh();
            if (AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions") != null)
                AssetDatabase.MoveAsset("Assets/InputSystem_Actions.inputactions", "Assets/Game/Input/Bootstrap.inputactions");
            string inputPath = "Assets/Game/Input/Bootstrap.inputactions";
            var currentActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(inputPath);
            if (currentActions == null || currentActions.FindActionMap("Pointer", false) == null)
            {
                var actions = ScriptableObject.CreateInstance<InputActionAsset>();
                var pointer = actions.AddActionMap("Pointer");
                pointer.AddAction("Position", InputActionType.PassThrough, "<Pointer>/position");
                pointer.AddAction("Press", InputActionType.Button, "<Pointer>/press");
                File.WriteAllText(inputPath, actions.ToJson());
                UnityEngine.Object.DestroyImmediate(actions);
                AssetDatabase.ImportAsset(inputPath);
            }
            // A single blank orthographic scene is the complete P00.3 presentation.
            if (!File.Exists(ScenePath))
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(BootstrapViewport));
                cameraObject.tag = "MainCamera";
                cameraObject.transform.position = new Vector3(0, 0, -10);
                var camera = cameraObject.GetComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 5;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color32(246, 235, 213, 255);
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("NARD_P003_CONFIGURATION_COMPLETE");
        }

        public static void BuildAndroid()
        {
            Configure();
            string output = Environment.GetEnvironmentVariable("NARD_BUILD_OUTPUT");
            if (string.IsNullOrEmpty(output))
                output = Path.GetFullPath(Path.Combine(Application.dataPath, "../../build/P00.3/NARD-P00.3.apk"));
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = output,
                target = BuildTarget.Android,
                options = BuildOptions.Development
            });
            var summary = report.summary;
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(output), "android-build.json"),
                JsonUtility.ToJson(new BuildSummary
                {
                    result = summary.result.ToString(), errors = summary.totalErrors,
                    warnings = summary.totalWarnings, bytes = summary.totalSize,
                    seconds = summary.totalTime.TotalSeconds
                }, true));
            if (summary.result != BuildResult.Succeeded)
                throw new Exception("Android build failed: " + summary.result);
        }

        [Serializable]
        private sealed class BuildSummary
        {
            public string result;
            public int errors;
            public int warnings;
            public ulong bytes;
            public double seconds;
        }
    }
}
