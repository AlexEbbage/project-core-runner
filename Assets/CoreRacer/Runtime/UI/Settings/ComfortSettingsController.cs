using CoreRacer.Bootstrap;
using CoreRacer.Services.Accessibility;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.Settings
{
    public sealed class ComfortSettingsController : MonoBehaviour
    {
        [SerializeField] private Slider screenShakeSlider;
        [SerializeField] private Slider flashSlider;
        [SerializeField] private Toggle reducedVfxToggle;
        [SerializeField] private Toggle highContrastToggle;
        [SerializeField] private Toggle hapticsToggle;
        [SerializeField] private Toggle dragControlsToggle;
        [SerializeField] private Slider inputSensitivitySlider;

        private AccessibilitySettingsService _settings;

        private void OnEnable()
        {
            GameServices.TryGet(out _settings);
            Bind();
            Refresh();
        }

        private void Bind()
        {
            if (screenShakeSlider != null) screenShakeSlider.onValueChanged.AddListener(v => _settings?.Update(s => s.ScreenShakeIntensity = v));
            if (flashSlider != null) flashSlider.onValueChanged.AddListener(v => _settings?.Update(s => s.FlashIntensity = v));
            if (reducedVfxToggle != null) reducedVfxToggle.onValueChanged.AddListener(v => _settings?.Update(s => s.ReducedVfxMode = v));
            if (highContrastToggle != null) highContrastToggle.onValueChanged.AddListener(v => _settings?.Update(s => s.HighContrastMode = v));
            if (hapticsToggle != null) hapticsToggle.onValueChanged.AddListener(v => _settings?.Update(s => s.HapticsEnabled = v));
            if (dragControlsToggle != null) dragControlsToggle.onValueChanged.AddListener(v => _settings?.Update(s => s.DragControlsEnabled = v));
            if (inputSensitivitySlider != null) inputSensitivitySlider.onValueChanged.AddListener(v => _settings?.Update(s => s.InputSensitivity = v));
        }

        public void Refresh()
        {
            if (_settings == null) return;
            var s = _settings.State;
            if (screenShakeSlider != null) screenShakeSlider.SetValueWithoutNotify(s.ScreenShakeIntensity);
            if (flashSlider != null) flashSlider.SetValueWithoutNotify(s.FlashIntensity);
            if (reducedVfxToggle != null) reducedVfxToggle.SetIsOnWithoutNotify(s.ReducedVfxMode);
            if (highContrastToggle != null) highContrastToggle.SetIsOnWithoutNotify(s.HighContrastMode);
            if (hapticsToggle != null) hapticsToggle.SetIsOnWithoutNotify(s.HapticsEnabled);
            if (dragControlsToggle != null) dragControlsToggle.SetIsOnWithoutNotify(s.DragControlsEnabled);
            if (inputSensitivitySlider != null) inputSensitivitySlider.SetValueWithoutNotify(s.InputSensitivity);
        }
    }
}
