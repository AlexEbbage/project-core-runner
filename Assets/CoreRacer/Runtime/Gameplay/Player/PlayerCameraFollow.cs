using UnityEngine;

namespace CoreRacer.Gameplay.Player
{
    public sealed class PlayerCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
        [SerializeField] private float followSharpness = 12f;
        [SerializeField] private bool lockRotation = true;

        private void LateUpdate()
        {
            if (target == null)
                return;

            var desired = target.position + offset;
            var t = 1f - Mathf.Exp(-followSharpness * UnityEngine.Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, t);

            if (lockRotation)
                transform.rotation = Quaternion.identity;
        }
    }
}
