using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Toggle vibrateToggle;
    [SerializeField] private Toggle inputToggle;
    [SerializeField] private Button backButton;

    public CanvasGroup CanvasGroup => canvasGroup;
    public Toggle MusicToggle => musicToggle;
    public Toggle SfxToggle => sfxToggle;
    public Toggle VibrateToggle => vibrateToggle;
    public Toggle InputToggle => inputToggle;
    public Button BackButton => backButton;
}
