using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathContinueView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text currentScoreText;
    [SerializeField] private Button watchAdContinueButton;
    [SerializeField] private Button backToBaseButton;

    public CanvasGroup CanvasGroup => canvasGroup;
    public TMP_Text CurrentScoreText => currentScoreText;
    public Button WatchAdContinueButton => watchAdContinueButton;
    public Button BackToBaseButton => backToBaseButton;
}
