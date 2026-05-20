using System.Collections.Generic;
using CoreRacer.Common.Pooling;
using UnityEngine;

namespace CoreRacer.Gameplay.Obstacles
{
    public sealed class ObstacleRingSpawner
    {
        private readonly ObstacleGenerationConfig _config;
        private readonly ObstaclePatternSelector _selector;
        private readonly ComponentPool<ObstacleRingView> _pool;
        private readonly List<ObstacleRingView> _active = new List<ObstacleRingView>();
        private float _nextSpawnZ;

        public IReadOnlyList<ObstacleRingView> Active => _active;

        public ObstacleRingSpawner(ObstacleGenerationConfig config, Transform parent)
        {
            _config = config;
            _selector = new ObstaclePatternSelector(config);
            _pool = new ComponentPool<ObstacleRingView>(config.RingPrefab, parent, config.PrewarmCount);
        }

        public void Reset(float playerZ)
        {
            for (int i = 0; i < _active.Count; i++)
                _pool.Return(_active[i]);
            _active.Clear();
            _nextSpawnZ = playerZ + _config.SpawnStartZ;
        }

        public void EnsureAhead(float playerZ, float difficulty)
        {
            while (_nextSpawnZ < playerZ + _config.SpawnAheadDistance)
            {
                var pattern = _selector.Select(difficulty, _config.TunnelSides);
                var ring = _pool.Take();
                ring.Build(pattern, _config.TunnelSides, _nextSpawnZ);
                _active.Add(ring);
                _nextSpawnZ += Mathf.Max(1f, _config.RingSpacing);
            }
        }

        public void RecycleBehind(float playerZ)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                if (_active[i].Z < playerZ - _config.RecycleBehindDistance)
                {
                    _pool.Return(_active[i]);
                    _active.RemoveAt(i);
                }
            }
        }
    }
}
