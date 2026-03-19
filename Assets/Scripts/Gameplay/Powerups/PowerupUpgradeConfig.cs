using UnityEngine;

[CreateAssetMenu(menuName = "Gameplay/Powerup Upgrade Config")]
public class PowerupUpgradeConfig : ScriptableObject
{
    private static PowerupUpgradeEntry[] _defaultEntries;
    private static readonly PowerupType[] TargetGameplayPowerups =
    {
        PowerupType.ScoreMultiplier,
        PowerupType.CoinMultiplier,
        PowerupType.Magnet,
        PowerupType.AutoPilot,
        PowerupType.Shield
    };

    public static System.Collections.Generic.IReadOnlyList<PowerupType> TargetPowerups => TargetGameplayPowerups;

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

    public PowerupUpgradeEntry[] GetAvailableUpgrades()
    {
        var filtered = new System.Collections.Generic.List<PowerupUpgradeEntry>();
        var targetPowerups = TargetPowerups;
        for (int targetIndex = 0; targetIndex < targetPowerups.Count; targetIndex++)
        {
            PowerupType targetType = targetPowerups[targetIndex];
            PowerupUpgradeEntry matchedEntry = null;

            if (upgrades != null)
            {
                for (int i = 0; i < upgrades.Length; i++)
                {
                    PowerupUpgradeEntry entry = upgrades[i];
                    if (entry == null || entry.powerupType != targetType)
                        continue;

                    matchedEntry = entry;
                    break;
                }
            }

            if (matchedEntry == null)
                matchedEntry = GetDefaultEntry(targetType);
            else
                matchedEntry = CloneEntryWithFallbacks(matchedEntry);

            if (matchedEntry != null)
                filtered.Add(matchedEntry);
        }

        return filtered.ToArray();
    }

    private static PowerupUpgradeEntry GetDefaultEntry(PowerupType powerupType)
    {
        PowerupUpgradeEntry[] defaultEntries = GetDefaultEntries();
        for (int i = 0; i < defaultEntries.Length; i++)
        {
            PowerupUpgradeEntry entry = defaultEntries[i];
            if (entry != null && entry.powerupType == powerupType)
                return entry;
        }

        return null;
    }

    public bool TryGetUpgrade(PowerupType powerupType, out PowerupUpgradeEntry entry)
    {
        entry = null;
        PowerupUpgradeEntry[] availableEntries = GetAvailableUpgrades();
        for (int i = 0; i < availableEntries.Length; i++)
        {
            if (availableEntries[i] != null && availableEntries[i].powerupType == powerupType)
            {
                entry = availableEntries[i];
                return true;
            }
        }

        return false;
    }

    public bool HasTargetRosterEntries()
    {
        if (upgrades == null || upgrades.Length == 0)
            return false;

        var targetPowerups = TargetPowerups;
        for (int i = 0; i < targetPowerups.Count; i++)
        {
            bool found = false;
            for (int upgradeIndex = 0; upgradeIndex < upgrades.Length; upgradeIndex++)
            {
                PowerupUpgradeEntry entry = upgrades[upgradeIndex];
                if (entry != null && entry.powerupType == targetPowerups[i])
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                return false;
        }

        return true;
    }

    public void MaterializeDefaultTargetRoster()
    {
        upgrades = CloneEntries(GetDefaultEntries());
    }

    public static PowerupUpgradeEntry[] GetDefaultEntries()
    {
        if (_defaultEntries != null)
            return _defaultEntries;

        _defaultEntries = new[]
        {
            CreateEntry(
                PowerupType.ScoreMultiplier,
                baseCost: 150,
                costIncrease: 100,
                levels: new[]
                {
                    CreateLevel(6f, 2f),
                    CreateLevel(7f, 2.25f),
                    CreateLevel(8f, 2.5f),
                    CreateLevel(9f, 3f)
                }),
            CreateEntry(
                PowerupType.CoinMultiplier,
                baseCost: 150,
                costIncrease: 100,
                levels: new[]
                {
                    CreateLevel(6f, 2f),
                    CreateLevel(7f, 2.25f),
                    CreateLevel(8f, 2.5f),
                    CreateLevel(9f, 3f)
                }),
            CreateEntry(
                PowerupType.Magnet,
                baseCost: 175,
                costIncrease: 125,
                levels: new[]
                {
                    CreateLevel(6f, 2f),
                    CreateLevel(7f, 2.5f),
                    CreateLevel(8f, 3f),
                    CreateLevel(9f, 3.5f)
                }),
            CreateEntry(
                PowerupType.AutoPilot,
                baseCost: 200,
                costIncrease: 150,
                levels: new[]
                {
                    CreateLevel(4f, 0f),
                    CreateLevel(4.5f, 0f),
                    CreateLevel(5f, 0f),
                    CreateLevel(6f, 0f)
                }),
            CreateEntry(
                PowerupType.Shield,
                baseCost: 225,
                costIncrease: 175,
                levels: new[]
                {
                    CreateLevel(5f, 0f),
                    CreateLevel(6f, 0f),
                    CreateLevel(7f, 0f),
                    CreateLevel(8f, 0f)
                })
        };

        return _defaultEntries;
    }

    public static PowerupEntry[] GetDefaultSpawnEntries()
    {
        return new[]
        {
            CreateSpawnEntry(PowerupType.ScoreMultiplier, 4),
            CreateSpawnEntry(PowerupType.CoinMultiplier, 4),
            CreateSpawnEntry(PowerupType.Magnet, 3),
            CreateSpawnEntry(PowerupType.AutoPilot, 2),
            CreateSpawnEntry(PowerupType.Shield, 2)
        };
    }

    public static PowerupUpgradeEntry[] CloneEntries(PowerupUpgradeEntry[] sourceEntries)
    {
        if (sourceEntries == null || sourceEntries.Length == 0)
            return System.Array.Empty<PowerupUpgradeEntry>();

        var clones = new PowerupUpgradeEntry[sourceEntries.Length];
        for (int i = 0; i < sourceEntries.Length; i++)
        {
            clones[i] = CloneEntry(sourceEntries[i]);
        }

        return clones;
    }

    public static PowerupUpgradeEntry CloneEntry(PowerupUpgradeEntry source)
    {
        if (source == null)
            return null;

        return new PowerupUpgradeEntry
        {
            powerupType = source.powerupType,
            displayName = string.IsNullOrWhiteSpace(source.displayName)
                ? GetDisplayName(source.powerupType)
                : source.displayName,
            icon = source.icon,
            baseCost = source.baseCost,
            costIncrease = source.costIncrease,
            levels = CloneLevels(source.levels)
        };
    }

    public static PowerupUpgradeLevel[] CloneLevels(PowerupUpgradeLevel[] sourceLevels)
    {
        if (sourceLevels == null || sourceLevels.Length == 0)
            return System.Array.Empty<PowerupUpgradeLevel>();

        var clones = new PowerupUpgradeLevel[sourceLevels.Length];
        for (int i = 0; i < sourceLevels.Length; i++)
        {
            clones[i] = sourceLevels[i];
        }

        return clones;
    }

    private static PowerupUpgradeEntry CreateEntry(
        PowerupType powerupType,
        int baseCost,
        int costIncrease,
        PowerupUpgradeLevel[] levels)
    {
        return new PowerupUpgradeEntry
        {
            powerupType = powerupType,
            displayName = GetDisplayName(powerupType),
            baseCost = baseCost,
            costIncrease = costIncrease,
            levels = levels
        };
    }

    private static PowerupEntry CreateSpawnEntry(PowerupType powerupType, int weight)
    {
        return new PowerupEntry
        {
            type = powerupType,
            weight = Mathf.Max(1, weight)
        };
    }

    private static PowerupUpgradeEntry CloneEntryWithFallbacks(PowerupUpgradeEntry source)
    {
        PowerupUpgradeEntry clone = CloneEntry(source);
        PowerupUpgradeEntry defaultEntry = GetDefaultEntry(source.powerupType);

        if (clone == null)
            return defaultEntry;

        if (clone.baseCost <= 0 && defaultEntry != null)
            clone.baseCost = defaultEntry.baseCost;

        if (clone.costIncrease <= 0 && defaultEntry != null)
            clone.costIncrease = defaultEntry.costIncrease;

        if (clone.levels == null || clone.levels.Length == 0)
            clone.levels = defaultEntry != null ? CloneLevels(defaultEntry.levels) : System.Array.Empty<PowerupUpgradeLevel>();

        if (string.IsNullOrWhiteSpace(clone.displayName))
            clone.displayName = GetDisplayName(clone.powerupType);

        return clone;
    }

    private static PowerupUpgradeLevel CreateLevel(float duration, float strength)
    {
        return new PowerupUpgradeLevel
        {
            duration = duration,
            strength = strength
        };
    }

    public static bool IsTargetGameplayPowerup(PowerupType type)
    {
        switch (type)
        {
            case PowerupType.ScoreMultiplier:
            case PowerupType.CoinMultiplier:
            case PowerupType.Magnet:
            case PowerupType.AutoPilot:
            case PowerupType.Shield:
                return true;
            default:
                return false;
        }
    }

    public static string GetDisplayName(PowerupType type)
    {
        switch (type)
        {
            case PowerupType.ScoreMultiplier:
                return "x2 Score";
            case PowerupType.CoinMultiplier:
                return "x2 Coin Spawn";
            case PowerupType.Magnet:
                return "Magnet";
            case PowerupType.AutoPilot:
                return "Autopilot";
            case PowerupType.Shield:
                return "Shield";
            default:
                return type.ToString();
        }
    }

    public static string GetShortDisplayName(PowerupType type)
    {
        switch (type)
        {
            case PowerupType.ScoreMultiplier:
                return "Score";
            case PowerupType.CoinMultiplier:
                return "Coins";
            case PowerupType.Magnet:
                return "Magnet";
            case PowerupType.AutoPilot:
                return "Auto";
            case PowerupType.Shield:
                return "Shield";
            default:
                return type.ToString();
        }
    }
}
