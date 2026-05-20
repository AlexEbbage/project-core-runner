using System;

namespace CoreRacer.Services.Time
{
    public interface ITimeAuthority
    {
        DateTimeOffset UtcNow { get; }
        bool IsTrusted { get; }
        string Source { get; }
    }
}
