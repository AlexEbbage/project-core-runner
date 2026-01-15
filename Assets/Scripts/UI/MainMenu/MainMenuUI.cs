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
/// - Remove Ads button / Premium badge
/// - Play button triggers GameManager
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private TMP_Text bestScoreText;

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

    [Header("Monetisation")]
    [SerializeField] private RemoveAdsIAPManager removeAdsIAPManager;
    [SerializeField] private GameObject removeAdsButtonRoot;
    [SerializeField] private GameObject restorePurchasesButtonRoot;
    [SerializeField] private GameObject premiumBadgeRoot;
    [SerializeField] private RemoveAdsThankYouUI thankYouPopup;

    private int _currentLevelIndex;

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

        if (leftArrowButton != null)
            leftArrowButton.onClick.AddListener(OnPrevLevel);

        if (rightArrowButton != null)
            rightArrowButton.onClick.AddListener(OnNextLevel);
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
    }

    public void Hide()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    public void OnPlayButtonPressed()
    {
        // Apply current level selection before starting
        ApplyLevelToWorld();

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
            bestScoreText.text = LocalizationService.Format("ui.highscore_multiline", scoreManager.BestScore);
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
            levelShapeImage.enabled = (info.shapeSprite != null);
            levelShapeImage.sprite = info.shapeSprite;
        }
    }

    private void HandleLanguageChanged()
    {
        UpdateBestScoreDisplay();
        UpdateLevelDisplay();
    }

    private void ApplyLevelToWorld()
    {
        if (levels == null || levels.Length == 0)
            return;

        var info = levels[_currentLevelIndex];
        int sides = Mathf.Max(3, info.sides);

        if (obstacleRingGenerator != null)
            obstacleRingGenerator.RebuildAll(sides);

        if (tunnelWallGenerator != null)
            tunnelWallGenerator.Rebuild(sides);
    }

    // --- Level arrows ---

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

    // --- Remove Ads UI ---

    private void HandleRemoveAdsUnlocked()
    {
        UpdateRemoveAdsUI();

        if (thankYouPopup != null)
        {
            thankYouPopup.Show();
        }
    }

    private void UpdateRemoveAdsUI()
    {
        bool hasRemoveAds = AdsConfig.RemoveAds;

        if (removeAdsButtonRoot != null)
            removeAdsButtonRoot.SetActive(!hasRemoveAds);

        if (restorePurchasesButtonRoot != null)
            restorePurchasesButtonRoot.SetActive(!hasRemoveAds);

        if (premiumBadgeRoot != null)
            premiumBadgeRoot.SetActive(hasRemoveAds);
    }

    public void OnRemoveAdsButtonPressed()
    {
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
}
