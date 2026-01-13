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
}

[CreateAssetMenu(menuName = "Main Menu/Ship Trail Definition")]
public class ShipTrailDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;
    public int cost;
}

[CreateAssetMenu(menuName = "Main Menu/Ship Core FX Definition")]
public class ShipCoreFxDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;
    public int cost;
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

    public bool HasUnlocked(string itemId)
    {
        return unlockedItemIds.Contains(itemId);
    }

    public void UnlockItem(string itemId)
    {
        if (!unlockedItemIds.Contains(itemId))
            unlockedItemIds.Add(itemId);
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
}
