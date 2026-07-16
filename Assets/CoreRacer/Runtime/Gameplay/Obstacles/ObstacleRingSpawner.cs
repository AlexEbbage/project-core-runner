using System.Collections.Generic;
using CoreRacer.Common.Pooling;
using UnityEngine;

namespace CoreRacer.Gameplay.Obstacles
{
    public sealed class ObstacleRingSpawner
    {
        private readonly ObstacleGenerationConfig _config;
        private readonly ObstaclePatternSelector _selector;
        private readonly ObstacleGroupRotationPlanner _rotationPlanner = new ObstacleGroupRotationPlanner();
        private readonly ComponentPool<ObstacleRingView> _pool;
        private readonly List<ObstacleRingView> _active = new List<ObstacleRingView>();
        private float _nextSpawnZ;
        private ObstaclePatternDefinition _groupPattern;
        private int _remainingInGroup;
        private float _groupRotationDegrees;

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
            _selector.Reset();
            _rotationPlanner.Reset();
            _groupPattern = null;
            _remainingInGroup = 0;
            _groupRotationDegrees = 0f;
        }

        public void EnsureAhead(float playerZ, float difficulty)
        {
            while (_nextSpawnZ < playerZ + _config.SpawnAheadDistance)
            {
                if (_remainingInGroup <= 0)
                {
                    _groupPattern = _selector.Select(difficulty, _config.TunnelSides);
                    _remainingInGroup = _groupPattern != null
                        ? Random.Range(Mathf.Max(1, _groupPattern.MinIterations), Mathf.Max(1, _groupPattern.MaxIterations) + 1)
                        : 1;
                    _groupRotationDegrees = SelectGroupRotation(_groupPattern);
                }

                var pattern = _groupPattern;
                var ring = _pool.Take();
                ring.Build(pattern, _config.TunnelSides, _nextSpawnZ, _groupRotationDegrees);
                _active.Add(ring);
                _remainingInGroup--;
                var spacingMultiplier = pattern != null ? Mathf.Max(0.25f, pattern.SpacingMultiplier) : 1f;
                _nextSpawnZ += Mathf.Max(1f, _config.RingSpacing * spacingMultiplier);
            }
        }

        private float SelectGroupRotation(ObstaclePatternDefinition pattern)
        {
            var sideCount = Mathf.Max(1, _config.TunnelSides);
            var sideAngle = 360f / sideCount;
            var minimum = pattern != null ? pattern.MinRotationDegrees : 0f;
            var maximum = pattern != null ? pattern.MaxRotationDegrees : 360f - sideAngle;
            var proposedDegrees = Random.Range(minimum, maximum);
            var proposedSide = Mathf.RoundToInt(proposedDegrees / sideAngle);
            return _rotationPlanner.NextSide(sideCount, proposedSide) * sideAngle;
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
