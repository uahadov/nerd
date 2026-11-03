using Nard.Bootstrap;
using Nard.Build;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Nard.Tests
{
    public sealed class FoundationTests
    {
        [Test]
        public void EditorVersionIsPinned()
        {
            Assert.That(Application.unityVersion, Is.EqualTo("6000.6.7f1"));
        }

        [Test]
        public void OnlyBootstrapIsEnabledForBuild()
        {
            Assert.That(EditorBuildSettings.scenes, Has.Length.EqualTo(1));
            Assert.That(EditorBuildSettings.scenes[0].enabled, Is.True);
            Assert.That(EditorBuildSettings.scenes[0].path, Is.EqualTo(P003Build.ScenePath));
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(P003Build.ScenePath), Is.Not.Null);
        }

        [Test]
        public void ProjectIs2DAndUsesTextSerialization()
        {
            Assert.That(EditorSettings.defaultBehaviorMode, Is.EqualTo(EditorBehaviorMode.Mode2D));
            Assert.That(EditorSettings.serializationMode, Is.EqualTo(SerializationMode.ForceText));
        }

        [TestCase(1080, 2400, 0, 90, 1080, 2200)]
        [TestCase(2400, 1080, 90, 0, 2200, 1080)]
        [TestCase(720, 1280, -20, -20, 800, 1400)]
        public void SafeAreaStaysWithinScreenAcrossAspectRatios(int width, int height, int x, int y, int w, int h)
        {
            var safe = BootstrapViewport.NormalizeSafeArea(new Rect(x, y, w, h), width, height);
            Assert.That(safe.xMin, Is.InRange(0f, 1f));
            Assert.That(safe.yMin, Is.InRange(0f, 1f));
            Assert.That(safe.xMax, Is.InRange(safe.xMin, 1f));
            Assert.That(safe.yMax, Is.InRange(safe.yMin, 1f));
        }

        [Test]
        public void ZeroSizeDuringStartupIsSafe()
        {
            Assert.That(BootstrapViewport.NormalizeSafeArea(Rect.zero, 0, 0), Is.EqualTo(Rect.zero));
        }
    }
}
