using UnityEngine;

namespace CoreRacer.Common.Pooling
{
    public class PoolableBehaviour : MonoBehaviour
    {
        public virtual void OnTakenFromPool() { }
        public virtual void OnReturnedToPool() { }
    }
}
