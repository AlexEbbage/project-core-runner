using UnityEngine;

/// <summary>
/// Central balancing config for core gameplay values.
/// If assigned to systems, it overrides their local inspector values.
/// Create via Assets -> Create -> Game Config -> Game Balance Config.
/// </summary>
[CreateAssetMenu(
    fileName = "GameBalanceConfig",
    menuName = "Game Config/Game Balance Config")]
public class GameBalanceConfig : ScriptableObject
{
    [Header("Player Health")]
    public float maxHealth = 100f;
    public float sideScrapeDamage = 25f;
    public float sideScrapeCooldown = 0.2f;

    [Header("Scoring & Combo")]
    public float distanceScoreMultiplier = 1f;
    public int pickupBaseScore = 10;
    public float comboIncreasePerPickup = 1f;
    public float maxComboValue = 10f;
    public float comboDecayPerSecond = 1f;
    public float comboToMultiplierFactor = 0.1f;

    [Header("Obstacles")]
    public int obstacleLaneCount = 3;
    public float obstacleLaneSpacing = 3f;
    [Range(0f, 1f)] public float obstacleSpawnProbability = 0.7f;
    public int obstacleSpawnEveryNthSegment = 2;

    [Header("Pickups")]
    public int coinValue = 1;
    public int pickupLaneCount = 3;
    public float pickupLaneSpacing = 3f;
    [Range(0f, 1f)] public float pickupSegmentProbability = 0.7f;
    public int pickupMinPerSegment = 1;
    public int pickupMaxPerSegment = 2;
    public float pickupFloatHeight = 0.5f;
    public float pickupSurfaceOffset = 0.25f;

    [Header("Powerups")]
    [Range(0f, 1f)] public float powerupSpawnChance = 0.15f;
    public float autoPilotDuration = 4f;
    public float coinMultiplierValue = 2f;
    public float coinMultiplierDuration = 6f;
    public float scoreMultiplierValue = 2f;
    public float scoreMultiplierDuration = 6f;
    public float magnetDuration = 6f;
    public float magnetRadiusMultiplier = 2f;
    public float shieldDuration = 5f;
    public float coinBonanzaSpawnMultiplier = 2.5f;
    public float coinBonanzaDuration = 6f;
    public float speedBoostMultiplier = 1.4f;
    public float speedBoostDuration = 5f;
    public float slowMoTimeScale = 0.6f;
    public float slowMoDuration = 4f;

    [Header("Upgrade Scaling")]
    [Tooltip("Added to combo multiplier factor per upgrade level.")]
    public float comboMultiplierFactorPerLevel = 0.02f;
    [Tooltip("Added to pickup radius multiplier per upgrade level (1 = base).")]
    public float pickupRadiusMultiplierPerLevel = 0.05f;
    [Tooltip("Base cooldown (seconds) before shield can be reactivated after breaking/ending.")]
    public float shieldRechargeSeconds = 5f;
    [Tooltip("Added to shield recharge seconds per upgrade level (can be negative).")]
    public float shieldRechargeSecondsPerLevel = -0.5f;

    [Header("Continues")]
    public int maxContinuesPerRun = 3;
    public float continueRespawnBackDistance = 8f;
    public float continueRespawnHeightOffset = 0.5f;
}

public enum RewardedOfferRewardKind
{
    Powerup,
    SoftCurrency,
    PremiumCurrency
}

[System.Serializable]
public class RewardedOfferRewardEntry
{
    public RewardedOfferRewardKind rewardKind = RewardedOfferRewardKind.Powerup;
    public PowerupType powerupType = PowerupType.ScoreMultiplier;
    public int amount = 50;
    public int weight = 1;
    public string title;
    [TextArea(2, 4)] public string body;
    public string rewardLabel;

    public RewardedOfferRewardEntry Clone()
    {
        return new RewardedOfferRewardEntry
        {
            rewardKind = rewardKind,
            powerupType = powerupType,
            amount = amount,
            weight = weight,
            title = title,
            body = body,
            rewardLabel = rewardLabel
        };
    }

    public string GetResolvedTitle()
    {
        if (!string.IsNullOrWhiteSpace(title))
            return title;

        return rewardKind == RewardedOfferRewardKind.Powerup
            ? "Mid-Run Boost"
            : "Mid-Run Reward";
    }

    public string GetResolvedBody()
    {
        if (!string.IsNullOrWhiteSpace(body))
            return body;

        switch (rewardKind)
        {
            case RewardedOfferRewardKind.Powerup:
                return "Tap in to watch a rewarded ad and activate a temporary powerup.";
            case RewardedOfferRewardKind.PremiumCurrency:
                return "Tap in to watch a rewarded ad and claim premium currency.";
            case RewardedOfferRewardKind.SoftCurrency:
            default:
                return "Tap in to watch a rewarded ad and claim bonus soft currency.";
        }
    }

    public string GetResolvedRewardLabel()
    {
        if (!string.IsNullOrWhiteSpace(rewardLabel))
            return rewardLabel;

        switch (rewardKind)
        {
            case RewardedOfferRewardKind.Powerup:
                return PowerupUpgradeConfig.GetDisplayName(powerupType);
            case RewardedOfferRewardKind.PremiumCurrency:
                return $"+{Mathf.Max(1, amount)} Premium";
            case RewardedOfferRewardKind.SoftCurrency:
            default:
                return $"+{Mathf.Max(1, amount)} Coins";
        }
    }
}

[CreateAssetMenu(
    fileName = "RewardedOfferConfig",
    menuName = "Game Config/Rewarded Offer Config")]
public class RewardedOfferConfig : ScriptableObject
{
    [Header("Timing")]
    public bool enabled = true;
    public float firstOfferDelaySeconds = 45f;
    public float repeatIntervalSeconds = 45f;
    public float offerPopoutLifetimeSeconds = 8f;
    public float offerCooldownSeconds = 20f;

    [Header("Rewards")]
    public RewardedOfferRewardEntry[] rewards;

    public RewardedOfferRewardEntry[] GetResolvedRewards()
    {
        if (rewards == null || rewards.Length == 0)
            return GetDefaultRewards();

        var resolvedRewards = new RewardedOfferRewardEntry[rewards.Length];
        for (int i = 0; i < rewards.Length; i++)
        {
            RewardedOfferRewardEntry reward = rewards[i];
            resolvedRewards[i] = reward != null ? reward.Clone() : null;
        }

        return resolvedRewards;
    }

    public static RewardedOfferRewardEntry[] GetDefaultRewards()
    {
        return new[]
        {
            CreatePowerupReward(PowerupType.ScoreMultiplier, 7),
            CreatePowerupReward(PowerupType.CoinMultiplier, 7),
            CreatePowerupReward(PowerupType.Magnet, 6),
            CreatePowerupReward(PowerupType.AutoPilot, 4),
            CreatePowerupReward(PowerupType.Shield, 4),
            CreateCurrencyReward(RewardedOfferRewardKind.SoftCurrency, 150, 8, "Coin Cache"),
            CreateCurrencyReward(RewardedOfferRewardKind.SoftCurrency, 300, 5, "Big Coin Cache"),
            CreateCurrencyReward(RewardedOfferRewardKind.PremiumCurrency, 2, 2, "Gem Drop"),
            CreateCurrencyReward(RewardedOfferRewardKind.PremiumCurrency, 5, 1, "Rare Gem Drop")
        };
    }

    private static RewardedOfferRewardEntry CreatePowerupReward(PowerupType powerupType, int weight)
    {
        return new RewardedOfferRewardEntry
        {
            rewardKind = RewardedOfferRewardKind.Powerup,
            powerupType = powerupType,
            weight = Mathf.Max(1, weight),
            title = "Mid-Run Boost",
            body = "Tap in to watch a rewarded ad and activate a temporary powerup.",
            rewardLabel = PowerupUpgradeConfig.GetDisplayName(powerupType)
        };
    }

    private static RewardedOfferRewardEntry CreateCurrencyReward(RewardedOfferRewardKind rewardKind, int amount, int weight, string title)
    {
        return new RewardedOfferRewardEntry
        {
            rewardKind = rewardKind,
            amount = Mathf.Max(1, amount),
            weight = Mathf.Max(1, weight),
            title = title,
            body = rewardKind == RewardedOfferRewardKind.PremiumCurrency
                ? "Tap in to watch a rewarded ad and claim premium currency."
                : "Tap in to watch a rewarded ad and claim bonus soft currency."
        };
    }
}
