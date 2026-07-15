using UnityEngine;

namespace CoreRacer.Gameplay.Player
{
    /// <summary>Small local obstacle-avoidance sensor used by the Auto Pilot powerup.</summary>
    public sealed class AutoPilotSteeringController : MonoBehaviour
    {
        [SerializeField] private Transform probeOrigin;
        [SerializeField] private float probeDistance = 16f;
        [SerializeField] private float probeRadius = 0.45f;
        [SerializeField] private float sideProbeOffset = 0.75f;
        [SerializeField] private float steeringStrength = 0.85f;
        [SerializeField] private LayerMask obstacleMask = ~0;

        public float EvaluateInput()
        {
            var origin = probeOrigin != null ? probeOrigin.position : transform.position;
            if (!IsBlocked(origin))
                return 0f;

            var radial = new Vector3(origin.x, origin.y, 0f);
            if (radial.sqrMagnitude < 0.001f)
                radial = Vector3.up;
            radial.Normalize();
            var tangent = new Vector3(-radial.y, radial.x, 0f);

            var positiveBlocked = IsBlocked(origin + tangent * sideProbeOffset);
            var negativeBlocked = IsBlocked(origin - tangent * sideProbeOffset);
            if (positiveBlocked && !negativeBlocked)
                return -steeringStrength;
            if (negativeBlocked && !positiveBlocked)
                return steeringStrength;

            return (Time.frameCount & 1) == 0 ? steeringStrength : -steeringStrength;
        }

        private bool IsBlocked(Vector3 origin)
        {
            return Physics.SphereCast(origin, probeRadius, Vector3.forward, out _, probeDistance, obstacleMask, QueryTriggerInteraction.Ignore);
        }
    }
}
