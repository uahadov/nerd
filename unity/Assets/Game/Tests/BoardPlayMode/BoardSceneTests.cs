using System;
using System.Collections;
using System.IO;
using System.Linq;
using Nard.Match;
using Nard.Presentation;
using Nard.Rules.Short;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Nard.Tests.Board
{
    public sealed class BoardSceneTests
    {
        [UnityTest] public IEnumerator TechnicalSceneProjectsBothModesAndCapturesRealCameraFrames()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Game/Scenes/TechnicalBoard.unity",new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("TechnicalBoard",LoadSceneMode.Single);
#endif
            yield return null;
            var view=UnityEngine.Object.FindAnyObjectByType<BoardView>();Assert.NotNull(view);
            var camera=Camera.main;Assert.NotNull(camera);Assert.IsTrue(camera.orthographic);
            for(int variant=0;variant<3;variant++)
            {
                BoardSnapshot frame;
                if(variant==2)
                {
                    var board=new int[24];board[3]=10;board[18]=-12;
                    frame=BoardSnapshot.From(new ShortPosition(board,new[]{2,1},new[]{3,2},new[]{3,4},0,ShortPhase.Moving));
                }
                else {var mode=variant==0?GameMode.Long:GameMode.Short;frame=BoardSnapshot.From(MatchSession.Create("scene",mode,RuleContract.VersionFor(mode)));}
                view.Display(frame);yield return null;yield return null;
                Assert.AreEqual(30,view.Checkers.Count);Assert.AreEqual(24,view.Points.Count);
                Assert.AreEqual(30,view.GetComponentsInChildren<SpriteRenderer>().Count(r=>r.name.StartsWith("Checker_")));
                foreach(var c in view.Checkers)
                {
                    var b=c.Renderer.bounds;foreach(var corner in new[]{b.min,b.max})
                    {var v=camera.WorldToViewportPoint(corner);Assert.That(v.x,Is.InRange(0f,1f));Assert.That(v.y,Is.InRange(0f,1f));Assert.Greater(v.z,0);}
                }
                string dir=Environment.GetEnvironmentVariable("NARD_BOARD_CAPTURE_DIR");
                if(!string.IsNullOrEmpty(dir))Capture(camera,Path.Combine(dir,variant==0?"long.png":variant==1?"short.png":"bar-off.png"));
            }
        }
        private static void Capture(Camera camera,string path)
        {
            var rt=new RenderTexture(540,960,24);var image=new Texture2D(540,960,TextureFormat.RGB24,false);
            var previous=RenderTexture.active;var previousTarget=camera.targetTexture;
            try
            {
                camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
                image.ReadPixels(new Rect(0,0,540,960),0,0);image.Apply();
                var pixels=image.GetPixels32();Assert.Greater(pixels.Count(c=>c.r>160&&c.r>c.g*1.15f),1000,"Terracotta points must actually render");
                Assert.Greater(pixels.Count(c=>c.g>100&&c.g>c.r*1.1f),1000,"Turquoise points must actually render");
                Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllBytes(path,image.EncodeToPNG());
            }
            finally{camera.targetTexture=previousTarget;RenderTexture.active=previous;rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(image);}
        }
    }
}
