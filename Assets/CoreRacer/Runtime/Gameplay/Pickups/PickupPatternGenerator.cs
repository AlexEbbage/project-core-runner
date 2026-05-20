using System.Collections.Generic;
using UnityEngine;

namespace CoreRacer.Gameplay.Pickups
{
    public sealed class PickupPatternGenerator
    {
        private readonly PickupGenerationConfig _config;

        public PickupPatternGenerator(PickupGenerationConfig config)
        {
            _config = config;
        }

        public List<Vector3> GenerateArc(float z)
        {
            var points = new List<Vector3>();
            var startSide = Random.Range(0, _config.TunnelSides);
            var count = Random.Range(2, Mathf.Max(3, _config.TunnelSides));
            for (int i = 0; i < count; i++)
            {
                var side = (startSide + i) % _config.TunnelSides;
                var angle = side * Mathf.PI * 2f / _config.TunnelSides;
                points.Add(new Vector3(Mathf.Cos(angle) * _config.RingRadius, Mathf.Sin(angle) * _config.RingRadius, z + i * 1.2f));
            }
            return points;
        }
    }
}
