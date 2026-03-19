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
        public Image progressFill;
        public TMP_Text labelText;
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
    [SerializeField] private RectTransform powerupStripRoot;
    [SerializeField] private TMP_Text powerupToastText;

    [Header("Pickup Score Popup")]
    [SerializeField] private RectTransform pickupScorePopupContainer;
    [SerializeField] private TMP_Text pickupScorePopupPrefab;
    [SerializeField] private RectTransform scoreUiAnchor;
    [SerializeField] private Vector2 pickupScorePopupSpawnOffset = new Vector2(0f, 80f);
    [SerializeField] private float pickupScorePopupDuration = 0.7f;

    private float _powerupToastExpiresAt;

    private void Awake()
    {
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
        if (scoreManager == null) scoreManager = FindFirstObjectByType<RunScoreManager>();
        if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (powerupController == null) powerupController = FindFirstObjectByType<PlayerPowerupController>();

        if (rootPanel == null) rootPanel = gameObject;
        EnsurePowerupStrip();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += HandleHealthChanged;
        }

        if (powerupController != null)
        {
            powerupController.OnPowerupCollected += HandlePowerupCollected;
            powerupController.OnPowerupEnded += HandlePowerupEnded;
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

        if (powerupController != null)
        {
            powerupController.OnPowerupCollected -= HandlePowerupCollected;
            powerupController.OnPowerupEnded -= HandlePowerupEnded;
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
        UpdatePowerupToast();
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
        EnsurePowerupStrip();

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
            float totalDuration = 0f;
            bool timed = false;

            foreach (var status in active)
            {
                if (status.Type == indicator.powerupType)
                {
                    isActive = true;
                    remaining = status.RemainingTime;
                    totalDuration = status.TotalDuration;
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

            if (indicator.progressFill != null)
            {
                indicator.progressFill.gameObject.SetActive(isActive && timed);
                if (isActive && timed)
                {
                    indicator.progressFill.fillAmount = Mathf.Clamp01(remaining / Mathf.Max(0.01f, totalDuration));
                }
                else
                {
                    indicator.progressFill.fillAmount = 0f;
                }
            }

            if (indicator.labelText != null)
                indicator.labelText.text = PowerupUpgradeConfig.GetShortDisplayName(indicator.powerupType);

            if (indicator.icon != null)
            {
                indicator.icon.enabled = isActive;
                indicator.icon.color = GetPowerupColor(indicator.powerupType);
            }
        }
    }

    private void HandlePowerupCollected(PowerupType powerupType)
    {
        ShowPowerupToast($"{PowerupUpgradeConfig.GetDisplayName(powerupType)} online", GetPowerupColor(powerupType));
    }

    private void HandlePowerupEnded(PowerupType powerupType)
    {
        ShowPowerupToast($"{PowerupUpgradeConfig.GetDisplayName(powerupType)} offline", Color.white);
    }

    private void ShowPowerupToast(string message, Color color)
    {
        if (powerupToastText == null)
            return;

        powerupToastText.text = message;
        powerupToastText.color = color;
        powerupToastText.gameObject.SetActive(true);
        _powerupToastExpiresAt = Time.unscaledTime + 1.2f;
    }

    private void UpdatePowerupToast()
    {
        if (powerupToastText == null || !powerupToastText.gameObject.activeSelf)
            return;

        if (Time.unscaledTime >= _powerupToastExpiresAt)
        {
            powerupToastText.gameObject.SetActive(false);
        }
    }

    private void EnsurePowerupStrip()
    {
        if (powerupIndicators != null && powerupIndicators.Length > 0)
            return;

        if (rootPanel == null)
            return;

        if (powerupStripRoot == null)
        {
            powerupStripRoot = CreatePowerupStripRoot();
        }

        if (powerupToastText == null && powerupStripRoot != null)
        {
            powerupToastText = CreateToastText(powerupStripRoot);
            powerupToastText.gameObject.SetActive(false);
        }

        if (powerupStripRoot == null)
            return;

        var supportedPowerups = PowerupUpgradeConfig.TargetPowerups;
        powerupIndicators = new PowerupIndicator[supportedPowerups.Count];
        for (int i = 0; i < supportedPowerups.Count; i++)
        {
            powerupIndicators[i] = CreateFallbackIndicator(powerupStripRoot, supportedPowerups[i]);
        }
    }

    private RectTransform CreatePowerupStripRoot()
    {
        var stripObject = new GameObject("PowerupStrip", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        stripObject.transform.SetParent(rootPanel.transform, false);

        RectTransform rect = stripObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-24f, -24f);
        rect.sizeDelta = new Vector2(720f, 180f);

        HorizontalLayoutGroup layout = stripObject.GetComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperRight;
        layout.spacing = 12f;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        return rect;
    }

    private PowerupIndicator CreateFallbackIndicator(RectTransform stripRoot, PowerupType powerupType)
    {
        TMP_FontAsset font = scoreText != null ? scoreText.font : TMP_Settings.defaultFontAsset;

        GameObject card = new GameObject($"{powerupType}Indicator", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(stripRoot, false);

        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(128f, 132f);

        Image background = card.GetComponent<Image>();
        background.color = new Color(0.06f, 0.08f, 0.12f, 0.88f);
        background.raycastTarget = false;

        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconObject.transform.SetParent(card.transform, false);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 1f);
        iconRect.anchorMax = new Vector2(0.5f, 1f);
        iconRect.pivot = new Vector2(0.5f, 1f);
        iconRect.anchoredPosition = new Vector2(0f, -12f);
        iconRect.sizeDelta = new Vector2(56f, 56f);
        Image iconImage = iconObject.GetComponent<Image>();
        iconImage.color = GetPowerupColor(powerupType);
        iconImage.raycastTarget = false;

        TMP_Text label = CreateIndicatorText(card.transform, font, "Label", PowerupUpgradeConfig.GetShortDisplayName(powerupType), 26f, FontStyles.Bold);
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.pivot = new Vector2(0.5f, 1f);
        labelRect.anchoredPosition = new Vector2(0f, -74f);
        labelRect.sizeDelta = new Vector2(-16f, 28f);

        TMP_Text timer = CreateIndicatorText(card.transform, font, "Timer", string.Empty, 28f, FontStyles.Normal);
        RectTransform timerRect = timer.rectTransform;
        timerRect.anchorMin = new Vector2(0f, 0f);
        timerRect.anchorMax = new Vector2(1f, 0f);
        timerRect.pivot = new Vector2(0.5f, 0f);
        timerRect.anchoredPosition = new Vector2(0f, 16f);
        timerRect.sizeDelta = new Vector2(-16f, 28f);

        GameObject progressObject = new GameObject("Progress", typeof(RectTransform), typeof(Image));
        progressObject.transform.SetParent(card.transform, false);
        RectTransform progressRect = progressObject.GetComponent<RectTransform>();
        progressRect.anchorMin = new Vector2(0f, 0f);
        progressRect.anchorMax = new Vector2(1f, 0f);
        progressRect.pivot = new Vector2(0.5f, 0f);
        progressRect.anchoredPosition = new Vector2(0f, 52f);
        progressRect.sizeDelta = new Vector2(-18f, 10f);

        Image progressImage = progressObject.GetComponent<Image>();
        progressImage.type = Image.Type.Filled;
        progressImage.fillMethod = Image.FillMethod.Horizontal;
        progressImage.fillOrigin = 0;
        progressImage.color = GetPowerupColor(powerupType);
        progressImage.raycastTarget = false;
        progressImage.fillAmount = 0f;

        card.SetActive(false);
        return new PowerupIndicator
        {
            powerupType = powerupType,
            root = card,
            icon = iconImage,
            progressFill = progressImage,
            labelText = label,
            timerText = timer
        };
    }

    private TMP_Text CreateIndicatorText(Transform parent, TMP_FontAsset font, string objectName, string text, float fontSize, FontStyles fontStyle)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TMP_Text label = textObject.GetComponent<TMP_Text>();
        label.font = font;
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = fontStyle;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
        return label;
    }

    private TMP_Text CreateToastText(Transform parent)
    {
        TMP_FontAsset font = comboText != null ? comboText.font : TMP_Settings.defaultFontAsset;
        TMP_Text toast = CreateIndicatorText(parent, font, "PowerupToast", string.Empty, 26f, FontStyles.Bold);
        RectTransform toastRect = toast.rectTransform;
        toastRect.anchorMin = new Vector2(1f, 1f);
        toastRect.anchorMax = new Vector2(1f, 1f);
        toastRect.pivot = new Vector2(1f, 1f);
        toastRect.anchoredPosition = new Vector2(0f, -148f);
        toastRect.sizeDelta = new Vector2(420f, 34f);
        return toast;
    }

    private static Color GetPowerupColor(PowerupType powerupType)
    {
        switch (powerupType)
        {
            case PowerupType.ScoreMultiplier:
                return new Color(1f, 0.79f, 0.24f, 1f);
            case PowerupType.CoinMultiplier:
                return new Color(0.28f, 0.85f, 0.45f, 1f);
            case PowerupType.Magnet:
                return new Color(0.23f, 0.74f, 0.98f, 1f);
            case PowerupType.AutoPilot:
                return new Color(0.74f, 0.47f, 1f, 1f);
            case PowerupType.Shield:
                return new Color(1f, 0.42f, 0.42f, 1f);
            default:
                return Color.white;
        }
    }
}
