using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class MainMenuPrefabBuilder
{
    private const string PrefabRoot = "Assets/Prefabs/UI/MainMenu";

    [MenuItem("Tools/UI/Build Main Menu UI")]
    public static void BuildMainMenuUI()
    {
        EnsureFolder(PrefabRoot);

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
        var topBar = BuildTopBar(uiRoot);
        var pageContainer = BuildPageContainer(uiRoot);
        var bottomNav = BuildBottomNav(uiRoot);
        var modalsRoot = CreateRect("ModalsRoot", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var modal = BuildShopModal(modalsRoot);

        var menuController = uiRoot.gameObject.AddComponent<MainMenuController>();
        AssignMenuController(menuController, pageContainer, bottomNav);

        var topBarController = topBar.gameObject.AddComponent<TopBarController>();
        AssignTopBar(topBarController, menuController, topBar);

        var bottomNavController = bottomNav.gameObject.AddComponent<BottomNavBarController>();
        AssignBottomNav(bottomNavController, menuController, bottomNav);

        AttachHangarController(pageContainer);
        AttachShopController(pageContainer, modal);

        PrefabUtility.SaveAsPrefabAsset(canvas, Path.Combine(PrefabRoot, "MainMenuCanvas.prefab"));
        Object.DestroyImmediate(canvas);
    }

    private static RectTransform BuildTopBar(Transform parent)
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

        BuildCurrencyPill(currencyPills, "SoftCurrencyPill");
        BuildCurrencyPill(currencyPills, "PremiumCurrencyPill");
        BuildCurrencyPill(currencyPills, "OtherCurrencyPill");

        topBar.gameObject.name = "TopBar";
        return topBar;
    }

    private static RectTransform BuildBottomNav(Transform parent)
    {
        var bottomBar = CreateRect("BottomNavBar", parent, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 0));
        bottomBar.sizeDelta = new Vector2(0, 200);
        var layout = bottomBar.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 24f;
        layout.padding = new RectOffset(20, 20, 20, 20);

        BuildNavButton(bottomBar, "ShopButton");
        BuildNavButton(bottomBar, "HangarButton");
        BuildNavButton(bottomBar, "PlayButton");
        BuildNavButton(bottomBar, "ChallengesButton");
        BuildNavButton(bottomBar, "ProgressionButton");

        return bottomBar;
    }

    private static RectTransform BuildPageContainer(Transform parent)
    {
        var container = CreateRect("PageContainer", parent, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 0));
        container.offsetMin = new Vector2(0, 200);
        container.offsetMax = new Vector2(0, -200);

        BuildHangarPage(container);
        BuildPlaceholderPage(container, "PlayPage");
        BuildShopPage(container);
        BuildPlaceholderPage(container, "ChallengesPage");
        BuildPlaceholderPage(container, "ProgressionPage");

        return container;
    }

    private static void BuildHangarPage(Transform parent)
    {
        var page = CreateRect("HangarPage", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        page.gameObject.AddComponent<CanvasGroup>();

        var shipDisplay = CreateRect("ShipDisplayArea", page, new Vector2(0, 0.5f), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 1));
        shipDisplay.sizeDelta = new Vector2(0, 400);

        var statsPanel = CreateRect("StatsPanel", page, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0, 1), new Vector2(0, 1));
        statsPanel.sizeDelta = new Vector2(0, 300);
        statsPanel.gameObject.AddComponent<VerticalLayoutGroup>().spacing = 8f;

        BuildStatRow(statsPanel, ShipStatType.Speed);
        BuildStatRow(statsPanel, ShipStatType.Handling);
        BuildStatRow(statsPanel, ShipStatType.Stability);
        BuildStatRow(statsPanel, ShipStatType.Boost);
        BuildStatRow(statsPanel, ShipStatType.Energy);

        var subTabBar = CreateRect("SubTabBar", page, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0, 1), new Vector2(0, 1));
        subTabBar.anchoredPosition = new Vector2(0, -300);
        subTabBar.sizeDelta = new Vector2(0, 120);
        subTabBar.gameObject.AddComponent<HorizontalLayoutGroup>().spacing = 16f;
        BuildNavButton(subTabBar, "UpgradesTabButton");
        BuildNavButton(subTabBar, "SkinsTabButton");
        BuildNavButton(subTabBar, "TrailsTabButton");
        BuildNavButton(subTabBar, "CoreFxTabButton");

        BuildHorizontalScroll(page, "ContentScroll", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 0), 360);
    }

    private static void BuildShopPage(Transform parent)
    {
        var page = CreateRect("ShopPage", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        page.gameObject.AddComponent<CanvasGroup>();

        var subTabBar = CreateRect("SubTabBar", page, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, 1));
        subTabBar.sizeDelta = new Vector2(0, 120);
        subTabBar.gameObject.AddComponent<HorizontalLayoutGroup>().spacing = 16f;
        BuildNavButton(subTabBar, "SkinsTabButton");
        BuildNavButton(subTabBar, "ShipsTabButton");
        BuildNavButton(subTabBar, "TrailsTabButton");
        BuildNavButton(subTabBar, "CurrencyTabButton");

        BuildHorizontalScroll(page, "ContentScroll", new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 0), 300);
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

    private static void AttachHangarController(RectTransform pageContainer)
    {
        var hangarPage = pageContainer.Find("HangarPage");
        if (hangarPage == null)
            return;

        var hangarController = hangarPage.gameObject.AddComponent<HangarPageController>();
        var statsPanel = hangarPage.Find("StatsPanel");
        var statRows = statsPanel.GetComponentsInChildren<HangarStatRowView>();
        SetSerializedReference(hangarController, "statRows", statRows);

        var scroll = hangarPage.Find("ContentScroll");
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

    private static void AttachShopController(RectTransform pageContainer, RectTransform modal)
    {
        var shopPage = pageContainer.Find("ShopPage");
        if (shopPage == null)
            return;

        var shopController = shopPage.gameObject.AddComponent<ShopPageController>();
        var scroll = shopPage.Find("ContentScroll");
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

    private static RectTransform BuildPlaceholderPage(Transform parent, string name)
    {
        var page = CreateRect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        page.gameObject.AddComponent<CanvasGroup>();
        CreateText("Title", page, name, 36);
        return page;
    }

    private static void BuildCurrencyPill(Transform parent, string name)
    {
        var pill = CreateButton(name, parent, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        var layout = pill.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 8f;
        var icon = CreateImage("IconImage", pill, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(40, 40));
        icon.raycastTarget = false;
        CreateText("AmountText", pill, "0", 24);
    }

    private static void BuildNavButton(Transform parent, string name)
    {
        var button = CreateButton(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        button.GetComponentInChildren<TMP_Text>().text = name.Replace("Button", string.Empty);
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

    private static TMP_Text CreateText(string name, Transform parent, string text, int fontSize)
    {
        var rect = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        var tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
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

    private static void AddButtonAction(Button button, UnityAction action)
    {
        if (button == null || action == null)
            return;

        button.onClick.AddListener(action);
    }

    private static void AssignMenuController(MainMenuController menu, RectTransform pageContainer, RectTransform bottomNav)
    {
        SetSerializedReference(menu, "hangarPage", pageContainer.Find("HangarPage"));
        SetSerializedReference(menu, "playPage", pageContainer.Find("PlayPage"));
        SetSerializedReference(menu, "shopPage", pageContainer.Find("ShopPage"));
        SetSerializedReference(menu, "challengesPage", pageContainer.Find("ChallengesPage"));
        SetSerializedReference(menu, "progressionPage", pageContainer.Find("ProgressionPage"));
        SetSerializedReference(menu, "bottomNavBar", bottomNav.GetComponent<BottomNavBarController>());
    }

    private static void AssignTopBar(TopBarController controller, MainMenuController menu, RectTransform topBar)
    {
        SetSerializedReference(controller, "menuController", menu);
        SetSerializedReference(controller, "levelText", topBar.Find("ProfileArea/LevelText").GetComponent<TMP_Text>());
        SetSerializedReference(controller, "xpProgressBar", topBar.Find("ProfileArea/XpProgressBar").GetComponent<Image>());
        SetSerializedReference(controller, "softCurrencyText", topBar.Find("CurrencyPills/SoftCurrencyPill/AmountText").GetComponent<TMP_Text>());
        SetSerializedReference(controller, "premiumCurrencyText", topBar.Find("CurrencyPills/PremiumCurrencyPill/AmountText").GetComponent<TMP_Text>());

        var softButton = topBar.Find("CurrencyPills/SoftCurrencyPill").GetComponent<Button>();
        var premiumButton = topBar.Find("CurrencyPills/PremiumCurrencyPill").GetComponent<Button>();
        AddButtonAction(softButton, controller.OnSoftCurrencyClicked);
        AddButtonAction(premiumButton, controller.OnPremiumCurrencyClicked);
    }

    private static void AssignBottomNav(BottomNavBarController controller, MainMenuController menu, RectTransform bottomNav)
    {
        SetSerializedReference(controller, "buttons", new[]
        {
            CreateNavButtonConfig(bottomNav, MainPage.Shop, "ShopButton"),
            CreateNavButtonConfig(bottomNav, MainPage.Hangar, "HangarButton"),
            CreateNavButtonConfig(bottomNav, MainPage.Play, "PlayButton"),
            CreateNavButtonConfig(bottomNav, MainPage.Challenges, "ChallengesButton"),
            CreateNavButtonConfig(bottomNav, MainPage.Progression, "ProgressionButton")
        });

        controller.Initialize(menu);
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

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
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
