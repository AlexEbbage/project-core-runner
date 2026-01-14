using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressionTasksContentView : MonoBehaviour
{
    [SerializeField] private ProgressionCadence cadence;
    [SerializeField] private TMP_Text pointsValueText;
    [SerializeField] private TMP_Text timeRemainingText;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private RectTransform rewardRow;
    [SerializeField] private RectTransform taskListContent;

    public ProgressionCadence Cadence => cadence;
    public TMP_Text PointsValueText => pointsValueText;
    public TMP_Text TimeRemainingText => timeRemainingText;
    public Image ProgressBarFill => progressBarFill;
    public RectTransform RewardRow => rewardRow;
    public RectTransform TaskListContent => taskListContent;
}
