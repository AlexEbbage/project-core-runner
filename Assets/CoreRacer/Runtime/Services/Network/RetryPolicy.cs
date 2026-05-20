using System;

namespace CoreRacer.Services.Network
{
    [Serializable]
    public sealed class RetryPolicy
    {
        public int MaxAttempts = 3;
        public float InitialDelaySeconds = 1f;
        public float BackoffMultiplier = 2f;
        public float MaxDelaySeconds = 30f;

        public float GetDelay(int attemptIndex)
        {
            var value = InitialDelaySeconds * (float)Math.Pow(BackoffMultiplier, Math.Max(0, attemptIndex));
            return Math.Min(MaxDelaySeconds, value);
        }
    }
}
