using System.Collections.Generic;
using UnityEngine;

namespace CoreRacer.Gameplay.Obstacles
{
    [CreateAssetMenu(menuName = "Core Racer/Obstacles/Generation Config")]
    public sealed class ObstacleGenerationConfig : ScriptableObject
    {
        public ObstacleRingView RingPrefab;
        public int PrewarmCount = 32;
        public float SpawnStartZ = 20f;
        public float SpawnAheadDistance = 160f;
        public float RecycleBehindDistance = 25f;
        public float RingSpacing = 8f;
        public int TunnelSides = 6;
        public float BaseDifficulty = 0f;
        public float DifficultyPerSecond = 0.05f;
        public List<ObstaclePatternDefinition> Patterns = new List<ObstaclePatternDefinition>();
    }
}
