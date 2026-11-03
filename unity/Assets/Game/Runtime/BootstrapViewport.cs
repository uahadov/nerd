using UnityEngine;
using UnityEngine.InputSystem;

namespace Nard.Bootstrap
{
    // Display/input foundation only. No board, match state or gameplay.
    [DisallowMultipleComponent]
    public sealed class BootstrapViewport : MonoBehaviour
    {
        public Rect NormalizedSafeArea { get; private set; }
        public bool PointerPressedThisFrame { get; private set; }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Debug.Log("NARD_P003_BOOTSTRAP_READY");
        }

        private void Update()
        {
            NormalizedSafeArea = NormalizeSafeArea(Screen.safeArea, Screen.width, Screen.height);
            PointerPressedThisFrame = Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
        }

        public static Rect NormalizeSafeArea(Rect pixels, int width, int height)
        {
            if (width <= 0 || height <= 0) return Rect.zero;
            float left = Mathf.Clamp01(pixels.xMin / width);
            float bottom = Mathf.Clamp01(pixels.yMin / height);
            float right = Mathf.Max(left, Mathf.Clamp01(pixels.xMax / width));
            float top = Mathf.Max(bottom, Mathf.Clamp01(pixels.yMax / height));
            return Rect.MinMaxRect(left, bottom, right, top);
        }

        private void OnApplicationPause(bool paused)
        {
            Debug.Log(paused ? "NARD_P003_PAUSED" : "NARD_P003_RESUMED");
        }
    }
}
