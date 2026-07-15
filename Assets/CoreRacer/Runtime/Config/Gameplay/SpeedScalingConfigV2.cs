using UnityEngine;

namespace CoreRacer.Config.Gameplay
{
    [CreateAssetMenu(menuName = "Core Racer/Gameplay/Speed Scaling V2")]
    public sealed class SpeedScalingConfigV2 : ScriptableObject
    {
        public float BaseForwardSpeed = 10f;
        public float MaxForwardSpeed = 40f;
        public float SpeedIncreasePerSecond = 0.2f;
        public AnimationCurve TimeScalingCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public float TimeCurveDuration = 120f;
        public float ComboSpeedFactor = 0.5f;
        public float ComboMaxSpeedBonus = 15f;

        public float EvaluateForwardSpeed(float elapsedSeconds, float combo)
        {
            return EvaluateForwardSpeed(elapsedSeconds, combo, BaseForwardSpeed);
        }

        public float EvaluateForwardSpeed(float elapsedSeconds, float combo, float startingSpeed)
        {
            var baseSpeed = Mathf.Max(0f, startingSpeed);
            var maxSpeed = Mathf.Max(baseSpeed, MaxForwardSpeed);
            var t = TimeCurveDuration <= 0f ? 1f : Mathf.Clamp01(elapsedSeconds / TimeCurveDuration);
            var curve = TimeScalingCurve == null ? 1f : TimeScalingCurve.Evaluate(t);
            var timeBonus = Mathf.Max(0f, elapsedSeconds) * SpeedIncreasePerSecond * curve;
            var comboBonus = Mathf.Min(ComboMaxSpeedBonus, Mathf.Max(0f, combo) * ComboSpeedFactor);
            return Mathf.Clamp(baseSpeed + timeBonus + comboBonus, baseSpeed, maxSpeed);
        }
    }
}
