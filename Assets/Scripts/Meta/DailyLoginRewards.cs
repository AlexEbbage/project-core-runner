using System;
using System.Collections.Generic;
using UnityEngine;

public enum DailyLoginRewardType
{
    SoftCurrency,
    PremiumCurrency,
    Skin,
    Item
}

[Serializable]
public struct DailyLoginRewardEntry
{
    public int dayIndex;
    public DailyLoginRewardType rewardType;
    public int amount;
    public string itemId;
}

[CreateAssetMenu(menuName = "Main Menu/Daily Login Rewards")]
public class DailyLoginRewardsConfig : ScriptableObject
{
    [Tooltip("Fallback soft currency when no specific reward is configured for the day.")]
    public int defaultSoftCurrencyAmount = 150;

    [Tooltip("Specific rewards for certain days (ex: 7, 14, 30 for skins/items).")]
    public List<DailyLoginRewardEntry> rewards = new();

    private void OnValidate()
    {
        if (rewards == null)
            return;

        var seenDays = new HashSet<int>();
        for (int i = 0; i < rewards.Count; i++)
        {
            var entry = rewards[i];
            entry.dayIndex = Mathf.Max(1, entry.dayIndex);
            rewards[i] = entry;

            if (!seenDays.Add(entry.dayIndex))
            {
                Debug.LogWarning($"DailyLoginRewardsConfig: duplicate day index {entry.dayIndex} configured.", this);
            }

            if ((entry.rewardType == DailyLoginRewardType.Skin || entry.rewardType == DailyLoginRewardType.Item)
                && string.IsNullOrWhiteSpace(entry.itemId))
            {
                Debug.LogWarning($"DailyLoginRewardsConfig: reward day {entry.dayIndex} needs an item id.", this);
            }
        }
    }

    public DailyLoginRewardEntry GetRewardForDay(int dayIndex)
    {
        int normalizedDay = Mathf.Max(1, dayIndex);
        foreach (var entry in rewards)
        {
            if (entry.dayIndex == normalizedDay)
                return entry;
        }

        return new DailyLoginRewardEntry
        {
            dayIndex = normalizedDay,
            rewardType = DailyLoginRewardType.SoftCurrency,
            amount = Mathf.Max(0, defaultSoftCurrencyAmount),
            itemId = string.Empty
        };
    }

    public int GetNextConfiguredRewardDay(int fromDayExclusive)
    {
        if (rewards == null || rewards.Count == 0)
            return -1;

        int nextDay = int.MaxValue;
        foreach (var entry in rewards)
        {
            if (entry.dayIndex > fromDayExclusive && entry.dayIndex < nextDay)
                nextDay = entry.dayIndex;
        }

        return nextDay == int.MaxValue ? -1 : nextDay;
    }
}

public class DailyLoginRewardsManager : MonoBehaviour
{
    [SerializeField] private PlayerProfile profile;
    [SerializeField] private DailyLoginRewardsConfig rewardsConfig;
    [SerializeField] private bool autoClaimOnStart = true;
    [SerializeField] private bool logRewards = true;

    private const string LastClaimKey = "DailyLogin.LastClaimDate";
    private const string DayIndexKey = "DailyLogin.DayIndex";

    private void Start()
    {
        if (autoClaimOnStart)
            TryClaimReward();
    }

    public bool TryClaimReward()
    {
        if (profile == null || rewardsConfig == null)
            return false;

        DateTime today = DateTime.UtcNow.Date;
        if (!CanClaimToday(today, out int nextDayIndex))
            return false;

        var reward = rewardsConfig.GetRewardForDay(nextDayIndex);
        GrantReward(reward);

        PlayerPrefs.SetString(LastClaimKey, today.ToString("O"));
        PlayerPrefs.SetInt(DayIndexKey, nextDayIndex);
        PlayerPrefs.Save();

        if (logRewards)
            Debug.Log($"DailyLoginRewards: Claimed day {nextDayIndex} ({reward.rewardType}).");

        return true;
    }

    public bool CanClaimToday()
    {
        return CanClaimToday(DateTime.UtcNow.Date, out _);
    }

    public DailyLoginRewardEntry GetNextRewardPreview(out int nextDayIndex)
    {
        nextDayIndex = GetNextClaimDayIndex(DateTime.UtcNow.Date);
        return rewardsConfig != null
            ? rewardsConfig.GetRewardForDay(nextDayIndex)
            : default;
    }

    private bool TryGetLastClaimDate(out DateTime lastClaimDate)
    {
        lastClaimDate = default;
        string stored = PlayerPrefs.GetString(LastClaimKey, string.Empty);
        if (string.IsNullOrEmpty(stored))
            return false;

        return DateTime.TryParse(stored, null, System.Globalization.DateTimeStyles.RoundtripKind, out lastClaimDate);
    }

    private bool CanClaimToday(DateTime today, out int nextDayIndex)
    {
        nextDayIndex = GetNextClaimDayIndex(today);

        if (TryGetLastClaimDate(out DateTime lastClaimDate))
        {
            int daysSinceClaim = (today - lastClaimDate.Date).Days;
            return daysSinceClaim > 0;
        }

        return true;
    }

    private int GetNextClaimDayIndex(DateTime today)
    {
        int currentDay = PlayerPrefs.GetInt(DayIndexKey, 0);
        if (TryGetLastClaimDate(out DateTime lastClaimDate))
        {
            int daysSinceClaim = (today - lastClaimDate.Date).Days;
            if (daysSinceClaim <= 0)
                return Mathf.Max(1, currentDay + 1);

            return daysSinceClaim == 1 ? currentDay + 1 : 1;
        }

        return 1;
    }

    private void GrantReward(DailyLoginRewardEntry reward)
    {
        switch (reward.rewardType)
        {
            case DailyLoginRewardType.SoftCurrency:
                profile.AddCurrency(ShopCurrencyType.Soft, reward.amount);
                break;
            case DailyLoginRewardType.PremiumCurrency:
                profile.AddCurrency(ShopCurrencyType.Premium, reward.amount);
                break;
            case DailyLoginRewardType.Skin:
            case DailyLoginRewardType.Item:
                if (!string.IsNullOrEmpty(reward.itemId))
                    profile.UnlockItem(reward.itemId);
                break;
        }
    }
}
