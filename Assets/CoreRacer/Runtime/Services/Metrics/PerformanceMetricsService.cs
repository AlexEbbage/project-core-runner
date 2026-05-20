using System;
using System.Collections.Generic;

namespace CoreRacer.Services.Metrics
{
    public sealed class PerformanceMetricsService
    {
        private readonly Queue<float> _frameTimes = new Queue<float>();
        private readonly int _maxSamples;
        private int _poolMisses;
        private int _activeVfx;

        public PerformanceMetricsService(int maxSamples = 600)
        {
            _maxSamples = Math.Max(60, maxSamples);
        }

        public void Tick(float unscaledDeltaTime)
        {
            if (unscaledDeltaTime <= 0f)
                return;

            while (_frameTimes.Count >= _maxSamples)
                _frameTimes.Dequeue();
            _frameTimes.Enqueue(unscaledDeltaTime);
        }

        public void RecordPoolMiss()
        {
            _poolMisses++;
        }

        public void SetActiveVfxCount(int activeVfx)
        {
            _activeVfx = Math.Max(0, activeVfx);
        }

        public PerformanceMetricsSnapshot Snapshot()
        {
            var samples = new List<float>(_frameTimes);
            float average = 0f;
            float onePercentLow = 0f;

            if (samples.Count > 0)
            {
                float total = 0f;
                for (int i = 0; i < samples.Count; i++)
                    total += samples[i];

                average = total > 0f ? samples.Count / total : 0f;
                samples.Sort();
                int index = Math.Min(samples.Count - 1, Math.Max(0, (int)(samples.Count * 0.99f)));
                onePercentLow = samples[index] > 0f ? 1f / samples[index] : 0f;
            }

            return new PerformanceMetricsSnapshot
            {
                AverageFps = average,
                OnePercentLowFps = onePercentLow,
                ManagedMemoryBytes = GC.GetTotalMemory(false),
                PoolMisses = _poolMisses,
                ActiveVfx = _activeVfx,
                UtcTimestamp = DateTimeOffset.UtcNow.ToString("o")
            };
        }
    }
}
