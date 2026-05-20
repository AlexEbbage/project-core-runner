using UnityEngine;

namespace CoreRacer.Gameplay.Obstacles
{
    public sealed class ObstacleRingController : MonoBehaviour
    {
        [SerializeField] private float rotationSpeedDegrees;
        [SerializeField] private bool reverseEveryOtherSegment;

        public void Configure(float rotationSpeed, bool reverseEveryOther = false)
        {
            rotationSpeedDegrees = rotationSpeed;
            reverseEveryOtherSegment = reverseEveryOther;
        }

        private void Update()
        {
            if (Mathf.Approximately(rotationSpeedDegrees, 0f))
                return;

            transform.Rotate(0f, 0f, rotationSpeedDegrees * UnityEngine.Time.deltaTime, Space.Self);

            if (!reverseEveryOtherSegment)
                return;

            for (int i = 0; i < transform.childCount; i++)
            {
                if ((i & 1) == 1)
                    transform.GetChild(i).Rotate(0f, 0f, -rotationSpeedDegrees * 2f * UnityEngine.Time.deltaTime, Space.Self);
            }
        }
    }
}
