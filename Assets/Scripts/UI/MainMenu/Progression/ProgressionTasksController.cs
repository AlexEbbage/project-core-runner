using System.Collections.Generic;
using UnityEngine;

public class ProgressionTasksController : MonoBehaviour
{
    [SerializeField] private ProgressionTasksConfig config;
    [SerializeField] private ProgressionTasksContentView[] contentViews;
    [SerializeField] private ProgressionTaskRowView defaultTaskRowPrefab;
    [SerializeField] private ProgressionTaskRowView sliderOnlyTaskRowPrefab;
    [SerializeField] private ProgressionTaskRowView completedTaskRowPrefab;
    [SerializeField] private ProgressionRewardNodeView rewardNodePrefab;

    private readonly Dictionary<ProgressionCadence, ProgressionTasksContentView> _contentLookup = new();

    private void Awake()
    {
        if (contentViews == null)
            return;

        foreach (var view in contentViews)
        {
            if (view != null)
                _contentLookup[view.Cadence] = view;
        }
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (config == null)
            return;

        foreach (var group in config.TaskGroups)
        {
            if (!_contentLookup.TryGetValue(group.Cadence, out var view) || view == null)
                continue;

            UpdateContent(view, group);
        }
    }

    private void UpdateContent(ProgressionTasksContentView view, ProgressionTaskGroupDefinition group)
    {
        if (view.PointsValueText != null)
            view.PointsValueText.text = $"{group.CurrentPoints} / {group.TargetPoints}";

        if (view.TimeRemainingText != null)
            view.TimeRemainingText.text = FormatDuration(group.TimeRemainingSeconds);

        if (view.ProgressBarFill != null)
            view.ProgressBarFill.fillAmount = group.TargetPoints > 0
                ? Mathf.Clamp01(group.CurrentPoints / (float)group.TargetPoints)
                : 0f;

        PopulateRewards(view, group);
        PopulateTasks(view, group);
    }

    private void PopulateRewards(ProgressionTasksContentView view, ProgressionTaskGroupDefinition group)
    {
        if (view.RewardRow == null || rewardNodePrefab == null)
            return;

        ClearChildren(view.RewardRow);
        foreach (var reward in group.Rewards)
        {
            var instance = Instantiate(rewardNodePrefab, view.RewardRow);
            if (instance == null)
                continue;

            if (instance.RewardLabel != null)
                instance.RewardLabel.text = reward.RewardLabel;
            if (instance.PointsText != null)
                instance.PointsText.text = $"{reward.PointsRequired} pts";
            if (instance.RewardIcon != null && reward.Icon != null)
                instance.RewardIcon.sprite = reward.Icon;

            instance.SetState(reward.State);
        }
    }

    private void PopulateTasks(ProgressionTasksContentView view, ProgressionTaskGroupDefinition group)
    {
        if (view.TaskListContent == null || defaultTaskRowPrefab == null)
            return;

        ClearChildren(view.TaskListContent);
        foreach (var task in group.Tasks)
        {
            var prefab = GetTaskPrefab(task.RowStyle);
            var instance = Instantiate(prefab, view.TaskListContent);
            if (instance == null)
                continue;

            if (instance.DescriptionText != null)
                instance.DescriptionText.text = task.Description;
            if (instance.ProgressText != null)
                instance.ProgressText.text = $"{task.Current}/{task.Target}";
            if (instance.ProgressSlider != null && task.Target > 0)
                instance.ProgressSlider.value = Mathf.Clamp01(task.Current / (float)task.Target);
            if (instance.IconImage != null && task.Icon != null)
                instance.IconImage.sprite = task.Icon;
            if (instance.RewardText != null)
                instance.RewardText.text = task.RewardLabel;
            if (instance.RewardIcon != null && task.RewardIcon != null)
                instance.RewardIcon.sprite = task.RewardIcon;
            if (instance.CompleteOverlay != null)
                instance.CompleteOverlay.SetActive(task.RowStyle == ProgressionTaskRowStyle.Completed);
            if (instance.CompleteIndicator != null)
                instance.CompleteIndicator.gameObject.SetActive(task.RowStyle == ProgressionTaskRowStyle.Completed);
        }
    }

    private ProgressionTaskRowView GetTaskPrefab(ProgressionTaskRowStyle style)
    {
        return style switch
        {
            ProgressionTaskRowStyle.SliderOnly when sliderOnlyTaskRowPrefab != null => sliderOnlyTaskRowPrefab,
            ProgressionTaskRowStyle.Completed when completedTaskRowPrefab != null => completedTaskRowPrefab,
            _ => defaultTaskRowPrefab
        };
    }

    private static void ClearChildren(RectTransform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    private static string FormatDuration(int totalSeconds)
    {
        if (totalSeconds < 0)
            totalSeconds = 0;

        var days = totalSeconds / 86400;
        var hours = (totalSeconds % 86400) / 3600;
        var minutes = (totalSeconds % 3600) / 60;

        if (days > 0)
            return $"Time remaining: {days}d {hours:00}h";

        return $"Time remaining: {hours:00}:{minutes:00}";
    }
}
