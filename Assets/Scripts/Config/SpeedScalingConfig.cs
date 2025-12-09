using UnityEngine;

/// <summary>
/// Configuration for forward speed and difficulty scaling.
/// Create via Assets -> Create -> Game Config -> Speed Scaling Config.
/// </summary>
[CreateAssetMenu(
    fileName = "SpeedScalingConfig",
    menuName = "Game Config/Speed Scaling Config")]
public class SpeedScalingConfig : ScriptableObject
{
    [Header("Base Speed")]
    [Tooltip("Initial forward speed at the start of the run.")]
    public float baseForwardSpeed = 10f;

    [Tooltip("Maximum forward speed (hard cap).")]
    public float maxForwardSpeed = 40f;

    [Header("Time-Based Scaling")]
    [Tooltip("How much speed is added per second of run time.")]
    public float speedIncreasePerSecond = 0.2f;

    [Tooltip("Optional curve to modify time-based scaling (0..1 time normalized). Leave null for linear.")]
    public AnimationCurve timeScalingCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Tooltip("Approximate time (seconds) at which we consider timeScalingCurve's X=1.")]
    public float timeCurveDuration = 120f;

    [Header("Combo-Based Scaling")]
    [Tooltip("Multiplier that converts combo value into extra speed.")]
    public float comboSpeedFactor = 0.5f;

    [Tooltip("Maximum extra speed we can get from combo-based scaling.")]
    public float comboMaxSpeedBonus = 15f;

    /// <summary>
    /// Gets a multiplier from the time curve based on elapsedTime.
    /// </summary>
    public float EvaluateTimeScale(float elapsedTime)
    {
        if (timeScalingCurve == null || timeCurveDuration <= 0f)
            return 1f;

        float t = Mathf.Clamp01(elapsedTime / timeCurveDuration);
        float curveValue = timeScalingCurve.Evaluate(t);
        return curveValue;
    }
}
