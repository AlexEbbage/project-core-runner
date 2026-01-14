using System.Collections.Generic;
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
    public int level = 1;
    public int softCurrency = 1000;
    public int premiumCurrency = 50;

    [SerializeField] private List<string> unlockedItemIds = new();
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
            unlockedItemIds.Add(itemId);
    }

    public void EnsureDefaults(ShipDatabase database)
    {
        if (database == null)
            return;

        selectedSkinId = EnsureDefaultSelection(selectedSkinId, database.skins);
        selectedTrailId = EnsureDefaultSelection(selectedTrailId, database.trails);
        selectedCoreFxId = EnsureDefaultSelection(selectedCoreFxId, database.coreFx);
    }

    public bool TrySelectSkin(string skinId, ShipDatabase database)
    {
        var skin = database != null ? database.GetSkin(skinId) : null;
        if (skin == null)
            return false;

        if (!HasUnlocked(skinId))
        {
            if (skin.cost > 0)
                return false;

            UnlockItem(skinId);
        }

        selectedSkinId = skinId;
        return true;
    }

    public bool TrySelectTrail(string trailId, ShipDatabase database)
    {
        var trail = database != null ? database.GetTrail(trailId) : null;
        if (trail == null)
            return false;

        if (!HasUnlocked(trailId))
        {
            if (trail.cost > 0)
                return false;

            UnlockItem(trailId);
        }

        selectedTrailId = trailId;
        return true;
    }

    public bool TrySelectCoreFx(string coreFxId, ShipDatabase database)
    {
        var coreFx = database != null ? database.GetCoreFx(coreFxId) : null;
        if (coreFx == null)
            return false;

        if (!HasUnlocked(coreFxId))
        {
            if (coreFx.cost > 0)
                return false;

            UnlockItem(coreFxId);
        }

        selectedCoreFxId = coreFxId;
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
            return true;
        }

        if (premiumCurrency < amount)
            return false;

        premiumCurrency -= amount;
        return true;
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
}
