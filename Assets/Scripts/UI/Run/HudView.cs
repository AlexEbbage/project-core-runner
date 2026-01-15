using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudView : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text bestText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Button pauseButton;

    public TMP_Text ScoreText => scoreText;
    public TMP_Text BestText => bestText;
    public Slider ProgressBar => progressBar;
    public Button PauseButton => pauseButton;
}
