using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle vibrateToggle;

    [SerializeField] private AudioManager audioManager;

    private void Awake()
    {
        if (audioManager == null)
            audioManager = FindFirstObjectByType<AudioManager>();

        // Load
        musicSlider.value = SettingsData.MusicVolume;
        sfxSlider.value = SettingsData.SfxVolume;
        vibrateToggle.isOn = SettingsData.VibrateEnabled;

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

    public void OnVibrateToggleChanged(bool on)
    {
        SettingsData.VibrateEnabled = on;
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
