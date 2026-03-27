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
    private const string DailyLoginRewardsConfigResourcePath = "DailyLoginRewardsConfig";

    [SerializeField] private PlayerProfile profile;
    [SerializeField] private DailyLoginRewardsConfig rewardsConfig;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private bool autoClaimOnStart;
    [SerializeField] private bool logRewards = true;

    private void Awake()
    {
        EnsureReferences();
    }

    private void Start()
    {
        if (autoClaimOnStart)
            TryClaimReward();
    }

    public bool TryClaimReward(bool doubleReward = false)
    {
        EnsureReferences();

        if (profile == null || rewardsConfig == null)
            return false;

        DateTime today = DateTime.UtcNow.Date;
        if (!profile.CanClaimDailyLogin(today))
            return false;

        int nextDayIndex = profile.GetNextDailyLoginDayIndex(today);
        DailyLoginRewardEntry reward = rewardsConfig.GetRewardForDay(nextDayIndex);
        GrantReward(reward, doubleReward);
        profile.MarkDailyLoginClaimed(today, nextDayIndex);

        gameManager?.LogAnalyticsEvent(AnalyticsEventNames.DailyLoginClaimed, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Source, "daily_login" },
            { AnalyticsEventNames.Params.DayIndex, nextDayIndex },
            { AnalyticsEventNames.Params.RewardKind, reward.rewardType.ToString() },
            { AnalyticsEventNames.Params.Amount, reward.amount * (doubleReward ? 2 : 1) },
            { AnalyticsEventNames.Params.Id, reward.itemId }
        });

        if (logRewards)
            Debug.Log($"DailyLoginRewards: Claimed day {nextDayIndex} ({reward.rewardType}).");

        return true;
    }

    public bool CanClaimToday()
    {
        EnsureReferences();
        return profile != null && profile.CanClaimDailyLogin(DateTime.UtcNow.Date);
    }

    public DailyLoginRewardEntry GetNextRewardPreview(out int nextDayIndex)
    {
        EnsureReferences();
        nextDayIndex = profile != null ? profile.GetNextDailyLoginDayIndex(DateTime.UtcNow.Date) : 0;
        return rewardsConfig != null
            ? rewardsConfig.GetRewardForDay(nextDayIndex)
            : default;
    }

    public int GetCurrentStreakDay()
    {
        EnsureReferences();
        return profile != null ? profile.GetDailyLoginDayIndex() : 0;
    }

    private void EnsureReferences()
    {
        if (profile == null)
        {
            PlayerProfile[] profiles = Resources.FindObjectsOfTypeAll<PlayerProfile>();
            if (profiles != null && profiles.Length > 0)
                profile = profiles[0];
        }

        if (rewardsConfig == null)
        {
            rewardsConfig = Resources.Load<DailyLoginRewardsConfig>(DailyLoginRewardsConfigResourcePath);

            DailyLoginRewardsConfig[] configs = rewardsConfig == null ? Resources.FindObjectsOfTypeAll<DailyLoginRewardsConfig>() : null;
            if (configs != null && configs.Length > 0)
                rewardsConfig = configs[0];
        }

        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
    }

    private void GrantReward(DailyLoginRewardEntry reward, bool doubleReward)
    {
        int amountMultiplier = doubleReward ? 2 : 1;

        switch (reward.rewardType)
        {
            case DailyLoginRewardType.SoftCurrency:
                profile.GrantProfileReward(ProfileGrantType.SoftCurrency, reward.amount * amountMultiplier);
                break;
            case DailyLoginRewardType.PremiumCurrency:
                profile.GrantProfileReward(ProfileGrantType.PremiumCurrency, reward.amount * amountMultiplier);
                break;
            case DailyLoginRewardType.Skin:
            case DailyLoginRewardType.Item:
                if (!string.IsNullOrEmpty(reward.itemId))
                    profile.GrantProfileReward(ProfileGrantType.UnlockItem, reward.amount, reward.itemId);
                break;
        }
    }
}
