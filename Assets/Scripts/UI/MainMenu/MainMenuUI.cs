using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct LevelInfo
{
    public string displayName;
    public string description;
    public int sides;
    public int requiredProfileLevel;
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
    private const string ShopDatabaseResourcePath = "ShopDatabase";
    private const string BoosterCatalogResourcePath = "BoosterCatalog";

    [Header("Core References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private MainMenuController mainMenuController;
    [SerializeField] private TopBarController topBarController;
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private TMP_Text bestScoreText;
    [SerializeField] private Button settingsButton;

    [Header("Level Select UI")]
    [SerializeField] private TMP_Text levelNameText;
    [SerializeField] private Image levelShapeImage;
    [SerializeField] private TMP_Text levelDescriptionText;
    [SerializeField] private TMP_Text levelRequirementText;
    [SerializeField] private TMP_Text boosterSummaryText;
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;
    [SerializeField] private RectTransform levelCardsRoot;
    [SerializeField] private GameObject levelUpToastRoot;
    [SerializeField] private TMP_Text levelUpToastText;

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
    [SerializeField] private ShopDatabase shopDatabase;
    [SerializeField] private ShipDatabase shipDatabase;
    [SerializeField] private PowerupUpgradeConfig powerupUpgradeConfig;
    [SerializeField] private BoosterCatalog boosterCatalog;
    [SerializeField] private DailyLoginRewardsManager dailyLoginRewardsManager;
    [SerializeField] private ProgressionTasksController progressionTasksController;
    [SerializeField] private GameObject featurePanelRoot;
    [SerializeField] private TMP_Text featureTitleText;
    [SerializeField] private RectTransform labContentRoot;
    [SerializeField] private TMP_Text labCurrencyText;
    [SerializeField] private Button featureCloseButton;
    [SerializeField] private string labButtonLabel = "LAB";
    [SerializeField] private string labTitle = "LAB";
    [SerializeField] private string shopTitle = "SHOP";
    [SerializeField] private string premiumTitle = "PREMIUM";

    [Header("Hub Side Entries")]
    [SerializeField] private GameObject dailyLoginEntryRoot;
    [SerializeField] private GameObject specialOffersEntryRoot;
    [SerializeField] private GameObject tasksEntryRoot;
    [SerializeField] private GameObject notificationsEntryRoot;
    [SerializeField] private GameObject dailyLoginBadgeRoot;
    [SerializeField] private GameObject specialOffersBadgeRoot;
    [SerializeField] private GameObject tasksBadgeRoot;
    [SerializeField] private GameObject notificationsBadgeRoot;

    private readonly List<GameObject> _runtimeLabWidgets = new();
    private readonly List<GameObject> _runtimeLevelWidgets = new();
    private readonly List<GameObject> _runtimeBoosterWidgets = new();
    private readonly Dictionary<ShopTab, Button> _shopTabButtons = new();
    private int _currentLevelIndex;
    private bool _labScaffoldReady;
    private bool _profileEventsBound;
    private RectTransform _shopTabRoot;
    private RectTransform _shopContentRoot;
    private ShopItemDetailsModal _shopDetailsModal;
    private ShopPageController _runtimeShopController;
    private TMP_Text _boosterSummaryTextRuntime;
    private TMP_Text _removeAdsButtonText;
    private TMP_Text _restorePurchasesButtonText;
    private Coroutine _levelUpToastRoutine;
    private string _queuedLevelUpMessage;
    private bool _featurePanelVisible;
    private bool _dailyLoginBadgeVisible;
    private bool _specialOffersBadgeVisible;
    private bool _tasksBadgeVisible;
    private bool _notificationsBadgeVisible;
    private bool _premiumBadgeVisible;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (mainMenuController == null)
            mainMenuController = FindFirstObjectByType<MainMenuController>();

        if (topBarController == null)
            topBarController = FindFirstObjectByType<TopBarController>();

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

        if (dailyLoginRewardsManager == null)
            dailyLoginRewardsManager = FindFirstObjectByType<DailyLoginRewardsManager>();

        if (progressionTasksController == null)
            progressionTasksController = FindFirstObjectByType<ProgressionTasksController>();

        ResolveDataReferences();
        ResolveLabReferences();
        EnsureLevelSelectScaffold();
        BindProfileEvents();

        if (leftArrowButton != null)
            leftArrowButton.onClick.AddListener(OnPrevLevel);

        if (rightArrowButton != null)
            rightArrowButton.onClick.AddListener(OnNextLevel);

        if (featureCloseButton != null)
            featureCloseButton.onClick.AddListener(() => CloseFeaturePanel());
    }

    private void OnEnable()
    {
        RemoveAdsIAPManager.OnRemoveAdsUnlocked += HandleRemoveAdsUnlocked;
        LocalizationService.LanguageChanged += HandleLanguageChanged;
        BindProfileEvents();

        UpdateBestScoreDisplay();
        EnsureValidLevelIndex();
        ApplyLevelToWorld();
        UpdateLevelDisplay();
        UpdateRemoveAdsUI();
        RefreshLabView();
        RefreshHubState();
        ShowQueuedLevelUpToast();
    }

    private void OnDisable()
    {
        RemoveAdsIAPManager.OnRemoveAdsUnlocked -= HandleRemoveAdsUnlocked;
        LocalizationService.LanguageChanged -= HandleLanguageChanged;
        UnbindProfileEvents();
    }

    public void Show()
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);

        mainMenuController?.RefreshHubChrome();
        UpdateBestScoreDisplay();
        EnsureValidLevelIndex();
        ApplyLevelToWorld();
        UpdateLevelDisplay();
        UpdateRemoveAdsUI();
        RefreshLabView();
        RefreshHubState();
        ShowQueuedLevelUpToast();
    }

    public void Hide()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);

        CloseFeaturePanel(true);
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
        CloseFeaturePanel(true);

        if (IsLevelSelectable(_currentLevelIndex))
        {
            LevelInfo info = levels[_currentLevelIndex];
            gameManager?.LogAnalyticsEvent(AnalyticsEventNames.LevelPlayPressed, new Dictionary<string, object>
            {
                { AnalyticsEventNames.Params.Source, "level_select" },
                { AnalyticsEventNames.Params.LevelIndex, _currentLevelIndex },
                { AnalyticsEventNames.Params.LevelName, info.displayName },
                { AnalyticsEventNames.Params.RequiredLevel, GetRequiredLevel(info, _currentLevelIndex) }
            });
        }

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

        int selectedIndex = profile != null ? profile.SelectedLevelIndex : _currentLevelIndex;
        if (!IsLevelSelectable(selectedIndex))
        {
            selectedIndex = GetFirstUnlockedLevelIndex();
        }

        _currentLevelIndex = Mathf.Clamp(selectedIndex, 0, levels.Length - 1);

        if (profile != null)
            profile.SetSelectedLevelIndex(_currentLevelIndex);
    }

    private void UpdateLevelDisplay()
    {
        EnsureLevelSelectScaffold();
        RefreshLevelSelectView();
    }

    private void HandleLanguageChanged()
    {
        UpdateBestScoreDisplay();
        RefreshLevelSelectView();
        RefreshLabView();
    }

    private void ApplyLevelToWorld()
    {
        if (levels == null || levels.Length == 0)
            return;

        EnsureValidLevelIndex();
        var info = levels[_currentLevelIndex];
        int sides = Mathf.Max(3, info.sides);

        if (tunnelWallGenerator != null)
            tunnelWallGenerator.Rebuild(sides);
    }

    private void OnPrevLevel()
    {
        if (levels == null || levels.Length == 0)
            return;

        int previousIndex = FindAdjacentSelectableLevelIndex(_currentLevelIndex, -1);
        SelectLevel(previousIndex >= 0 ? previousIndex : GetFirstUnlockedLevelIndex());
    }

    private void OnNextLevel()
    {
        if (levels == null || levels.Length == 0)
            return;

        int nextIndex = FindAdjacentSelectableLevelIndex(_currentLevelIndex, 1);
        SelectLevel(nextIndex >= 0 ? nextIndex : GetFirstUnlockedLevelIndex());
    }

    private void RefreshLevelSelectView()
    {
        EnsureLevelSelectScaffold();
        EnsureBoosterDefaults();

        if (levels == null || levels.Length == 0)
        {
            if (levelNameText != null)
                levelNameText.text = LocalizationService.Get("ui.no_levels", "No Levels");

            if (levelDescriptionText != null)
                levelDescriptionText.text = LocalizationService.Get("ui.level_select_empty", "No levels available.");

            if (levelRequirementText != null)
                levelRequirementText.text = string.Empty;

            if (levelShapeImage != null)
                levelShapeImage.enabled = false;

            RefreshLevelCards();
            return;
        }

        EnsureValidLevelIndex();
        LevelInfo info = levels[_currentLevelIndex];

        if (levelNameText != null)
            levelNameText.text = info.displayName;

        if (levelDescriptionText != null)
            levelDescriptionText.text = GetLevelDescription(info);

        if (levelRequirementText != null)
            levelRequirementText.text = GetLevelRequirementText(info, _currentLevelIndex, IsLevelUnlocked(_currentLevelIndex));

        if (levelShapeImage != null)
        {
            levelShapeImage.enabled = info.shapeSprite != null;
            levelShapeImage.sprite = info.shapeSprite;
        }

        if (leftArrowButton != null)
            leftArrowButton.interactable = FindAdjacentSelectableLevelIndex(_currentLevelIndex, -1) >= 0;

        if (rightArrowButton != null)
            rightArrowButton.interactable = FindAdjacentSelectableLevelIndex(_currentLevelIndex, 1) >= 0;

        RefreshLevelCards();
    }

    private void EnsureLevelSelectScaffold()
    {
        if (rootPanel == null)
            return;

        if (levelCardsRoot == null)
        {
            levelCardsRoot = CreateFeatureContentRoot(rootPanel.transform, "LevelSelectCards");
        }

        if (levelCardsRoot != null)
            levelCardsRoot.gameObject.SetActive(true);

        if (levelDescriptionText == null)
            levelDescriptionText = CreateTopLeftLabel(rootPanel.transform, "LevelDescription", new Vector2(72f, -236f), 26f, TextAlignmentOptions.Left);

        if (levelRequirementText == null)
            levelRequirementText = CreateTopLeftLabel(rootPanel.transform, "LevelRequirement", new Vector2(72f, -272f), 24f, TextAlignmentOptions.Left);

        if (_boosterSummaryTextRuntime == null)
        {
            _boosterSummaryTextRuntime = boosterSummaryText != null
                ? boosterSummaryText
                : CreateTopLeftLabel(rootPanel.transform, "BoosterSummary", new Vector2(72f, -310f), 22f, TextAlignmentOptions.Left);
        }

        if (levelUpToastRoot == null)
        {
            GameObject toastObject = new GameObject("LevelUpToast", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            toastObject.transform.SetParent(rootPanel.transform, false);

            RectTransform toastRect = toastObject.GetComponent<RectTransform>();
            toastRect.anchorMin = new Vector2(0.5f, 1f);
            toastRect.anchorMax = new Vector2(0.5f, 1f);
            toastRect.pivot = new Vector2(0.5f, 1f);
            toastRect.anchoredPosition = new Vector2(0f, -32f);
            toastRect.sizeDelta = new Vector2(420f, 72f);

            Image toastBackground = toastObject.GetComponent<Image>();
            toastBackground.color = new Color(0.14f, 0.33f, 0.18f, 0.96f);

            TMP_Text toastText = CreateCardText(toastObject.transform, bestScoreText != null ? bestScoreText.font : TMP_Settings.defaultFontAsset, string.Empty, 22f, FontStyles.Bold);
            toastText.alignment = TextAlignmentOptions.Center;
            RectTransform toastTextRect = toastText.rectTransform;
            toastTextRect.anchorMin = Vector2.zero;
            toastTextRect.anchorMax = Vector2.one;
            toastTextRect.offsetMin = new Vector2(16f, 8f);
            toastTextRect.offsetMax = new Vector2(-16f, -8f);

            levelUpToastRoot = toastObject;
            levelUpToastText = toastText;
            toastObject.SetActive(false);
        }
    }

    private void RefreshLevelCards()
    {
        if (levelCardsRoot == null)
            return;

        for (int i = 0; i < _runtimeLevelWidgets.Count; i++)
        {
            if (_runtimeLevelWidgets[i] != null)
                Destroy(_runtimeLevelWidgets[i]);
        }
        _runtimeLevelWidgets.Clear();

        for (int i = 0; i < _runtimeBoosterWidgets.Count; i++)
        {
            if (_runtimeBoosterWidgets[i] != null)
                Destroy(_runtimeBoosterWidgets[i]);
        }
        _runtimeBoosterWidgets.Clear();

        TMP_FontAsset font = bestScoreText != null ? bestScoreText.font : TMP_Settings.defaultFontAsset;
        if (levels != null && levels.Length > 0)
        {
            _runtimeLevelWidgets.Add(CreateSectionLabel(levelCardsRoot, "Level Routes"));

            for (int i = 0; i < levels.Length; i++)
            {
                LevelInfo levelInfo = levels[i];
                if (string.IsNullOrWhiteSpace(levelInfo.displayName))
                    continue;

                bool unlocked = IsLevelUnlocked(i);
                bool selected = i == _currentLevelIndex;
                _runtimeLevelWidgets.Add(CreateLevelCard(levelCardsRoot, font, i, levelInfo, unlocked, selected));
            }
        }

        RefreshBoosterCards(font);
    }

    private GameObject CreateLevelCard(Transform parent, TMP_FontAsset font, int index, LevelInfo info, bool unlocked, bool selected)
    {
        GameObject card = new GameObject($"{info.displayName}LevelCard", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        card.transform.SetParent(parent, false);

        LayoutElement layout = card.GetComponent<LayoutElement>();
        layout.preferredHeight = 170f;

        Image background = card.GetComponent<Image>();
        background.color = selected
            ? new Color(0.22f, 0.42f, 0.58f, 0.95f)
            : unlocked
                ? new Color(0.07f, 0.1f, 0.14f, 0.92f)
                : new Color(0.08f, 0.08f, 0.1f, 0.82f);

        VerticalLayoutGroup layoutGroup = card.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(18, 18, 16, 16);
        layoutGroup.spacing = 8f;
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;

        ContentSizeFitter fitter = card.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

        TMP_Text titleText = CreateCardText(card.transform, font, selected ? $"{info.displayName} · {LocalizationService.Get("ui.level_select_selected", "Selected")}" : info.displayName, 28f, FontStyles.Bold);
        titleText.alignment = TextAlignmentOptions.Left;

        TMP_Text descriptionText = CreateCardText(card.transform, font, GetLevelDescription(info), 22f, FontStyles.Normal);
        descriptionText.alignment = TextAlignmentOptions.Left;
        descriptionText.enableWordWrapping = true;

        GameObject footer = new GameObject("Footer", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        footer.transform.SetParent(card.transform, false);
        LayoutElement footerLayout = footer.GetComponent<LayoutElement>();
        footerLayout.preferredHeight = 48f;

        HorizontalLayoutGroup footerGroup = footer.GetComponent<HorizontalLayoutGroup>();
        footerGroup.spacing = 12f;
        footerGroup.childControlHeight = true;
        footerGroup.childControlWidth = false;
        footerGroup.childForceExpandWidth = false;
        footerGroup.childForceExpandHeight = false;

        TMP_Text unlockText = CreateCardText(footer.transform, font, GetLevelRequirementText(info, index, unlocked), 22f, FontStyles.Bold);
        unlockText.alignment = TextAlignmentOptions.Left;
        LayoutElement unlockLayout = unlockText.gameObject.AddComponent<LayoutElement>();
        unlockLayout.preferredWidth = 220f;

        string actionLabel = selected
            ? LocalizationService.Get("ui.level_select_selected", "Selected")
            : unlocked
                ? LocalizationService.Get("ui.level_select_select", "Select")
                : LocalizationService.Get("ui.level_select_locked_short", "Locked");

        Button actionButton = CreateActionButton(footer.transform, font, actionLabel, unlocked && !selected, () => SelectLevel(index));
        LayoutElement buttonLayout = actionButton.gameObject.AddComponent<LayoutElement>();
        buttonLayout.preferredWidth = 170f;
        buttonLayout.preferredHeight = 44f;

        if (actionButton != null)
            actionButton.interactable = unlocked && !selected;

        return card;
    }

    private void RefreshBoosterCards(TMP_FontAsset font)
    {
        if (levelCardsRoot == null)
            return;

        BoosterDefinition[] boosters = GetResolvedBoosterDefinitions();
        if (_boosterSummaryTextRuntime != null)
            _boosterSummaryTextRuntime.text = GetBoosterSummaryText(boosters);

        _runtimeBoosterWidgets.Add(CreateSectionLabel(levelCardsRoot, "Boosters"));

        if (boosters == null || boosters.Length == 0)
        {
            _runtimeBoosterWidgets.Add(CreateBoosterNotice(levelCardsRoot, font, "No boosters configured."));
            return;
        }

        if (profile == null)
        {
            _runtimeBoosterWidgets.Add(CreateBoosterNotice(levelCardsRoot, font, "Booster loadout unavailable."));
            return;
        }

        BoosterFamily[] families = { BoosterFamily.Score, BoosterFamily.Rewards, BoosterFamily.Speed };
        foreach (BoosterFamily family in families)
        {
            _runtimeBoosterWidgets.Add(CreateSectionLabel(levelCardsRoot, GetBoosterFamilyLabel(family)));

            bool hasEntries = false;
            for (int i = 0; i < boosters.Length; i++)
            {
                BoosterDefinition booster = boosters[i];
                if (booster == null || booster.family != family)
                    continue;

                hasEntries = true;
                string equippedId = profile.GetEquippedBoosterId(family);
                bool selected = booster.id == equippedId;
                bool unlocked = profile.HasUnlockedBooster(booster.id) || booster.unlockedByDefault;
                _runtimeBoosterWidgets.Add(CreateBoosterCard(levelCardsRoot, font, booster, unlocked, selected, () => SelectBooster(booster)));
            }

            if (!hasEntries)
                _runtimeBoosterWidgets.Add(CreateBoosterNotice(levelCardsRoot, font, $"No {GetBoosterFamilyLabel(family).ToLowerInvariant()} boosters configured."));
        }
    }

    private GameObject CreateBoosterCard(
        Transform parent,
        TMP_FontAsset font,
        BoosterDefinition booster,
        bool unlocked,
        bool selected,
        UnityEngine.Events.UnityAction onPressed)
    {
        GameObject card = new GameObject($"{booster.displayName}BoosterCard", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        card.transform.SetParent(parent, false);

        LayoutElement layout = card.GetComponent<LayoutElement>();
        layout.preferredHeight = 158f;

        Image background = card.GetComponent<Image>();
        background.color = selected
            ? new Color(0.22f, 0.42f, 0.58f, 0.95f)
            : unlocked
                ? new Color(0.07f, 0.1f, 0.14f, 0.92f)
                : new Color(0.08f, 0.08f, 0.1f, 0.82f);
        background.raycastTarget = false;

        VerticalLayoutGroup verticalLayout = card.AddComponent<VerticalLayoutGroup>();
        verticalLayout.padding = new RectOffset(18, 18, 16, 16);
        verticalLayout.spacing = 8f;
        verticalLayout.childControlHeight = false;
        verticalLayout.childControlWidth = true;
        verticalLayout.childForceExpandHeight = false;
        verticalLayout.childForceExpandWidth = true;

        ContentSizeFitter fitter = card.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

        TMP_Text titleText = CreateCardText(card.transform, font, selected ? $"{booster.displayName} · Equipped" : booster.displayName, 28f, FontStyles.Bold);
        titleText.alignment = TextAlignmentOptions.Left;

        TMP_Text descriptionText = CreateCardText(card.transform, font, booster.description, 21f, FontStyles.Normal);
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

        TMP_Text familyText = CreateCardText(footer.transform, font, $"{GetBoosterFamilyLabel(booster.family)} · x{Mathf.Max(1f, booster.multiplier):0.##}", 22f, FontStyles.Bold);
        familyText.alignment = TextAlignmentOptions.Left;
        LayoutElement familyLayout = familyText.gameObject.AddComponent<LayoutElement>();
        familyLayout.preferredWidth = 240f;

        string actionLabel = selected
            ? "EQUIPPED"
            : unlocked
                ? "EQUIP"
                : "LOCKED";

        Button button = CreateActionButton(footer.transform, font, actionLabel, unlocked && !selected, onPressed);
        LayoutElement buttonLayout = button.gameObject.AddComponent<LayoutElement>();
        buttonLayout.preferredWidth = 180f;
        buttonLayout.preferredHeight = 44f;

        return card;
    }

    private GameObject CreateBoosterNotice(Transform parent, TMP_FontAsset font, string message)
    {
        GameObject notice = new GameObject("BoosterNotice", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        notice.transform.SetParent(parent, false);

        LayoutElement layout = notice.GetComponent<LayoutElement>();
        layout.preferredHeight = 84f;

        Image background = notice.GetComponent<Image>();
        background.color = new Color(0.07f, 0.08f, 0.1f, 0.72f);
        background.raycastTarget = false;

        TMP_Text text = CreateCardText(notice.transform, font, message, 22f, FontStyles.Normal);
        text.alignment = TextAlignmentOptions.Left;
        text.enableWordWrapping = true;

        return notice;
    }

    private void SelectBooster(BoosterDefinition booster)
    {
        if (profile == null || booster == null)
            return;

        BoosterDefinition[] boosters = GetResolvedBoosterDefinitions();
        if (!profile.TryEquipBooster(booster.family, booster.id, boosters))
            return;

        RefreshLevelCards();
    }

    private string GetBoosterSummaryText(BoosterDefinition[] boosters)
    {
        if (profile == null)
            return "Boosters unavailable.";

        string score = DescribeEquippedBooster(BoosterFamily.Score, boosters);
        string rewards = DescribeEquippedBooster(BoosterFamily.Rewards, boosters);
        string speed = DescribeEquippedBooster(BoosterFamily.Speed, boosters);
        return $"Selected boosters: {score} | {rewards} | {speed}";
    }

    private string DescribeEquippedBooster(BoosterFamily family, BoosterDefinition[] boosters)
    {
        string equippedId = profile != null ? profile.GetEquippedBoosterId(family) : string.Empty;
        BoosterDefinition booster = FindBoosterDefinition(boosters, family, equippedId);
        if (booster == null)
            return $"{GetBoosterFamilyLabel(family)} none";

        return $"{GetBoosterFamilyLabel(family)} {booster.displayName}";
    }

    private BoosterDefinition FindBoosterDefinition(BoosterDefinition[] boosters, BoosterFamily family, string boosterId)
    {
        if (boosters == null || string.IsNullOrWhiteSpace(boosterId))
            return null;

        for (int i = 0; i < boosters.Length; i++)
        {
            BoosterDefinition booster = boosters[i];
            if (booster != null && booster.family == family && booster.id == boosterId)
                return booster;
        }

        return null;
    }

    private string GetBoosterFamilyLabel(BoosterFamily family)
    {
        return family switch
        {
            BoosterFamily.Score => "Score",
            BoosterFamily.Rewards => "Rewards",
            BoosterFamily.Speed => "Speed",
            _ => family.ToString()
        };
    }

    private BoosterDefinition[] GetResolvedBoosterDefinitions()
    {
        return boosterCatalog != null
            ? boosterCatalog.GetResolvedBoosters()
            : BoosterCatalog.GetDefaultBoosters();
    }

    private void EnsureBoosterDefaults()
    {
        if (profile == null)
            return;

        profile.EnsureBoosterLoadout(GetResolvedBoosterDefinitions());
    }

    private string GetLevelDescription(LevelInfo info)
    {
        if (!string.IsNullOrWhiteSpace(info.description))
            return info.description;

        int sides = Mathf.Max(3, info.sides);
        return LocalizationService.Format("ui.level_select_description", sides);
    }

    private string GetLevelRequirementText(LevelInfo info, int index, bool unlocked)
    {
        int requiredLevel = GetRequiredLevel(info, index);
        if (unlocked)
            return LocalizationService.Format("ui.level_select_unlocked", requiredLevel);

        return LocalizationService.Format("ui.level_select_locked", requiredLevel);
    }

    private int GetRequiredLevel(LevelInfo info, int index)
    {
        return Mathf.Max(1, info.requiredProfileLevel > 0 ? info.requiredProfileLevel : index + 1);
    }

    private bool IsLevelUnlocked(int index)
    {
        if (!IsLevelSelectable(index))
            return false;

        int requiredLevel = GetRequiredLevel(levels[index], index);
        int playerLevel = profile != null ? profile.level : 1;
        return playerLevel >= requiredLevel;
    }

    private bool IsLevelSelectable(int index)
    {
        return levels != null && index >= 0 && index < levels.Length;
    }

    private int GetFirstUnlockedLevelIndex()
    {
        if (levels == null || levels.Length == 0)
            return 0;

        for (int i = 0; i < levels.Length; i++)
        {
            if (IsLevelUnlocked(i))
                return i;
        }

        return 0;
    }

    private int FindAdjacentSelectableLevelIndex(int startIndex, int direction)
    {
        if (levels == null || levels.Length == 0 || direction == 0)
            return -1;

        int count = levels.Length;
        int index = Mathf.Clamp(startIndex, 0, count - 1);

        for (int i = 0; i < count; i++)
        {
            index = (index + direction + count) % count;
            if (IsLevelUnlocked(index))
                return index;
        }

        return -1;
    }

    private void SelectLevel(int index)
    {
        if (!IsLevelSelectable(index))
            return;

        if (!IsLevelUnlocked(index))
        {
            UpdateLevelDisplay();
            return;
        }

        _currentLevelIndex = index;
        if (profile != null)
            profile.SetSelectedLevelIndex(index);

        LevelInfo info = levels[index];
        gameManager?.LogAnalyticsEvent(AnalyticsEventNames.LevelSelected, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Source, "level_select" },
            { AnalyticsEventNames.Params.LevelIndex, index },
            { AnalyticsEventNames.Params.LevelName, info.displayName },
            { AnalyticsEventNames.Params.RequiredLevel, GetRequiredLevel(info, index) }
        });

        ApplyLevelToWorld();
        UpdateLevelDisplay();
    }

    private void BindProfileEvents()
    {
        if (_profileEventsBound || profile == null)
            return;

        profile.LevelChanged += HandleProfileLevelChanged;
        _profileEventsBound = true;
    }

    private void UnbindProfileEvents()
    {
        if (!_profileEventsBound || profile == null)
            return;

        profile.LevelChanged -= HandleProfileLevelChanged;
        _profileEventsBound = false;
    }

    private void HandleProfileLevelChanged(int previousLevel, int newLevel)
    {
        mainMenuController?.RefreshHubChrome();
        RefreshLevelSelectView();

        string message = LocalizationService.Format("ui.level_up_toast", previousLevel, newLevel);
        QueueLevelUpToast(message);
    }

    private void QueueLevelUpToast(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        _queuedLevelUpMessage = message;
        ShowQueuedLevelUpToast();
    }

    private void ShowQueuedLevelUpToast()
    {
        if (string.IsNullOrWhiteSpace(_queuedLevelUpMessage))
            return;

        if (rootPanel == null || !rootPanel.activeInHierarchy)
            return;

        EnsureLevelSelectScaffold();

        if (levelUpToastText != null)
            levelUpToastText.text = _queuedLevelUpMessage;

        if (_levelUpToastRoutine != null)
            StopCoroutine(_levelUpToastRoutine);

        UiMotion.ShowPanel(levelUpToastRoot);
        _levelUpToastRoutine = StartCoroutine(HideLevelUpToastAfterDelay());
        _queuedLevelUpMessage = null;
    }

    private IEnumerator HideLevelUpToastAfterDelay()
    {
        yield return new WaitForSecondsRealtime(2.5f);

        UiMotion.HidePanel(levelUpToastRoot);

        _levelUpToastRoutine = null;
    }

    private void HandleRemoveAdsUnlocked()
    {
        UpdateRemoveAdsUI();

        gameManager?.LogAnalyticsEvent(AnalyticsEventNames.PremiumEntitlementUnlocked, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Source, "remove_ads" },
            { AnalyticsEventNames.Params.ProductId, RemoveAdsIAPManager.Product_RemoveAds }
        });

        if (thankYouPopup != null)
        {
            thankYouPopup.Show();
        }
    }

    private void UpdateRemoveAdsUI()
    {
        EnsureMenuShopLabels();

        bool hasRemoveAds = AdsConfig.RemoveAds;

        if (!hidePremiumUserIAPButton && removeAdsButtonRoot != null)
            removeAdsButtonRoot.SetActive(true);

        if (!hideRestorePurchasesButton && restorePurchasesButtonRoot != null)
            restorePurchasesButtonRoot.SetActive(true);

        bool wasPremiumBadgeVisible = _premiumBadgeVisible;
        if (premiumBadgeRoot != null)
            premiumBadgeRoot.SetActive(hasRemoveAds);

        _premiumBadgeVisible = hasRemoveAds;
        if (hasRemoveAds && !wasPremiumBadgeVisible)
            UiMotion.PulseBadge(premiumBadgeRoot != null ? premiumBadgeRoot.transform : null);
    }

    public void OnRemoveAdsButtonPressed()
    {
        OpenShopPanel(ShopTab.Skins);
    }

    public void OnRestorePurchasesButtonPressed()
    {
        OpenShopPanel(ShopTab.Currency);
    }

    private void ResolveDataReferences()
    {
        if (profile == null)
        {
            PlayerProfile[] profiles = Resources.FindObjectsOfTypeAll<PlayerProfile>();
            if (profiles != null && profiles.Length > 0)
                profile = profiles[0];
        }

        if (shopDatabase == null)
        {
            shopDatabase = Resources.Load<ShopDatabase>(ShopDatabaseResourcePath);

            if (shopDatabase == null)
            {
                ShopDatabase[] shopDatabases = Resources.FindObjectsOfTypeAll<ShopDatabase>();
                if (shopDatabases != null && shopDatabases.Length > 0)
                    shopDatabase = shopDatabases[0];
            }
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

        if (boosterCatalog == null)
        {
            boosterCatalog = Resources.Load<BoosterCatalog>(BoosterCatalogResourcePath);

            if (boosterCatalog == null)
            {
                BoosterCatalog[] catalogs = Resources.FindObjectsOfTypeAll<BoosterCatalog>();
                if (catalogs != null && catalogs.Length > 0)
                    boosterCatalog = catalogs[0];
            }
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

    public void OpenLabPanelFromHub()
    {
        if (!HasLabPanel())
            return;

        EnsureLabScaffold();
        EnsureShopScaffold();
        SetFeatureMode(showLab: true, showShop: false);
        RefreshLabView();
        ShowFeaturePanel();
        NotifyHubEntryOpened(AnalyticsEventNames.HubLabOpened, "lab");
    }

    public void OpenShopPanel(ShopTab initialTab)
    {
        if (!HasLabPanel())
            return;

        EnsureLabScaffold();
        EnsureShopScaffold();
        SetFeatureMode(showLab: false, showShop: true);
        RefreshShopView(initialTab);
        ShowFeaturePanel();
        NotifyHubEntryOpened(AnalyticsEventNames.HubShopOpened, initialTab == ShopTab.Currency ? "currency" : "shop");
    }

    private void ShowFeaturePanel()
    {
        if (featurePanelRoot == null)
            return;

        if (_featurePanelVisible && featurePanelRoot.activeSelf)
        {
            featurePanelRoot.SetActive(true);
            return;
        }

        _featurePanelVisible = true;
        UiMotion.ShowPanel(featurePanelRoot);
    }

    public void CloseFeaturePanel(bool instant = false)
    {
        if (featurePanelRoot != null)
        {
            if (!_featurePanelVisible && !featurePanelRoot.activeSelf)
            {
                if (_shopDetailsModal != null)
                    _shopDetailsModal.Hide();

                return;
            }

            _featurePanelVisible = false;
            if (instant || !featurePanelRoot.activeSelf)
                featurePanelRoot.SetActive(false);
            else
                UiMotion.HidePanel(featurePanelRoot);
        }

        if (_shopDetailsModal != null)
            _shopDetailsModal.Hide();
    }

    public void RefreshLabView()
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

    private void RefreshShopView(ShopTab initialTab)
    {
        if (_runtimeShopController == null || _shopContentRoot == null)
            return;

        if (featureTitleText != null)
            featureTitleText.text = initialTab == ShopTab.Currency ? premiumTitle : shopTitle;

        if (labCurrencyText != null && profile != null)
            labCurrencyText.text = $"SOFT {profile.softCurrency}   GEMS {profile.premiumCurrency}";

        _runtimeShopController.Initialize(profile, shopDatabase, _shopContentRoot, _shopDetailsModal, removeAdsIAPManager, FindFirstObjectByType<HangarPageController>(), gameManager);
        _runtimeShopController.SelectTab(initialTab);
        UpdateShopTabSelection(initialTab);
    }

    private void EnsureShopScaffold()
    {
        if (featurePanelRoot == null)
            return;

        EnsureMenuShopLabels();

        if (_shopTabRoot == null)
            _shopTabRoot = CreateShopTabRoot(featurePanelRoot.transform);

        if (_shopContentRoot == null)
            _shopContentRoot = CreateFeatureContentRoot(featurePanelRoot.transform, "ShopContent");

        if (_shopDetailsModal == null)
        {
            GameObject modalObject = new GameObject("ShopDetailsModal", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(ShopItemDetailsModal));
            modalObject.transform.SetParent(featurePanelRoot.transform, false);
            _shopDetailsModal = modalObject.GetComponent<ShopItemDetailsModal>();
            _shopDetailsModal.Hide();
        }

        if (_runtimeShopController == null)
        {
            GameObject controllerObject = new GameObject("ShopRuntimeController", typeof(RectTransform), typeof(ShopPageController));
            controllerObject.transform.SetParent(featurePanelRoot.transform, false);
            _runtimeShopController = controllerObject.GetComponent<ShopPageController>();
        }
    }

    private void SetFeatureMode(bool showLab, bool showShop)
    {
        if (labContentRoot != null)
            labContentRoot.gameObject.SetActive(showLab);

        if (_shopTabRoot != null)
            _shopTabRoot.gameObject.SetActive(showShop);

        if (_shopContentRoot != null)
            _shopContentRoot.gameObject.SetActive(showShop);

        if (_runtimeShopController != null)
            _runtimeShopController.gameObject.SetActive(showShop);
    }

    private void EnsureMenuShopLabels()
    {
        if (_removeAdsButtonText == null && removeAdsButtonRoot != null)
            _removeAdsButtonText = removeAdsButtonRoot.GetComponentInChildren<TMP_Text>(true);

        if (_restorePurchasesButtonText == null && restorePurchasesButtonRoot != null)
            _restorePurchasesButtonText = restorePurchasesButtonRoot.GetComponentInChildren<TMP_Text>(true);

        if (_removeAdsButtonText != null)
            _removeAdsButtonText.text = shopTitle;

        if (_restorePurchasesButtonText != null)
            _restorePurchasesButtonText.text = premiumTitle;
    }

    private void UpdateShopTabSelection(ShopTab selectedTab)
    {
        foreach (KeyValuePair<ShopTab, Button> tabButton in _shopTabButtons)
        {
            if (tabButton.Value == null || !tabButton.Value.TryGetComponent(out Image image))
                continue;

            bool isSelected = tabButton.Key == selectedTab;
            image.color = isSelected
                ? new Color(0.98f, 0.5f, 0.18f, 1f)
                : new Color(0.18f, 0.22f, 0.28f, 0.96f);
        }
    }

    public void RefreshHubState()
    {
        UpdateRemoveAdsUI();

        if (topBarController != null)
            topBarController.RefreshFromProfile(profile);

        bool dailyLoginAvailable = dailyLoginRewardsManager != null && dailyLoginRewardsManager.CanClaimToday();
        bool tasksAvailable = progressionTasksController != null && progressionTasksController.HasClaimableRewards();
        bool specialOffersAvailable = !AdsConfig.RemoveAds && removeAdsIAPManager != null;
        bool notificationsAvailable = dailyLoginAvailable || tasksAvailable || specialOffersAvailable;

        SetBadgeState(dailyLoginEntryRoot, dailyLoginBadgeRoot, dailyLoginAvailable, ref _dailyLoginBadgeVisible);
        SetBadgeState(tasksEntryRoot, tasksBadgeRoot, tasksAvailable, ref _tasksBadgeVisible);
        SetBadgeState(specialOffersEntryRoot, specialOffersBadgeRoot, specialOffersAvailable, ref _specialOffersBadgeVisible);
        SetBadgeState(notificationsEntryRoot, notificationsBadgeRoot, notificationsAvailable, ref _notificationsBadgeVisible);
    }

    public void RefreshShopView()
    {
        if (_runtimeShopController == null || !_runtimeShopController.gameObject.activeSelf)
            return;

        RefreshShopView(_runtimeShopController.CurrentTab);
    }

    public void OpenDailyLoginFromHub()
    {
        NotifyHubEntryOpened(AnalyticsEventNames.HubDailyLoginOpened, "daily_login");
        mainMenuController?.ShowPage(MainPage.Tasks, false, false);
        FindFirstObjectByType<ProgressionTasksHubView>()?.Show(ProgressionCadence.Daily);
        progressionTasksController?.Refresh();
        RefreshHubState();
    }

    public void OpenTasksFromHub()
    {
        NotifyHubEntryOpened(AnalyticsEventNames.HubTasksOpened, "tasks");
        mainMenuController?.ShowPage(MainPage.Tasks, false, false);
        FindFirstObjectByType<ProgressionTasksHubView>()?.Show(ProgressionCadence.Daily);
        progressionTasksController?.Refresh();
        RefreshHubState();
    }

    public void OpenSpecialOffersFromHub()
    {
        NotifyHubEntryOpened(AnalyticsEventNames.HubSpecialOffersOpened, "special_offers");
        mainMenuController?.ShowShopPage(ShopTab.Currency);
        RefreshHubState();
    }

    public void OpenNotificationsFromHub()
    {
        NotifyHubEntryOpened(AnalyticsEventNames.HubNotificationsOpened, "notifications");

        bool dailyLoginAvailable = dailyLoginRewardsManager != null && dailyLoginRewardsManager.CanClaimToday();
        bool tasksAvailable = progressionTasksController != null && progressionTasksController.HasClaimableRewards();

        if (dailyLoginAvailable)
        {
            OpenDailyLoginFromHub();
            return;
        }

        if (tasksAvailable)
        {
            OpenTasksFromHub();
            return;
        }

        if (!AdsConfig.RemoveAds && removeAdsIAPManager != null)
            OpenSpecialOffersFromHub();
    }

    public void NotifyHubEntryOpened(string eventName, string entry)
    {
        if (gameManager == null || string.IsNullOrWhiteSpace(eventName))
            return;

        gameManager.LogAnalyticsEvent(eventName, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Source, "hub" },
            { AnalyticsEventNames.Params.Type, entry }
        });
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
        gameManager?.LogAnalyticsEvent(AnalyticsEventNames.UpgradePurchased, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Source, "lab" },
            { AnalyticsEventNames.Params.Type, UpgradeType.ComboMultiplier.ToString() },
            { AnalyticsEventNames.Params.Price, cost }
        });
        RefreshLabView();
        mainMenuController?.RefreshHubChrome();
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
        gameManager?.LogAnalyticsEvent(AnalyticsEventNames.UpgradePurchased, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Source, "lab" },
            { AnalyticsEventNames.Params.Type, entry.powerupType.ToString() },
            { AnalyticsEventNames.Params.Price, cost }
        });
        RefreshLabView();
        mainMenuController?.RefreshHubChrome();
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

    private RectTransform CreateShopTabRoot(Transform parent)
    {
        GameObject rootObject = new GameObject("ShopTabs", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        rootObject.transform.SetParent(parent, false);

        RectTransform rect = rootObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = new Vector2(72f, -140f);
        rect.offsetMax = new Vector2(-72f, -80f);

        HorizontalLayoutGroup layout = rootObject.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 12f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        TMP_FontAsset font = bestScoreText != null ? bestScoreText.font : TMP_Settings.defaultFontAsset;
        CreateShopTabButton(rootObject.transform, font, ShopTab.Skins, "SKINS");
        CreateShopTabButton(rootObject.transform, font, ShopTab.Ships, "SHIPS");
        CreateShopTabButton(rootObject.transform, font, ShopTab.Trails, "TRAILS");
        CreateShopTabButton(rootObject.transform, font, ShopTab.Currency, "CURRENCY");
        rootObject.SetActive(false);
        return rect;
    }

    private void CreateShopTabButton(Transform parent, TMP_FontAsset font, ShopTab tab, string label)
    {
        GameObject buttonObject = new GameObject($"{tab}Tab", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetParent(parent, false);

        LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
        layout.preferredHeight = 52f;
        layout.flexibleWidth = 1f;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.18f, 0.22f, 0.28f, 0.96f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => RefreshShopView(tab));

        TMP_Text text = CreateCardText(buttonObject.transform, font, label, 20f, FontStyles.Bold);
        text.alignment = TextAlignmentOptions.Center;
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        _shopTabButtons[tab] = button;
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
        button.onClick.AddListener(() => CloseFeaturePanel());

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

    private TMP_Text CreateTopLeftLabel(Transform parent, string objectName, Vector2 anchoredPosition, float fontSize, TextAlignmentOptions alignment)
    {
        TMP_FontAsset font = bestScoreText != null ? bestScoreText.font : TMP_Settings.defaultFontAsset;
        GameObject labelObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(parent, false);

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(760f, 44f);

        TMP_Text text = labelObject.GetComponent<TMP_Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private RectTransform CreateFeatureContentRoot(Transform parent, string objectName)
    {
        GameObject rootObject = new GameObject(objectName, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
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

        rootObject.SetActive(false);
        return rect;
    }

    private RectTransform CreateLabContentRoot(Transform parent)
    {
        return CreateFeatureContentRoot(parent, "LabContent");
    }

    private void SetBadgeState(GameObject entryRoot, GameObject badgeRoot, bool active, ref bool wasVisible)
    {
        if (entryRoot != null)
            entryRoot.SetActive(true);

        bool wasActive = wasVisible;

        if (badgeRoot != null)
            badgeRoot.SetActive(active);

        wasVisible = active;
        if (active && !wasActive)
            UiMotion.PulseBadge(badgeRoot != null ? badgeRoot.transform : null);
    }
}
