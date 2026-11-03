using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Nard.Build
{
    public static class P023Build
    {
        public const string ScenePath = "Assets/Game/Scenes/TurnBoard.unity";
        public static void BuildAndroid()
        {
            // Keep the accepted bootstrap scene and project configuration intact.
            string output=Environment.GetEnvironmentVariable("NARD_BUILD_OUTPUT");
            if(string.IsNullOrEmpty(output))throw new ArgumentException("NARD_BUILD_OUTPUT required");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                scenes=new[]{ScenePath,P022Build.ScenePath,P021Build.ScenePath,P003Build.ScenePath},locationPathName=output,
                target=BuildTarget.Android,options=BuildOptions.Development});
            var s=report.summary;
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(output),"android-build.json"),
                JsonUtility.ToJson(new Summary{result=s.result.ToString(),errors=s.totalErrors,warnings=s.totalWarnings,bytes=s.totalSize,seconds=s.totalTime.TotalSeconds},true));
            if(s.result!=BuildResult.Succeeded)throw new Exception("Board build failed: "+s.result);
        }
        [Serializable] private sealed class Summary { public string result;public int errors,warnings;public ulong bytes;public double seconds; }
    }
}
