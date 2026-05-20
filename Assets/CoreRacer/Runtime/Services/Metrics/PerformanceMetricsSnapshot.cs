using System;

namespace CoreRacer.Services.Metrics
{
    [Serializable]
    public sealed class PerformanceMetricsSnapshot
    {
        public float AverageFps;
        public float OnePercentLowFps;
        public long ManagedMemoryBytes;
        public int PoolMisses;
        public int ActiveVfx;
        public string UtcTimestamp;
    }
}
