using UnityEngine;

namespace CoreRacer.Gameplay.Player
{
    public static class TouchSteeringInterpreter
    {
        public static float EvaluateScreenSide(float positionX, float screenWidth, float deadZonePixels)
        {
            var width = Mathf.Max(1f, screenWidth);
            var offsetFromCentre = positionX - width * 0.5f;
            return Mathf.Abs(offsetFromCentre) <= Mathf.Max(0f, deadZonePixels)
                ? 0f
                : Mathf.Sign(offsetFromCentre);
        }

        public static float EvaluateDrag(float positionX, float startX, float deadZonePixels)
        {
            var deadZone = Mathf.Max(1f, deadZonePixels);
            var delta = positionX - startX;
            return Mathf.Abs(delta) <= deadZone
                ? 0f
                : Mathf.Clamp(delta / (deadZone * 3f), -1f, 1f);
        }
    }
}
