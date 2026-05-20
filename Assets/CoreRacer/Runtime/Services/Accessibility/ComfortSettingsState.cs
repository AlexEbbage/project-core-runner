using System;

namespace CoreRacer.Services.Accessibility
{
    [Serializable]
    public sealed class ComfortSettingsState
    {
        public float ScreenShakeIntensity = 1f;
        public float FlashIntensity = 1f;
        public bool MotionBlurEnabled = true;
        public bool SpeedLinesEnabled = true;
        public bool ReducedVfxMode;
        public bool HighContrastMode;
        public bool ColorBlindFriendlyPalette;
        public bool HapticsEnabled = true;
        public float HapticsStrength = 1f;
        public float InputSensitivity = 1f;
        public bool DragControlsEnabled;
        public bool LowEndPerformanceMode;
    }
}
