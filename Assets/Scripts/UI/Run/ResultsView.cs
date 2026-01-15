using UnityEngine;
using UnityEngine.UI;

public class ResultsView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rewardsContentRoot;
    [SerializeField] private RewardRowView rewardRowPrefab;
    [SerializeField] private Button doubleRewardsButton;
    [SerializeField] private Slider timerBar;
    [SerializeField] private Button backToHubButton;

    public CanvasGroup CanvasGroup => canvasGroup;
    public RectTransform RewardsContentRoot => rewardsContentRoot;
    public RewardRowView RewardRowPrefab => rewardRowPrefab;
    public Button DoubleRewardsButton => doubleRewardsButton;
    public Slider TimerBar => timerBar;
    public Button BackToHubButton => backToHubButton;
}
