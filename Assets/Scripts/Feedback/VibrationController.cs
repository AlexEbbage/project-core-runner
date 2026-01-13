using System;
using UnityEngine;

public enum HapticPreset
{
    Light,
    Medium,
    Heavy
}

public static class VibrationController
{
    private const float LightDuration = 0.03f;
    private const float MediumDuration = 0.06f;
    private const float HeavyDuration = 0.12f;

    private const float LightAmplitude = 0.35f;
    private const float MediumAmplitude = 0.6f;
    private const float HeavyAmplitude = 1f;

    public static float MinimumIntervalSeconds { get; set; } = 0.04f;

    private static float _lastVibrateTime = -999f;

    public static void Vibrate(HapticPreset preset = HapticPreset.Medium)
    {
        if (!SettingsData.VibrateEnabled)
        {
            return;
        }

        float duration = MediumDuration;
        float amplitude = MediumAmplitude;

        switch (preset)
        {
            case HapticPreset.Light:
                duration = LightDuration;
                amplitude = LightAmplitude;
                break;
            case HapticPreset.Heavy:
                duration = HeavyDuration;
                amplitude = HeavyAmplitude;
                break;
        }

        Vibrate(duration, amplitude);
    }

    public static void Vibrate(float durationSeconds, float amplitude)
    {
        if (!SettingsData.VibrateEnabled)
        {
            return;
        }

        if (durationSeconds <= 0f || amplitude <= 0f)
        {
            return;
        }

        if (!CanVibrate())
        {
            return;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        TryVibrateAndroid(durationSeconds, amplitude);
#else
        Handheld.Vibrate();
#endif
        _lastVibrateTime = Time.unscaledTime;
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static void TryVibrateAndroid(float durationSeconds, float amplitude)
    {
        try
        {
            long durationMs = (long)Mathf.Max(1f, durationSeconds * 1000f);
            int amplitudeValue = Mathf.Clamp(Mathf.RoundToInt(amplitude * 255f), 1, 255);

            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
            {
                if (vibrator == null)
                {
                    Handheld.Vibrate();
                    return;
                }

                bool hasVibrator = vibrator.Call<bool>("hasVibrator");
                if (!hasVibrator)
                {
                    return;
                }

                using (AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION"))
                {
                    int sdkInt = version.GetStatic<int>("SDK_INT");
                    if (sdkInt >= 26)
                    {
                        using (AndroidJavaClass vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect"))
                        using (AndroidJavaObject effect = vibrationEffect.CallStatic<AndroidJavaObject>("createOneShot", durationMs, amplitudeValue))
                        {
                            vibrator.Call("vibrate", effect);
                        }
                    }
                    else
                    {
                        vibrator.Call("vibrate", durationMs);
                    }
                }
            }
        }
        catch (Exception)
        {
            Handheld.Vibrate();
        }
    }
#endif

    private static bool CanVibrate()
    {
        if (MinimumIntervalSeconds <= 0f)
        {
            return true;
        }

        return Time.unscaledTime - _lastVibrateTime >= MinimumIntervalSeconds;
    }
}
