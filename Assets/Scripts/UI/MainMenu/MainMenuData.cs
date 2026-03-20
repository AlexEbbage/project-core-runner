using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public enum MainPage
{
    Shop,
    Hangar,
    Play,
    Challenges,
    Progression,
    Ship = Hangar,
    LevelSelect = Play,
    Achievements = Challenges,
    Tasks = Progression,
    Lab = 5
}

public enum HangarTab
{
    Upgrades,
    Skins,
    Trails,
    CoreFx,
    Ships
}

public enum ShopTab
{
    Skins,
    Ships,
    Trails,
    Currency
}

public enum ShipStatType
{
    Speed,
    Handling,
    Stability,
    Boost,
    Energy
}

public enum UpgradeType
{
    ComboMultiplier,
    PickupRadius,
    Handling,
    ShieldRecharge
}

public enum ShopCurrencyType
{
    Soft,
    Premium
}

public enum ProfileGrantType
{
    SoftCurrency,
    PremiumCurrency,
    Xp,
    UnlockItem
}

public enum AchievementMetricType
{
    ProfileLevel,
    UnlockedItems,
    TotalUpgradeLevels,
    SoftCurrencyBalance,
    PremiumCurrencyBalance
}

public enum ShopItemAction
{
    UnlockItem,
    OpenRemoveAdsPurchase,
    RestorePurchases
}

[System.Serializable]
public struct ShipStats
{
    public float speed;
    public float handling;
    public float stability;
    public float boost;
    public float energy;

    public float GetValue(ShipStatType type)
    {
        return type switch
        {
            ShipStatType.Speed => speed,
            ShipStatType.Handling => handling,
            ShipStatType.Stability => stability,
            ShipStatType.Boost => boost,
            ShipStatType.Energy => energy,
            _ => 0f
        };
    }
}

[CreateAssetMenu(menuName = "Main Menu/Ship Definition")]
public class ShipDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;
    public ShipStats baseStats;
}

[CreateAssetMenu(menuName = "Main Menu/Ship Upgrade Definition")]
public class ShipUpgradeDefinition : ScriptableObject
{
    public UpgradeType upgradeType;
    public string displayName;
    public Sprite icon;
    public int maxLevel = 5;
    public int baseCost = 100;
    public int costIncrease = 50;

    public int GetCostForLevel(int level)
    {
        return Mathf.Max(0, baseCost + costIncrease * Mathf.Max(0, level));
    }
}

[CreateAssetMenu(menuName = "Main Menu/Ship Skin Definition")]
public class ShipSkinDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;
    public int cost;
    public GameObject prefab;
}

[CreateAssetMenu(menuName = "Main Menu/Ship Trail Definition")]
public class ShipTrailDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;
    public int cost;
    public GameObject prefab;
}

[CreateAssetMenu(menuName = "Main Menu/Ship Core FX Definition")]
public class ShipCoreFxDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;
    public int cost;
    public GameObject prefab;
}

[CreateAssetMenu(menuName = "Main Menu/Shop Item Definition")]
public class ShopItemDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public string description;
    public Sprite icon;
    public ShopTab tab;
    public ShopCurrencyType currencyType;
    public int price;
    public ShopItemAction action = ShopItemAction.UnlockItem;
}

[CreateAssetMenu(menuName = "Main Menu/Ship Database")]
public class ShipDatabase : ScriptableObject
{
    public ShipDefinition[] ships;
    public ShipUpgradeDefinition[] upgrades;
    public ShipSkinDefinition[] skins;
    public ShipTrailDefinition[] trails;
    public ShipCoreFxDefinition[] coreFx;

    public ShipDefinition GetShip(string shipId)
    {
        if (ships == null)
            return null;

        foreach (var ship in ships)
        {
            if (ship != null && ship.id == shipId)
                return ship;
        }

        return null;
    }

    public ShipSkinDefinition GetSkin(string skinId)
    {
        if (skins == null)
            return null;

        foreach (var skin in skins)
        {
            if (skin != null && skin.id == skinId)
                return skin;
        }

        return null;
    }

    public ShipTrailDefinition GetTrail(string trailId)
    {
        if (trails == null)
            return null;

        foreach (var trail in trails)
        {
            if (trail != null && trail.id == trailId)
                return trail;
        }

        return null;
    }

    public ShipCoreFxDefinition GetCoreFx(string coreFxId)
    {
        if (coreFx == null)
            return null;

        foreach (var fx in coreFx)
        {
            if (fx != null && fx.id == coreFxId)
                return fx;
        }

        return null;
    }
}

[CreateAssetMenu(menuName = "Main Menu/Shop Database")]
public class ShopDatabase : ScriptableObject
{
    public ShopItemDefinition[] skinItems;
    public ShopItemDefinition[] shipItems;
    public ShopItemDefinition[] trailItems;
    public ShopItemDefinition[] currencyItems;

    public IEnumerable<ShopItemDefinition> GetItemsForTab(ShopTab tab)
    {
        return tab switch
        {
            ShopTab.Skins => skinItems,
            ShopTab.Ships => shipItems,
            ShopTab.Trails => trailItems,
            ShopTab.Currency => currencyItems,
            _ => skinItems
        };
    }
}

[System.Serializable]
public class AchievementTierDefinition
{
    public int targetValue = 1;
    public string rewardLabel = "50 Coins";
    public ProfileGrantType rewardType = ProfileGrantType.SoftCurrency;
    public int rewardAmount = 50;
    public string rewardItemId;
}

[System.Serializable]
public class AchievementDefinition
{
    public string id;
    public string title = "Achievement";
    public string description = "Complete milestone goals.";
    public AchievementMetricType metricType = AchievementMetricType.ProfileLevel;
    public List<AchievementTierDefinition> tiers = new();
}

[CreateAssetMenu(menuName = "Main Menu/Achievements Config")]
public class AchievementsConfig : ScriptableObject
{
    public List<AchievementDefinition> achievements = new();
}

[CreateAssetMenu(menuName = "Main Menu/Player Profile")]
public class PlayerProfile : ScriptableObject
{
    private const string ProfileKey = "PlayerProfile";
    private const string ProfileHashKey = "PlayerProfileHash";
    private const string HashSalt = "profile_v1";
    public const int XpPerLevel = 1000;

    public event System.Action<UpgradeType, int> UpgradeLevelChanged;
    public event System.Action<int, int> LevelChanged;

    public int level = 1;
    public int xp;
    public int softCurrency = 1000;
    public int premiumCurrency = 50;
    public string selectedShipId;
    [SerializeField] private int selectedLevelIndex;

    [SerializeField] private List<string> unlockedItemIds = new();
    [SerializeField] private List<UpgradeLevelEntry> upgradeLevels = new();
    [SerializeField] private List<PowerupUpgradeLevelEntry> powerupUpgradeLevels = new();
    [SerializeField] private List<string> unlockedBoosterIds = new();
    [SerializeField] private List<BoosterSelectionEntry> boosterLoadout = new();
    [SerializeField] private int dailyLoginDayIndex;
    [SerializeField] private long dailyLoginLastClaimTicks;
    [SerializeField] private List<TaskCadenceState> taskCadenceStates = new();
    [SerializeField] private List<AchievementClaimState> achievementClaimStates = new();

    private void OnEnable()
    {
        if (!Application.isPlaying)
            return;

        Load();
    }

    public void Load()
    {
        if (!PlayerPrefs.HasKey(ProfileKey))
            return;

        string json = PlayerPrefs.GetString(ProfileKey, string.Empty);
        if (string.IsNullOrEmpty(json))
            return;

        string storedHash = PlayerPrefs.GetString(ProfileHashKey, string.Empty);
        if (string.IsNullOrEmpty(storedHash) || storedHash != ComputeHash(json))
            return;

        var data = JsonUtility.FromJson<PlayerProfileData>(json);
        if (data == null)
            return;

        level = Mathf.Max(1, data.level);
        xp = Mathf.Max(0, data.xp);
        softCurrency = Mathf.Max(0, data.softCurrency);
        premiumCurrency = Mathf.Max(0, data.premiumCurrency);
        selectedShipId = data.selectedShipId;
        selectedSkinId = data.selectedSkinId;
        selectedTrailId = data.selectedTrailId;
        selectedCoreFxId = data.selectedCoreFxId;
        selectedLevelIndex = Mathf.Max(0, data.selectedLevelIndex);
        unlockedItemIds = data.unlockedItemIds ?? new List<string>();
        upgradeLevels = data.upgradeLevels ?? new List<UpgradeLevelEntry>();
        powerupUpgradeLevels = data.powerupUpgradeLevels ?? new List<PowerupUpgradeLevelEntry>();
        unlockedBoosterIds = data.unlockedBoosterIds ?? new List<string>();
        boosterLoadout = data.boosterLoadout ?? new List<BoosterSelectionEntry>();
        dailyLoginDayIndex = Mathf.Max(0, data.dailyLoginDayIndex);
        dailyLoginLastClaimTicks = data.dailyLoginLastClaimTicks;
        taskCadenceStates = data.taskCadenceStates ?? new List<TaskCadenceState>();
        achievementClaimStates = data.achievementClaimStates ?? new List<AchievementClaimState>();
        NormalizeLevelProgress();
    }

    public void Save()
    {
        var data = new PlayerProfileData
        {
            level = level,
            xp = xp,
            softCurrency = softCurrency,
            premiumCurrency = premiumCurrency,
            selectedShipId = selectedShipId,
            unlockedItemIds = new List<string>(unlockedItemIds),
            upgradeLevels = new List<UpgradeLevelEntry>(upgradeLevels),
            powerupUpgradeLevels = new List<PowerupUpgradeLevelEntry>(powerupUpgradeLevels),
            unlockedBoosterIds = new List<string>(unlockedBoosterIds),
            boosterLoadout = new List<BoosterSelectionEntry>(boosterLoadout),
            dailyLoginDayIndex = dailyLoginDayIndex,
            dailyLoginLastClaimTicks = dailyLoginLastClaimTicks,
            taskCadenceStates = new List<TaskCadenceState>(taskCadenceStates),
            achievementClaimStates = new List<AchievementClaimState>(achievementClaimStates),
            selectedLevelIndex = Mathf.Max(0, selectedLevelIndex),
            selectedSkinId = selectedSkinId,
            selectedTrailId = selectedTrailId,
            selectedCoreFxId = selectedCoreFxId
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(ProfileKey, json);
        PlayerPrefs.SetString(ProfileHashKey, ComputeHash(json));
        PlayerPrefs.Save();
    }
    
    [SerializeField] private string selectedSkinId;
    [SerializeField] private string selectedTrailId;
    [SerializeField] private string selectedCoreFxId;

    public string SelectedSkinId => selectedSkinId;
    public string SelectedTrailId => selectedTrailId;
    public string SelectedCoreFxId => selectedCoreFxId;
    public int SelectedLevelIndex => Mathf.Max(0, selectedLevelIndex);

    public bool HasUnlocked(string itemId)
    {
        return unlockedItemIds.Contains(itemId);
    }

    public void UnlockItem(string itemId)
    {
        if (!unlockedItemIds.Contains(itemId))
        {
            unlockedItemIds.Add(itemId);
            Save();
        }
    }

    public void AddCurrency(ShopCurrencyType currencyType, int amount)
    {
        if (amount <= 0)
            return;

        if (currencyType == ShopCurrencyType.Soft)
        {
            softCurrency += amount;
            return;
        }

        premiumCurrency += amount;
    }

    public void AddCurrencyAndSave(ShopCurrencyType currencyType, int amount)
    {
        if (amount <= 0)
            return;

        AddCurrency(currencyType, amount);
        Save();
    }
    
    public void EnsureDefaults(ShipDatabase database)
    {
        if (database == null)
            return;

        string previousShipId = selectedShipId;
        string previousSkinId = selectedSkinId;
        string previousTrailId = selectedTrailId;
        string previousCoreFxId = selectedCoreFxId;

        EnsureDefaultShipUnlocked(database);
        selectedShipId = EnsureOwnedShipSelection(selectedShipId, database.ships);
        selectedSkinId = EnsureDefaultSelection(selectedSkinId, database.skins);
        selectedTrailId = EnsureDefaultSelection(selectedTrailId, database.trails);
        selectedCoreFxId = EnsureDefaultSelection(selectedCoreFxId, database.coreFx);

        if (previousShipId != selectedShipId
            || previousSkinId != selectedSkinId
            || previousTrailId != selectedTrailId
            || previousCoreFxId != selectedCoreFxId)
        {
            Save();
        }
    }

    public bool TrySelectSkin(string skinId, ShipDatabase database)
    {
        return TrySelectCosmetic(
            skinId,
            database != null ? database.GetSkin(skinId) : null,
            id => selectedSkinId = id);
    }

    public bool TrySelectTrail(string trailId, ShipDatabase database)
    {
        return TrySelectCosmetic(
            trailId,
            database != null ? database.GetTrail(trailId) : null,
            id => selectedTrailId = id);
    }

    public bool TrySelectCoreFx(string coreFxId, ShipDatabase database)
    {
        return TrySelectCosmetic(
            coreFxId,
            database != null ? database.GetCoreFx(coreFxId) : null,
            id => selectedCoreFxId = id);
    }

    public bool TrySelectShip(string shipId, ShipDatabase database)
    {
        ShipDefinition ship = database != null ? database.GetShip(shipId) : null;
        if (ship == null || string.IsNullOrEmpty(ship.id) || !HasUnlocked(ship.id))
            return false;

        selectedShipId = ship.id;
        Save();
        return true;
    }

    public bool TrySpend(ShopCurrencyType currencyType, int amount)
    {
        if (amount <= 0)
            return true;

        if (currencyType == ShopCurrencyType.Soft)
        {
            if (softCurrency < amount)
                return false;
            softCurrency -= amount;
            Save();
            return true;
        }

        if (premiumCurrency < amount)
            return false;

        premiumCurrency -= amount;
        Save();
        return true;
    }

    public int GetUpgradeLevel(UpgradeType upgradeType)
    {
        for (int i = 0; i < upgradeLevels.Count; i++)
        {
            if (upgradeLevels[i].upgradeType == upgradeType)
                return upgradeLevels[i].level;
        }

        return 0;
    }

    public void SetUpgradeLevel(UpgradeType upgradeType, int levelValue)
    {
        int clampedLevel = Mathf.Max(0, levelValue);
        int previousLevel = GetUpgradeLevel(upgradeType);

        if (previousLevel == clampedLevel)
            return;

        bool updated = false;
        for (int i = 0; i < upgradeLevels.Count; i++)
        {
            if (upgradeLevels[i].upgradeType == upgradeType)
            {
                upgradeLevels[i] = new UpgradeLevelEntry
                {
                    upgradeType = upgradeType,
                    level = clampedLevel
                };
                updated = true;
                break;
            }
        }

        if (!updated)
        {
            upgradeLevels.Add(new UpgradeLevelEntry
            {
                upgradeType = upgradeType,
                level = clampedLevel
            });
        }

        Save();
        UpgradeLevelChanged?.Invoke(upgradeType, clampedLevel);
    }

    public int GetPowerupUpgradeLevel(PowerupType powerupType)
    {
        for (int i = 0; i < powerupUpgradeLevels.Count; i++)
        {
            if (powerupUpgradeLevels[i].powerupType == powerupType)
                return powerupUpgradeLevels[i].level;
        }

        return 0;
    }

    public void SetPowerupUpgradeLevel(PowerupType powerupType, int levelValue)
    {
        int clampedLevel = Mathf.Max(0, levelValue);
        int previousLevel = GetPowerupUpgradeLevel(powerupType);

        if (previousLevel == clampedLevel)
            return;

        bool updated = false;
        for (int i = 0; i < powerupUpgradeLevels.Count; i++)
        {
            if (powerupUpgradeLevels[i].powerupType == powerupType)
            {
                powerupUpgradeLevels[i] = new PowerupUpgradeLevelEntry
                {
                    powerupType = powerupType,
                    level = clampedLevel
                };
                updated = true;
                break;
            }
        }

        if (!updated)
        {
            powerupUpgradeLevels.Add(new PowerupUpgradeLevelEntry
            {
                powerupType = powerupType,
                level = clampedLevel
            });
        }

        Save();
    }

    public void SetSelectedShip(string shipId)
    {
        selectedShipId = shipId;
        Save();
    }

    public void AddXp(int amount)
    {
        if (amount <= 0)
            return;

        int previousLevel = level;
        xp = Mathf.Max(0, xp + amount);
        NormalizeLevelProgress();
        Save();

        if (level > previousLevel)
            LevelChanged?.Invoke(previousLevel, level);
    }

    public int GetUnlockedItemCount()
    {
        return unlockedItemIds != null ? unlockedItemIds.Count : 0;
    }

    public int GetTotalUpgradeLevels()
    {
        int total = 0;

        for (int i = 0; i < upgradeLevels.Count; i++)
            total += Mathf.Max(0, upgradeLevels[i].level);

        for (int i = 0; i < powerupUpgradeLevels.Count; i++)
            total += Mathf.Max(0, powerupUpgradeLevels[i].level);

        return total;
    }

    public int GetAchievementMetricValue(AchievementMetricType metricType)
    {
        return metricType switch
        {
            AchievementMetricType.ProfileLevel => level,
            AchievementMetricType.UnlockedItems => GetUnlockedItemCount(),
            AchievementMetricType.TotalUpgradeLevels => GetTotalUpgradeLevels(),
            AchievementMetricType.SoftCurrencyBalance => softCurrency,
            AchievementMetricType.PremiumCurrencyBalance => premiumCurrency,
            _ => 0
        };
    }

    public void GrantProfileReward(ProfileGrantType rewardType, int rewardAmount, string rewardItemId = null)
    {
        switch (rewardType)
        {
            case ProfileGrantType.SoftCurrency:
                AddCurrency(ShopCurrencyType.Soft, rewardAmount);
                break;
            case ProfileGrantType.PremiumCurrency:
                AddCurrency(ShopCurrencyType.Premium, rewardAmount);
                break;
            case ProfileGrantType.Xp:
                AddXp(rewardAmount);
                return;
            case ProfileGrantType.UnlockItem:
                if (!string.IsNullOrWhiteSpace(rewardItemId))
                    UnlockItemWithoutSave(rewardItemId);
                break;
        }

        Save();
    }

    public bool HasUnlockedBooster(string boosterId)
    {
        return !string.IsNullOrWhiteSpace(boosterId) && unlockedBoosterIds.Contains(boosterId);
    }

    public string GetEquippedBoosterId(BoosterFamily family)
    {
        for (int i = 0; i < boosterLoadout.Count; i++)
        {
            if (boosterLoadout[i].family == family)
                return boosterLoadout[i].boosterId;
        }

        return string.Empty;
    }

    public void EnsureBoosterLoadout(BoosterDefinition[] definitions)
    {
        if (definitions == null || definitions.Length == 0)
            return;

        bool changed = false;
        Dictionary<BoosterFamily, string> firstUnlockedByFamily = new();

        for (int i = 0; i < definitions.Length; i++)
        {
            BoosterDefinition definition = definitions[i];
            if (definition == null || string.IsNullOrWhiteSpace(definition.id))
                continue;

            if (definition.unlockedByDefault && !HasUnlockedBooster(definition.id))
            {
                UnlockBoosterWithoutSave(definition.id);
                changed = true;
            }

            if (!HasUnlockedBooster(definition.id))
                continue;

            if (!firstUnlockedByFamily.ContainsKey(definition.family))
                firstUnlockedByFamily[definition.family] = definition.id;
        }

        foreach (BoosterFamily family in System.Enum.GetValues(typeof(BoosterFamily)))
        {
            string equippedId = GetEquippedBoosterId(family);
            if (!string.IsNullOrWhiteSpace(equippedId) && HasUnlockedBooster(equippedId))
                continue;

            if (firstUnlockedByFamily.TryGetValue(family, out string defaultId))
            {
                if (SetBoosterSelectionWithoutSave(family, defaultId))
                    changed = true;
            }
        }

        if (changed)
            Save();
    }

    public bool TryEquipBooster(BoosterFamily family, string boosterId, BoosterDefinition[] definitions = null)
    {
        if (string.IsNullOrWhiteSpace(boosterId))
            return false;

        if (definitions != null && definitions.Length > 0)
        {
            for (int i = 0; i < definitions.Length; i++)
            {
                BoosterDefinition definition = definitions[i];
                if (definition == null || definition.family != family || definition.id != boosterId)
                    continue;

                if (!HasUnlockedBooster(definition.id) && !definition.unlockedByDefault)
                    return false;

                if (definition.unlockedByDefault && !HasUnlockedBooster(definition.id))
                    UnlockBoosterWithoutSave(definition.id);

                if (SetBoosterSelectionWithoutSave(family, definition.id))
                {
                    Save();
                    return true;
                }

                return false;
            }

            return false;
        }

        if (!HasUnlockedBooster(boosterId))
            return false;

        if (SetBoosterSelectionWithoutSave(family, boosterId))
        {
            Save();
            return true;
        }

        return false;
    }

    public bool CanClaimDailyLogin(System.DateTime todayUtcDate)
    {
        if (dailyLoginLastClaimTicks <= 0)
            return true;

        System.DateTime lastClaim = new System.DateTime(dailyLoginLastClaimTicks, System.DateTimeKind.Utc).Date;
        return (todayUtcDate.Date - lastClaim).Days > 0;
    }

    public int GetNextDailyLoginDayIndex(System.DateTime todayUtcDate)
    {
        if (dailyLoginLastClaimTicks <= 0)
            return 1;

        System.DateTime lastClaim = new System.DateTime(dailyLoginLastClaimTicks, System.DateTimeKind.Utc).Date;
        int daysSinceClaim = (todayUtcDate.Date - lastClaim).Days;
        if (daysSinceClaim <= 0)
            return Mathf.Max(1, dailyLoginDayIndex);

        return daysSinceClaim == 1 ? Mathf.Max(1, dailyLoginDayIndex + 1) : 1;
    }

    public void MarkDailyLoginClaimed(System.DateTime todayUtcDate, int claimedDayIndex)
    {
        dailyLoginLastClaimTicks = todayUtcDate.Date.Ticks;
        dailyLoginDayIndex = Mathf.Max(1, claimedDayIndex);
        Save();
    }

    public int GetDailyLoginDayIndex()
    {
        return Mathf.Max(0, dailyLoginDayIndex);
    }

    public TaskCadenceState GetOrCreateTaskCadenceState(ProgressionCadence cadence, System.DateTime cycleStartUtcDate, ProgressionTaskGroupDefinition definition)
    {
        long cycleTicks = cycleStartUtcDate.Date.Ticks;
        TaskCadenceState state = taskCadenceStates.Find(entry => entry.cadence == cadence);
        if (state == null)
        {
            state = new TaskCadenceState { cadence = cadence };
            taskCadenceStates.Add(state);
        }

        if (state.cycleStartTicks != cycleTicks || state.tasks == null || state.rewards == null || state.tasks.Count == 0)
        {
            state.cycleStartTicks = cycleTicks;
            state.points = definition != null ? Mathf.Max(0, definition.CurrentPoints) : 0;
            state.tasks = new List<TaskProgressState>();
            state.rewards = new List<TaskRewardClaimState>();

            if (definition != null)
            {
                if (definition.Tasks != null)
                {
                    for (int i = 0; i < definition.Tasks.Count; i++)
                    {
                        ProgressionTaskDefinition task = definition.Tasks[i];
                        if (task == null)
                            continue;

                        state.tasks.Add(new TaskProgressState
                        {
                            id = task.Id,
                            current = Mathf.Max(0, task.Current),
                            claimed = false
                        });
                    }
                }

                if (definition.Rewards != null)
                {
                    for (int i = 0; i < definition.Rewards.Count; i++)
                    {
                        ProgressionRewardDefinition reward = definition.Rewards[i];
                        if (reward == null)
                            continue;

                        state.rewards.Add(new TaskRewardClaimState
                        {
                            pointsRequired = reward.PointsRequired,
                            claimed = reward.DefaultState == ProgressionRewardState.Claimed
                        });
                    }
                }
            }

            Save();
        }

        return state;
    }

    public bool TryClaimTaskReward(ProgressionCadence cadence, System.DateTime cycleStartUtcDate, ProgressionTaskGroupDefinition definition, string taskId)
    {
        if (definition == null || string.IsNullOrWhiteSpace(taskId))
            return false;

        TaskCadenceState state = GetOrCreateTaskCadenceState(cadence, cycleStartUtcDate, definition);
        TaskProgressState taskState = state.tasks.Find(entry => entry.id == taskId);
        ProgressionTaskDefinition taskDefinition = null;
        for (int i = 0; i < definition.Tasks.Count; i++)
        {
            if (definition.Tasks[i] != null && definition.Tasks[i].Id == taskId)
            {
                taskDefinition = definition.Tasks[i];
                break;
            }
        }

        if (taskState == null || taskDefinition == null || taskState.claimed || taskState.current < taskDefinition.Target)
            return false;

        taskState.claimed = true;
        state.points += Mathf.Max(0, taskDefinition.ProgressPointsReward);
        GrantProfileReward(taskDefinition.RewardType, taskDefinition.RewardAmount, taskDefinition.RewardItemId);
        Save();
        return true;
    }

    public bool TryClaimTaskMilestoneReward(ProgressionCadence cadence, System.DateTime cycleStartUtcDate, ProgressionTaskGroupDefinition definition, int pointsRequired)
    {
        if (definition == null)
            return false;

        TaskCadenceState state = GetOrCreateTaskCadenceState(cadence, cycleStartUtcDate, definition);
        TaskRewardClaimState rewardState = state.rewards.Find(entry => entry.pointsRequired == pointsRequired);
        ProgressionRewardDefinition rewardDefinition = null;
        for (int i = 0; i < definition.Rewards.Count; i++)
        {
            if (definition.Rewards[i] != null && definition.Rewards[i].PointsRequired == pointsRequired)
            {
                rewardDefinition = definition.Rewards[i];
                break;
            }
        }

        if (rewardState == null || rewardDefinition == null || rewardState.claimed || state.points < pointsRequired)
            return false;

        rewardState.claimed = true;
        GrantProfileReward(rewardDefinition.RewardType, rewardDefinition.RewardAmount, rewardDefinition.RewardItemId);
        Save();
        return true;
    }

    public int GetClaimedAchievementTierCount(string achievementId)
    {
        AchievementClaimState state = achievementClaimStates.Find(entry => entry.id == achievementId);
        return state != null ? Mathf.Max(0, state.claimedTierCount) : 0;
    }

    public bool TryClaimAchievementTier(string achievementId, int tierIndex, AchievementTierDefinition tierDefinition, int currentProgress)
    {
        if (tierDefinition == null || string.IsNullOrWhiteSpace(achievementId) || currentProgress < tierDefinition.targetValue)
            return false;

        AchievementClaimState state = achievementClaimStates.Find(entry => entry.id == achievementId);
        if (state == null)
        {
            state = new AchievementClaimState { id = achievementId, claimedTierCount = 0 };
            achievementClaimStates.Add(state);
        }

        if (tierIndex < state.claimedTierCount || tierIndex > state.claimedTierCount)
            return false;

        state.claimedTierCount = tierIndex + 1;
        GrantProfileReward(tierDefinition.rewardType, tierDefinition.rewardAmount, tierDefinition.rewardItemId);
        Save();
        return true;
    }

    public void SetSelectedLevelIndex(int index)
    {
        int clamped = Mathf.Max(0, index);
        if (selectedLevelIndex == clamped)
            return;

        selectedLevelIndex = clamped;
        Save();
    }

    public float GetXpProgressNormalized()
    {
        return Mathf.Clamp01((float)xp / Mathf.Max(1, XpPerLevel));
    }

    [System.Serializable]
    private struct UpgradeLevelEntry
    {
        public UpgradeType upgradeType;
        public int level;
    }

    [System.Serializable]
    private struct PowerupUpgradeLevelEntry
    {
        public PowerupType powerupType;
        public int level;
    }

    [System.Serializable]
    private class PlayerProfileData
    {
        public int level;
        public int xp;
        public int softCurrency;
        public int premiumCurrency;
        public string selectedShipId;
        public List<string> unlockedItemIds;
        public List<UpgradeLevelEntry> upgradeLevels;
        public List<PowerupUpgradeLevelEntry> powerupUpgradeLevels;
        public List<string> unlockedBoosterIds;
        public List<BoosterSelectionEntry> boosterLoadout;
        public int dailyLoginDayIndex;
        public long dailyLoginLastClaimTicks;
        public List<TaskCadenceState> taskCadenceStates;
        public List<AchievementClaimState> achievementClaimStates;
        public string selectedSkinId;
        public string selectedTrailId;
        public string selectedCoreFxId;
        public int selectedLevelIndex;
    }

    [System.Serializable]
    public class TaskProgressState
    {
        public string id;
        public int current;
        public bool claimed;
    }

    [System.Serializable]
    public class TaskRewardClaimState
    {
        public int pointsRequired;
        public bool claimed;
    }

    [System.Serializable]
    public class TaskCadenceState
    {
        public ProgressionCadence cadence;
        public long cycleStartTicks;
        public int points;
        public List<TaskProgressState> tasks = new();
        public List<TaskRewardClaimState> rewards = new();
    }

    [System.Serializable]
    public class AchievementClaimState
    {
        public string id;
        public int claimedTierCount;
    }

    [System.Serializable]
    public class BoosterSelectionEntry
    {
        public BoosterFamily family;
        public string boosterId;
    }

    private static string ComputeHash(string json)
    {
        string deviceId = SystemInfo.deviceUniqueIdentifier ?? "unknown-device";
        string payload = $"{json}|{deviceId}|{HashSalt}";
        byte[] bytes = Encoding.UTF8.GetBytes(payload);
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(bytes);
            return System.Convert.ToBase64String(hash);
        }
    }
    
    private string EnsureDefaultSelection<T>(string currentId, T[] items) where T : ScriptableObject
    {
        if (items == null)
            return currentId;

        if (!string.IsNullOrEmpty(currentId))
        {
            foreach (var item in items)
            {
                if (item == null)
                    continue;

                if (!TryGetItemData(item, out string id, out int cost))
                    continue;

                if (id != currentId)
                    continue;

                if (HasUnlocked(id) || cost <= 0)
                    return currentId;
            }
        }

        foreach (var item in items)
        {
            if (item == null)
                continue;

            if (!TryGetItemData(item, out string id, out int cost))
                continue;

            if (string.IsNullOrEmpty(id))
                continue;

            if (HasUnlocked(id))
                return id;

            if (cost <= 0)
            {
                UnlockItem(id);
                return id;
            }
        }

        return string.Empty;
    }

    private void EnsureDefaultShipUnlocked(ShipDatabase database)
    {
        if (database == null || database.ships == null)
            return;

        for (int i = 0; i < database.ships.Length; i++)
        {
            ShipDefinition ship = database.ships[i];
            if (ship == null || string.IsNullOrEmpty(ship.id))
                continue;

            if (!HasUnlocked(ship.id))
                UnlockItemWithoutSave(ship.id);

            return;
        }
    }

    private string EnsureOwnedShipSelection(string currentId, ShipDefinition[] ships)
    {
        if (ships == null)
            return currentId;

        if (!string.IsNullOrEmpty(currentId) && HasUnlocked(currentId))
        {
            for (int i = 0; i < ships.Length; i++)
            {
                ShipDefinition ship = ships[i];
                if (ship != null && ship.id == currentId)
                    return currentId;
            }
        }

        for (int i = 0; i < ships.Length; i++)
        {
            ShipDefinition ship = ships[i];
            if (ship == null || string.IsNullOrEmpty(ship.id))
                continue;

            if (HasUnlocked(ship.id))
                return ship.id;
        }

        return string.Empty;
    }

    private static bool TryGetItemData(ScriptableObject item, out string id, out int cost)
    {
        switch (item)
        {
            case ShipDefinition ship:
                id = ship.id;
                cost = 0;
                return true;
            case ShipSkinDefinition skin:
                id = skin.id;
                cost = skin.cost;
                return true;
            case ShipTrailDefinition trail:
                id = trail.id;
                cost = trail.cost;
                return true;
            case ShipCoreFxDefinition fx:
                id = fx.id;
                cost = fx.cost;
                return true;
            default:
                id = null;
                cost = 0;
                return false;
        }
    }

    private bool TrySelectCosmetic<T>(string itemId, T item, System.Action<string> setSelection)
        where T : ScriptableObject
    {
        if (item == null)
            return false;

        if (!HasUnlocked(itemId))
        {
            if (!TryGetItemData(item, out _, out int cost) || cost > 0)
                return false;

            UnlockItemWithoutSave(itemId);
        }

        setSelection?.Invoke(itemId);
        Save();
        return true;
    }

    private void UnlockItemWithoutSave(string itemId)
    {
        if (!unlockedItemIds.Contains(itemId))
            unlockedItemIds.Add(itemId);
    }

    private void UnlockBoosterWithoutSave(string boosterId)
    {
        if (!unlockedBoosterIds.Contains(boosterId))
            unlockedBoosterIds.Add(boosterId);
    }

    private bool SetBoosterSelectionWithoutSave(BoosterFamily family, string boosterId)
    {
        if (string.IsNullOrWhiteSpace(boosterId))
            return false;

        for (int i = 0; i < boosterLoadout.Count; i++)
        {
            if (boosterLoadout[i].family == family)
            {
                if (boosterLoadout[i].boosterId == boosterId)
                    return false;

                boosterLoadout[i] = new BoosterSelectionEntry
                {
                    family = family,
                    boosterId = boosterId
                };
                return true;
            }
        }

        boosterLoadout.Add(new BoosterSelectionEntry
        {
            family = family,
            boosterId = boosterId
        });
        return true;
    }

    private void NormalizeLevelProgress()
    {
        if (level < 1)
            level = 1;

        if (xp < 0)
            xp = 0;

        while (xp >= XpPerLevel)
        {
            xp -= XpPerLevel;
            level++;
        }
    }
}
