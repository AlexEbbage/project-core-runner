using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Toggle vibrateToggle;
    [SerializeField] private Toggle touchInputToggle;

    [SerializeField] private AudioManager audioManager;

    private void Awake()
    {
        if (audioManager == null)
            audioManager = FindFirstObjectByType<AudioManager>();

        // Load
        if (musicSlider != null)
            musicSlider.value = SettingsData.MusicVolume;
        if (sfxSlider != null)
            sfxSlider.value = SettingsData.SfxVolume;
        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = SettingsData.TouchSensitivityMin;
            sensitivitySlider.maxValue = SettingsData.TouchSensitivityMax;
            sensitivitySlider.value = SettingsData.TouchSensitivity;
        }
        if (vibrateToggle != null)
            vibrateToggle.isOn = SettingsData.VibrateEnabled;
        if (touchInputToggle != null)
            touchInputToggle.isOn = SettingsData.CurrentTouchInputMode == SettingsData.TouchInputMode.Buttons;

        ApplyToAudio();
    }

    public void OnMusicSliderChanged(float value)
    {
        SettingsData.MusicVolume = value;
        ApplyToAudio();
    }

    public void OnSfxSliderChanged(float value)
    {
        SettingsData.SfxVolume = value;
        ApplyToAudio();
    }

    public void OnSensitivitySliderChanged(float value)
    {
        SettingsData.TouchSensitivity = value;
    }

    public void OnVibrateToggleChanged(bool on)
    {
        SettingsData.VibrateEnabled = on;
    }

    public void OnTouchInputToggleChanged(bool on)
    {
        SettingsData.CurrentTouchInputMode = on
            ? SettingsData.TouchInputMode.Buttons
            : SettingsData.TouchInputMode.Drag;
    }

    private void ApplyToAudio()
    {
        if (audioManager == null) return;

        audioManager.SetMusicVolume(SettingsData.MusicVolume);
        audioManager.SetSfxVolume(SettingsData.SfxVolume);
    }

    public static void Vibrate()
    {
        VibrationController.Vibrate();
    }
}
