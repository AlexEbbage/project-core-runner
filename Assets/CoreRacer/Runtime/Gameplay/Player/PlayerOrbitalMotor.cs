using UnityEngine;

namespace CoreRacer.Gameplay.Player
{
    public sealed class PlayerOrbitalMotor : MonoBehaviour
    {
        [SerializeField] private float orbitRadius = 3f;
        [SerializeField] private float angularSpeedDegrees = 180f;
        [SerializeField] private float forwardSpeed = 10f;
        [SerializeField] private float angleDegrees;

        public float AngleDegrees => angleDegrees;
        public float ForwardSpeed { get => forwardSpeed; set => forwardSpeed = Mathf.Max(0f, value); }

        public void ResetMotor(float startZ = 0f)
        {
            angleDegrees = 90f;
            ApplyPosition(startZ);
        }

        public void Move(float horizontalInput, float deltaTime)
        {
            angleDegrees += horizontalInput * angularSpeedDegrees * deltaTime;
            var nextZ = transform.position.z + forwardSpeed * deltaTime;
            ApplyPosition(nextZ);
        }

        private void ApplyPosition(float z)
        {
            var radians = angleDegrees * Mathf.Deg2Rad;
            transform.position = new Vector3(Mathf.Cos(radians) * orbitRadius, Mathf.Sin(radians) * orbitRadius, z);
            transform.rotation = Quaternion.Euler(0f, 0f, angleDegrees - 90f);
        }
    }
}
