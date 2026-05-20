using CoreRacer.Bootstrap;
using CoreRacer.Services.Settings;
using CoreRacer.UI.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.Settings
{
    public sealed class SettingsMenuController : UiView
    {
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Toggle hapticsToggle;
        private SettingsService _settings;

        private void Awake()
        {
            GameServices.TryGet(out _settings);
        }

        public override void Show()
        {
            base.Show();
            Refresh();
        }

        public void Refresh()
        {
            if (_settings == null) return;
            if (musicSlider != null) musicSlider.value = _settings.State.MusicVolume;
            if (sfxSlider != null) sfxSlider.value = _settings.State.SfxVolume;
            if (hapticsToggle != null) hapticsToggle.isOn = _settings.State.HapticsEnabled;
        }

        public void SetMusicVolume(float value) => _settings?.SetMusicVolume(value);
        public void SetSfxVolume(float value) => _settings?.SetSfxVolume(value);
        public void SetHaptics(bool value) => _settings?.SetHaptics(value);
    }
}
