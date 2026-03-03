using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple pause menu with Resume and Menu buttons.
/// Attach to a panel under the Canvas (PausePanel).
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button settingsButton;

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

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(OnResumePressed);
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(OnMenuPressed);
        }
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

    public void ShowSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    private void OnResumePressed()
    {
        if (gameManager != null)
        {
            gameManager.OnResumeButtonPressedFromPause();
        }
    }

    private void OnMenuPressed()
    {
        if (gameManager != null)
        {
            gameManager.OnMenuButtonPressedFromPause();
        }
    }
}
