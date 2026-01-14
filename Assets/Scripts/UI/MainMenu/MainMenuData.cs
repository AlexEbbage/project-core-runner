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
    Progression
}

public enum HangarTab
{
    Upgrades,
    Skins,
    Trails,
    CoreFx
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

[CreateAssetMenu(menuName = "Main Menu/Player Profile")]
public class PlayerProfile : ScriptableObject
{
    private const string ProfileKey = "PlayerProfile";
    private const string ProfileHashKey = "PlayerProfileHash";
    private const string HashSalt = "profile_v1";

    public int level = 1;
    public int xp;
    public int softCurrency = 1000;
    public int premiumCurrency = 50;
    public string selectedShipId;

    [SerializeField] private List<string> unlockedItemIds = new();
    [SerializeField] private List<UpgradeLevelEntry> upgradeLevels = new();

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
        unlockedItemIds = data.unlockedItemIds ?? new List<string>();
        upgradeLevels = data.upgradeLevels ?? new List<UpgradeLevelEntry>();
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
    
    public void EnsureDefaults(ShipDatabase database)
    {
        if (database == null)
            return;

        string previousSkinId = selectedSkinId;
        string previousTrailId = selectedTrailId;
        string previousCoreFxId = selectedCoreFxId;

        selectedSkinId = EnsureDefaultSelection(selectedSkinId, database.skins);
        selectedTrailId = EnsureDefaultSelection(selectedTrailId, database.trails);
        selectedCoreFxId = EnsureDefaultSelection(selectedCoreFxId, database.coreFx);

        if (previousSkinId != selectedSkinId
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
        bool updated = false;
        for (int i = 0; i < upgradeLevels.Count; i++)
        {
            if (upgradeLevels[i].upgradeType == upgradeType)
            {
                upgradeLevels[i] = new UpgradeLevelEntry
                {
                    upgradeType = upgradeType,
                    level = Mathf.Max(0, levelValue)
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
                level = Mathf.Max(0, levelValue)
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
        xp = Mathf.Max(0, xp + amount);
        Save();
    }

    [System.Serializable]
    private struct UpgradeLevelEntry
    {
        public UpgradeType upgradeType;
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
        public string selectedSkinId;
        public string selectedTrailId;
        public string selectedCoreFxId;
    }

    private static string ComputeHash(string json)
    {
        string deviceId = SystemInfo.deviceUniqueIdentifier ?? "unknown-device";
        string payload = $"{json}|{deviceId}|{HashSalt}";
        byte[] bytes = Encoding.UTF8.GetBytes(payload);
        byte[] hash = SHA256.HashData(bytes);
        return System.Convert.ToBase64String(hash);
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

    private static bool TryGetItemData(ScriptableObject item, out string id, out int cost)
    {
        switch (item)
        {
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
}
