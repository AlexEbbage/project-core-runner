// Assets/Scripts/Settings/SettingsData.cs
using UnityEngine;

public static class SettingsData
{
    private const string MusicVolumeKey = "settings_music_volume";
    private const string SfxVolumeKey = "settings_sfx_volume";
    private const string VibrateKey = "settings_vibrate";
    private const string TouchInputModeKey = "settings_touch_input_mode";

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
}
