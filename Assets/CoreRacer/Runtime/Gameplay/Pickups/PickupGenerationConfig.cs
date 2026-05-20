using System.Collections.Generic;
using CoreRacer.Gameplay.Powerups;
using UnityEngine;

namespace CoreRacer.Gameplay.Pickups
{
    [CreateAssetMenu(menuName = "Core Racer/Pickups/Generation Config")]
    public sealed class PickupGenerationConfig : ScriptableObject
    {
        public PickupView CoinPrefab;
        public PickupView PowerupPrefab;
        public int PrewarmCoins = 64;
        public int PrewarmPowerups = 16;
        public float SpawnAheadDistance = 140f;
        public float RecycleBehindDistance = 20f;
        public float RingSpacing = 12f;
        public float RingRadius = 3f;
        public int TunnelSides = 6;
        [Range(0f, 1f)] public float PowerupChance = 0.12f;
        public List<WeightedPowerupEntry> PowerupLootTable = new List<WeightedPowerupEntry>();
    }

    [System.Serializable]
    public struct WeightedPowerupEntry
    {
        public PowerupType Type;
        public float Weight;
    }
}
