using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct LevelInfo
{
    public string displayName;
    public int sides;
    public Sprite shapeSprite;
}

/// <summary>
/// Controls the main menu panel:
/// - Shows best score
/// - Level selection (left/right arrows)
/// - Lab entry and upgrade surface
/// - Play button triggers GameManager
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    private const int DefaultComboMaxLevel = 5;
    private const int DefaultComboBaseCost = 200;
    private const int DefaultComboCostIncrease = 150;

    [Header("Core References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private TMP_Text bestScoreText;
    [SerializeField] private Button settingsButton;

    [Header("Level Select UI")]
    [SerializeField] private TMP_Text levelNameText;
    [SerializeField] private Image levelShapeImage;
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;

    [Header("Levels")]
    [SerializeField] private LevelInfo[] levels;

    [Header("Gameplay Systems")]
    [SerializeField] private ObstacleRingGenerator obstacleRingGenerator;
    [SerializeField] private TunnelWallGenerator tunnelWallGenerator;
    [SerializeField] private RunSpeedController runSpeedController;
    [SerializeField] private SpeedScalingConfig defaultSpeedConfig;

    [Header("Monetisation / Lab Entry")]
    [SerializeField] private RemoveAdsIAPManager removeAdsIAPManager;
    [SerializeField] private GameObject removeAdsButtonRoot;
    [SerializeField] private GameObject restorePurchasesButtonRoot;
    [SerializeField] private GameObject premiumBadgeRoot;
    [SerializeField] private RemoveAdsThankYouUI thankYouPopup;

    [Header("ButtonVisibility")]
    [SerializeField] private bool hidePremiumUserIAPButton;
    [SerializeField] private bool hideRestorePurchasesButton;

    [Header("Lab")]
    [SerializeField] private PlayerProfile profile;
    [SerializeField] private ShipDatabase shipDatabase;
    [SerializeField] private PowerupUpgradeConfig powerupUpgradeConfig;
    [SerializeField] private GameObject featurePanelRoot;
    [SerializeField] private TMP_Text featureTitleText;
    [SerializeField] private RectTransform labContentRoot;
    [SerializeField] private TMP_Text labCurrencyText;
    [SerializeField] private Button featureCloseButton;
    [SerializeField] private string labButtonLabel = "LAB";
    [SerializeField] private string labTitle = "LAB";

    private readonly List<GameObject> _runtimeLabWidgets = new();
    private int _currentLevelIndex;
    private bool _labScaffoldReady;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (rootPanel == null)
            rootPanel = gameObject;

        if (obstacleRingGenerator == null)
            obstacleRingGenerator = FindFirstObjectByType<ObstacleRingGenerator>();

        if (tunnelWallGenerator == null)
            tunnelWallGenerator = FindFirstObjectByType<TunnelWallGenerator>();

        if (runSpeedController == null)
            runSpeedController = FindFirstObjectByType<RunSpeedController>();

        if (removeAdsIAPManager == null)
            removeAdsIAPManager = FindFirstObjectByType<RemoveAdsIAPManager>();

        ResolveDataReferences();
        ResolveLabReferences();

        if (leftArrowButton != null)
            leftArrowButton.onClick.AddListener(OnPrevLevel);

        if (rightArrowButton != null)
            rightArrowButton.onClick.AddListener(OnNextLevel);

        if (featureCloseButton != null)
            featureCloseButton.onClick.AddListener(CloseFeaturePanel);
    }

    private void OnEnable()
    {
        RemoveAdsIAPManager.OnRemoveAdsUnlocked += HandleRemoveAdsUnlocked;
        LocalizationService.LanguageChanged += HandleLanguageChanged;

        UpdateBestScoreDisplay();
        EnsureValidLevelIndex();
        ApplyLevelToWorld();
        UpdateLevelDisplay();
        UpdateRemoveAdsUI();
        RefreshLabView();
    }

    private void OnDisable()
    {
        RemoveAdsIAPManager.OnRemoveAdsUnlocked -= HandleRemoveAdsUnlocked;
        LocalizationService.LanguageChanged -= HandleLanguageChanged;
    }

    public void Show()
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);

        UpdateBestScoreDisplay();
        EnsureValidLevelIndex();
        ApplyLevelToWorld();
        UpdateLevelDisplay();
        UpdateRemoveAdsUI();
        RefreshLabView();
    }

    public void Hide()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);

        CloseFeaturePanel();
    }

    public void ShowSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void OnPlayButtonPressed()
    {
        ApplyLevelToWorld();
        CloseFeaturePanel();

        if (defaultSpeedConfig != null && runSpeedController != null)
        {
            runSpeedController.SetSpeedConfig(defaultSpeedConfig);
        }

        if (gameManager != null)
        {
            gameManager.OnPlayButtonPressed();
        }
    }

    private void UpdateBestScoreDisplay()
    {
        if (bestScoreText == null)
            return;

        RunScoreManager scoreManager = FindFirstObjectByType<RunScoreManager>();
        if (scoreManager != null)
        {
            int bestScore = Mathf.RoundToInt(scoreManager.BestScore);
            bestScoreText.text = LocalizationService.Format("ui.highscore_multiline", bestScore);
        }
    }

    private void EnsureValidLevelIndex()
    {
        if (levels == null || levels.Length == 0)
        {
            _currentLevelIndex = 0;
            return;
        }

        if (_currentLevelIndex < 0 || _currentLevelIndex >= levels.Length)
        {
            _currentLevelIndex = 0;
        }
    }

    private void UpdateLevelDisplay()
    {
        if (levels == null || levels.Length == 0)
        {
            if (levelNameText != null)
                levelNameText.text = LocalizationService.Get("ui.no_levels", "No Levels");

            if (levelShapeImage != null)
                levelShapeImage.enabled = false;

            return;
        }

        var info = levels[_currentLevelIndex];

        if (levelNameText != null)
            levelNameText.text = info.displayName;

        if (levelShapeImage != null)
        {
            levelShapeImage.enabled = info.shapeSprite != null;
            levelShapeImage.sprite = info.shapeSprite;
        }
    }

    private void HandleLanguageChanged()
    {
        UpdateBestScoreDisplay();
        UpdateLevelDisplay();
        RefreshLabView();
    }

    private void ApplyLevelToWorld()
    {
        if (levels == null || levels.Length == 0)
            return;

        var info = levels[_currentLevelIndex];
        int sides = Mathf.Max(3, info.sides);

        if (tunnelWallGenerator != null)
            tunnelWallGenerator.Rebuild(sides);
    }

    private void OnPrevLevel()
    {
        if (levels == null || levels.Length == 0)
            return;

        _currentLevelIndex--;
        if (_currentLevelIndex < 0)
            _currentLevelIndex = levels.Length - 1;

        ApplyLevelToWorld();
        UpdateLevelDisplay();
    }

    private void OnNextLevel()
    {
        if (levels == null || levels.Length == 0)
            return;

        _currentLevelIndex++;
        if (_currentLevelIndex >= levels.Length)
            _currentLevelIndex = 0;

        ApplyLevelToWorld();
        UpdateLevelDisplay();
    }

    private void HandleRemoveAdsUnlocked()
    {
        UpdateRemoveAdsUI();

        if (!HasLabPanel() && thankYouPopup != null)
        {
            thankYouPopup.Show();
        }
    }

    private void UpdateRemoveAdsUI()
    {
        if (HasLabPanel())
        {
            if (removeAdsButtonRoot != null)
            {
                removeAdsButtonRoot.SetActive(true);
                TMP_Text label = removeAdsButtonRoot.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                    label.text = labButtonLabel;
            }

            if (restorePurchasesButtonRoot != null)
                restorePurchasesButtonRoot.SetActive(false);

            if (premiumBadgeRoot != null)
                premiumBadgeRoot.SetActive(false);

            return;
        }

        bool hasRemoveAds = AdsConfig.RemoveAds;

        if (!hidePremiumUserIAPButton && removeAdsButtonRoot != null)
            removeAdsButtonRoot.SetActive(!hasRemoveAds);

        if (!hideRestorePurchasesButton && restorePurchasesButtonRoot != null)
            restorePurchasesButtonRoot.SetActive(!hasRemoveAds);

        if (premiumBadgeRoot != null)
            premiumBadgeRoot.SetActive(hasRemoveAds);
    }

    public void OnRemoveAdsButtonPressed()
    {
        if (HasLabPanel())
        {
            OpenLabPanel();
            return;
        }

        if (removeAdsIAPManager != null)
        {
            removeAdsIAPManager.BuyRemoveAds();
        }
    }

    public void OnRestorePurchasesButtonPressed()
    {
        if (removeAdsIAPManager != null)
        {
            removeAdsIAPManager.RestorePurchases();
        }
    }

    private void ResolveDataReferences()
    {
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

        if (powerupUpgradeConfig == null)
        {
            PowerupUpgradeConfig[] configs = Resources.FindObjectsOfTypeAll<PowerupUpgradeConfig>();
            if (configs != null && configs.Length > 0)
                powerupUpgradeConfig = configs[0];
        }
    }

    private void ResolveLabReferences()
    {
        if (featurePanelRoot == null)
        {
            Transform[] transforms = rootPanel != null
                ? rootPanel.transform.root.GetComponentsInChildren<Transform>(true)
                : FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i] != null && transforms[i].name == "FeaturePanel")
                {
                    featurePanelRoot = transforms[i].gameObject;
                    break;
                }
            }
        }

        if (featurePanelRoot == null)
            return;

        if (featureTitleText == null)
            featureTitleText = featurePanelRoot.GetComponentInChildren<TMP_Text>(true);

        EnsureLabScaffold();
    }

    private bool HasLabPanel()
    {
        return featurePanelRoot != null;
    }

    private void EnsureLabScaffold()
    {
        if (_labScaffoldReady || featurePanelRoot == null)
            return;

        VerticalLayoutGroup placeholderLayout = featurePanelRoot.GetComponent<VerticalLayoutGroup>();
        if (placeholderLayout != null)
            placeholderLayout.enabled = false;

        ContentSizeFitter placeholderFitter = featurePanelRoot.GetComponent<ContentSizeFitter>();
        if (placeholderFitter != null)
            placeholderFitter.enabled = false;

        if (featureTitleText != null)
        {
            featureTitleText.text = labTitle;
            RectTransform titleRect = featureTitleText.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(0f, 1f);
            titleRect.pivot = new Vector2(0f, 1f);
            titleRect.anchoredPosition = new Vector2(72f, -56f);
            titleRect.sizeDelta = new Vector2(500f, 80f);
            featureTitleText.alignment = TextAlignmentOptions.Left;
        }

        if (featureCloseButton == null)
            featureCloseButton = CreateFeatureCloseButton(featurePanelRoot.transform);

        if (labCurrencyText == null)
            labCurrencyText = CreateTopRightLabel(featurePanelRoot.transform, "LabCurrency", new Vector2(-120f, -64f), 36f, TextAlignmentOptions.Right);

        if (labContentRoot == null)
            labContentRoot = CreateLabContentRoot(featurePanelRoot.transform);

        featurePanelRoot.SetActive(false);
        _labScaffoldReady = true;
    }

    private void OpenLabPanel()
    {
        if (!HasLabPanel())
            return;

        EnsureLabScaffold();
        RefreshLabView();
        featurePanelRoot.SetActive(true);
    }

    private void CloseFeaturePanel()
    {
        if (featurePanelRoot != null)
            featurePanelRoot.SetActive(false);
    }

    private void RefreshLabView()
    {
        if (!HasLabPanel())
            return;

        EnsureLabScaffold();

        if (featureTitleText != null)
            featureTitleText.text = labTitle;

        if (labCurrencyText != null && profile != null)
            labCurrencyText.text = $"SOFT {profile.softCurrency}";

        if (labContentRoot == null)
            return;

        for (int i = 0; i < _runtimeLabWidgets.Count; i++)
        {
            if (_runtimeLabWidgets[i] != null)
                Destroy(_runtimeLabWidgets[i]);
        }
        _runtimeLabWidgets.Clear();

        _runtimeLabWidgets.Add(CreateSectionLabel(labContentRoot, "Combo"));
        _runtimeLabWidgets.Add(CreateUpgradeCard(
            labContentRoot,
            "Combo Modifier",
            GetComboDescription(),
            profile != null ? profile.GetUpgradeLevel(UpgradeType.ComboMultiplier) : 0,
            DefaultComboMaxLevel,
            GetComboUpgradeCost(),
            CanUpgradeCombo(),
            HandleComboUpgradePressed));

        _runtimeLabWidgets.Add(CreateSectionLabel(labContentRoot, "Powerups"));

        PowerupUpgradeConfig.PowerupUpgradeEntry[] powerupEntries = powerupUpgradeConfig != null
            ? powerupUpgradeConfig.GetAvailableUpgrades()
            : PowerupUpgradeConfig.GetDefaultEntries();

        for (int i = 0; i < powerupEntries.Length; i++)
        {
            PowerupUpgradeConfig.PowerupUpgradeEntry entry = powerupEntries[i];
            if (entry == null)
                continue;

            int currentLevel = profile != null ? profile.GetPowerupUpgradeLevel(entry.powerupType) : 0;
            int cost = entry.GetCostForLevel(currentLevel);
            bool canUpgrade = profile != null && currentLevel < entry.MaxLevel && profile.softCurrency >= cost;

            _runtimeLabWidgets.Add(CreateUpgradeCard(
                labContentRoot,
                entry.displayName,
                GetPowerupDescription(entry, currentLevel),
                currentLevel,
                entry.MaxLevel,
                cost,
                canUpgrade,
                () => HandlePowerupUpgradePressed(entry)));
        }
    }

    private int GetComboUpgradeCost()
    {
        int level = profile != null ? profile.GetUpgradeLevel(UpgradeType.ComboMultiplier) : 0;
        return DefaultComboBaseCost + DefaultComboCostIncrease * Mathf.Max(0, level);
    }

    private bool CanUpgradeCombo()
    {
        if (profile == null)
            return false;

        int currentLevel = profile.GetUpgradeLevel(UpgradeType.ComboMultiplier);
        return currentLevel < DefaultComboMaxLevel && profile.softCurrency >= GetComboUpgradeCost();
    }

    private string GetComboDescription()
    {
        int currentLevel = profile != null ? profile.GetUpgradeLevel(UpgradeType.ComboMultiplier) : 0;
        return $"Improves how quickly combos ramp score. Current level {currentLevel}.";
    }

    private string GetPowerupDescription(PowerupUpgradeConfig.PowerupUpgradeEntry entry, int currentLevel)
    {
        if (entry == null)
            return "No data available.";

        entry.TryGetLevel(currentLevel, out PowerupUpgradeConfig.PowerupUpgradeLevel currentLevelData);
        entry.TryGetLevel(Mathf.Min(currentLevel + 1, entry.MaxLevel), out PowerupUpgradeConfig.PowerupUpgradeLevel nextLevelData);

        switch (entry.powerupType)
        {
            case PowerupType.ScoreMultiplier:
                return $"Duration {currentLevelData.duration:0.#}s, multiplier x{currentLevelData.strength:0.##}. Next x{nextLevelData.strength:0.##}.";
            case PowerupType.CoinMultiplier:
                return $"Duration {currentLevelData.duration:0.#}s, spawn multiplier x{currentLevelData.strength:0.##}. Next x{nextLevelData.strength:0.##}.";
            case PowerupType.Magnet:
                return $"Duration {currentLevelData.duration:0.#}s, range x{currentLevelData.strength:0.##}. Next x{nextLevelData.strength:0.##}.";
            case PowerupType.AutoPilot:
                return $"Duration {currentLevelData.duration:0.#}s. Next {nextLevelData.duration:0.#}s.";
            case PowerupType.Shield:
                return $"Blocks one hit for {currentLevelData.duration:0.#}s. Next {nextLevelData.duration:0.#}s.";
            default:
                return $"Duration {currentLevelData.duration:0.#}s.";
        }
    }

    private void HandleComboUpgradePressed()
    {
        if (profile == null)
            return;

        int currentLevel = profile.GetUpgradeLevel(UpgradeType.ComboMultiplier);
        if (currentLevel >= DefaultComboMaxLevel)
            return;

        int cost = GetComboUpgradeCost();
        if (!profile.TrySpend(ShopCurrencyType.Soft, cost))
            return;

        profile.SetUpgradeLevel(UpgradeType.ComboMultiplier, currentLevel + 1);
        RefreshLabView();
    }

    private void HandlePowerupUpgradePressed(PowerupUpgradeConfig.PowerupUpgradeEntry entry)
    {
        if (profile == null || entry == null)
            return;

        int currentLevel = profile.GetPowerupUpgradeLevel(entry.powerupType);
        if (currentLevel >= entry.MaxLevel)
            return;

        int cost = entry.GetCostForLevel(currentLevel);
        if (!profile.TrySpend(ShopCurrencyType.Soft, cost))
            return;

        profile.SetPowerupUpgradeLevel(entry.powerupType, currentLevel + 1);
        RefreshLabView();
    }

    private GameObject CreateSectionLabel(Transform parent, string label)
    {
        TMP_FontAsset font = bestScoreText != null ? bestScoreText.font : TMP_Settings.defaultFontAsset;
        GameObject labelObject = new GameObject($"{label}Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        labelObject.transform.SetParent(parent, false);

        LayoutElement layout = labelObject.GetComponent<LayoutElement>();
        layout.preferredHeight = 40f;

        TMP_Text text = labelObject.GetComponent<TMP_Text>();
        text.font = font;
        text.fontSize = 32f;
        text.fontStyle = FontStyles.Bold;
        text.text = label.ToUpperInvariant();
        text.alignment = TextAlignmentOptions.Left;
        text.color = Color.white;
        text.raycastTarget = false;
        return labelObject;
    }

    private GameObject CreateUpgradeCard(
        Transform parent,
        string title,
        string description,
        int currentLevel,
        int maxLevel,
        int cost,
        bool canUpgrade,
        UnityEngine.Events.UnityAction onPressed)
    {
        TMP_FontAsset font = bestScoreText != null ? bestScoreText.font : TMP_Settings.defaultFontAsset;

        GameObject card = new GameObject($"{title}Card", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        card.transform.SetParent(parent, false);

        LayoutElement layout = card.GetComponent<LayoutElement>();
        layout.preferredHeight = 150f;

        Image background = card.GetComponent<Image>();
        background.color = new Color(0.07f, 0.1f, 0.14f, 0.92f);
        background.raycastTarget = false;

        VerticalLayoutGroup verticalLayout = card.AddComponent<VerticalLayoutGroup>();
        verticalLayout.padding = new RectOffset(18, 18, 16, 16);
        verticalLayout.spacing = 10f;
        verticalLayout.childControlHeight = false;
        verticalLayout.childControlWidth = true;
        verticalLayout.childForceExpandHeight = false;
        verticalLayout.childForceExpandWidth = true;

        ContentSizeFitter fitter = card.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

        TMP_Text titleText = CreateCardText(card.transform, font, title, 30f, FontStyles.Bold);
        titleText.alignment = TextAlignmentOptions.Left;

        TMP_Text descriptionText = CreateCardText(card.transform, font, description, 22f, FontStyles.Normal);
        descriptionText.alignment = TextAlignmentOptions.Left;
        descriptionText.enableWordWrapping = true;

        GameObject footer = new GameObject("Footer", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        footer.transform.SetParent(card.transform, false);
        LayoutElement footerLayout = footer.GetComponent<LayoutElement>();
        footerLayout.preferredHeight = 44f;

        HorizontalLayoutGroup footerGroup = footer.GetComponent<HorizontalLayoutGroup>();
        footerGroup.spacing = 12f;
        footerGroup.childControlHeight = true;
        footerGroup.childControlWidth = false;
        footerGroup.childForceExpandWidth = false;
        footerGroup.childForceExpandHeight = false;

        TMP_Text levelText = CreateCardText(footer.transform, font, $"Lv {currentLevel}/{maxLevel}", 24f, FontStyles.Bold);
        levelText.alignment = TextAlignmentOptions.Left;
        LayoutElement levelLayout = levelText.gameObject.AddComponent<LayoutElement>();
        levelLayout.preferredWidth = 130f;

        TMP_Text costText = CreateCardText(footer.transform, font, currentLevel >= maxLevel ? "MAX" : $"Cost {cost}", 24f, FontStyles.Normal);
        costText.alignment = TextAlignmentOptions.Left;
        LayoutElement costLayout = costText.gameObject.AddComponent<LayoutElement>();
        costLayout.preferredWidth = 150f;

        Button button = CreateActionButton(footer.transform, font, currentLevel >= maxLevel ? "MAXED" : "UPGRADE", canUpgrade, onPressed);
        LayoutElement buttonLayout = button.gameObject.AddComponent<LayoutElement>();
        buttonLayout.preferredWidth = 180f;
        buttonLayout.preferredHeight = 44f;

        return card;
    }

    private TMP_Text CreateCardText(Transform parent, TMP_FontAsset font, string textValue, float fontSize, FontStyles fontStyle)
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
        return text;
    }

    private Button CreateActionButton(Transform parent, TMP_FontAsset font, string label, bool interactable, UnityEngine.Events.UnityAction onPressed)
    {
        GameObject buttonObject = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.GetComponent<Image>();
        image.color = interactable
            ? new Color(0.98f, 0.5f, 0.18f, 1f)
            : new Color(0.3f, 0.33f, 0.38f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.interactable = interactable;
        if (onPressed != null)
            button.onClick.AddListener(onPressed);

        TMP_Text labelText = CreateCardText(buttonObject.transform, font, label, 22f, FontStyles.Bold);
        labelText.alignment = TextAlignmentOptions.Center;
        RectTransform labelRect = labelText.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        return button;
    }

    private Button CreateFeatureCloseButton(Transform parent)
    {
        TMP_FontAsset font = bestScoreText != null ? bestScoreText.font : TMP_Settings.defaultFontAsset;
        GameObject buttonObject = new GameObject("FeatureCloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-32f, -40f);
        rect.sizeDelta = new Vector2(140f, 54f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.18f, 0.22f, 0.28f, 0.95f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(CloseFeaturePanel);

        TMP_Text text = CreateCardText(buttonObject.transform, font, "CLOSE", 22f, FontStyles.Bold);
        text.alignment = TextAlignmentOptions.Center;
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    private TMP_Text CreateTopRightLabel(Transform parent, string objectName, Vector2 anchoredPosition, float fontSize, TextAlignmentOptions alignment)
    {
        TMP_FontAsset font = bestScoreText != null ? bestScoreText.font : TMP_Settings.defaultFontAsset;
        GameObject labelObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(parent, false);

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(360f, 48f);

        TMP_Text text = labelObject.GetComponent<TMP_Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private RectTransform CreateLabContentRoot(Transform parent)
    {
        GameObject rootObject = new GameObject("LabContent", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        rootObject.transform.SetParent(parent, false);

        RectTransform rect = rootObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = new Vector2(72f, 80f);
        rect.offsetMax = new Vector2(-72f, -170f);

        VerticalLayoutGroup layout = rootObject.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 14f;
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = rootObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return rect;
    }
}
