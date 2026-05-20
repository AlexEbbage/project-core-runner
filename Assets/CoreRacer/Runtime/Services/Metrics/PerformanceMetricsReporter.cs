using CoreRacer.Bootstrap;
using UnityEngine;

namespace CoreRacer.Services.Metrics
{
    public sealed class PerformanceMetricsReporter : MonoBehaviour
    {
        [SerializeField] private float logIntervalSeconds = 30f;

        private PerformanceMetricsService _metrics;
        private float _timer;

        private void Start()
        {
            GameServices.TryGet(out _metrics);
        }

        private void Update()
        {
            if (_metrics == null)
                return;

            _metrics.Tick(UnityEngine.Time.unscaledDeltaTime);
            _timer += UnityEngine.Time.unscaledDeltaTime;
            if (_timer >= logIntervalSeconds)
            {
                _timer = 0f;
                var snapshot = _metrics.Snapshot();
                Debug.Log($"[CoreRacer:Metrics] avg_fps={snapshot.AverageFps:0.0}, p1_low={snapshot.OnePercentLowFps:0.0}, memory_mb={snapshot.ManagedMemoryBytes / (1024f * 1024f):0.0}");
            }
        }
    }
}
