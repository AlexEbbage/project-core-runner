using System;
using System.Collections.Generic;
using UnityEngine;

public enum BoosterFamily
{
    Score,
    Rewards,
    Speed
}

[Serializable]
public class BoosterDefinition
{
    public string id;
    public BoosterFamily family = BoosterFamily.Score;
    public string displayName = "Booster";
    [TextArea]
    public string description = "Boosts the run.";
    public float multiplier = 1.1f;
    public bool unlockedByDefault = true;
}

[CreateAssetMenu(menuName = "Main Menu/Booster Catalog")]
public class BoosterCatalog : ScriptableObject
{
    [SerializeField] private List<BoosterDefinition> boosters = new();

    public IReadOnlyList<BoosterDefinition> Boosters => boosters;

    public BoosterDefinition[] GetResolvedBoosters()
    {
        if (boosters != null && boosters.Count > 0)
        {
            List<BoosterDefinition> resolved = new();
            for (int i = 0; i < boosters.Count; i++)
            {
                BoosterDefinition booster = boosters[i];
                if (booster == null || string.IsNullOrWhiteSpace(booster.id))
                    continue;

                resolved.Add(booster);
            }

            if (resolved.Count > 0)
                return resolved.ToArray();
        }

        return GetDefaultBoosters();
    }

    public BoosterDefinition GetBooster(string boosterId)
    {
        if (string.IsNullOrWhiteSpace(boosterId))
            return null;

        BoosterDefinition[] resolved = GetResolvedBoosters();
        for (int i = 0; i < resolved.Length; i++)
        {
            BoosterDefinition booster = resolved[i];
            if (booster != null && booster.id == boosterId)
                return booster;
        }

        return null;
    }

    public BoosterDefinition[] GetBoostersForFamily(BoosterFamily family)
    {
        BoosterDefinition[] resolved = GetResolvedBoosters();
        List<BoosterDefinition> matches = new();

        for (int i = 0; i < resolved.Length; i++)
        {
            BoosterDefinition booster = resolved[i];
            if (booster != null && booster.family == family)
                matches.Add(booster);
        }

        return matches.ToArray();
    }

    public static BoosterDefinition[] GetDefaultBoosters()
    {
        return new[]
        {
            new BoosterDefinition
            {
                id = "score_combo_surge",
                family = BoosterFamily.Score,
                displayName = "Combo Surge",
                description = "Increase score and pickup scoring by 15% for the whole run.",
                multiplier = 1.15f,
                unlockedByDefault = true
            },
            new BoosterDefinition
            {
                id = "score_precision_line",
                family = BoosterFamily.Score,
                displayName = "Precision Line",
                description = "Increase score and pickup scoring by 25% for the whole run.",
                multiplier = 1.25f,
                unlockedByDefault = true
            },
            new BoosterDefinition
            {
                id = "reward_gold_rush",
                family = BoosterFamily.Rewards,
                displayName = "Gold Rush",
                description = "Increase run rewards by 15% after the run ends.",
                multiplier = 1.15f,
                unlockedByDefault = true
            },
            new BoosterDefinition
            {
                id = "reward_bonus_cache",
                family = BoosterFamily.Rewards,
                displayName = "Bonus Cache",
                description = "Increase run rewards by 25% after the run ends.",
                multiplier = 1.25f,
                unlockedByDefault = true
            },
            new BoosterDefinition
            {
                id = "speed_nitro_start",
                family = BoosterFamily.Speed,
                displayName = "Nitro Start",
                description = "Increase run speed by 8% from the moment the run begins.",
                multiplier = 1.08f,
                unlockedByDefault = true
            },
            new BoosterDefinition
            {
                id = "speed_flow_state",
                family = BoosterFamily.Speed,
                displayName = "Flow State",
                description = "Increase run speed by 12% from the moment the run begins.",
                multiplier = 1.12f,
                unlockedByDefault = true
            }
        };
    }
}
