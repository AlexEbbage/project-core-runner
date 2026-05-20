using System;
using UnityEngine;

namespace CoreRacer.Common.Time
{
    public sealed class UnityGameClock : IGameClock
    {
        public float DeltaTime => UnityEngine.Time.deltaTime;
        public float UnscaledDeltaTime => UnityEngine.Time.unscaledDeltaTime;
        public float TimeScale
        {
            get => UnityEngine.Time.timeScale;
            set => UnityEngine.Time.timeScale = Mathf.Max(0f, value);
        }
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
