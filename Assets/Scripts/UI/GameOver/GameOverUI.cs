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

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    private bool _hasContinueRemaining;
    private float _lastFinalScore;
    private float _lastBestScore;

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

    public void Show(float finalScore, float bestScore, int continuesUsed, int continuesRemaining, int maxContinues)
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);

        _lastFinalScore = finalScore;
        _lastBestScore = bestScore;
        UpdateScoreLabels();

        _hasContinueRemaining = continuesRemaining > 0;
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(_hasContinueRemaining);
            UpdateContinueButtonState();
        }
    }


    private void UpdateContinueButtonState()
    {
        if (continueButton == null)
            return;

        bool adReady = gameManager != null && gameManager.IsContinueAdReady();
        continueButton.interactable = _hasContinueRemaining && adReady;
    }

    public void Hide()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    private void UpdateScoreLabels()
    {
        if (finalScoreText != null)
            finalScoreText.text = LocalizationService.Format("ui.score", _lastFinalScore);

        if (bestScoreText != null)
            bestScoreText.text = LocalizationService.Format("ui.highscore", _lastBestScore);
    }

    private void HandleLanguageChanged()
    {
        if (rootPanel != null && rootPanel.activeInHierarchy)
            UpdateScoreLabels();
    }

    private void OnContinuePressed()
    {
        if (gameManager != null)
        {
            gameManager.OnContinueButtonPressed();
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
