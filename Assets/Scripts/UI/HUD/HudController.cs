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
    [System.Serializable]
    public class PowerupIndicator
    {
        public PowerupType powerupType;
        public GameObject root;
        public Image icon;
        public TMP_Text timerText;
    }

    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private RunScoreManager scoreManager;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerPowerupController powerupController;

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

    [Header("Powerups")]
    [SerializeField] private PowerupIndicator[] powerupIndicators;

    private void Awake()
    {
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
        if (scoreManager == null) scoreManager = FindFirstObjectByType<RunScoreManager>();
        if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (powerupController == null) powerupController = FindFirstObjectByType<PlayerPowerupController>();

        if (rootPanel == null) rootPanel = gameObject;
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += HandleHealthChanged;
        }

        LocalizationService.LanguageChanged += UpdateBestScoreDisplay;
        UpdateBestScoreDisplay();
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= HandleHealthChanged;
        }

        LocalizationService.LanguageChanged -= UpdateBestScoreDisplay;
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
        UpdatePowerupIndicators();
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

        bestScoreText.text = LocalizationService.Format("ui.best", scoreManager.BestScore);
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

    private void UpdatePowerupIndicators()
    {
        if (powerupIndicators == null || powerupIndicators.Length == 0 || powerupController == null)
            return;

        var active = powerupController.GetActivePowerups();
        for (int i = 0; i < powerupIndicators.Length; i++)
        {
            var indicator = powerupIndicators[i];
            if (indicator == null)
                continue;

            bool isActive = false;
            float remaining = 0f;
            bool timed = false;

            foreach (var status in active)
            {
                if (status.Type == indicator.powerupType)
                {
                    isActive = true;
                    remaining = status.RemainingTime;
                    timed = status.IsTimed;
                    break;
                }
            }

            if (indicator.root != null)
                indicator.root.SetActive(isActive);

            if (indicator.timerText != null)
            {
                indicator.timerText.gameObject.SetActive(isActive && timed);
                if (isActive && timed)
                {
                    indicator.timerText.text = $"{Mathf.CeilToInt(remaining)}";
                }
            }

            if (indicator.icon != null)
                indicator.icon.enabled = isActive;
        }
    }

}
