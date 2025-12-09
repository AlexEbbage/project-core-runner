using UnityEngine;
using UnityEngine.UI;

public class RemoveAdsThankYouUI : MonoBehaviour
{
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (rootPanel == null)
            rootPanel = gameObject;

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        Hide();
    }

    public void Show()
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);
    }

    public void Hide()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);
    }
}
