using UnityEngine;

/// <summary>
/// Global helper for triggering camera shake from anywhere.
/// Usage: ScreenShakeHelper.Shake(strength, duration);
/// </summary>
public static class ScreenShakeHelper
{
    public static CameraShake ActiveCameraShake { get; private set; }

    public static void RegisterCameraShake(CameraShake cameraShake)
    {
        ActiveCameraShake = cameraShake;
    }

    /// <summary>
    /// Shake with default strength/duration defined on the CameraShake.
    /// </summary>
    public static void Shake()
    {
        if (ActiveCameraShake == null) return;
        ActiveCameraShake.PlayShake();
    }

    /// <summary>
    /// Shake with custom strength/duration.
    /// </summary>
    public static void Shake(float strength, float duration)
    {
        if (ActiveCameraShake == null) return;
        ActiveCameraShake.PlayShake(strength, duration);
    }

    /// <summary>
    /// Shake with custom strength/duration/frequency.
    /// </summary>
    public static void Shake(float strength, float duration, float frequency)
    {
        if (ActiveCameraShake == null) return;
        ActiveCameraShake.PlayShake(strength, duration, frequency);
    }
}
