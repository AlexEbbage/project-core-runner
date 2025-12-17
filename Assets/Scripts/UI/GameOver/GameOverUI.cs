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

    public void Show(float finalScore, float bestScore, int continuesUsed, int continuesRemaining, int maxContinues)
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = $"Score: {finalScore:0}";

        if (bestScoreText != null)
            bestScoreText.text = $"Highscore: {bestScore:0}";

        bool canContinue = continuesRemaining > 0;
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(canContinue);
        }
    }

    public void Hide()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);
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
            gameManager.OnRestartButtonPressed();
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
