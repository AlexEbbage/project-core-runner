using CoreRacer.Services.Metrics;
using UnityEngine;

namespace CoreRacer.Debugging
{
    public sealed class DevicePerformanceOverlay : MonoBehaviour
    {
        [SerializeField] private bool visible = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.F10;
        private PerformanceMetricsService _metrics;
        private DevicePerformanceProfile _profile;

        public void Initialize(PerformanceMetricsService metrics)
        {
            _metrics = metrics;
            _profile = DevicePerformanceProfile.Capture();
        }

        private void Awake()
        {
            _profile = DevicePerformanceProfile.Capture();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey)) visible = !visible;
        }

        private void OnGUI()
        {
            if (!visible) return;
            var fps = 1f / Mathf.Max(UnityEngine.Time.unscaledDeltaTime, 0.0001f);
            GUILayout.BeginArea(new Rect(12, 12, 360, 180), GUI.skin.box);
            GUILayout.Label("Core Racer Performance");
            GUILayout.Label($"FPS approx: {fps:0}");
            GUILayout.Label($"Device: {_profile.DeviceModel}");
            GUILayout.Label($"GPU: {_profile.GpuName}");
            GUILayout.Label($"RAM: {_profile.SystemMemoryMb} MB | VRAM: {_profile.GraphicsMemoryMb} MB");
            GUILayout.Label($"CPU cores: {_profile.ProcessorCount}");
            GUILayout.EndArea();
        }
    }
}
