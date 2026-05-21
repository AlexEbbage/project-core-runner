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
        private bool _bound;

        private void OnEnable()
        {
            GameServices.TryGet(out _settings);
            Bind();
            Refresh();
        }

        private void Bind()
        {
            if (_bound)
                return;

            if (screenShakeSlider != null) screenShakeSlider.onValueChanged.AddListener(SetScreenShake);
            if (flashSlider != null) flashSlider.onValueChanged.AddListener(SetFlash);
            if (reducedVfxToggle != null) reducedVfxToggle.onValueChanged.AddListener(SetReducedVfx);
            if (highContrastToggle != null) highContrastToggle.onValueChanged.AddListener(SetHighContrast);
            if (hapticsToggle != null) hapticsToggle.onValueChanged.AddListener(SetHaptics);
            if (dragControlsToggle != null) dragControlsToggle.onValueChanged.AddListener(SetDragControls);
            if (inputSensitivitySlider != null) inputSensitivitySlider.onValueChanged.AddListener(SetInputSensitivity);
            _bound = true;
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

        private void SetScreenShake(float value) => _settings?.Update(s => s.ScreenShakeIntensity = value);
        private void SetFlash(float value) => _settings?.Update(s => s.FlashIntensity = value);
        private void SetReducedVfx(bool value) => _settings?.Update(s => s.ReducedVfxMode = value);
        private void SetHighContrast(bool value) => _settings?.Update(s => s.HighContrastMode = value);
        private void SetHaptics(bool value) => _settings?.Update(s => s.HapticsEnabled = value);
        private void SetDragControls(bool value) => _settings?.Update(s => s.DragControlsEnabled = value);
        private void SetInputSensitivity(float value) => _settings?.Update(s => s.InputSensitivity = value);
    }
}
