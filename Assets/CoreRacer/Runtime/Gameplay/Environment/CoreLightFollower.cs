using UnityEngine;

namespace CoreRacer.Gameplay.Environment
{
    public sealed class CoreLightFollower : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset;
        [SerializeField] private bool followZOnly = true;

        private void LateUpdate()
        {
            if (target == null)
                return;

            if (followZOnly)
                transform.position = new Vector3(transform.position.x, transform.position.y, target.position.z + offset.z);
            else
                transform.position = target.position + offset;
        }
    }
}
