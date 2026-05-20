using UnityEngine;

namespace CoreRacer.Services.Haptics
{
    public enum HapticType { Light, Medium, Heavy, Success, Warning }

    public sealed class HapticsService
    {
        private readonly bool _enabled;

        public HapticsService(bool enabled = true)
        {
            _enabled = enabled;
        }

        public void Play(HapticType type)
        {
            if (!_enabled) return;
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }
    }
}
