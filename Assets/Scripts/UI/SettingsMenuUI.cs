using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Toggle vibrateToggle;

    [SerializeField] private AudioManager audioManager;
    
    private float _lastMusicVolume = 1f;
    private float _lastSfxVolume = 1f;

    private void Awake()
    {
        if (audioManager == null)
            audioManager = FindFirstObjectByType<AudioManager>();

        _lastMusicVolume = SettingsData.MusicVolume > 0f ? SettingsData.MusicVolume : _lastMusicVolume;
        _lastSfxVolume = SettingsData.SfxVolume > 0f ? SettingsData.SfxVolume : _lastSfxVolume;

        if (musicToggle != null)
            musicToggle.isOn = SettingsData.MusicVolume > 0.01f;

        if (sfxToggle != null)
            sfxToggle.isOn = SettingsData.SfxVolume > 0.01f;

        if (vibrateToggle != null)
            vibrateToggle.isOn = SettingsData.VibrateEnabled;

        ApplyToAudio();
    }

    public void OnMusicToggleChanged(bool isOn)
    {
        if (isOn)
        {
            SettingsData.MusicVolume = Mathf.Clamp01(_lastMusicVolume <= 0f ? 1f : _lastMusicVolume);
        }
        else
        {
            _lastMusicVolume = SettingsData.MusicVolume;
            SettingsData.MusicVolume = 0f;
        }

        ApplyToAudio();
    }

    public void OnSfxToggleChanged(bool isOn)
    {
        if (isOn)
        {
            SettingsData.SfxVolume = Mathf.Clamp01(_lastSfxVolume <= 0f ? 1f : _lastSfxVolume);
        }
        else
        {
            _lastSfxVolume = SettingsData.SfxVolume;
            SettingsData.SfxVolume = 0f;
        }

        ApplyToAudio();
    }

    //public void OnSensitivitySliderChanged(float value)
    //{
    //    SettingsData.TouchSensitivity = value;
    //}

    //public void OnRunSensitivitySliderChanged(float value)
    //{
    //    SettingsData.RunSensitivity = value;
    //}

    public void OnVibrateToggleChanged(bool isOn)
    {
        SettingsData.VibrateEnabled = isOn;
    }

    public void OnTouchInputToggleChanged(bool on)
    {
        SettingsData.CurrentTouchInputMode = on
            ? SettingsData.TouchInputMode.Buttons
            : SettingsData.TouchInputMode.Drag;
    }

    private void ApplyToAudio()
    {
        if (audioManager == null)
            return;

        audioManager.SetMusicVolume(SettingsData.MusicVolume);
        audioManager.SetSfxVolume(SettingsData.SfxVolume);
    }

    public static void Vibrate()
    {
        VibrationController.Vibrate();
    }

    public void Hide()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
}
