using UnityEngine;

namespace CoreRacer.Debugging
{
    /// <summary>
    /// Lightweight collision logger for scene wiring and mobile QA. Keep disabled in release scenes unless needed.
    /// </summary>
    public sealed class CollisionDebugProbe : MonoBehaviour
    {
        [SerializeField] private bool logTriggers = true;
        [SerializeField] private bool logCollisions = true;
        [SerializeField] private bool includeObjectPath = true;

        private void OnTriggerEnter(Collider other)
        {
            if (logTriggers)
                UnityEngine.Debug.Log($"[CollisionDebugProbe] Trigger enter: {Describe(gameObject)} <- {Describe(other.gameObject)}", this);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (logCollisions)
                UnityEngine.Debug.Log($"[CollisionDebugProbe] Collision enter: {Describe(gameObject)} <- {Describe(collision.gameObject)}", this);
        }

        private string Describe(GameObject go)
        {
            if (go == null)
                return "<null>";

            if (!includeObjectPath)
                return go.name;

            var path = go.name;
            var current = go.transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }
    }
}
