// Assets/Scripts/Settings/SettingsData.cs
using UnityEngine;

public static class SettingsData
{
    private const string MusicVolumeKey = "settings_music_volume";
    private const string SfxVolumeKey = "settings_sfx_volume";
    private const string VibrateKey = "settings_vibrate";
    private const string TouchInputModeKey = "settings_touch_input_mode";
    private const string TouchSensitivityKey = "settings_touch_sensitivity";
    private const string RunSensitivityKey = "settings_run_sensitivity";
    public const float TouchSensitivityMin = 0.5f;
    public const float TouchSensitivityMax = 2f;
    public const float TouchSensitivityDefault = 1f;
    public const float RunSensitivityMin = 0.5f;
    public const float RunSensitivityMax = 2f;
    public const float RunSensitivityDefault = 1f;

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

    public static float RunSensitivity
    {
        get => PlayerPrefs.GetFloat(RunSensitivityKey, RunSensitivityDefault);
        set
        {
            float clamped = Mathf.Clamp(value, RunSensitivityMin, RunSensitivityMax);
            float current = RunSensitivity;
            if (Mathf.Approximately(current, clamped))
                return;

            PlayerPrefs.SetFloat(RunSensitivityKey, clamped);
            PlayerPrefs.Save();
            RunSensitivityChanged?.Invoke(clamped);
        }
    }

    public static event System.Action<float> TouchSensitivityChanged;
    public static event System.Action<float> RunSensitivityChanged;

    private static bool _initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeOnLoad()
    {
        Initialize();
    }

    public static void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;

        bool needsSave = false;
        int savedInputMode = PlayerPrefs.GetInt(TouchInputModeKey, (int)TouchInputMode.Drag);
        if (!System.Enum.IsDefined(typeof(TouchInputMode), savedInputMode))
        {
            savedInputMode = (int)TouchInputMode.Drag;
            PlayerPrefs.SetInt(TouchInputModeKey, savedInputMode);
            needsSave = true;
        }

        float savedSensitivity = PlayerPrefs.GetFloat(TouchSensitivityKey, TouchSensitivityDefault);
        float clampedSensitivity = Mathf.Clamp(savedSensitivity, TouchSensitivityMin, TouchSensitivityMax);
        if (!Mathf.Approximately(savedSensitivity, clampedSensitivity))
        {
            PlayerPrefs.SetFloat(TouchSensitivityKey, clampedSensitivity);
            needsSave = true;
        }

        float savedRunSensitivity = PlayerPrefs.GetFloat(RunSensitivityKey, RunSensitivityDefault);
        float clampedRunSensitivity = Mathf.Clamp(savedRunSensitivity, RunSensitivityMin, RunSensitivityMax);
        if (!Mathf.Approximately(savedRunSensitivity, clampedRunSensitivity))
        {
            PlayerPrefs.SetFloat(RunSensitivityKey, clampedRunSensitivity);
            needsSave = true;
        }

        if (needsSave)
            PlayerPrefs.Save();
    }
}
