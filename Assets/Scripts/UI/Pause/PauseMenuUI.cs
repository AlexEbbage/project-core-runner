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

        EnsureSettingsButton();
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

    private void EnsureSettingsButton()
    {
        if (settingsButton != null || rootPanel == null)
            return;

        settingsButton = SettingsModalController.CreateCogButton(rootPanel.transform, new Vector2(-20f, -20f));
    }
}
