using System;

namespace CoreRacer.Services.Metrics
{
    [Serializable]
    public sealed class MetricSample
    {
        public string Name;
        public double Value;
        public string Unit;
        public string UtcTimestamp;

        public MetricSample() { }

        public MetricSample(string name, double value, string unit)
        {
            Name = name;
            Value = value;
            Unit = unit;
            UtcTimestamp = DateTimeOffset.UtcNow.ToString("o");
        }
    }
}
