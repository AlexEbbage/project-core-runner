using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the authored Game Over screen and binds resolved run-end data.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject rootPanel;

    [Header("Text Fields")]
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text bestScoreText;
    [SerializeField] private TMP_Text elapsedTimeText;
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private TMP_Text coinsCollectedText;
    [SerializeField] private TMP_Text rewardsText;
    [SerializeField] private TMP_Text comboModifierText;
    [SerializeField] private TMP_Text continuesUsedText;
    [SerializeField] private TMP_Text continuesRemainingText;
    [SerializeField] private TMP_Text continueButtonLabel;
    [SerializeField] private TMP_Text continueDelayText;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button doubleRewardsButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Slider continueDelaySlider;

    private bool _hasContinueRemaining;
    private float _continueDelaySeconds;
    private float _continueUnlockedAt;
    private bool _continueDelayReported;
    private string _defaultContinueButtonLabel;
    private GameManager.GameOverPresentationData _presentation;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (rootPanel == null)
        {
            rootPanel = gameObject;
        }

        if (continueButtonLabel == null && continueButton != null)
        {
            continueButtonLabel = continueButton.GetComponentInChildren<TMP_Text>(true);
        }

        if (continueButtonLabel != null)
        {
            _defaultContinueButtonLabel = continueButtonLabel.text;
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinuePressed);
        }

        if (doubleRewardsButton != null)
        {
            doubleRewardsButton.onClick.AddListener(OnDoubleRewardsPressed);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartPressed);
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(OnMenuPressed);
        }
    }

    private void Update()
    {
        if (rootPanel == null || !rootPanel.activeInHierarchy)
            return;

        UpdateContinueButtonState();
        UpdateDoubleRewardsButtonState();
        UpdateContinueDelayVisuals();
    }

    private void OnEnable()
    {
        LocalizationService.LanguageChanged += HandleLanguageChanged;
    }

    private void OnDisable()
    {
        LocalizationService.LanguageChanged -= HandleLanguageChanged;
    }

    public void Show(GameManager.GameOverPresentationData presentation)
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);

        _presentation = presentation;
        _continueDelaySeconds = Mathf.Max(0f, presentation.continueUnlockDelaySeconds);
        _continueUnlockedAt = Time.unscaledTime + _continueDelaySeconds;
        _continueDelayReported = false;
        _hasContinueRemaining = presentation.canContinue;

        UpdateLabels();

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(_hasContinueRemaining);
        }

        UpdateContinueButtonState();
        UpdateDoubleRewardsButtonState();
        UpdateContinueDelayVisuals();
    }

    public void Hide()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);

        _continueDelayReported = false;
    }

    private void UpdateContinueButtonState()
    {
        if (continueButton == null)
            return;

        bool unlocked = IsContinueUnlocked();
        bool adReady = gameManager != null && gameManager.IsContinueAdReady();
        continueButton.interactable = _hasContinueRemaining && unlocked && adReady;
    }

    private void UpdateDoubleRewardsButtonState()
    {
        if (doubleRewardsButton == null)
            return;

        if (AdsConfig.RemoveAds)
        {
            doubleRewardsButton.gameObject.SetActive(false);
            return;
        }

        bool canDouble = _presentation.canDoubleRewards && gameManager != null && gameManager.CanDoubleRunRewards;
        doubleRewardsButton.gameObject.SetActive(canDouble);
        doubleRewardsButton.interactable = canDouble && gameManager.IsDoubleRewardsAdReady();
    }

    private void UpdateLabels()
    {
        if (finalScoreText != null)
        {
            finalScoreText.text = LocalizationService.Format("ui.score", Mathf.RoundToInt(_presentation.finalScore));
        }

        if (bestScoreText != null)
        {
            string bestText = LocalizationService.Format("ui.highscore", Mathf.RoundToInt(_presentation.bestScore));
            if (comboModifierText == null)
            {
                bestText = $"{bestText}  {FormatComboModifier()}";
            }

            bestScoreText.text = bestText;
        }

        if (elapsedTimeText != null)
        {
            elapsedTimeText.text = LocalizationService.Format(
                "ui.elapsed_time",
                TimeFormatUtility.FormatElapsedTime(_presentation.elapsedTime));
        }

        if (distanceText != null)
        {
            distanceText.text = LocalizationService.Format("ui.distance", Mathf.RoundToInt(_presentation.distance));
        }

        if (coinsCollectedText != null)
        {
            coinsCollectedText.text = LocalizationService.Format("ui.run_coins", _presentation.coinsCollected);
        }

        if (rewardsText != null)
        {
            rewardsText.text = LocalizationService.Format(
                "ui.game_over_rewards",
                _presentation.baseRewardCoins,
                _presentation.baseRewardPremiumCurrency,
                _presentation.baseRewardXp);
        }

        if (comboModifierText != null)
        {
            comboModifierText.text = FormatComboModifier();
        }

        if (continuesUsedText != null)
        {
            continuesUsedText.text = distanceText == null
                ? LocalizationService.Format("ui.distance", Mathf.RoundToInt(_presentation.distance))
                : LocalizationService.Format("ui.continues_used", _presentation.continuesUsed, _presentation.maxContinues);
        }

        if (continuesRemainingText != null)
        {
            continuesRemainingText.text = rewardsText == null
                ? LocalizationService.Format(
                    "ui.game_over_rewards",
                    _presentation.baseRewardCoins,
                    _presentation.baseRewardPremiumCurrency,
                    _presentation.baseRewardXp)
                : LocalizationService.Format("ui.continues_remaining", _presentation.continuesRemaining, _presentation.maxContinues);
        }
    }

    private void UpdateContinueDelayVisuals()
    {
        bool showDelay = _hasContinueRemaining && _continueDelaySeconds > 0f;
        bool unlocked = IsContinueUnlocked();
        float remaining = showDelay ? Mathf.Max(0f, _continueUnlockedAt - Time.unscaledTime) : 0f;

        if (continueDelaySlider != null)
        {
            continueDelaySlider.gameObject.SetActive(showDelay);
            continueDelaySlider.value = showDelay && _continueDelaySeconds > 0f
                ? 1f - Mathf.Clamp01(remaining / _continueDelaySeconds)
                : 1f;
        }

        if (continueDelayText != null)
        {
            continueDelayText.gameObject.SetActive(showDelay);
            continueDelayText.text = showDelay && !unlocked
                ? LocalizationService.Format("ui.continue_in", Mathf.CeilToInt(remaining))
                : LocalizationService.Get("ui.continue_ready");
        }

        if (continueButtonLabel != null)
        {
            continueButtonLabel.text = showDelay && !unlocked
                ? LocalizationService.Format("ui.continue_in", Mathf.CeilToInt(remaining))
                : GetDefaultContinueButtonLabel();
        }

        if (!_continueDelayReported && showDelay && unlocked)
        {
            _continueDelayReported = true;
            gameManager?.NotifyGameOverContinueDelayCompleted();
        }
    }

    private bool IsContinueUnlocked()
    {
        return !_hasContinueRemaining || Time.unscaledTime >= _continueUnlockedAt;
    }

    private string GetDefaultContinueButtonLabel()
    {
        if (!string.IsNullOrWhiteSpace(_defaultContinueButtonLabel))
            return _defaultContinueButtonLabel;

        return LocalizationService.Get("ui.continue_ready");
    }

    private string FormatComboModifier()
    {
        return LocalizationService.Format("ui.combo_modifier", _presentation.comboModifier);
    }

    private void HandleLanguageChanged()
    {
        if (rootPanel != null && rootPanel.activeInHierarchy)
        {
            UpdateLabels();
            UpdateContinueDelayVisuals();
        }
    }

    private void OnContinuePressed()
    {
        gameManager?.OnContinueButtonPressed();
    }

    private void OnDoubleRewardsPressed()
    {
        gameManager?.OnDoubleRewardsButtonPressed();
    }

    private void OnRestartPressed()
    {
        gameManager?.OnRestartButtonPressed();
    }

    private void OnMenuPressed()
    {
        gameManager?.OnMenuButtonPressedFromGameOver();
    }
}
