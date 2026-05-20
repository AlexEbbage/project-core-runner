using System;

namespace CoreRacer.Services.Time
{
    [Serializable]
    public sealed class CachedTrustedTime
    {
        public string TrustedUtcIso;
        public string CapturedDeviceUtcIso;
        public string Source;

        public bool TryGetEstimatedUtc(out DateTimeOffset value)
        {
            value = default;
            DateTimeOffset trusted;
            DateTimeOffset captured;
            if (!DateTimeOffset.TryParse(TrustedUtcIso, out trusted) || !DateTimeOffset.TryParse(CapturedDeviceUtcIso, out captured))
                return false;

            var elapsed = DateTimeOffset.UtcNow - captured;
            value = trusted + elapsed;
            return true;
        }
    }
}
