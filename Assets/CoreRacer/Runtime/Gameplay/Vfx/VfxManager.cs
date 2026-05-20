using System.Collections;
using System.Collections.Generic;
using CoreRacer.Common.Pooling;
using UnityEngine;

namespace CoreRacer.Gameplay.Vfx
{
    public sealed class VfxManager : MonoBehaviour
    {
        [SerializeField] private Transform poolRoot;
        private readonly Dictionary<VfxPooledInstance, ComponentPool<VfxPooledInstance>> _pools = new Dictionary<VfxPooledInstance, ComponentPool<VfxPooledInstance>>();

        public VfxPooledInstance Play(VfxPooledInstance prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
                return null;

            var pool = GetPool(prefab);
            var instance = pool.Take();
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.Play();
            StartCoroutine(ReleaseAfter(pool, instance, instance.EstimatedLifetime));
            return instance;
        }

        private IEnumerator ReleaseAfter(ComponentPool<VfxPooledInstance> pool, VfxPooledInstance instance, float seconds)
        {
            yield return new WaitForSeconds(Mathf.Max(0.01f, seconds));
            if (instance != null)
            {
                instance.Stop();
                pool.Return(instance);
            }
        }

        private ComponentPool<VfxPooledInstance> GetPool(VfxPooledInstance prefab)
        {
            ComponentPool<VfxPooledInstance> pool;
            if (_pools.TryGetValue(prefab, out pool))
                return pool;

            pool = new ComponentPool<VfxPooledInstance>(prefab, poolRoot != null ? poolRoot : transform, 4);
            _pools.Add(prefab, pool);
            return pool;
        }
    }
}
