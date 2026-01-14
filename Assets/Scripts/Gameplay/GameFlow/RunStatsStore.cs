using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized persistence for run and lifetime stats.
/// </summary>
public static class RunStatsStore
{
    private const string TotalDistanceKey = "Stats_TotalDistance";
    private const string LongestRunDistanceKey = "Stats_LongestRunDistance";
    private const string MaxSpeedKey = "Stats_MaxSpeed";
    private const string HighestComboKey = "Stats_HighestCombo";
    private const string TotalCoinsKey = "Stats_TotalCoins";
    private const string TotalPowerupsKey = "Stats_TotalPowerups";
    private const string TotalDeathsKey = "Stats_TotalDeaths";
    private const string TotalGlancingHitsKey = "Stats_TotalGlancingHits";
    private const string TotalObstaclesDodgedKey = "Stats_TotalObstaclesDodged";
    private const string DeathsBySourceKey = "Stats_DeathsBySource";
    private const string GlancingBySourceKey = "Stats_GlancingBySource";
    private const string PowerupsByTypeKey = "Stats_PowerupsByType";
    private const string BestScoreKey = "BestScore";

    [System.Serializable]
    private struct StringIntEntry
    {
        public string key;
        public int value;
    }

    [System.Serializable]
    private class StringIntList
    {
        public List<StringIntEntry> entries = new List<StringIntEntry>();
    }

    public static float BestScore => PlayerPrefs.GetFloat(BestScoreKey, 0f);

    public static void SetBestScore(float bestScore)
    {
        PlayerPrefs.SetFloat(BestScoreKey, bestScore);
    }

    public static void AddTotalDistance(float distance)
    {
        float totalDistance = PlayerPrefs.GetFloat(TotalDistanceKey, 0f) + distance;
        PlayerPrefs.SetFloat(TotalDistanceKey, totalDistance);
    }

    public static void AddTotalCoins(int coins)
    {
        int totalCoins = PlayerPrefs.GetInt(TotalCoinsKey, 0) + coins;
        PlayerPrefs.SetInt(TotalCoinsKey, totalCoins);
    }

    public static void AddTotalPowerups(int powerups)
    {
        int totalPowerups = PlayerPrefs.GetInt(TotalPowerupsKey, 0) + powerups;
        PlayerPrefs.SetInt(TotalPowerupsKey, totalPowerups);
    }

    public static void AddTotalDeaths(int deaths)
    {
        int totalDeaths = PlayerPrefs.GetInt(TotalDeathsKey, 0) + deaths;
        PlayerPrefs.SetInt(TotalDeathsKey, totalDeaths);
    }

    public static void AddTotalGlancingHits(int glancingHits)
    {
        int totalGlancingHits = PlayerPrefs.GetInt(TotalGlancingHitsKey, 0) + glancingHits;
        PlayerPrefs.SetInt(TotalGlancingHitsKey, totalGlancingHits);
    }

    public static void AddTotalObstaclesDodged(int obstaclesDodged)
    {
        int totalObstaclesDodged = PlayerPrefs.GetInt(TotalObstaclesDodgedKey, 0) + obstaclesDodged;
        PlayerPrefs.SetInt(TotalObstaclesDodgedKey, totalObstaclesDodged);
    }

    public static void SetLongestRunDistance(float distance)
    {
        float longestRun = PlayerPrefs.GetFloat(LongestRunDistanceKey, 0f);
        if (distance > longestRun)
        {
            PlayerPrefs.SetFloat(LongestRunDistanceKey, distance);
        }
    }

    public static void SetMaxSpeed(float maxSpeed)
    {
        float storedMaxSpeed = PlayerPrefs.GetFloat(MaxSpeedKey, 0f);
        if (maxSpeed > storedMaxSpeed)
        {
            PlayerPrefs.SetFloat(MaxSpeedKey, maxSpeed);
        }
    }

    public static void SetHighestCombo(float highestCombo)
    {
        float storedHighestCombo = PlayerPrefs.GetFloat(HighestComboKey, 0f);
        if (highestCombo > storedHighestCombo)
        {
            PlayerPrefs.SetFloat(HighestComboKey, highestCombo);
        }
    }

    public static void MergeDeathsBySource(Dictionary<string, int> delta)
    {
        MergeStringIntMap(DeathsBySourceKey, delta);
    }

    public static void MergeGlancingHitsBySource(Dictionary<string, int> delta)
    {
        MergeStringIntMap(GlancingBySourceKey, delta);
    }

    public static void MergePowerupsByType(Dictionary<PowerupType, int> delta)
    {
        if (delta == null || delta.Count == 0)
            return;

        var current = LoadStringIntMap(PowerupsByTypeKey);
        foreach (var kvp in delta)
        {
            string key = kvp.Key.ToString();
            if (current.ContainsKey(key))
            {
                current[key] += kvp.Value;
            }
            else
            {
                current[key] = kvp.Value;
            }
        }

        SaveStringIntMap(PowerupsByTypeKey, current);
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    private static void MergeStringIntMap(string prefsKey, Dictionary<string, int> delta)
    {
        if (delta == null || delta.Count == 0)
            return;

        var current = LoadStringIntMap(prefsKey);
        foreach (var kvp in delta)
        {
            if (current.ContainsKey(kvp.Key))
            {
                current[kvp.Key] += kvp.Value;
            }
            else
            {
                current[kvp.Key] = kvp.Value;
            }
        }

        SaveStringIntMap(prefsKey, current);
    }

    private static Dictionary<string, int> LoadStringIntMap(string prefsKey)
    {
        string json = PlayerPrefs.GetString(prefsKey, string.Empty);
        if (string.IsNullOrEmpty(json))
            return new Dictionary<string, int>();

        var list = JsonUtility.FromJson<StringIntList>(json);
        var result = new Dictionary<string, int>();
        if (list == null || list.entries == null)
            return result;

        for (int i = 0; i < list.entries.Count; i++)
        {
            var entry = list.entries[i];
            if (string.IsNullOrEmpty(entry.key))
                continue;

            result[entry.key] = entry.value;
        }

        return result;
    }

    private static void SaveStringIntMap(string prefsKey, Dictionary<string, int> map)
    {
        var list = new StringIntList();
        foreach (var kvp in map)
        {
            list.entries.Add(new StringIntEntry
            {
                key = kvp.Key,
                value = kvp.Value
            });
        }

        string json = JsonUtility.ToJson(list);
        PlayerPrefs.SetString(prefsKey, json);
    }
}
