using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the Game Over screen. Shows final and best scores,
/// and buttons for Continue (ads later), Restart, and Menu.
/// Attach to a GameObject under the Canvas (GameOverPanel).
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
    [SerializeField] private TMP_Text continuesUsedText;
    [SerializeField] private TMP_Text continuesRemainingText;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button doubleRewardsButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    private bool _hasContinueRemaining;
    private float _lastFinalScore;
    private float _lastBestScore;
    private float _lastElapsedTime;
    private int _lastContinuesUsed;
    private int _lastContinuesRemaining;
    private int _lastMaxContinues;

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
        if (rootPanel != null && rootPanel.activeInHierarchy)
        {
            UpdateContinueButtonState();
            UpdateDoubleRewardsButtonState();
        }
    }

    private void OnEnable()
    {
        LocalizationService.LanguageChanged += HandleLanguageChanged;
    }

    private void OnDisable()
    {
        LocalizationService.LanguageChanged -= HandleLanguageChanged;
    }

    public void Show(float finalScore, float bestScore, float elapsedTime, int continuesUsed, int continuesRemaining, int maxContinues)
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);

        _lastFinalScore = finalScore;
        _lastBestScore = bestScore;
        _lastElapsedTime = elapsedTime;
        _lastContinuesUsed = continuesUsed;
        _lastContinuesRemaining = continuesRemaining;
        _lastMaxContinues = maxContinues;
        UpdateLabels();

        _hasContinueRemaining = continuesRemaining > 0;
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(_hasContinueRemaining);
            UpdateContinueButtonState();
        }

        UpdateDoubleRewardsButtonState();
    }


    private void UpdateContinueButtonState()
    {
        if (continueButton == null)
            return;

        bool adReady = gameManager != null && gameManager.IsContinueAdReady();
        continueButton.interactable = _hasContinueRemaining && adReady;
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

        bool canDouble = gameManager != null && gameManager.CanDoubleRunRewards;
        doubleRewardsButton.gameObject.SetActive(canDouble);
        doubleRewardsButton.interactable = canDouble && gameManager != null && gameManager.IsDoubleRewardsAdReady();
    }

    public void Hide()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    private void UpdateScoreLabels()
    {
        if (finalScoreText != null)
        {
            int finalScore = Mathf.RoundToInt(_lastFinalScore);
            finalScoreText.text = LocalizationService.Format("ui.score", finalScore);
        }

        if (bestScoreText != null)
        {
            int bestScore = Mathf.RoundToInt(_lastBestScore);
            bestScoreText.text = LocalizationService.Format("ui.highscore", bestScore);
        }
    }

    private void UpdateLabels()
    {
        UpdateScoreLabels();

        if (elapsedTimeText != null)
        {
            string formattedTime = TimeFormatUtility.FormatElapsedTime(_lastElapsedTime);
            elapsedTimeText.text = LocalizationService.Format("ui.elapsed_time", formattedTime);
        }

        if (continuesUsedText != null)
        {
            continuesUsedText.text = LocalizationService.Format("ui.continues_used", _lastContinuesUsed, _lastMaxContinues);
        }

        if (continuesRemainingText != null)
        {
            continuesRemainingText.text = LocalizationService.Format("ui.continues_remaining", _lastContinuesRemaining, _lastMaxContinues);
        }
    }

    private void HandleLanguageChanged()
    {
        if (rootPanel != null && rootPanel.activeInHierarchy)
            UpdateLabels();
    }

    private void OnContinuePressed()
    {
        if (gameManager != null)
        {
            gameManager.OnContinueButtonPressed();
        }
    }

    private void OnDoubleRewardsPressed()
    {
        if (gameManager != null)
        {
            gameManager.OnDoubleRewardsButtonPressed();
        }
    }

    private void OnRestartPressed()
    {
        if (gameManager != null)
        {
            gameManager.OnPlayButtonPressed();
        }
    }

    private void OnMenuPressed()
    {
        if (gameManager != null)
        {
            gameManager.OnMenuButtonPressedFromGameOver();
        }
    }
}
