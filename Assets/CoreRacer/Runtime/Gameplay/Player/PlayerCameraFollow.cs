using UnityEngine;

namespace CoreRacer.Gameplay.Player
{
    public sealed class PlayerCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
        [SerializeField] private float followSharpness = 12f;
        [SerializeField] private bool lockRotation = true;
        [SerializeField] private bool followTargetRoll;

        public bool FollowsTargetRoll => followTargetRoll;

        private void LateUpdate()
        {
            if (target == null)
                return;

            var desired = target.position + (followTargetRoll ? target.rotation * offset : offset);
            var t = 1f - Mathf.Exp(-followSharpness * UnityEngine.Time.deltaTime);
            transform.position = followTargetRoll
                ? new Vector3(desired.x, desired.y, Mathf.Lerp(transform.position.z, desired.z, t))
                : Vector3.Lerp(transform.position, desired, t);

            if (followTargetRoll)
                transform.rotation = Quaternion.Euler(0f, 0f, target.eulerAngles.z);
            else if (lockRotation)
                transform.rotation = Quaternion.identity;
        }
    }
}
