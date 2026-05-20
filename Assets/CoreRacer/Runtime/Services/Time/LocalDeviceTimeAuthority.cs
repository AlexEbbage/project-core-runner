using System;

namespace CoreRacer.Services.Time
{
    public sealed class LocalDeviceTimeAuthority : ITimeAuthority
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
        public bool IsTrusted => false;
        public string Source => "local_device";
    }
}
