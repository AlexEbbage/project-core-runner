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

    private Action _onAccept;
    private Action _onDecline;

    private void Awake()
    {
        if (rootPanel == null)
            rootPanel = gameObject;

        if (acceptButton != null)
            acceptButton.onClick.AddListener(HandleAcceptPressed);

        if (declineButton != null)
            declineButton.onClick.AddListener(HandleDeclinePressed);
    }

    public void Show(string title, string body, string reward, string acceptLabel, string declineLabel, Action onAccept, Action onDecline, bool acceptEnabled = true)
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);

        _onAccept = onAccept;
        _onDecline = onDecline;

        SetText(titleText, title);
        SetText(bodyText, body);
        SetText(rewardText, reward);
        SetText(acceptButtonLabel, acceptLabel);
        SetText(declineButtonLabel, declineLabel);

        if (acceptButton != null)
            acceptButton.interactable = acceptEnabled;
    }

    public void Hide()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);

        _onAccept = null;
        _onDecline = null;
    }

    public bool IsVisible => rootPanel != null && rootPanel.activeSelf;

    private void HandleAcceptPressed()
    {
        _onAccept?.Invoke();
    }

    private void HandleDeclinePressed()
    {
        _onDecline?.Invoke();
    }

    private static void SetText(TMP_Text textComponent, string value)
    {
        if (textComponent == null)
            return;

        textComponent.text = value ?? string.Empty;
    }
}
