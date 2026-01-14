using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class MainMenuPrefabBuilder
{
    private const string PrefabRoot = "Assets/Prefabs/UI/MainMenu";

    private readonly struct ProgressionRewardTrackElements
    {
        public readonly TMP_Text pointsValueText;
        public readonly Image progressFill;
        public readonly RectTransform rewardRow;

        public ProgressionRewardTrackElements(TMP_Text pointsValueText, Image progressFill, RectTransform rewardRow)
        {
            this.pointsValueText = pointsValueText;
            this.progressFill = progressFill;
            this.rewardRow = rewardRow;
        }
    }

    [MenuItem("Tools/UI/Build Main Menu UI")]
    public static void BuildMainMenuUI()
    {
        BuildMainMenuUI(MainMenuBuilderConfig.CreateDefault());
    }

    public static void BuildMainMenuUI(MainMenuBuilderConfig builderConfig)
    {
        EnsureFolder(PrefabRoot);

        builderConfig = MainMenuBuilderConfig.Merge(builderConfig, MainMenuBuilderConfig.CreateDefault());
        var progressionConfig = GetOrCreateProgressionTasksConfig();
        var taskRowPrefab = BuildProgressionTaskRowPrefab();
        var taskRowVariants = BuildProgressionTaskRowVariants(taskRowPrefab);
        var rewardNodePrefab = BuildProgressionRewardNodePrefab();
        var progressionHubPrefab = BuildProgressionHubPrefab(taskRowPrefab, taskRowVariants, rewardNodePrefab, progressionConfig, builderConfig.ProgressionPage);

        var canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvasComponent = canvas.GetComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        var eventSystem = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }

        var uiRoot = CreateRect("UIRoot", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var topBar = BuildTopBar(uiRoot, builderConfig.TopBar);
        var pageContainer = BuildPageContainer(uiRoot, progressionHubPrefab, builderConfig);
        var bottomNav = BuildBottomNav(uiRoot, builderConfig.BottomNavButtons);
        var modalsRoot = CreateRect("ModalsRoot", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var modal = BuildShopModal(modalsRoot);

        var menuController = uiRoot.gameObject.AddComponent<MainMenuController>();
        AssignMenuController(menuController, pageContainer, bottomNav, builderConfig);

        var topBarController = topBar.gameObject.AddComponent<TopBarController>();
        AssignTopBar(topBarController, menuController, topBar, builderConfig.TopBar);

        var bottomNavController = bottomNav.gameObject.AddComponent<BottomNavBarController>();
        AssignBottomNav(bottomNavController, menuController, bottomNav, builderConfig.BottomNavButtons);

        AttachHangarController(pageContainer, builderConfig.HangarPage);
        AttachShopController(pageContainer, modal, builderConfig.ShopPage);

        PrefabUtility.SaveAsPrefabAsset(canvas, Path.Combine(PrefabRoot, "MainMenuCanvas.prefab"));
        Object.DestroyImmediate(canvas);
    }

    private static RectTransform BuildTopBar(Transform parent, TopBarConfig config)
    {
        var topBar = CreateRect("TopBar", parent, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, 1));
        topBar.sizeDelta = new Vector2(0, 200);
        var layout = topBar.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.spacing = 20f;
        layout.padding = new RectOffset(20, 20, 20, 20);
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;

        var profileArea = CreateRect("ProfileArea", topBar, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        profileArea.sizeDelta = new Vector2(400, 160);
        profileArea.gameObject.AddComponent<HorizontalLayoutGroup>().spacing = 16f;

        var avatar = CreateImage("AvatarImage", profileArea, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(120, 120));
        avatar.raycastTarget = false;

        var levelText = CreateText("LevelText", profileArea, "Lv 1", 36);
        var xpBar = CreateImage("XpProgressBar", profileArea, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(200, 30));
        xpBar.type = Image.Type.Filled;
        xpBar.fillMethod = Image.FillMethod.Horizontal;
        xpBar.fillAmount = 0.5f;

        var currencyPills = CreateRect("CurrencyPills", topBar, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
        currencyPills.sizeDelta = new Vector2(600, 160);
        currencyPills.gameObject.AddComponent<HorizontalLayoutGroup>().spacing = 16f;

        if (config?.CurrencyPills != null)
        {
            foreach (var pillConfig in config.CurrencyPills)
            {
                if (pillConfig == null)
                    continue;

                BuildCurrencyPill(currencyPills, pillConfig);
            }
        }

        topBar.gameObject.name = "TopBar";
        return topBar;
    }

    private static RectTransform BuildBottomNav(Transform parent, IReadOnlyList<BottomNavButtonConfig> buttonConfigs)
    {
        var bottomBar = CreateRect("BottomNavBar", parent, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 0));
        bottomBar.sizeDelta = new Vector2(0, 200);
        var layout = bottomBar.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 24f;
        layout.padding = new RectOffset(20, 20, 20, 20);

        if (buttonConfigs != null)
        {
            foreach (var buttonConfig in buttonConfigs)
            {
                if (buttonConfig == null)
                    continue;

                BuildNavButton(bottomBar, buttonConfig);
            }
        }

        return bottomBar;
    }

    private static RectTransform BuildPageContainer(Transform parent, RectTransform progressionHubPrefab, MainMenuBuilderConfig config)
    {
        config ??= MainMenuBuilderConfig.CreateDefault();
        var container = CreateRect("PageContainer", parent, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 0));
        container.offsetMin = new Vector2(0, 200);
        container.offsetMax = new Vector2(0, -200);

        foreach (var pageKind in config.PageOrder)
        {
            switch (pageKind)
            {
                case PageKind.Hangar:
                    BuildHangarPage(container, config.HangarPage);
                    break;
                case PageKind.Play:
                case PageKind.Challenges:
                    BuildPlaceholderPage(container, config.PlaceholderPages, pageKind);
                    break;
                case PageKind.Shop:
                    BuildShopPage(container, config.ShopPage);
                    break;
                case PageKind.Progression:
                    BuildProgressionPage(container, progressionHubPrefab, config.ProgressionPage);
                    break;
            }
        }

        return container;
    }

    private static void BuildHangarPage(Transform parent, HangarPageConfig config)
    {
        if (config == null)
            return;

        var page = CreateRect(config.Name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        page.gameObject.AddComponent<CanvasGroup>();

        var shipDisplay = CreateRect(config.ShipDisplayPanelName, page, new Vector2(0, 0.5f), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 1));
        shipDisplay.sizeDelta = new Vector2(0, 400);

        var statsPanel = CreateRect(config.StatsPanelName, page, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0, 1), new Vector2(0, 1));
        statsPanel.sizeDelta = new Vector2(0, 300);
        statsPanel.gameObject.AddComponent<VerticalLayoutGroup>().spacing = 8f;

        if (config.StatTypes != null)
        {
            foreach (var statType in config.StatTypes)
            {
                BuildStatRow(statsPanel, statType);
            }
        }

        var subTabBar = CreateRect(config.SubTabBarName, page, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0, 1), new Vector2(0, 1));
        subTabBar.anchoredPosition = new Vector2(0, -300);
        subTabBar.sizeDelta = new Vector2(0, 120);
        subTabBar.gameObject.AddComponent<HorizontalLayoutGroup>().spacing = 16f;
        if (config.Tabs != null)
        {
            foreach (var tab in config.Tabs)
            {
                if (tab == null)
                    continue;

                BuildNavButton(subTabBar, tab);
            }
        }

        BuildHorizontalScroll(page, config.ContentScrollName, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 0), 360);
    }

    private static void BuildShopPage(Transform parent, ShopPageConfig config)
    {
        if (config == null)
            return;

        var page = CreateRect(config.Name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        page.gameObject.AddComponent<CanvasGroup>();

        var subTabBar = CreateRect(config.SubTabBarName, page, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, 1));
        subTabBar.sizeDelta = new Vector2(0, 120);
        subTabBar.gameObject.AddComponent<HorizontalLayoutGroup>().spacing = 16f;
        if (config.Tabs != null)
        {
            foreach (var tab in config.Tabs)
            {
                if (tab == null)
                    continue;

                BuildNavButton(subTabBar, tab);
            }
        }

        BuildHorizontalScroll(page, config.ContentScrollName, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 0), 300);
    }

    private static void BuildProgressionPage(Transform parent, RectTransform progressionHubPrefab, ProgressionPageConfig config)
    {
        if (config == null)
            return;

        var page = CreateRect(config.Name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        page.gameObject.AddComponent<CanvasGroup>();

        if (progressionHubPrefab == null)
        {
            CreateText("Title", page, config.TitleLabel, 36);
            return;
        }

        var hubInstance = PrefabUtility.InstantiatePrefab(progressionHubPrefab.gameObject) as GameObject;
        if (hubInstance == null)
            return;

        var hubRect = hubInstance.transform as RectTransform;
        hubRect.SetParent(page, false);
        hubRect.anchorMin = Vector2.zero;
        hubRect.anchorMax = Vector2.one;
        hubRect.offsetMin = Vector2.zero;
        hubRect.offsetMax = Vector2.zero;
    }

    private static RectTransform BuildShopModal(Transform parent)
    {
        var modal = CreateRect("ShopItemDetailsModal", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var modalComponent = modal.gameObject.AddComponent<ShopItemDetailsModal>();

        var background = CreateButton("Background", modal, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 0));
        var panel = CreateRect("Panel", modal, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        panel.sizeDelta = new Vector2(800, 900);

        var icon = CreateImage("ItemIcon", panel, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(200, 200));
        var nameText = CreateText("ItemNameText", panel, "Item", 36);
        var descriptionText = CreateText("DescriptionText", panel, "Description", 24);
        var priceArea = CreateRect("PriceArea", panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        priceArea.sizeDelta = new Vector2(400, 100);
        priceArea.gameObject.AddComponent<HorizontalLayoutGroup>().spacing = 12f;
        var currencyIcon = CreateImage("CurrencyIcon", priceArea, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(60, 60));
        var priceText = CreateText("PriceText", priceArea, "0", 28);
        var buyButton = CreateButton("BuyButton", panel, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        buyButton.GetComponentInChildren<TMP_Text>().text = "BUY";
        var cancelButton = CreateButton("CancelButton", panel, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        cancelButton.GetComponentInChildren<TMP_Text>().text = "CANCEL";

        SetSerializedReference(modalComponent, "itemIcon", icon);
        SetSerializedReference(modalComponent, "itemNameText", nameText);
        SetSerializedReference(modalComponent, "descriptionText", descriptionText);
        SetSerializedReference(modalComponent, "currencyIcon", currencyIcon);
        SetSerializedReference(modalComponent, "priceText", priceText);
        SetSerializedReference(modalComponent, "buyButton", buyButton.GetComponent<Button>());
        SetSerializedReference(modalComponent, "cancelButton", cancelButton.GetComponent<Button>());

        background.GetComponent<Button>().onClick.AddListener(modalComponent.Hide);
        modal.gameObject.SetActive(false);

        return modal;
    }

    private static void AttachHangarController(RectTransform pageContainer, HangarPageConfig config)
    {
        if (config == null)
            return;

        var hangarPage = pageContainer.Find(config.Name);
        if (hangarPage == null)
            return;

        var hangarController = hangarPage.gameObject.AddComponent<HangarPageController>();
        var statsPanel = hangarPage.Find(config.StatsPanelName);
        if (statsPanel == null)
            return;

        var statRows = statsPanel.GetComponentsInChildren<HangarStatRowView>();
        SetSerializedReference(hangarController, "statRows", statRows);

        var scroll = hangarPage.Find(config.ContentScrollName);
        if (scroll != null)
        {
            var content = scroll.Find("Viewport/Content");
            SetSerializedReference(hangarController, "contentRoot", content);
        }

        var upgradePrefab = BuildHangarUpgradePrefab();
        var cosmeticPrefab = BuildHangarCosmeticPrefab();

        SetSerializedReference(hangarController, "upgradeItemPrefab", upgradePrefab.GetComponent<HangarUpgradeItemView>());
        SetSerializedReference(hangarController, "cosmeticItemPrefab", cosmeticPrefab.GetComponent<HangarCosmeticItemView>());
    }

    private static void AttachShopController(RectTransform pageContainer, RectTransform modal, ShopPageConfig config)
    {
        if (config == null)
            return;

        var shopPage = pageContainer.Find(config.Name);
        if (shopPage == null)
            return;

        var shopController = shopPage.gameObject.AddComponent<ShopPageController>();
        var scroll = shopPage.Find(config.ContentScrollName);
        if (scroll != null)
        {
            var content = scroll.Find("Viewport/Content");
            SetSerializedReference(shopController, "contentRoot", content);
        }

        var shopItemPrefab = BuildShopItemPrefab();
        SetSerializedReference(shopController, "itemCardPrefab", shopItemPrefab.GetComponent<ShopItemCardView>());
        SetSerializedReference(shopController, "detailsModal", modal.GetComponent<ShopItemDetailsModal>());
    }

    private static RectTransform BuildHangarUpgradePrefab()
    {
        var root = CreateRect("HangarUpgradeItemCard", null, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var component = root.gameObject.AddComponent<HangarUpgradeItemView>();
        var icon = CreateImage("IconImage", root, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(100, 100));
        var nameText = CreateText("NameText", root, "Upgrade", 24);
        var levelText = CreateText("LevelText", root, "Lv 1", 20);
        var costText = CreateText("CostText", root, "100", 20);
        var button = CreateButton("UpgradeButton", root, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
        button.GetComponentInChildren<TMP_Text>().text = "UPGRADE";

        SetSerializedReference(component, "iconImage", icon);
        SetSerializedReference(component, "nameText", nameText);
        SetSerializedReference(component, "levelText", levelText);
        SetSerializedReference(component, "costText", costText);
        SetSerializedReference(component, "upgradeButton", button.GetComponent<Button>());

        var prefabPath = Path.Combine(PrefabRoot, "HangarUpgradeItemCard.prefab");
        var prefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject, prefabPath);
        Object.DestroyImmediate(root.gameObject);
        return prefab.transform as RectTransform;
    }

    private static RectTransform BuildHangarCosmeticPrefab()
    {
        var root = CreateRect("HangarCosmeticItemCard", null, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var component = root.gameObject.AddComponent<HangarCosmeticItemView>();
        var icon = CreateImage("IconImage", root, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(100, 100));
        var nameText = CreateText("NameText", root, "Item", 24);
        var priceText = CreateText("PriceText", root, "100", 20);
        var locked = CreateRect("LockedState", root, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
        var equipped = CreateRect("EquippedState", root, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
        var button = CreateButton("ActionButton", root, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
        button.GetComponentInChildren<TMP_Text>().text = "SELECT";

        SetSerializedReference(component, "iconImage", icon);
        SetSerializedReference(component, "nameText", nameText);
        SetSerializedReference(component, "priceText", priceText);
        SetSerializedReference(component, "lockedState", locked.gameObject);
        SetSerializedReference(component, "equippedState", equipped.gameObject);
        SetSerializedReference(component, "actionButton", button.GetComponent<Button>());

        var prefabPath = Path.Combine(PrefabRoot, "HangarCosmeticItemCard.prefab");
        var prefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject, prefabPath);
        Object.DestroyImmediate(root.gameObject);
        return prefab.transform as RectTransform;
    }

    private static RectTransform BuildShopItemPrefab()
    {
        var root = CreateRect("ShopItemCard", null, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var component = root.gameObject.AddComponent<ShopItemCardView>();
        var icon = CreateImage("IconImage", root, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(100, 100));
        var nameText = CreateText("NameText", root, "Item", 24);
        var priceText = CreateText("PriceText", root, "100", 20);
        var button = CreateButton("BuyButton", root, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
        button.GetComponentInChildren<TMP_Text>().text = "BUY";

        SetSerializedReference(component, "iconImage", icon);
        SetSerializedReference(component, "nameText", nameText);
        SetSerializedReference(component, "priceText", priceText);
        SetSerializedReference(component, "buyButton", button.GetComponent<Button>());

        var prefabPath = Path.Combine(PrefabRoot, "ShopItemCard.prefab");
        var prefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject, prefabPath);
        Object.DestroyImmediate(root.gameObject);
        return prefab.transform as RectTransform;
    }

    private static RectTransform BuildProgressionTaskRowPrefab()
    {
        var root = CreateRect("ProgressionTaskRow", null, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        root.sizeDelta = new Vector2(0, 120);
        var layout = root.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.spacing = 16f;
        layout.padding = new RectOffset(16, 16, 12, 12);
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;

        var component = root.gameObject.AddComponent<ProgressionTaskRowView>();
        var icon = CreateImage("Icon", root, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(72, 72));
        icon.color = new Color(1f, 1f, 1f, 0.9f);

        var description = CreateText("DescriptionText", root, "Complete 3 runs", 26, TextAlignmentOptions.Left);
        var descriptionLayout = description.gameObject.AddComponent<LayoutElement>();
        descriptionLayout.preferredWidth = 360f;

        var progressText = CreateText("ProgressText", root, "1/3", 22, TextAlignmentOptions.Center);
        progressText.gameObject.AddComponent<LayoutElement>().preferredWidth = 80f;
        var completeIndicator = CreateText("CompleteIndicator", root, "✓", 26, TextAlignmentOptions.Center);
        completeIndicator.gameObject.AddComponent<LayoutElement>().preferredWidth = 40f;
        completeIndicator.gameObject.SetActive(false);

        var slider = CreateProgressSlider("ProgressSlider", root, new Vector2(220, 20));
        slider.value = 0.33f;
        slider.gameObject.AddComponent<LayoutElement>().preferredWidth = 220f;

        var rewardRoot = CreateRect("Reward", root, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        rewardRoot.sizeDelta = new Vector2(120, 90);
        var rewardImage = rewardRoot.gameObject.AddComponent<Image>();
        rewardImage.color = new Color(1f, 1f, 1f, 0.12f);
        var rewardIcon = CreateImage("RewardIcon", rewardRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(48, 48));
        rewardIcon.color = new Color(1f, 1f, 1f, 0.95f);
        var rewardText = CreateText("RewardText", rewardRoot, "50", 20, TextAlignmentOptions.Center);
        var completeOverlay = BuildStatusOverlay(rewardRoot, "Complete");
        completeOverlay.name = "CompleteOverlay";
        completeOverlay.SetActive(false);

        SetSerializedReference(component, "iconImage", icon);
        SetSerializedReference(component, "descriptionText", description);
        SetSerializedReference(component, "progressText", progressText);
        SetSerializedReference(component, "progressSlider", slider);
        SetSerializedReference(component, "rewardIcon", rewardIcon);
        SetSerializedReference(component, "rewardText", rewardText);
        SetSerializedReference(component, "completeOverlay", completeOverlay);
        SetSerializedReference(component, "completeIndicator", completeIndicator);

        var prefabPath = Path.Combine(PrefabRoot, "ProgressionTaskRow.prefab");
        var prefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject, prefabPath);
        Object.DestroyImmediate(root.gameObject);
        return prefab.transform as RectTransform;
    }

    private static Dictionary<ProgressionTaskRowStyle, RectTransform> BuildProgressionTaskRowVariants(RectTransform basePrefab)
    {
        var variants = new Dictionary<ProgressionTaskRowStyle, RectTransform>
        {
            [ProgressionTaskRowStyle.MultiStep] = basePrefab
        };

        if (basePrefab == null)
            return variants;

        variants[ProgressionTaskRowStyle.SliderOnly] = BuildTaskRowVariant(basePrefab, "ProgressionTaskRow_SliderOnly", row =>
        {
            var progressText = row.transform.Find("ProgressText");
            if (progressText != null)
                progressText.gameObject.SetActive(false);

            var slider = row.transform.Find("ProgressSlider");
            var sliderLayout = slider != null ? slider.GetComponent<LayoutElement>() : null;
            if (sliderLayout != null)
                sliderLayout.preferredWidth = 300f;
        });

        variants[ProgressionTaskRowStyle.Completed] = BuildTaskRowVariant(basePrefab, "ProgressionTaskRow_Completed", row =>
        {
            var progressText = row.transform.Find("ProgressText")?.GetComponent<TMP_Text>();
            if (progressText != null)
                progressText.text = "3/3";

            var slider = row.transform.Find("ProgressSlider")?.GetComponent<Slider>();
            if (slider != null)
                slider.value = 1f;

            var overlay = row.transform.Find("Reward/CompleteOverlay");
            if (overlay != null)
                overlay.gameObject.SetActive(true);

            var indicator = row.transform.Find("CompleteIndicator");
            if (indicator != null)
                indicator.gameObject.SetActive(true);
        });

        return variants;
    }

    private static RectTransform BuildTaskRowVariant(RectTransform basePrefab, string name, System.Action<GameObject> mutate)
    {
        var instance = PrefabUtility.InstantiatePrefab(basePrefab.gameObject) as GameObject;
        if (instance == null)
            return null;

        instance.name = name;
        mutate?.Invoke(instance);
        var prefabPath = Path.Combine(PrefabRoot, $"{name}.prefab");
        var prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        Object.DestroyImmediate(instance);
        return prefab.transform as RectTransform;
    }

    private static RectTransform BuildProgressionRewardNodePrefab()
    {
        var root = CreateRect("ProgressionRewardNode", null, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        root.sizeDelta = new Vector2(100, 120);
        var background = root.gameObject.AddComponent<Image>();
        background.color = new Color(1f, 1f, 1f, 0.08f);
        var component = root.gameObject.AddComponent<ProgressionRewardNodeView>();
        var layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 6f;
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        var icon = CreateImage("RewardIcon", root, new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), Vector2.zero, new Vector2(52, 52));
        icon.color = new Color(1f, 1f, 1f, 0.95f);
        var iconLayout = icon.gameObject.AddComponent<LayoutElement>();
        iconLayout.preferredWidth = 52f;
        iconLayout.preferredHeight = 52f;
        var rewardLabel = CreateText("RewardLabel", root, "50 Coins", 20, TextAlignmentOptions.Center);
        var pointsText = CreateText("PointsText", root, "50 pts", 16, TextAlignmentOptions.Center);
        var stateRoot = CreateRect("StatusStates", root, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        var locked = BuildStatusOverlay(stateRoot, "Locked");
        locked.name = "LockedState";
        var claimable = BuildStatusOverlay(stateRoot, "Claim");
        claimable.name = "ClaimableState";
        var claimed = BuildStatusOverlay(stateRoot, "Claimed");
        claimed.name = "ClaimedState";
        claimable.SetActive(false);
        claimed.SetActive(false);

        var claimButton = CreateTransparentButton("ClaimButton", root, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero);
        claimButton.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);

        SetSerializedReference(component, "rewardIcon", icon);
        SetSerializedReference(component, "rewardLabel", rewardLabel);
        SetSerializedReference(component, "pointsText", pointsText);
        SetSerializedReference(component, "lockedState", locked);
        SetSerializedReference(component, "claimableState", claimable);
        SetSerializedReference(component, "claimedState", claimed);
        SetSerializedReference(component, "claimButton", claimButton.GetComponent<Button>());

        var prefabPath = Path.Combine(PrefabRoot, "ProgressionRewardNode.prefab");
        var prefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject, prefabPath);
        Object.DestroyImmediate(root.gameObject);
        return prefab.transform as RectTransform;
    }

    private static RectTransform BuildProgressionHubPrefab(
        RectTransform taskRowPrefab,
        Dictionary<ProgressionTaskRowStyle, RectTransform> taskRowVariants,
        RectTransform rewardNodePrefab,
        ProgressionTasksConfig config,
        ProgressionPageConfig pageConfig)
    {
        var root = CreateRect("ProgressionTasksHub", null, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var hubView = root.gameObject.AddComponent<ProgressionTasksHubView>();
        var hubController = root.gameObject.AddComponent<ProgressionTasksController>();
        var layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 20f;
        layout.padding = new RectOffset(24, 24, 24, 24);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var tabBar = CreateRect("TabBar", root, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        tabBar.sizeDelta = new Vector2(0, 100);
        var tabLayout = tabBar.gameObject.AddComponent<HorizontalLayoutGroup>();
        tabLayout.spacing = 16f;
        tabLayout.childAlignment = TextAnchor.MiddleCenter;
        tabLayout.childForceExpandWidth = false;
        tabLayout.childForceExpandHeight = false;
        var tabConfigs = BuildProgressionTabConfigMap(pageConfig);
        var dailyTab = BuildTabButton(tabBar, tabConfigs[ProgressionCadence.Daily]);
        var weeklyTab = BuildTabButton(tabBar, tabConfigs[ProgressionCadence.Weekly]);
        var monthlyTab = BuildTabButton(tabBar, tabConfigs[ProgressionCadence.Monthly]);

        var contentRoot = CreateRect("ContentRoot", root, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        contentRoot.sizeDelta = new Vector2(0, 1100);
        var contentLayout = contentRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        contentLayout.spacing = 20f;
        contentLayout.childAlignment = TextAnchor.UpperCenter;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;

        var dailyContent = BuildProgressionContentContainer(contentRoot, "DailyContent", ProgressionCadence.Daily, taskRowPrefab, taskRowVariants, rewardNodePrefab, config);
        var weeklyContent = BuildProgressionContentContainer(contentRoot, "WeeklyContent", ProgressionCadence.Weekly, taskRowPrefab, taskRowVariants, rewardNodePrefab, config);
        var monthlyContent = BuildProgressionContentContainer(contentRoot, "MonthlyContent", ProgressionCadence.Monthly, taskRowPrefab, taskRowVariants, rewardNodePrefab, config);

        if (weeklyContent != null)
            weeklyContent.gameObject.SetActive(false);
        if (monthlyContent != null)
            monthlyContent.gameObject.SetActive(false);

        SetSerializedReference(hubView, "dailyTabButton", dailyTab.GetComponent<Button>());
        SetSerializedReference(hubView, "weeklyTabButton", weeklyTab.GetComponent<Button>());
        SetSerializedReference(hubView, "monthlyTabButton", monthlyTab.GetComponent<Button>());
        SetSerializedReference(hubView, "dailyTabImage", dailyTab.GetComponent<Image>());
        SetSerializedReference(hubView, "weeklyTabImage", weeklyTab.GetComponent<Image>());
        SetSerializedReference(hubView, "monthlyTabImage", monthlyTab.GetComponent<Image>());
        SetSerializedReference(hubView, "dailyContent", dailyContent.gameObject);
        SetSerializedReference(hubView, "weeklyContent", weeklyContent.gameObject);
        SetSerializedReference(hubView, "monthlyContent", monthlyContent.gameObject);

        SetSerializedReference(hubController, "config", config);
        SetSerializedReference(hubController, "rewardNodePrefab", rewardNodePrefab != null ? rewardNodePrefab.GetComponent<ProgressionRewardNodeView>() : null);
        SetSerializedReference(hubController, "defaultTaskRowPrefab", taskRowPrefab != null ? taskRowPrefab.GetComponent<ProgressionTaskRowView>() : null);
        SetSerializedReference(hubController, "sliderOnlyTaskRowPrefab", GetVariantView(taskRowVariants, ProgressionTaskRowStyle.SliderOnly));
        SetSerializedReference(hubController, "completedTaskRowPrefab", GetVariantView(taskRowVariants, ProgressionTaskRowStyle.Completed));
        SetSerializedReference(hubController, "contentViews", new[]
        {
            dailyContent.GetComponent<ProgressionTasksContentView>(),
            weeklyContent.GetComponent<ProgressionTasksContentView>(),
            monthlyContent.GetComponent<ProgressionTasksContentView>()
        });

        var prefabPath = Path.Combine(PrefabRoot, "ProgressionTasksHub.prefab");
        var prefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject, prefabPath);
        Object.DestroyImmediate(root.gameObject);
        return prefab.transform as RectTransform;
    }

    private static Dictionary<ProgressionCadence, ProgressionTabConfig> BuildProgressionTabConfigMap(ProgressionPageConfig pageConfig)
    {
        var defaults = new Dictionary<ProgressionCadence, ProgressionTabConfig>
        {
            { ProgressionCadence.Daily, new ProgressionTabConfig(ProgressionCadence.Daily, "DailyTab", "Daily", null) },
            { ProgressionCadence.Weekly, new ProgressionTabConfig(ProgressionCadence.Weekly, "WeeklyTab", "Weekly", null) },
            { ProgressionCadence.Monthly, new ProgressionTabConfig(ProgressionCadence.Monthly, "MonthlyTab", "Monthly", null) }
        };

        if (pageConfig?.Tabs == null)
            return defaults;

        foreach (var tab in pageConfig.Tabs)
        {
            defaults[tab.Cadence] = tab;
        }

        return defaults;
    }

    private static RectTransform BuildProgressionContentContainer(
        Transform parent,
        string name,
        ProgressionCadence cadence,
        RectTransform taskRowPrefab,
        Dictionary<ProgressionTaskRowStyle, RectTransform> taskRowVariants,
        RectTransform rewardNodePrefab,
        ProgressionTasksConfig config)
    {
        var contentRoot = CreateRect(name, parent, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        var contentView = contentRoot.gameObject.AddComponent<ProgressionTasksContentView>();
        var layout = contentRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 20f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var group = config != null ? config.GetGroup(cadence) : null;
        var rewardTrack = BuildRewardTrack(contentRoot, rewardNodePrefab, group);

        var timeText = CreateText("TimeRemainingText", contentRoot, group != null ? FormatDuration(group.TimeRemainingSeconds) : "Time remaining: 00:00", 22, TextAlignmentOptions.Center);
        timeText.gameObject.AddComponent<LayoutElement>().preferredHeight = 36f;

        var taskList = BuildVerticalScroll(contentRoot, "TaskList", 720f);
        PopulateTaskList(taskList, taskRowPrefab, taskRowVariants, group);

        SetSerializedReference(contentView, "cadence", cadence);
        SetSerializedReference(contentView, "pointsValueText", rewardTrack.pointsValueText);
        SetSerializedReference(contentView, "timeRemainingText", timeText);
        SetSerializedReference(contentView, "progressBarFill", rewardTrack.progressFill);
        SetSerializedReference(contentView, "rewardRow", rewardTrack.rewardRow);
        SetSerializedReference(contentView, "taskListContent", taskList.Find("Viewport/Content") as RectTransform);

        return contentRoot;
    }

    private static ProgressionRewardTrackElements BuildRewardTrack(Transform parent, RectTransform rewardNodePrefab, ProgressionTaskGroupDefinition group)
    {
        var rewardTrack = CreateRect("RewardTrack", parent, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        rewardTrack.sizeDelta = new Vector2(0, 240);
        var rewardLayout = rewardTrack.gameObject.AddComponent<VerticalLayoutGroup>();
        rewardLayout.spacing = 12f;
        rewardLayout.childAlignment = TextAnchor.UpperCenter;
        rewardLayout.childForceExpandWidth = true;
        rewardLayout.childForceExpandHeight = false;

        var pointsLabel = group != null ? group.PointsLabel : "Progress Points";
        var pointsValue = group != null ? $"{group.CurrentPoints} / {group.TargetPoints}" : "0 / 0";
        var pointsValueText = BuildProgressPointsSummary(rewardTrack, pointsLabel, pointsValue);

        var progressBar = CreateProgressBar("ProgressBar", rewardTrack, new Vector2(0, 24));
        if (group != null && group.TargetPoints > 0)
            progressBar.fillAmount = Mathf.Clamp01(group.CurrentPoints / (float)group.TargetPoints);
        else
            progressBar.fillAmount = 0f;

        var rewardRow = CreateRect("RewardRow", rewardTrack, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        rewardRow.sizeDelta = new Vector2(0, 150);
        var rewardRowLayout = rewardRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        rewardRowLayout.spacing = 24f;
        rewardRowLayout.childAlignment = TextAnchor.MiddleCenter;
        rewardRowLayout.childForceExpandHeight = false;
        rewardRowLayout.childForceExpandWidth = false;

        if (rewardNodePrefab == null)
            return new ProgressionRewardTrackElements(pointsValueText, progressBar, rewardRow);

        if (group == null)
        {
            var rewardInstance = PrefabUtility.InstantiatePrefab(rewardNodePrefab.gameObject) as GameObject;
            if (rewardInstance != null)
                rewardInstance.transform.SetParent(rewardRow, false);
            return new ProgressionRewardTrackElements(pointsValueText, progressBar, rewardRow);
        }

        foreach (var reward in group.Rewards)
        {
            var rewardInstance = PrefabUtility.InstantiatePrefab(rewardNodePrefab.gameObject) as GameObject;
            if (rewardInstance == null)
                continue;
            rewardInstance.transform.SetParent(rewardRow, false);
            ApplyRewardDefinition(rewardInstance, reward);
        }

        return new ProgressionRewardTrackElements(pointsValueText, progressBar, rewardRow);
    }

    private static void ApplyRewardDefinition(GameObject rewardInstance, ProgressionRewardDefinition reward)
    {
        if (rewardInstance == null || reward == null)
            return;

        var view = rewardInstance.GetComponent<ProgressionRewardNodeView>();
        if (view == null)
            return;

        if (view.RewardLabel != null)
            view.RewardLabel.text = reward.RewardLabel;
        if (view.PointsText != null)
            view.PointsText.text = $"{reward.PointsRequired} pts";
        if (view.RewardIcon != null && reward.Icon != null)
            view.RewardIcon.sprite = reward.Icon;
        view.SetState(reward.State);
    }

    private static void PopulateTaskList(
        RectTransform taskList,
        RectTransform taskRowPrefab,
        Dictionary<ProgressionTaskRowStyle, RectTransform> taskRowVariants,
        ProgressionTaskGroupDefinition group)
    {
        if (taskList == null)
            return;

        var content = taskList.Find("Viewport/Content");
        if (content == null)
            return;

        if (group == null || group.Tasks.Count == 0)
        {
            if (taskRowPrefab == null)
                return;
            for (int i = 0; i < 4; i++)
            {
                var taskInstance = PrefabUtility.InstantiatePrefab(taskRowPrefab.gameObject) as GameObject;
                if (taskInstance == null)
                    continue;
                taskInstance.transform.SetParent(content, false);
            }
            return;
        }

        foreach (var task in group.Tasks)
        {
            var taskPrefab = GetTaskRowPrefab(taskRowPrefab, taskRowVariants, task.RowStyle);
            if (taskPrefab == null)
                continue;

            var taskInstance = PrefabUtility.InstantiatePrefab(taskPrefab.gameObject) as GameObject;
            if (taskInstance == null)
                continue;
            taskInstance.transform.SetParent(content, false);
            ApplyTaskDefinition(taskInstance, task);
        }
    }

    private static RectTransform GetTaskRowPrefab(
        RectTransform defaultPrefab,
        Dictionary<ProgressionTaskRowStyle, RectTransform> variants,
        ProgressionTaskRowStyle style)
    {
        if (variants != null && variants.TryGetValue(style, out var prefab) && prefab != null)
            return prefab;

        return defaultPrefab;
    }

    private static void ApplyTaskDefinition(GameObject taskInstance, ProgressionTaskDefinition task)
    {
        if (taskInstance == null || task == null)
            return;

        var view = taskInstance.GetComponent<ProgressionTaskRowView>();
        if (view == null)
            return;

        if (view.DescriptionText != null)
            view.DescriptionText.text = task.Description;

        if (view.ProgressText != null)
            view.ProgressText.text = $"{task.Current}/{task.Target}";

        if (view.ProgressSlider != null && task.Target > 0)
            view.ProgressSlider.value = Mathf.Clamp01(task.Current / (float)task.Target);

        if (view.IconImage != null && task.Icon != null)
            view.IconImage.sprite = task.Icon;

        if (view.RewardText != null)
            view.RewardText.text = task.RewardLabel;

        if (view.RewardIcon != null && task.RewardIcon != null)
            view.RewardIcon.sprite = task.RewardIcon;

        if (view.CompleteOverlay != null)
            view.CompleteOverlay.SetActive(task.RowStyle == ProgressionTaskRowStyle.Completed);
        if (view.CompleteIndicator != null)
            view.CompleteIndicator.gameObject.SetActive(task.RowStyle == ProgressionTaskRowStyle.Completed);
    }

    private static ProgressionTaskRowView GetVariantView(Dictionary<ProgressionTaskRowStyle, RectTransform> taskRowVariants, ProgressionTaskRowStyle style)
    {
        if (taskRowVariants != null && taskRowVariants.TryGetValue(style, out var prefab) && prefab != null)
            return prefab.GetComponent<ProgressionTaskRowView>();

        return null;
    }

    private static RectTransform BuildHorizontalScroll(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, float height)
    {
        var scrollRoot = CreateRect(name, parent, anchorMin, anchorMax, pivot, pivot);
        scrollRoot.anchoredPosition = anchoredPosition;
        scrollRoot.sizeDelta = new Vector2(0, height);

        var scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = true;
        scrollRect.vertical = false;

        var viewport = CreateRect("Viewport", scrollRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        var viewportImage = viewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(0, 0, 0, 0.1f);

        var content = CreateRect("Content", viewport, Vector2.zero, Vector2.one, new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        var layout = content.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 16f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        content.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewport;
        scrollRect.content = content;

        return scrollRoot;
    }

    private static RectTransform BuildVerticalScroll(Transform parent, string name, float height)
    {
        var scrollRoot = CreateRect(name, parent, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        scrollRoot.sizeDelta = new Vector2(0, height);

        var scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        var viewport = CreateRect("Viewport", scrollRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        var viewportImage = viewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(0, 0, 0, 0.08f);

        var content = CreateRect("Content", viewport, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewport;
        scrollRect.content = content;

        return scrollRoot;
    }

    private static RectTransform BuildStatRow(Transform parent, ShipStatType statType)
    {
        var row = CreateRect($"{statType}StatRow", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        row.sizeDelta = new Vector2(0, 40);
        var view = row.gameObject.AddComponent<HangarStatRowView>();
        SetSerializedReference(view, "statType", statType);
        var fill = CreateImage("Fill", row, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 20));
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        var valueText = CreateText("ValueText", row, "0", 18);
        SetSerializedReference(view, "fillImage", fill);
        SetSerializedReference(view, "valueText", valueText);
        return row;
    }

    private static void BuildPlaceholderPage(Transform parent, IReadOnlyList<PlaceholderPageConfig> pageConfigs, PageKind pageKind)
    {
        if (pageConfigs == null)
            return;

        foreach (var pageConfig in pageConfigs)
        {
            if (pageConfig == null)
                continue;

            if (pageConfig.Kind != pageKind)
                continue;

            var page = CreateRect(pageConfig.Name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            page.gameObject.AddComponent<CanvasGroup>();
            CreateText("Title", page, pageConfig.Label, 36);
            return;
        }
    }

    private static void BuildCurrencyPill(Transform parent, CurrencyPillConfig config)
    {
        if (config == null)
            return;

        var pill = CreateButton(config.Name, parent, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        var layout = pill.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 8f;
        var label = pill.GetComponentInChildren<TMP_Text>();
        if (label != null)
            label.text = GetLabelOrFallback(config.Label, config.Name);

        if (config.Icon != null)
        {
            var icon = CreateImage("IconImage", pill, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(40, 40));
            icon.sprite = config.Icon;
            icon.raycastTarget = false;
        }
        CreateText("AmountText", pill, "0", 24);
    }

    private static void BuildNavButton(Transform parent, TabButtonConfig config)
    {
        if (config == null)
            return;

        var button = CreateButton(config.Name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        var label = button.GetComponentInChildren<TMP_Text>();
        if (label != null)
            label.text = GetLabelOrFallback(config.Label, config.Name);

        if (config.Icon != null)
        {
            var layout = button.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            var icon = CreateImage("IconImage", button, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(36, 36));
            icon.sprite = config.Icon;
            icon.raycastTarget = false;
        }
    }

    private static void BuildNavButton(Transform parent, BottomNavButtonConfig config)
    {
        if (config == null)
            return;

        var button = CreateButton(config.Name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        var label = button.GetComponentInChildren<TMP_Text>();
        if (label != null)
            label.text = GetLabelOrFallback(config.Label, config.Name);

        if (config.Icon != null)
        {
            var layout = button.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            var icon = CreateImage("IconImage", button, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(36, 36));
            icon.sprite = config.Icon;
            icon.raycastTarget = false;
        }
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        if (parent != null)
            rect.SetParent(parent, false);
        return rect;
    }

    private static Image CreateImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        var rect = CreateRect(name, parent, anchorMin, anchorMax, pivot, anchoredPosition);
        rect.sizeDelta = sizeDelta;
        return rect.gameObject.AddComponent<Image>();
    }

    private static TMP_Text CreateText(string name, Transform parent, string text, int fontSize, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        var rect = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        var tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        return tmp;
    }

    private static Slider CreateProgressSlider(string name, Transform parent, Vector2 sizeDelta)
    {
        var root = CreateRect(name, parent, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero);
        root.sizeDelta = sizeDelta;
        var background = root.gameObject.AddComponent<Image>();
        background.color = new Color(1f, 1f, 1f, 0.2f);

        var fillArea = CreateRect("FillArea", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var fill = CreateImage("Fill", fillArea, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f), new Vector2(0, 0), new Vector2(0, 0));
        fill.color = new Color(0.3f, 0.8f, 0.2f, 1f);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = 1f;

        var slider = root.gameObject.AddComponent<Slider>();
        slider.fillRect = fill.rectTransform;
        slider.targetGraphic = background;
        slider.transition = Selectable.Transition.ColorTint;
        slider.interactable = false;
        return slider;
    }

    private static Image CreateProgressBar(string name, Transform parent, Vector2 sizeDelta)
    {
        var root = CreateRect(name, parent, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        root.sizeDelta = sizeDelta;
        var background = root.gameObject.AddComponent<Image>();
        background.color = new Color(1f, 1f, 1f, 0.15f);
        background.type = Image.Type.Sliced;

        var fill = CreateImage("Fill", root, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f), Vector2.zero, Vector2.zero);
        fill.color = new Color(0.3f, 0.8f, 0.2f, 1f);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = 0f;
        return fill;
    }

    private static TMP_Text BuildProgressPointsSummary(Transform parent, string label, string value)
    {
        var summary = CreateRect("PointsSummary", parent, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        summary.sizeDelta = new Vector2(0, 40);
        var layout = summary.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 16f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var labelText = CreateText("LabelText", summary, label, 22, TextAlignmentOptions.Left);
        labelText.gameObject.AddComponent<LayoutElement>().preferredWidth = 320f;
        var valueText = CreateText("ValueText", summary, value, 22, TextAlignmentOptions.Right);
        valueText.gameObject.AddComponent<LayoutElement>().preferredWidth = 200f;
        return valueText;
    }

    private static GameObject BuildStatusOverlay(Transform parent, string text)
    {
        var overlay = CreateRect("StatusOverlay", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var overlayImage = overlay.gameObject.AddComponent<Image>();
        overlayImage.color = new Color(0, 0, 0, 0.55f);
        var label = CreateText("StatusText", overlay, text, 18, TextAlignmentOptions.Center);
        label.color = Color.white;
        return overlay.gameObject;
    }

    private static RectTransform BuildTabButton(Transform parent, ProgressionTabConfig config)
    {
        var button = CreateButton(config.Name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        var layoutElement = button.gameObject.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = 220f;
        layoutElement.preferredHeight = 80f;
        var label = button.GetComponentInChildren<TMP_Text>();
        if (label != null)
            label.text = GetLabelOrFallback(config.Label, config.Name);

        if (config.Icon != null)
        {
            var layout = button.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            var icon = CreateImage("IconImage", button, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(32, 32));
            icon.sprite = config.Icon;
            icon.raycastTarget = false;
        }
        return button;
    }

    private static RectTransform CreateButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition)
    {
        var rect = CreateRect(name, parent, anchorMin, anchorMax, pivot, anchoredPosition);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.2f);
        var button = rect.gameObject.AddComponent<Button>();
        var handler = rect.gameObject.AddComponent<UiInteractionHandler>();
        button.onClick.AddListener(handler.HandleClick);
        var label = CreateText("Label", rect, name, 24);
        label.raycastTarget = false;
        return rect;
    }

    private static RectTransform CreateTransparentButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition)
    {
        var rect = CreateRect(name, parent, anchorMin, anchorMax, pivot, anchoredPosition);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);
        var button = rect.gameObject.AddComponent<Button>();
        var handler = rect.gameObject.AddComponent<UiInteractionHandler>();
        button.onClick.AddListener(handler.HandleClick);
        return rect;
    }

    private static void AddButtonAction(Button button, UnityAction action)
    {
        if (button == null || action == null)
            return;

        button.onClick.AddListener(action);
    }

    private static void AssignMenuController(MainMenuController menu, RectTransform pageContainer, RectTransform bottomNav, MainMenuBuilderConfig config)
    {
        var hangarName = config?.HangarPage?.Name ?? "HangarPage";
        var shopName = config?.ShopPage?.Name ?? "ShopPage";
        var progressionName = config?.ProgressionPage?.Name ?? "ProgressionPage";
        var playName = GetPlaceholderPageName(config, PageKind.Play, "PlayPage");
        var challengesName = GetPlaceholderPageName(config, PageKind.Challenges, "ChallengesPage");

        SetSerializedReference(menu, "hangarPage", pageContainer.Find(hangarName));
        SetSerializedReference(menu, "playPage", pageContainer.Find(playName));
        SetSerializedReference(menu, "shopPage", pageContainer.Find(shopName));
        SetSerializedReference(menu, "challengesPage", pageContainer.Find(challengesName));
        SetSerializedReference(menu, "progressionPage", pageContainer.Find(progressionName));
        SetSerializedReference(menu, "bottomNavBar", bottomNav.GetComponent<BottomNavBarController>());
    }

    private static void AssignTopBar(TopBarController controller, MainMenuController menu, RectTransform topBar, TopBarConfig config)
    {
        SetSerializedReference(controller, "menuController", menu);
        SetSerializedReference(controller, "levelText", topBar.Find("ProfileArea/LevelText").GetComponent<TMP_Text>());
        SetSerializedReference(controller, "xpProgressBar", topBar.Find("ProfileArea/XpProgressBar").GetComponent<Image>());

        var softPill = FindCurrencyPill(config, "SoftCurrencyPill");
        var premiumPill = FindCurrencyPill(config, "PremiumCurrencyPill");

        if (softPill != null)
        {
            var softRoot = topBar.Find($"CurrencyPills/{softPill.Name}");
            if (softRoot != null)
            {
                SetSerializedReference(controller, "softCurrencyText", softRoot.Find("AmountText").GetComponent<TMP_Text>());
                AddButtonAction(softRoot.GetComponent<Button>(), controller.OnSoftCurrencyClicked);
            }
        }

        if (premiumPill != null)
        {
            var premiumRoot = topBar.Find($"CurrencyPills/{premiumPill.Name}");
            if (premiumRoot != null)
            {
                SetSerializedReference(controller, "premiumCurrencyText", premiumRoot.Find("AmountText").GetComponent<TMP_Text>());
                AddButtonAction(premiumRoot.GetComponent<Button>(), controller.OnPremiumCurrencyClicked);
            }
        }
    }

    private static void AssignBottomNav(BottomNavBarController controller, MainMenuController menu, RectTransform bottomNav, IReadOnlyList<BottomNavButtonConfig> buttonConfigs)
    {
        if (buttonConfigs == null)
        {
            controller.Initialize(menu);
            return;
        }

        var configs = new List<SerializedNavButton>(buttonConfigs.Count);
        for (var i = 0; i < buttonConfigs.Count; i++)
        {
            var config = buttonConfigs[i];
            if (config == null)
                continue;

            configs.Add(CreateNavButtonConfig(bottomNav, config.Page, config.Name));
        }

        SetSerializedReference(controller, "buttons", configs.ToArray());
        controller.Initialize(menu);
    }

    private static CurrencyPillConfig FindCurrencyPill(TopBarConfig config, string name)
    {
        if (config?.CurrencyPills == null)
            return null;

        foreach (var pill in config.CurrencyPills)
        {
            if (pill.Name == name)
                return pill;
        }

        return null;
    }

    private static string GetLabelOrFallback(string label, string fallback)
    {
        return string.IsNullOrWhiteSpace(label) ? fallback : label;
    }

    private static string GetPlaceholderPageName(MainMenuBuilderConfig config, PageKind kind, string fallbackName)
    {
        if (config?.PlaceholderPages == null)
            return fallbackName;

        foreach (var page in config.PlaceholderPages)
        {
            if (page.Kind == kind)
                return page.Name;
        }

        return fallbackName;
    }

    private static SerializedNavButton CreateNavButtonConfig(Transform root, MainPage page, string name)
    {
        var button = root.Find(name).GetComponent<Button>();
        return new SerializedNavButton
        {
            page = page,
            button = button,
            selectedState = null,
            lockedState = null
        };
    }

    private static void SetSerializedReference(Object target, string propertyName, object value)
    {
        var serializedObject = new SerializedObject(target);
        var property = serializedObject.FindProperty(propertyName);
        if (property == null)
            return;

        if (value is Object unityObject)
        {
            property.objectReferenceValue = unityObject;
        }
        else if (value is Object[] unityObjects)
        {
            property.arraySize = unityObjects.Length;
            for (int i = 0; i < unityObjects.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = unityObjects[i];
            }
        }
        else if (value is HangarStatRowView[] hangarRows)
        {
            property.arraySize = hangarRows.Length;
            for (int i = 0; i < hangarRows.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = hangarRows[i];
            }
        }
        else if (value is SerializedNavButton[] navButtons)
        {
            property.arraySize = navButtons.Length;
            for (int i = 0; i < navButtons.Length; i++)
            {
                var element = property.GetArrayElementAtIndex(i);
                var nav = navButtons[i];
                element.FindPropertyRelative("page").enumValueIndex = (int)nav.page;
                element.FindPropertyRelative("button").objectReferenceValue = nav.button;
                element.FindPropertyRelative("selectedState").objectReferenceValue = nav.selectedState;
                element.FindPropertyRelative("lockedState").objectReferenceValue = nav.lockedState;
            }
        }
        else if (value is ShipStatType statType)
        {
            property.enumValueIndex = (int)statType;
        }
        else if (value is ProgressionCadence cadence)
        {
            property.enumValueIndex = (int)cadence;
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static ProgressionTasksConfig GetOrCreateProgressionTasksConfig()
    {
        const string configFolder = "Assets/Config/Progression";
        const string configPath = "Assets/Config/Progression/ProgressionTasksConfig.asset";

        EnsureFolder(configFolder);
        var config = AssetDatabase.LoadAssetAtPath<ProgressionTasksConfig>(configPath);
        if (config != null)
            return config;

        config = ScriptableObject.CreateInstance<ProgressionTasksConfig>();
        PopulateDefaultProgressionConfig(config);
        AssetDatabase.CreateAsset(config, configPath);
        AssetDatabase.SaveAssets();
        return config;
    }

    private static void PopulateDefaultProgressionConfig(ProgressionTasksConfig config)
    {
        if (config == null)
            return;

        var serializedObject = new SerializedObject(config);
        var taskGroupsProperty = serializedObject.FindProperty("taskGroups");
        if (taskGroupsProperty == null)
            return;

        taskGroupsProperty.ClearArray();
        AddDefaultGroup(taskGroupsProperty, ProgressionCadence.Daily, 120, 300, 22320);
        AddDefaultGroup(taskGroupsProperty, ProgressionCadence.Weekly, 480, 1200, 187200);
        AddDefaultGroup(taskGroupsProperty, ProgressionCadence.Monthly, 900, 2400, 1105920);

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AddDefaultGroup(SerializedProperty taskGroupsProperty, ProgressionCadence cadence, int currentPoints, int targetPoints, int timeRemainingSeconds)
    {
        var index = taskGroupsProperty.arraySize;
        taskGroupsProperty.InsertArrayElementAtIndex(index);
        var groupProperty = taskGroupsProperty.GetArrayElementAtIndex(index);

        groupProperty.FindPropertyRelative("cadence").enumValueIndex = (int)cadence;
        groupProperty.FindPropertyRelative("pointsLabel").stringValue = "Progress Points";
        groupProperty.FindPropertyRelative("currentPoints").intValue = currentPoints;
        groupProperty.FindPropertyRelative("targetPoints").intValue = targetPoints;
        groupProperty.FindPropertyRelative("timeRemainingSeconds").intValue = timeRemainingSeconds;

        var rewardsProperty = groupProperty.FindPropertyRelative("rewards");
        rewardsProperty.ClearArray();
        AddReward(rewardsProperty, "50 Coins", 50, ProgressionRewardState.Claimable);
        AddReward(rewardsProperty, "100 Coins", 100, ProgressionRewardState.Locked);
        AddReward(rewardsProperty, "Rare Chest", 200, ProgressionRewardState.Locked);

        var tasksProperty = groupProperty.FindPropertyRelative("tasks");
        tasksProperty.ClearArray();
        AddTask(tasksProperty, "task_run", "Complete 3 runs", 1, 3, ProgressionTaskRowStyle.MultiStep, "50 Coins");
        AddTask(tasksProperty, "task_distance", "Travel 2,000m", 1200, 2000, ProgressionTaskRowStyle.SliderOnly, "30 Coins");
        AddTask(tasksProperty, "task_perfect", "Finish a run without crashing", 1, 1, ProgressionTaskRowStyle.Completed, "100 Coins");
    }

    private static void AddReward(SerializedProperty rewardsProperty, string label, int pointsRequired, ProgressionRewardState state)
    {
        var rewardIndex = rewardsProperty.arraySize;
        rewardsProperty.InsertArrayElementAtIndex(rewardIndex);
        var rewardProperty = rewardsProperty.GetArrayElementAtIndex(rewardIndex);
        rewardProperty.FindPropertyRelative("rewardLabel").stringValue = label;
        rewardProperty.FindPropertyRelative("pointsRequired").intValue = pointsRequired;
        rewardProperty.FindPropertyRelative("state").enumValueIndex = (int)state;
    }

    private static void AddTask(SerializedProperty tasksProperty, string id, string description, int current, int target, ProgressionTaskRowStyle rowStyle, string rewardLabel)
    {
        var taskIndex = tasksProperty.arraySize;
        tasksProperty.InsertArrayElementAtIndex(taskIndex);
        var taskProperty = tasksProperty.GetArrayElementAtIndex(taskIndex);
        taskProperty.FindPropertyRelative("id").stringValue = id;
        taskProperty.FindPropertyRelative("description").stringValue = description;
        taskProperty.FindPropertyRelative("current").intValue = current;
        taskProperty.FindPropertyRelative("target").intValue = target;
        taskProperty.FindPropertyRelative("rowStyle").enumValueIndex = (int)rowStyle;
        taskProperty.FindPropertyRelative("rewardLabel").stringValue = rewardLabel;
    }

    private static string FormatDuration(int totalSeconds)
    {
        if (totalSeconds < 0)
            totalSeconds = 0;

        var days = totalSeconds / 86400;
        var hours = (totalSeconds % 86400) / 3600;
        var minutes = (totalSeconds % 3600) / 60;

        if (days > 0)
            return $"Time remaining: {days}d {hours:00}h";

        return $"Time remaining: {hours:00}:{minutes:00}";
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        var parent = Path.GetDirectoryName(path);
        var folderName = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent ?? "Assets", folderName);
    }

    private struct SerializedNavButton
    {
        public MainPage page;
        public Button button;
        public GameObject selectedState;
        public GameObject lockedState;
    }
}
