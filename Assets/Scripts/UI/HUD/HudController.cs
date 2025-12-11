using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays live score, best score, combo multiplier, and health.
/// Also hosts the Pause button.
/// Attach to a HUD panel under the Canvas.
/// </summary>
public class HudController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private RunScoreManager scoreManager;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Root")]
    [SerializeField] private GameObject rootPanel;

    [Header("Text Fields")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text bestScoreText;
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private TMP_Text timeText;

    [Header("Health")]
    [SerializeField] private Slider healthSlider;

    [Header("Speed")]
    [SerializeField] private TMP_Text speedText;

    private void Awake()
    {
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
        if (scoreManager == null) scoreManager = FindFirstObjectByType<RunScoreManager>();
        if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (rootPanel == null) rootPanel = gameObject;
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += HandleHealthChanged;
        }

        UpdateBestScoreDisplay();
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= HandleHealthChanged;
        }
    }

    private void Update()
    {
        if (scoreManager == null)
            return;

        if (scoreText != null)
        {
            scoreText.text = $"{scoreManager.CurrentScore:0}";
        }

        if (comboText != null)
        {
            float mult = scoreManager.CurrentMultiplier;
            comboText.text = mult <= 1.01f
                ? "x1"
                : $"x{mult:0.0}";
        }

        if(timeText != null)
        {
            var _elapsed = gameManager.GetElapsedGameTime;

            if (_elapsed < 3600f)
            {
                int totalSeconds = Mathf.FloorToInt(_elapsed);
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;
                timeText.text = $"{minutes:00}:{seconds:00}";
            }
            else
            {
                int totalSeconds = Mathf.FloorToInt(_elapsed);
                int hours = totalSeconds / 3600;
                int minutes = (totalSeconds % 3600) / 60;
                int seconds = totalSeconds % 60;
                timeText.text = $"{hours:00}:{minutes:00}:{seconds:00}";
            }
        }

        UpdateBestScoreDisplay();
    }

    private void HandleHealthChanged(float current, float max)
    {
        if (healthSlider == null)
            return;

        healthSlider.maxValue = max;
        healthSlider.value = current;
    }

    private void UpdateBestScoreDisplay()
    {
        if (bestScoreText == null || scoreManager == null)
            return;

        bestScoreText.text = $"BEST: {scoreManager.BestScore:0}";
    }

    public void Show()
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);

        if (playerHealth != null)
        {
            HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }

        UpdateBestScoreDisplay();
    }

    public void Hide()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    public void OnPauseButtonPressed()
    {
        if (gameManager != null)
        {
            gameManager.OnPauseButtonPressed();
        }
    }

    public void SetSpeed(float speed)
    {
        if (speedText == null) return;
        speedText.text = $"{speed:0}"; // plain number, or $"{speed:0} u/s"
    }

}
