using UnityEngine;

namespace CoreRacer.Gameplay.Player
{
    public sealed class PlayerOrbitalMotor : MonoBehaviour
    {
        [SerializeField] private float orbitRadius = 3f;
        [SerializeField] private float angularSpeedDegrees = 180f;
        [SerializeField] private float forwardSpeed = 10f;
        [SerializeField] private float angleDegrees;
        private float _speedMultiplier = 1f;
        private float _shipSpeedMultiplier = 1f;
        private float _handlingMultiplier = 1f;

        public float AngleDegrees => angleDegrees;
        public float AngularSpeedDegrees => angularSpeedDegrees;
        public float ForwardSpeed { get => forwardSpeed; set => forwardSpeed = Mathf.Max(0f, value); }
        public float EffectiveForwardSpeed => forwardSpeed * _speedMultiplier * _shipSpeedMultiplier;

        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = Mathf.Clamp(multiplier, 0.1f, 5f);
        }

        public void SetShipModifiers(float speedMultiplier, float handlingMultiplier)
        {
            _shipSpeedMultiplier = Mathf.Clamp(speedMultiplier, 0.5f, 2f);
            _handlingMultiplier = Mathf.Clamp(handlingMultiplier, 0.5f, 2f);
        }

        public void ResetMotor(float startZ = 0f)
        {
            angleDegrees = 270f;
            _speedMultiplier = 1f;
            ApplyPosition(startZ);
        }

        public void Move(float horizontalInput, float deltaTime)
        {
            angleDegrees += horizontalInput * angularSpeedDegrees * _handlingMultiplier * deltaTime;
            var nextZ = transform.position.z + EffectiveForwardSpeed * deltaTime;
            ApplyPosition(nextZ);
        }

        private void ApplyPosition(float z)
        {
            var radians = angleDegrees * Mathf.Deg2Rad;
            transform.position = new Vector3(Mathf.Cos(radians) * orbitRadius, Mathf.Sin(radians) * orbitRadius, z);
            transform.rotation = Quaternion.Euler(0f, 0f, angleDegrees + 90f);
        }
    }
}
