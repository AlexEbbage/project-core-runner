using System.Collections.Generic;
using UnityEngine;

namespace CoreRacer.Gameplay.Powerups
{
    [System.Serializable]
    public sealed class PowerupUpgradeEntry
    {
        public PowerupType Type;
        public string DisplayName;
        public Sprite Icon;
        public int BaseCost = 100;
        public int CostIncrease = 50;
        public List<PowerupTuning> Levels = new List<PowerupTuning>();

        public int MaxLevel => Mathf.Max(0, Levels.Count - 1);
        public int GetCostForLevel(int level) => Mathf.Max(0, BaseCost + CostIncrease * Mathf.Max(0, level));
        public PowerupTuning GetTuning(int level) => Levels.Count == 0 ? new PowerupTuning(5f, 1f) : Levels[Mathf.Clamp(level, 0, Levels.Count - 1)];
    }

    [CreateAssetMenu(menuName = "Core Racer/Powerups/Upgrade Config V2")]
    public sealed class PowerupUpgradeConfigV2 : ScriptableObject
    {
        public List<PowerupUpgradeEntry> Upgrades = new List<PowerupUpgradeEntry>();

        public PowerupUpgradeEntry GetEntry(PowerupType type)
        {
            for (int i = 0; i < Upgrades.Count; i++)
                if (Upgrades[i] != null && Upgrades[i].Type == type)
                    return Upgrades[i];
            return CreateFallback(type);
        }

        private static PowerupUpgradeEntry CreateFallback(PowerupType type)
        {
            var entry = new PowerupUpgradeEntry { Type = type, DisplayName = type.ToString() };
            entry.Levels.Add(new PowerupTuning(5f, 1f));
            entry.Levels.Add(new PowerupTuning(6f, 1.15f));
            entry.Levels.Add(new PowerupTuning(7f, 1.3f));
            return entry;
        }
    }
}
