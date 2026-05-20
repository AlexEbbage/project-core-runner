using UnityEngine;

namespace CoreRacer.Gameplay.Camera
{
    public static class ScreenShakeHelper
    {
        public static Vector3 CalculateShakeOffset(float trauma, float strength, float frequency, float seed)
        {
            var amount = Mathf.Clamp01(trauma) * Mathf.Max(0f, strength);
            if (amount <= 0f)
                return Vector3.zero;

            var t = UnityEngine.Time.unscaledTime * Mathf.Max(0.01f, frequency);
            var x = (Mathf.PerlinNoise(seed, t) - 0.5f) * 2f * amount;
            var y = (Mathf.PerlinNoise(seed + 11.7f, t) - 0.5f) * 2f * amount;
            return new Vector3(x, y, 0f);
        }
    }
}
