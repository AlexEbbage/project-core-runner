using System;

namespace CoreRacer.Common.Time
{
    public interface IGameClock
    {
        float DeltaTime { get; }
        float UnscaledDeltaTime { get; }
        float TimeScale { get; set; }
        DateTimeOffset UtcNow { get; }
    }
}
