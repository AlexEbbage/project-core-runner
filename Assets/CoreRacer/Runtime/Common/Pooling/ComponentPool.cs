using System.Collections.Generic;
using UnityEngine;

namespace CoreRacer.Common.Pooling
{
    /// <summary>
    /// Small reusable component pool. Keeps obstacle/pickup/VFX pooling out of gameplay generation logic.
    /// </summary>
    public sealed class ComponentPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _available = new Stack<T>();
        private readonly List<T> _all = new List<T>();

        public ComponentPool(T prefab, Transform parent, int prewarmCount = 0)
        {
            _prefab = prefab;
            _parent = parent;
            Prewarm(prewarmCount);
        }

        public IReadOnlyList<T> All => _all;
        public int AvailableCount => _available.Count;
        public int TotalCount => _all.Count;

        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
                Return(CreateInstance());
        }

        public T Take()
        {
            var instance = _available.Count > 0 ? _available.Pop() : CreateInstance();
            instance.gameObject.SetActive(true);

            var poolable = instance as PoolableBehaviour;
            if (poolable != null)
                poolable.OnTakenFromPool();

            return instance;
        }

        public void Return(T instance)
        {
            if (instance == null)
                return;

            var poolable = instance as PoolableBehaviour;
            if (poolable != null)
                poolable.OnReturnedToPool();

            instance.gameObject.SetActive(false);
            if (_parent != null)
                instance.transform.SetParent(_parent, false);
            _available.Push(instance);
        }

        public void ReturnAllActive()
        {
            for (int i = 0; i < _all.Count; i++)
            {
                var instance = _all[i];
                if (instance != null && instance.gameObject.activeSelf)
                    Return(instance);
            }
        }

        private T CreateInstance()
        {
            var instance = Object.Instantiate(_prefab, _parent);
            _all.Add(instance);
            return instance;
        }
    }
}
