using System;
using CoreRacer.Services.Save;

namespace CoreRacer.Services.Time
{
    public sealed class ClockTamperDetector
    {
        private const string LastSeenKey = "core_racer_last_seen_utc";
        private readonly ISaveStorage _storage;
        private readonly TimeSpan _backwardsTolerance;

        public bool SuspiciousBackwardsClockDetected { get; private set; }

        public ClockTamperDetector(ISaveStorage storage, TimeSpan? backwardsTolerance = null)
        {
            _storage = storage;
            _backwardsTolerance = backwardsTolerance ?? TimeSpan.FromMinutes(10);
        }

        public void CheckAndRecord(DateTimeOffset now)
        {
            if (_storage != null && _storage.Exists(LastSeenKey))
            {
                DateTimeOffset last;
                if (DateTimeOffset.TryParse(_storage.Load(LastSeenKey), out last) && now + _backwardsTolerance < last)
                    SuspiciousBackwardsClockDetected = true;
            }
            _storage?.Save(LastSeenKey, now.ToString("o"));
        }
    }
}
