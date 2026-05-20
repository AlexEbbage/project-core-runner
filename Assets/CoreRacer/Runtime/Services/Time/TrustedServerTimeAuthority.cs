using System;

namespace CoreRacer.Services.Time
{
    /// <summary>
    /// Placeholder seam for future backend/remote-config trusted time. For MVP it falls back to local device time.
    /// </summary>
    public sealed class TrustedServerTimeAuthority : ITimeAuthority
    {
        private readonly CachedTrustedTime _cached;
        public TrustedServerTimeAuthority(CachedTrustedTime cached = null) { _cached = cached; }

        public DateTimeOffset UtcNow
        {
            get
            {
                DateTimeOffset estimated;
                return _cached != null && _cached.TryGetEstimatedUtc(out estimated) ? estimated : DateTimeOffset.UtcNow;
            }
        }

        public bool IsTrusted => _cached != null && !string.IsNullOrWhiteSpace(_cached.TrustedUtcIso);
        public string Source => IsTrusted ? _cached.Source : "local_fallback";
    }
}
