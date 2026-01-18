using UnityEngine;

[CreateAssetMenu(menuName = "Gameplay/Powerup Upgrade Config")]
public class PowerupUpgradeConfig : ScriptableObject
{
    [System.Serializable]
    public struct PowerupUpgradeLevel
    {
        public float duration;
        public float strength;
    }

    [System.Serializable]
    public class PowerupUpgradeEntry
    {
        public PowerupType powerupType;
        public string displayName;
        public Sprite icon;
        public int baseCost = 100;
        public int costIncrease = 50;
        public PowerupUpgradeLevel[] levels;

        public int MaxLevel => Mathf.Max(0, (levels?.Length ?? 0) - 1);

        public int GetCostForLevel(int level)
        {
            return Mathf.Max(0, baseCost + costIncrease * Mathf.Max(0, level));
        }

        public bool TryGetLevel(int level, out PowerupUpgradeLevel upgradeLevel)
        {
            if (levels == null || levels.Length == 0)
            {
                upgradeLevel = default;
                return false;
            }

            int clamped = Mathf.Clamp(level, 0, levels.Length - 1);
            upgradeLevel = levels[clamped];
            return true;
        }
    }

    public PowerupUpgradeEntry[] upgrades;

    public bool TryGetUpgrade(PowerupType powerupType, out PowerupUpgradeEntry entry)
    {
        entry = null;
        if (upgrades == null)
            return false;

        for (int i = 0; i < upgrades.Length; i++)
        {
            if (upgrades[i] != null && upgrades[i].powerupType == powerupType)
            {
                entry = upgrades[i];
                return true;
            }
        }

        return false;
    }
}
