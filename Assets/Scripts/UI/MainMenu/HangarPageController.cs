using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HangarPageController : MonoBehaviour
{
    private const int DefaultComboBaseCost = 200;
    private const int DefaultComboCostIncrease = 150;

    [Header("Data")]
    [SerializeField] private PlayerProfile profile;
    [SerializeField] private ShipDatabase shipDatabase;
    [SerializeField] private ShopDatabase shopDatabase;
    [SerializeField] private PowerupUpgradeConfig powerupUpgradeConfig;
    [SerializeField] private string currentShipId;
    [SerializeField] private PlayerCosmetics previewCosmetics;

    [Header("Stats")]
    [SerializeField] private HangarStatRowView[] statRows;
    [SerializeField] private float statMaxValue = 10f;

    [Header("Tabs")]
    [SerializeField] private HangarTab selectedTab = HangarTab.Ships;

    [Header("Content")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private HangarUpgradeItemView upgradeItemPrefab;
    [SerializeField] private HangarCosmeticItemView cosmeticItemPrefab;

    private readonly List<GameObject> _spawnedItems = new();
    private readonly Dictionary<HangarTab, Button> _runtimeTabButtons = new();

    [SerializeField] private GameManager gameManager;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (profile == null)
        {
            PlayerProfile[] profiles = Resources.FindObjectsOfTypeAll<PlayerProfile>();
            if (profiles != null && profiles.Length > 0)
                profile = profiles[0];
        }

        if (shipDatabase == null)
        {
            ShipDatabase[] databases = Resources.FindObjectsOfTypeAll<ShipDatabase>();
            if (databases != null && databases.Length > 0)
                shipDatabase = databases[0];
        }

        if (shopDatabase == null)
        {
            shopDatabase = Resources.Load<ShopDatabase>("ShopDatabase");
            if (shopDatabase == null)
            {
                ShopDatabase[] databases = Resources.FindObjectsOfTypeAll<ShopDatabase>();
                if (databases != null && databases.Length > 0)
                    shopDatabase = databases[0];
            }
        }

        if (previewCosmetics == null)
            previewCosmetics = FindFirstObjectByType<PlayerCosmetics>();
    }

    private void OnEnable()
    {
        if (profile == null || shipDatabase == null)
            return;

        profile.EnsureDefaults(shipDatabase);
        if (selectedTab == HangarTab.Upgrades)
            selectedTab = HangarTab.Ships;
        RefreshContent();
    }

    public void Initialize(PlayerProfile playerProfile, ShipDatabase database)
    {
        profile = playerProfile;
        shipDatabase = database;
        profile?.EnsureDefaults(shipDatabase);
        if (selectedTab == HangarTab.Upgrades)
            selectedTab = HangarTab.Ships;
        RefreshStats();
        SelectTab(selectedTab);
    }

    public void SelectTab(HangarTab tab)
    {
        selectedTab = tab;
        ClearContent();
        BuildTabRow();
        BuildPreviewCard();

        switch (selectedTab)
        {
            case HangarTab.Ships:
                BuildShips();
                break;
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

        UpdateTabSelection();
    }

    public void OnUpgradeButtonClicked(HangarUpgradeItemView itemView)
    {
        if (itemView == null || profile == null)
            return;

        if (itemView.IsPowerupUpgrade)
        {
            HandlePowerupUpgrade(itemView);
            return;
        }

        if (itemView.Definition == null)
            return;

        int currentLevel = GetUpgradeLevel(itemView.Definition);
        if (currentLevel >= itemView.Definition.maxLevel)
            return;

        int cost = itemView.Definition.GetCostForLevel(currentLevel);
        if (!profile.TrySpend(ShopCurrencyType.Soft, cost))
            return;

        profile.SetUpgradeLevel(itemView.Definition.upgradeType, currentLevel + 1);

        gameManager?.LogAnalyticsEvent(AnalyticsEventNames.UpgradePurchased, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Type, itemView.Definition.upgradeType.ToString() },
            { AnalyticsEventNames.Params.Price, cost }
        });

        RefreshContent();
    }

    public void OnShipSelected(string shipId)
    {
        if (profile == null || string.IsNullOrEmpty(shipId))
            return;

        if (profile.TrySelectShip(shipId, shipDatabase))
            RefreshContent();
    }

    public void OnSkinSelected(string skinId)
    {
        if (profile == null || string.IsNullOrEmpty(skinId))
            return;

        if (profile.TrySelectSkin(skinId, shipDatabase))
            RefreshContent();
    }

    public void OnTrailSelected(string trailId)
    {
        if (profile == null || string.IsNullOrEmpty(trailId))
            return;

        if (profile.TrySelectTrail(trailId, shipDatabase))
            RefreshContent();
    }

    public void OnCoreFxSelected(string coreFxId)
    {
        if (profile == null || string.IsNullOrEmpty(coreFxId))
            return;

        if (profile.TrySelectCoreFx(coreFxId, shipDatabase))
            RefreshContent();
    }

    public void RefreshContent()
    {
        profile?.EnsureDefaults(shipDatabase);
        previewCosmetics?.ApplyCosmetics();
        SelectTab(selectedTab);
        RefreshStats();
    }

    private void RefreshStats()
    {
        if (shipDatabase == null || profile == null)
            return;

        currentShipId = !string.IsNullOrEmpty(profile.selectedShipId)
            ? profile.selectedShipId
            : currentShipId;

        ShipDefinition ship = shipDatabase.GetShip(currentShipId);
        if (ship == null)
            return;

        foreach (HangarStatRowView row in statRows)
        {
            if (row == null)
                continue;

            float value = ship.baseStats.GetValue(row.StatType);
            float normalized = statMaxValue > 0f ? value / statMaxValue : 0f;
            row.SetValue(normalized, value);
        }
    }

    private void BuildShips()
    {
        if (shipDatabase == null || shipDatabase.ships == null)
            return;

        foreach (ShipDefinition ship in shipDatabase.ships)
        {
            if (ship == null || string.IsNullOrEmpty(ship.id))
                continue;

            bool unlocked = profile != null && profile.HasUnlocked(ship.id);
            bool equipped = profile != null && profile.selectedShipId == ship.id;
            int cost = GetShopPrice(ship.id);

            HangarCosmeticItemView instance = CreateCosmeticItemInstance();
            if (instance == null)
                continue;

            instance.Initialize(ship.id, ship.displayName, ship.icon, cost, unlocked, equipped);
            instance.SetAction(() => OnShipSelected(ship.id));
            _spawnedItems.Add(instance.gameObject);
        }
    }

    private void BuildUpgrades()
    {
        if (upgradeItemPrefab == null)
            return;

        bool comboAdded = false;

        if (shipDatabase != null && shipDatabase.upgrades != null)
        {
            foreach (ShipUpgradeDefinition upgrade in shipDatabase.upgrades)
            {
                if (upgrade == null || upgrade.upgradeType != UpgradeType.ComboMultiplier)
                    continue;

                int currentLevel = GetUpgradeLevel(upgrade);
                int cost = upgrade.GetCostForLevel(currentLevel);
                bool canUpgrade = profile != null && currentLevel < upgrade.maxLevel && profile.softCurrency >= cost;

                HangarUpgradeItemView instance = Instantiate(upgradeItemPrefab, contentRoot);
                instance.Initialize(upgrade, currentLevel, cost, canUpgrade);
                _spawnedItems.Add(instance.gameObject);
                comboAdded = true;
            }
        }

        if (!comboAdded)
            BuildFallbackComboUpgrade();

        BuildPowerupUpgrades();
    }

    private void BuildSkins()
    {
        if (shipDatabase == null || shipDatabase.skins == null)
            return;

        foreach (ShipSkinDefinition skin in shipDatabase.skins)
        {
            if (skin == null)
                continue;

            bool unlocked = profile != null && profile.HasUnlocked(skin.id);
            bool equipped = profile != null && profile.SelectedSkinId == skin.id;
            HangarCosmeticItemView instance = CreateCosmeticItemInstance();
            if (instance == null)
                continue;

            instance.Initialize(skin.id, skin.displayName, skin.icon, skin.cost, unlocked, equipped);
            instance.SetAction(() => OnSkinSelected(skin.id));
            _spawnedItems.Add(instance.gameObject);
        }
    }

    private void BuildTrails()
    {
        if (shipDatabase == null || shipDatabase.trails == null)
            return;

        foreach (ShipTrailDefinition trail in shipDatabase.trails)
        {
            if (trail == null)
                continue;

            bool unlocked = profile != null && profile.HasUnlocked(trail.id);
            bool equipped = profile != null && profile.SelectedTrailId == trail.id;
            HangarCosmeticItemView instance = CreateCosmeticItemInstance();
            if (instance == null)
                continue;

            instance.Initialize(trail.id, trail.displayName, trail.icon, trail.cost, unlocked, equipped);
            instance.SetAction(() => OnTrailSelected(trail.id));
            _spawnedItems.Add(instance.gameObject);
        }
    }

    private void BuildCoreFx()
    {
        if (shipDatabase == null || shipDatabase.coreFx == null)
            return;

        foreach (ShipCoreFxDefinition coreFx in shipDatabase.coreFx)
        {
            if (coreFx == null)
                continue;

            bool unlocked = profile != null && profile.HasUnlocked(coreFx.id);
            bool equipped = profile != null && profile.SelectedCoreFxId == coreFx.id;
            HangarCosmeticItemView instance = CreateCosmeticItemInstance();
            if (instance == null)
                continue;

            instance.Initialize(coreFx.id, coreFx.displayName, coreFx.icon, coreFx.cost, unlocked, equipped);
            instance.SetAction(() => OnCoreFxSelected(coreFx.id));
            _spawnedItems.Add(instance.gameObject);
        }
    }

    private void ClearContent()
    {
        foreach (GameObject item in _spawnedItems)
        {
            if (item != null)
                Destroy(item);
        }

        _spawnedItems.Clear();
        _runtimeTabButtons.Clear();
    }

    private int GetUpgradeLevel(ShipUpgradeDefinition upgrade)
    {
        if (upgrade == null)
            return 0;

        return profile != null ? profile.GetUpgradeLevel(upgrade.upgradeType) : 0;
    }

    private void BuildPowerupUpgrades()
    {
        if (upgradeItemPrefab == null)
            return;

        PowerupUpgradeConfig.PowerupUpgradeEntry[] upgradeEntries = powerupUpgradeConfig != null
            ? powerupUpgradeConfig.GetAvailableUpgrades()
            : PowerupUpgradeConfig.GetDefaultEntries();

        foreach (PowerupUpgradeConfig.PowerupUpgradeEntry upgrade in upgradeEntries)
        {
            if (upgrade == null)
                continue;

            int currentLevel = GetPowerupUpgradeLevel(upgrade);
            int maxLevel = upgrade.MaxLevel;
            int cost = upgrade.GetCostForLevel(currentLevel);
            bool canUpgrade = profile != null && currentLevel < maxLevel && profile.softCurrency >= cost;

            HangarUpgradeItemView instance = Instantiate(upgradeItemPrefab, contentRoot);
            instance.InitializePowerupUpgrade(upgrade, currentLevel, cost, canUpgrade);
            _spawnedItems.Add(instance.gameObject);
        }
    }

    private int GetPowerupUpgradeLevel(PowerupUpgradeConfig.PowerupUpgradeEntry upgrade)
    {
        if (upgrade == null)
            return 0;

        return profile != null ? profile.GetPowerupUpgradeLevel(upgrade.powerupType) : 0;
    }

    private void HandlePowerupUpgrade(HangarUpgradeItemView itemView)
    {
        PowerupUpgradeConfig.PowerupUpgradeEntry upgradeEntry = itemView.PowerupUpgradeEntry;
        if (upgradeEntry == null || profile == null)
            return;

        int currentLevel = GetPowerupUpgradeLevel(upgradeEntry);
        int maxLevel = upgradeEntry.MaxLevel;
        if (currentLevel >= maxLevel)
            return;

        int cost = upgradeEntry.GetCostForLevel(currentLevel);
        if (!profile.TrySpend(ShopCurrencyType.Soft, cost))
            return;

        profile.SetPowerupUpgradeLevel(upgradeEntry.powerupType, currentLevel + 1);
        RefreshContent();
    }

    private void BuildFallbackComboUpgrade()
    {
        ShipUpgradeDefinition comboUpgrade = ScriptableObject.CreateInstance<ShipUpgradeDefinition>();
        comboUpgrade.upgradeType = UpgradeType.ComboMultiplier;
        comboUpgrade.displayName = "Combo Modifier";
        comboUpgrade.maxLevel = 5;
        comboUpgrade.baseCost = DefaultComboBaseCost;
        comboUpgrade.costIncrease = DefaultComboCostIncrease;

        int currentLevel = GetUpgradeLevel(comboUpgrade);
        int cost = comboUpgrade.GetCostForLevel(currentLevel);
        bool canUpgrade = profile != null && currentLevel < comboUpgrade.maxLevel && profile.softCurrency >= cost;

        HangarUpgradeItemView instance = Instantiate(upgradeItemPrefab, contentRoot);
        instance.Initialize(comboUpgrade, currentLevel, cost, canUpgrade);
        _spawnedItems.Add(instance.gameObject);
    }

    private void BuildTabRow()
    {
        if (contentRoot == null)
            return;

        TMP_FontAsset font = TMP_Settings.defaultFontAsset;
        GameObject rowObject = new GameObject("HangarTabs", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        rowObject.transform.SetParent(contentRoot, false);

        LayoutElement layout = rowObject.GetComponent<LayoutElement>();
        layout.preferredHeight = 56f;

        HorizontalLayoutGroup group = rowObject.GetComponent<HorizontalLayoutGroup>();
        group.spacing = 10f;
        group.childControlWidth = true;
        group.childControlHeight = true;
        group.childForceExpandWidth = true;
        group.childForceExpandHeight = true;

        CreateTabButton(rowObject.transform, font, HangarTab.Ships, LocalizationService.Get("ui.hangar_tab_ships", "Ships"));
        CreateTabButton(rowObject.transform, font, HangarTab.Skins, LocalizationService.Get("ui.hangar_tab_skins", "Skins"));
        CreateTabButton(rowObject.transform, font, HangarTab.Trails, LocalizationService.Get("ui.hangar_tab_trails", "Trails"));
        CreateTabButton(rowObject.transform, font, HangarTab.CoreFx, LocalizationService.Get("ui.hangar_tab_corefx", "Core FX"));
        CreateTabButton(rowObject.transform, font, HangarTab.Upgrades, LocalizationService.Get("ui.hangar_tab_upgrades", "Upgrades"));
        _spawnedItems.Add(rowObject);
    }

    private void BuildPreviewCard()
    {
        if (contentRoot == null || shipDatabase == null || profile == null)
            return;

        ShipDefinition ship = shipDatabase.GetShip(profile.selectedShipId);
        string shipName = ship != null && !string.IsNullOrEmpty(ship.displayName)
            ? ship.displayName
            : LocalizationService.Get("ui.hangar_no_ship", "No ship selected");

        string skinName = GetSelectedLabel(profile.SelectedSkinId, shipDatabase.GetSkin(profile.SelectedSkinId));
        string trailName = GetSelectedLabel(profile.SelectedTrailId, shipDatabase.GetTrail(profile.SelectedTrailId));
        string coreFxName = GetSelectedLabel(profile.SelectedCoreFxId, shipDatabase.GetCoreFx(profile.SelectedCoreFxId));

        GameObject cardObject = new GameObject("CurrentLoadoutCard", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter), typeof(LayoutElement));
        cardObject.transform.SetParent(contentRoot, false);

        Image background = cardObject.GetComponent<Image>();
        background.color = new Color(0.07f, 0.1f, 0.14f, 0.92f);

        LayoutElement layout = cardObject.GetComponent<LayoutElement>();
        layout.preferredHeight = 150f;

        VerticalLayoutGroup group = cardObject.GetComponent<VerticalLayoutGroup>();
        group.padding = new RectOffset(18, 18, 16, 16);
        group.spacing = 8f;
        group.childControlWidth = true;
        group.childControlHeight = false;
        group.childForceExpandWidth = true;
        group.childForceExpandHeight = false;

        ContentSizeFitter fitter = cardObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        TMP_FontAsset font = TMP_Settings.defaultFontAsset;
        CreateCardText(cardObject.transform, font, LocalizationService.Get("ui.hangar_preview_title", "Current Loadout"), 28f, FontStyles.Bold);
        CreateCardText(cardObject.transform, font, $"{LocalizationService.Get("ui.hangar_preview_ship", "Ship")}: {shipName}", 22f, FontStyles.Normal);
        CreateCardText(cardObject.transform, font, $"{LocalizationService.Get("ui.hangar_preview_cosmetics", "Cosmetics")}: {skinName} / {trailName} / {coreFxName}", 20f, FontStyles.Normal);

        if (ship != null)
        {
            string statsLabel =
                $"{LocalizationService.Get("ui.hangar_stat_speed", "SPD")} {ship.baseStats.speed:0.#}   " +
                $"{LocalizationService.Get("ui.hangar_stat_handling", "HDL")} {ship.baseStats.handling:0.#}   " +
                $"{LocalizationService.Get("ui.hangar_stat_boost", "BST")} {ship.baseStats.boost:0.#}";
            CreateCardText(cardObject.transform, font, statsLabel, 18f, FontStyles.Normal);
        }

        _spawnedItems.Add(cardObject);
    }

    private void CreateTabButton(Transform parent, TMP_FontAsset font, HangarTab tab, string label)
    {
        GameObject buttonObject = new GameObject($"{tab}Tab", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetParent(parent, false);

        LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
        layout.preferredHeight = 54f;
        layout.flexibleWidth = 1f;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.18f, 0.22f, 0.28f, 0.96f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => SelectTab(tab));

        TMP_Text text = CreateCardText(buttonObject.transform, font, label, 18f, FontStyles.Bold);
        text.alignment = TextAlignmentOptions.Center;
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        _runtimeTabButtons[tab] = button;
    }

    private void UpdateTabSelection()
    {
        foreach (KeyValuePair<HangarTab, Button> pair in _runtimeTabButtons)
        {
            if (pair.Value == null || !pair.Value.TryGetComponent(out Image image))
                continue;

            bool isSelected = pair.Key == selectedTab;
            image.color = isSelected
                ? new Color(0.98f, 0.5f, 0.18f, 1f)
                : new Color(0.18f, 0.22f, 0.28f, 0.96f);
        }
    }

    private HangarCosmeticItemView CreateCosmeticItemInstance()
    {
        if (contentRoot == null)
            return null;

        if (cosmeticItemPrefab != null)
            return Instantiate(cosmeticItemPrefab, contentRoot);

        GameObject cardObject = new GameObject("HangarCosmeticItem", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(HorizontalLayoutGroup), typeof(HangarCosmeticItemView));
        cardObject.transform.SetParent(contentRoot, false);
        return cardObject.GetComponent<HangarCosmeticItemView>();
    }

    private int GetShopPrice(string itemId)
    {
        if (shopDatabase == null || string.IsNullOrEmpty(itemId))
            return 0;

        foreach (ShopTab tab in System.Enum.GetValues(typeof(ShopTab)))
        {
            IEnumerable<ShopItemDefinition> items = shopDatabase.GetItemsForTab(tab);
            if (items == null)
                continue;

            foreach (ShopItemDefinition item in items)
            {
                if (item != null && item.id == itemId)
                    return item.price;
            }
        }

        return 0;
    }

    private static string GetSelectedLabel(string itemId, ScriptableObject item)
    {
        if (string.IsNullOrEmpty(itemId) || item == null)
            return LocalizationService.Get("ui.hangar_none", "None");

        return item switch
        {
            ShipSkinDefinition skin => skin.displayName,
            ShipTrailDefinition trail => trail.displayName,
            ShipCoreFxDefinition coreFx => coreFx.displayName,
            _ => item.name
        };
    }

    private static TMP_Text CreateCardText(Transform parent, TMP_FontAsset font, string textValue, float fontSize, FontStyles fontStyle)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.font = font;
        text.text = textValue;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = Color.white;
        text.raycastTarget = false;
        text.enableWordWrapping = true;
        return text;
    }
}
