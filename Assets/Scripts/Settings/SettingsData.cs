// Assets/Scripts/Settings/SettingsData.cs
using UnityEngine;

public static class SettingsData
{
    private const string MusicVolumeKey = "settings_music_volume";
    private const string SfxVolumeKey = "settings_sfx_volume";
    private const string VibrateKey = "settings_vibrate";
    private const string TouchInputModeKey = "settings_touch_input_mode";
    private const string TouchSensitivityKey = "settings_touch_sensitivity";
    public const float TouchSensitivityMin = 0.5f;
    public const float TouchSensitivityMax = 2f;
    public const float TouchSensitivityDefault = 1f;

    public enum TouchInputMode
    {
        Drag = 0,
        Buttons = 1
    }

    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        set { PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(value)); PlayerPrefs.Save(); }
    }

    public static float SfxVolume
    {
        get => PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        set { PlayerPrefs.SetFloat(SfxVolumeKey, Mathf.Clamp01(value)); PlayerPrefs.Save(); }
    }

    public static bool VibrateEnabled
    {
        get => PlayerPrefs.GetInt(VibrateKey, 1) == 1;
        set { PlayerPrefs.SetInt(VibrateKey, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public static TouchInputMode CurrentTouchInputMode
    {
        get
        {
            int saved = PlayerPrefs.GetInt(TouchInputModeKey, (int)TouchInputMode.Drag);
            if (!System.Enum.IsDefined(typeof(TouchInputMode), saved))
                saved = (int)TouchInputMode.Drag;
            return (TouchInputMode)saved;
        }
        set
        {
            PlayerPrefs.SetInt(TouchInputModeKey, (int)value);
            PlayerPrefs.Save();
        }
    }

    public static float TouchSensitivity
    {
        get => PlayerPrefs.GetFloat(TouchSensitivityKey, TouchSensitivityDefault);
        set
        {
            float clamped = Mathf.Clamp(value, TouchSensitivityMin, TouchSensitivityMax);
            float current = TouchSensitivity;
            if (Mathf.Approximately(current, clamped))
                return;

            PlayerPrefs.SetFloat(TouchSensitivityKey, clamped);
            PlayerPrefs.Save();
            TouchSensitivityChanged?.Invoke(clamped);
        }
    }

    public static event System.Action<float> TouchSensitivityChanged;
}
