using System.Collections;
using Nard.Bootstrap;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Nard.Tests
{
    public sealed class BootstrapSmokeTests
    {
        [UnityTest]
        public IEnumerator BootstrapLoadsAndContinuesAdvancingFrames()
        {
            yield return SceneManager.LoadSceneAsync("Bootstrap", LoadSceneMode.Single);
            yield return null;
            var camera = Camera.main;
            Assert.That(camera, Is.Not.Null);
            Assert.That(camera.orthographic, Is.True);
            Assert.That(camera.GetComponent<BootstrapViewport>(), Is.Not.Null);
            int frame = Time.frameCount;
            yield return null;
            yield return null;
            Assert.That(Time.frameCount, Is.GreaterThan(frame));
            var safe = camera.GetComponent<BootstrapViewport>().NormalizedSafeArea;
            Assert.That(safe.width, Is.GreaterThan(0f));
            Assert.That(safe.height, Is.GreaterThan(0f));
        }
    }
}
