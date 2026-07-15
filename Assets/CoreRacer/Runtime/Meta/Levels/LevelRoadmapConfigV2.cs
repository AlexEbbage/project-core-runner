using System.Collections.Generic;
using CoreRacer.Meta.Economy;
using UnityEngine;

namespace CoreRacer.Meta.Levels
{
    [System.Serializable]
    public sealed class LevelDefinition
    {
        public string Id = "hex_sector_01";
        public string DisplayName = "Hex Sector";
        [TextArea] public string Description = "Classic six-sided tunnel run.";
        public int RequiredPlayerLevel = 1;
        public int TunnelSides = 6;
        public float StartingSpeed = 16f;
        public float DifficultyMultiplier = 1f;
        public string ZoneId = "neon_hex";
        public string EnvironmentName = "Neon Hex";
        public CurrencyAmount FirstClearReward = new CurrencyAmount(CurrencyType.Premium, 5);
        public string ChallengeOne;
        public string ChallengeTwo;
        public string ChallengeThree;
    }

    [CreateAssetMenu(menuName = "Core Racer/Progression/Level Roadmap V2")]
    public sealed class LevelRoadmapConfigV2 : ScriptableObject
    {
        public List<LevelDefinition> Levels = new List<LevelDefinition>();

        public LevelDefinition Get(string id)
        {
            for (int i = 0; i < Levels.Count; i++)
                if (Levels[i] != null && Levels[i].Id == id)
                    return Levels[i];
            return Levels.Count > 0 ? Levels[0] : null;
        }
    }
}
