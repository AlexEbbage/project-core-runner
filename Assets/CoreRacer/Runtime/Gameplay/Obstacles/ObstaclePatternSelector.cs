using System.Collections.Generic;
using UnityEngine;

namespace CoreRacer.Gameplay.Obstacles
{
    public sealed class ObstaclePatternSelector
    {
        private readonly ObstacleGenerationConfig _config;
        private readonly List<ObstaclePatternDefinition> _valid = new List<ObstaclePatternDefinition>();
        private ObstaclePatternDefinition _last;

        public ObstaclePatternSelector(ObstacleGenerationConfig config)
        {
            _config = config;
        }

        public ObstaclePatternDefinition Select(float difficulty, int sideCount)
        {
            _valid.Clear();
            float totalWeight = 0f;
            for (int i = 0; i < _config.Patterns.Count; i++)
            {
                var pattern = _config.Patterns[i];
                if (pattern == null || !pattern.IsValidFor(difficulty, sideCount))
                    continue;

                var weight = pattern == _last && _config.Patterns.Count > 1 ? pattern.Weight * 0.25f : pattern.Weight;
                if (weight <= 0f) continue;
                _valid.Add(pattern);
                totalWeight += weight;
            }

            if (_valid.Count == 0)
                return null;

            var roll = Random.value * totalWeight;
            for (int i = 0; i < _valid.Count; i++)
            {
                var p = _valid[i];
                var weight = p == _last && _config.Patterns.Count > 1 ? p.Weight * 0.25f : p.Weight;
                roll -= weight;
                if (roll <= 0f)
                {
                    _last = p;
                    return p;
                }
            }

            _last = _valid[_valid.Count - 1];
            return _last;
        }

        public void Reset()
        {
            _last = null;
        }
    }
}
