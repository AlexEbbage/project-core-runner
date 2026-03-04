using System.Collections;
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

    [Header("Pickup Score Popup")]
    [SerializeField] private RectTransform pickupScorePopupContainer;
    [SerializeField] private TMP_Text pickupScorePopupPrefab;
    [SerializeField] private RectTransform scoreUiAnchor;
    [SerializeField] private Vector2 pickupScorePopupSpawnOffset = new Vector2(0f, 80f);
    [SerializeField] private float pickupScorePopupDuration = 0.7f;

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

        if (timeText != null)
        {
            var _elapsed = gameManager.GetElapsedGameTime;
            timeText.text = TimeFormatUtility.FormatElapsedTime(_elapsed);
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

        int bestScore = Mathf.RoundToInt(scoreManager.BestScore);
        bestScoreText.text = LocalizationService.Format("ui.best", bestScore);
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
        speedText.text = $"{speed:0}";
    }

    public void ShowPickupScorePopup(float scoreValue, Vector3 worldPosition)
    {
        if (scoreValue <= 0f)
            return;

        if (pickupScorePopupContainer == null || pickupScorePopupPrefab == null)
            return;

        TMP_Text popupInstance = Instantiate(pickupScorePopupPrefab, pickupScorePopupContainer);
        popupInstance.text = $"+{Mathf.RoundToInt(scoreValue)}";

        RectTransform popupRect = popupInstance.rectTransform;
        popupRect.anchoredPosition = WorldToCanvasPosition(worldPosition) + pickupScorePopupSpawnOffset;
        popupRect.localScale = Vector3.one;

        float safeDuration = Mathf.Max(0.01f, pickupScorePopupDuration);
        StartCoroutine(AnimatePickupScorePopup(popupInstance, popupRect, safeDuration));
    }

    private IEnumerator AnimatePickupScorePopup(TMP_Text popupText, RectTransform popupRect, float duration)
    {
        if (popupText == null || popupRect == null)
            yield break;

        Color baseColor = popupText.color;
        Vector2 startPosition = popupRect.anchoredPosition;
        Vector2 endPosition = GetScoreAnchorPosition(startPosition);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Continue animating even when gameplay is paused/timeScale is set to 0
            // (for example, right after a crash/game over).
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            popupRect.anchoredPosition = Vector2.Lerp(startPosition, endPosition, easedT);
            popupRect.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.6f, easedT);

            Color color = baseColor;
            color.a = 1f - t;
            popupText.color = color;

            yield return null;
        }

        Destroy(popupText.gameObject);
    }

    private Vector2 WorldToCanvasPosition(Vector3 worldPosition)
    {
        if (pickupScorePopupContainer == null)
            return Vector2.zero;

        Camera eventCamera = GetCanvasEventCamera();
        Camera worldCamera = Camera.main;

        if (worldCamera != null)
        {
            Vector3 viewportPoint = worldCamera.WorldToViewportPoint(worldPosition);
            if (viewportPoint.z < 0f)
            {
                return GetScoreAnchorPosition(Vector2.zero);
            }
        }

        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(worldCamera, worldPosition);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                pickupScorePopupContainer,
                screenPosition,
                eventCamera,
                out Vector2 localPoint))
        {
            return localPoint;
        }

        return GetScoreAnchorPosition(Vector2.zero);
    }

    private Vector2 GetScoreAnchorPosition(Vector2 fallback)
    {
        if (scoreUiAnchor == null)
            return fallback;

        Camera eventCamera = GetCanvasEventCamera();
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(eventCamera, scoreUiAnchor.position);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                pickupScorePopupContainer,
                screenPosition,
                eventCamera,
                out Vector2 localPoint))
        {
            return localPoint;
        }

        return fallback;
    }

    private Camera GetCanvasEventCamera()
    {
        var canvas = pickupScorePopupContainer != null
            ? pickupScorePopupContainer.GetComponentInParent<Canvas>()
            : GetComponentInParent<Canvas>();

        if (canvas == null)
            return null;

        return canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;
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
