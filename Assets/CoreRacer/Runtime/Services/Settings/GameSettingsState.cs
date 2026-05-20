using System;

namespace CoreRacer.Services.Settings
{
    [Serializable]
    public sealed class GameSettingsState
    {
        public float MasterVolume = 1f;
        public float MusicVolume = 1f;
        public float SfxVolume = 1f;
        public bool HapticsEnabled = true;
        public bool HighQualityGraphics = true;
    }
}
