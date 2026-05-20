using UnityEngine;

namespace CoreRacer.Gameplay.Environment
{
    public sealed class TunnelLightFollower : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float zOffset = 8f;
        [SerializeField] private float smoothing = 12f;

        private void LateUpdate()
        {
            if (target == null)
                return;

            var desired = new Vector3(transform.position.x, transform.position.y, target.position.z + zOffset);
            var t = 1f - Mathf.Exp(-smoothing * UnityEngine.Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, t);
        }
    }
}
