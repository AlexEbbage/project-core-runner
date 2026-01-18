using System.Collections.Generic;
using UnityEngine;

public class HangarPageController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PlayerProfile profile;
    [SerializeField] private ShipDatabase shipDatabase;
    [SerializeField] private string currentShipId;

    [Header("Stats")]
    [SerializeField] private HangarStatRowView[] statRows;
    [SerializeField] private float statMaxValue = 10f;

    [Header("Tabs")]
    [SerializeField] private HangarTab selectedTab = HangarTab.Upgrades;

    [Header("Content")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private HangarUpgradeItemView upgradeItemPrefab;
    [SerializeField] private HangarCosmeticItemView cosmeticItemPrefab;

    private readonly List<GameObject> _spawnedItems = new();

    public void Initialize(PlayerProfile playerProfile, ShipDatabase database)
    {
        profile = playerProfile;
        shipDatabase = database;
        profile?.EnsureDefaults(shipDatabase);
        RefreshStats();
        SelectTab(selectedTab);
    }

    public void SelectTab(HangarTab tab)
    {
        selectedTab = tab;
        ClearContent();

        switch (selectedTab)
        {
            case HangarTab.Upgrades:
                BuildUpgrades();
                break;
            case HangarTab.Skins:
                BuildSkins();
                break;
            case HangarTab.Trails:
                BuildTrails();
                break;
            case HangarTab.CoreFx:
                BuildCoreFx();
                break;
        }
    }

    public void OnUpgradeButtonClicked(HangarUpgradeItemView itemView)
    {
        if (itemView == null || itemView.Definition == null || profile == null)
            return;

        int currentLevel = GetUpgradeLevel(itemView.Definition);
        if (currentLevel >= itemView.Definition.maxLevel)
            return;

        int cost = itemView.Definition.GetCostForLevel(currentLevel);
        if (!profile.TrySpend(ShopCurrencyType.Soft, cost))
            return;

        if (profile != null)
            profile.SetUpgradeLevel(itemView.Definition.upgradeType, currentLevel + 1);
        SelectTab(selectedTab);
        RefreshStats();
    }

    public void OnSkinSelected(string skinId)
    {
        if (profile == null || string.IsNullOrEmpty(skinId))
            return;

        if (profile.TrySelectSkin(skinId, shipDatabase))
            SelectTab(selectedTab);
    }

    public void OnTrailSelected(string trailId)
    {
        if (profile == null || string.IsNullOrEmpty(trailId))
            return;

        if (profile.TrySelectTrail(trailId, shipDatabase))
            SelectTab(selectedTab);
    }

    public void OnCoreFxSelected(string coreFxId)
    {
        if (profile == null || string.IsNullOrEmpty(coreFxId))
            return;

        if (profile.TrySelectCoreFx(coreFxId, shipDatabase))
            SelectTab(selectedTab);
    }

    public void RefreshContent()
    {
        SelectTab(selectedTab);
        RefreshStats();
    }

    private void RefreshStats()
    {
        if (shipDatabase == null)
            return;

        var ship = shipDatabase.GetShip(currentShipId);
        if (ship == null)
            return;

        foreach (var row in statRows)
        {
            if (row == null)
                continue;

            float value = ship.baseStats.GetValue(row.StatType);
            float normalized = statMaxValue > 0f ? value / statMaxValue : 0f;
            row.SetValue(normalized, value);
        }
    }

    private void BuildUpgrades()
    {
        if (shipDatabase == null || shipDatabase.upgrades == null || upgradeItemPrefab == null)
            return;

        foreach (var upgrade in shipDatabase.upgrades)
        {
            if (upgrade == null)
                continue;

            int currentLevel = GetUpgradeLevel(upgrade);
            int cost = upgrade.GetCostForLevel(currentLevel);
            bool canUpgrade = profile != null && currentLevel < upgrade.maxLevel && profile.softCurrency >= cost;

            var instance = Instantiate(upgradeItemPrefab, contentRoot);
            instance.Initialize(upgrade, currentLevel, cost, canUpgrade);
            _spawnedItems.Add(instance.gameObject);
        }
    }

    private void BuildSkins()
    {
        if (shipDatabase == null || shipDatabase.skins == null || cosmeticItemPrefab == null)
            return;

        foreach (var skin in shipDatabase.skins)
        {
            if (skin == null)
                continue;

            bool unlocked = profile != null && profile.HasUnlocked(skin.id);
            bool equipped = profile != null && profile.SelectedSkinId == skin.id;
            var instance = Instantiate(cosmeticItemPrefab, contentRoot);
            instance.Initialize(skin.id, skin.displayName, skin.icon, skin.cost, unlocked, equipped);
            instance.SetAction(() => OnSkinSelected(skin.id));
            _spawnedItems.Add(instance.gameObject);
        }
    }

    private void BuildTrails()
    {
        if (shipDatabase == null || shipDatabase.trails == null || cosmeticItemPrefab == null)
            return;

        foreach (var trail in shipDatabase.trails)
        {
            if (trail == null)
                continue;

            bool unlocked = profile != null && profile.HasUnlocked(trail.id);
            bool equipped = profile != null && profile.SelectedTrailId == trail.id;
            var instance = Instantiate(cosmeticItemPrefab, contentRoot);
            instance.Initialize(trail.id, trail.displayName, trail.icon, trail.cost, unlocked, equipped);
            instance.SetAction(() => OnTrailSelected(trail.id));
            _spawnedItems.Add(instance.gameObject);
        }
    }

    private void BuildCoreFx()
    {
        if (shipDatabase == null || shipDatabase.coreFx == null || cosmeticItemPrefab == null)
            return;

        foreach (var coreFx in shipDatabase.coreFx)
        {
            if (coreFx == null)
                continue;

            bool unlocked = profile != null && profile.HasUnlocked(coreFx.id);
            bool equipped = profile != null && profile.SelectedCoreFxId == coreFx.id;
            var instance = Instantiate(cosmeticItemPrefab, contentRoot);
            instance.Initialize(coreFx.id, coreFx.displayName, coreFx.icon, coreFx.cost, unlocked, equipped);
            instance.SetAction(() => OnCoreFxSelected(coreFx.id));
            _spawnedItems.Add(instance.gameObject);
        }
    }

    private void ClearContent()
    {
        foreach (var item in _spawnedItems)
        {
            if (item != null)
                Destroy(item);
        }

        _spawnedItems.Clear();
    }

    private int GetUpgradeLevel(ShipUpgradeDefinition upgrade)
    {
        if (upgrade == null)
            return 0;

        return profile != null ? profile.GetUpgradeLevel(upgrade.upgradeType) : 0;
    }
}
