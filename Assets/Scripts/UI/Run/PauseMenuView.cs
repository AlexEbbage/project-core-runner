using UnityEngine;
using UnityEngine.UI;

public class PauseMenuView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;

    public CanvasGroup CanvasGroup => canvasGroup;
    public Button ContinueButton => continueButton;
    public Button SettingsButton => settingsButton;
    public Button MainMenuButton => mainMenuButton;
}
