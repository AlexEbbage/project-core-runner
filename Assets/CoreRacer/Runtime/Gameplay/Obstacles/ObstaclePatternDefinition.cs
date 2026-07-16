using System.Collections.Generic;
using CoreRacer.Common.Validation;
using UnityEngine;

namespace CoreRacer.Gameplay.Obstacles
{
    [System.Serializable]
    public struct ObstacleSegmentRule
    {
        public int SideIndex;
        public bool Blocked;
    }

    [CreateAssetMenu(menuName = "Core Racer/Obstacles/Pattern Definition")]
    public sealed class ObstaclePatternDefinition : ScriptableObject, IValidatableConfig
    {
        public string Id;
        public string DisplayName;
        public ObstacleType Type = ObstacleType.Walls;
        public int MinimumSides = 6;
        public int MaximumSides = 12;
        public float MinimumDifficulty;
        public float MaximumDifficulty = 999f;
        public float Weight = 1f;
        public float MinRotationDegrees;
        public float MaxRotationDegrees;
        public int MinIterations = 1;
        public int MaxIterations = 1;
        public GameObject ObstaclePrefab;
        public float ObstacleScale = 1f;
        public float SpacingMultiplier = 1f;
        public float MinRotationSpeedDegrees;
        public float MaxRotationSpeedDegrees;
        public List<ObstacleSegmentRule> Segments = new List<ObstacleSegmentRule>();

        public bool IsValidFor(float difficulty, int sideCount)
        {
            return sideCount >= MinimumSides && sideCount <= MaximumSides && difficulty >= MinimumDifficulty && difficulty <= MaximumDifficulty && Weight > 0f;
        }

        public ValidationResult ValidateConfig()
        {
            var result = new ValidationResult();
            if (string.IsNullOrWhiteSpace(Id)) result.Error($"Obstacle pattern {name} has no id.");
            if (MinimumSides <= 2) result.Error($"Obstacle pattern {Id} minimum sides too low.");
            if (MaximumSides < MinimumSides) result.Error($"Obstacle pattern {Id} maximum sides < minimum sides.");
            if (Weight < 0f) result.Error($"Obstacle pattern {Id} has negative weight.");
            if (MaxIterations < MinIterations) result.Error($"Obstacle pattern {Id} maximum iterations < minimum iterations.");
            if (ObstacleScale <= 0f) result.Error($"Obstacle pattern {Id} obstacle scale must be positive.");
            if (SpacingMultiplier <= 0f) result.Error($"Obstacle pattern {Id} spacing multiplier must be positive.");
            return result;
        }
    }
}
