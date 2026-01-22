using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardedRunPromptUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject rootPanel;

    [Header("Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text acceptButtonLabel;
    [SerializeField] private TMP_Text declineButtonLabel;

    [Header("Buttons")]
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button declineButton;

    [Header("Countdown")]
    [SerializeField] private Slider countdownSlider;

    private Action _onAccept;
    private Action _onDecline;
    private Action _onTimeout;
    private float _autoDismissSeconds;
    private float _autoDismissRemaining;

    private void Awake()
    {
        if (rootPanel == null)
            rootPanel = gameObject;

        if (acceptButton != null)
            acceptButton.onClick.AddListener(HandleAcceptPressed);

        if (declineButton != null)
            declineButton.onClick.AddListener(HandleDeclinePressed);
    }

    public void Show(
        string title,
        string body,
        string reward,
        string acceptLabel,
        string declineLabel,
        Action onAccept,
        Action onDecline,
        bool acceptEnabled = true,
        float autoDismissSeconds = 0f,
        Action onTimeout = null)
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);

        _onAccept = onAccept;
        _onDecline = onDecline;
        _onTimeout = onTimeout;
        _autoDismissSeconds = Mathf.Max(0f, autoDismissSeconds);
        _autoDismissRemaining = _autoDismissSeconds;

        SetText(titleText, title);
        SetText(bodyText, body);
        SetText(rewardText, reward);
        SetText(acceptButtonLabel, acceptLabel);
        SetText(declineButtonLabel, declineLabel);

        if (acceptButton != null)
            acceptButton.interactable = acceptEnabled;

        UpdateCountdownVisual();
    }

    public void Hide()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);

        _onAccept = null;
        _onDecline = null;
        _onTimeout = null;
        _autoDismissSeconds = 0f;
        _autoDismissRemaining = 0f;
        UpdateCountdownVisual();
    }

    public bool IsVisible => rootPanel != null && rootPanel.activeSelf;

    private void HandleAcceptPressed()
    {
        CancelCountdown();
        _onAccept?.Invoke();
    }

    private void HandleDeclinePressed()
    {
        CancelCountdown();
        _onDecline?.Invoke();
    }

    private void Update()
    {
        if (_autoDismissSeconds <= 0f || _autoDismissRemaining <= 0f)
            return;

        _autoDismissRemaining = Mathf.Max(0f, _autoDismissRemaining - Time.unscaledDeltaTime);
        UpdateCountdownVisual();

        if (_autoDismissRemaining <= 0f)
        {
            CancelCountdown();
            _onTimeout?.Invoke();
        }
    }

    private void CancelCountdown()
    {
        _autoDismissSeconds = 0f;
        _autoDismissRemaining = 0f;
        UpdateCountdownVisual();
    }

    private void UpdateCountdownVisual()
    {
        if (countdownSlider == null)
            return;

        if (_autoDismissSeconds <= 0f)
        {
            countdownSlider.gameObject.SetActive(false);
            countdownSlider.value = 0f;
            return;
        }

        countdownSlider.gameObject.SetActive(true);
        float normalized = _autoDismissSeconds > 0f
            ? Mathf.Clamp01(_autoDismissRemaining / _autoDismissSeconds)
            : 0f;
        countdownSlider.value = normalized;
    }

    private static void SetText(TMP_Text textComponent, string value)
    {
        if (textComponent == null)
            return;

        textComponent.text = value ?? string.Empty;
    }
}
