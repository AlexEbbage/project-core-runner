using System;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionTasksController : MonoBehaviour
{
    [SerializeField] private ProgressionTasksConfig config;
    [SerializeField] private PlayerProfile profile;
    [SerializeField] private ProgressionTasksContentView[] contentViews;
    [SerializeField] private ProgressionTaskRowView defaultTaskRowPrefab;
    [SerializeField] private ProgressionTaskRowView sliderOnlyTaskRowPrefab;
    [SerializeField] private ProgressionTaskRowView completedTaskRowPrefab;
    [SerializeField] private ProgressionRewardNodeView rewardNodePrefab;

    private readonly Dictionary<ProgressionCadence, ProgressionTasksContentView> _contentLookup = new();

    private void Awake()
    {
        if (profile == null)
        {
            PlayerProfile[] profiles = Resources.FindObjectsOfTypeAll<PlayerProfile>();
            if (profiles != null && profiles.Length > 0)
                profile = profiles[0];
        }

        if (contentViews == null)
            return;

        foreach (ProgressionTasksContentView view in contentViews)
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
        if (config == null || profile == null)
            return;

        foreach (ProgressionTaskGroupDefinition group in config.TaskGroups)
        {
            if (group == null || !_contentLookup.TryGetValue(group.Cadence, out ProgressionTasksContentView view) || view == null)
                continue;

            UpdateContent(view, group);
        }
    }

    public bool HasClaimableRewards()
    {
        if (config == null || profile == null)
            return false;

        foreach (ProgressionTaskGroupDefinition group in config.TaskGroups)
        {
            if (group == null)
                continue;

            DateTime cycleStart = GetCycleStartUtc(group.Cadence, DateTime.UtcNow);
            PlayerProfile.TaskCadenceState state = profile.GetOrCreateTaskCadenceState(group.Cadence, cycleStart, group);

            for (int i = 0; i < group.Tasks.Count; i++)
            {
                ProgressionTaskDefinition task = group.Tasks[i];
                if (task == null)
                    continue;

                PlayerProfile.TaskProgressState taskState = state.tasks.Find(entry => entry.id == task.Id);
                if (taskState != null && !taskState.claimed && taskState.current >= task.Target)
                    return true;
            }

            for (int i = 0; i < group.Rewards.Count; i++)
            {
                ProgressionRewardDefinition reward = group.Rewards[i];
                if (reward == null)
                    continue;

                PlayerProfile.TaskRewardClaimState rewardState = state.rewards.Find(entry => entry.pointsRequired == reward.PointsRequired);
                if (rewardState != null && !rewardState.claimed && state.points >= reward.PointsRequired)
                    return true;
            }
        }

        return false;
    }

    private void UpdateContent(ProgressionTasksContentView view, ProgressionTaskGroupDefinition group)
    {
        DateTime now = DateTime.UtcNow;
        DateTime cycleStart = GetCycleStartUtc(group.Cadence, now);
        PlayerProfile.TaskCadenceState state = profile.GetOrCreateTaskCadenceState(group.Cadence, cycleStart, group);

        if (view.PointsValueText != null)
            view.PointsValueText.text = $"{state.points} / {group.TargetPoints}";

        if (view.TimeRemainingText != null)
            view.TimeRemainingText.text = FormatDuration(GetRemainingSeconds(group.Cadence, now));

        if (view.ProgressBarFill != null)
        {
            view.ProgressBarFill.fillAmount = group.TargetPoints > 0
                ? Mathf.Clamp01(state.points / (float)group.TargetPoints)
                : 0f;
        }

        PopulateRewards(view, group, state, cycleStart);
        PopulateTasks(view, group, state, cycleStart);
    }

    private void PopulateRewards(ProgressionTasksContentView view, ProgressionTaskGroupDefinition group, PlayerProfile.TaskCadenceState state, DateTime cycleStart)
    {
        if (view.RewardRow == null || rewardNodePrefab == null)
            return;

        ClearChildren(view.RewardRow);

        foreach (ProgressionRewardDefinition reward in group.Rewards)
        {
            ProgressionRewardNodeView instance = Instantiate(rewardNodePrefab, view.RewardRow);
            if (instance == null || reward == null)
                continue;

            if (instance.RewardLabel != null)
                instance.RewardLabel.text = reward.RewardLabel;
            if (instance.PointsText != null)
                instance.PointsText.text = $"{reward.PointsRequired} pts";
            if (instance.RewardIcon != null && reward.Icon != null)
                instance.RewardIcon.sprite = reward.Icon;

            PlayerProfile.TaskRewardClaimState rewardState = state.rewards.Find(entry => entry.pointsRequired == reward.PointsRequired);
            ProgressionRewardState uiState = ProgressionRewardState.Locked;
            if (rewardState != null)
            {
                uiState = rewardState.claimed
                    ? ProgressionRewardState.Claimed
                    : state.points >= reward.PointsRequired
                        ? ProgressionRewardState.Claimable
                        : ProgressionRewardState.Locked;
            }

            instance.SetState(uiState);
            instance.SetClaimAction(() =>
            {
                if (profile.TryClaimTaskMilestoneReward(group.Cadence, cycleStart, group, reward.PointsRequired))
                    HandleStateChanged();
            });
        }
    }

    private void PopulateTasks(ProgressionTasksContentView view, ProgressionTaskGroupDefinition group, PlayerProfile.TaskCadenceState state, DateTime cycleStart)
    {
        if (view.TaskListContent == null || defaultTaskRowPrefab == null)
            return;

        ClearChildren(view.TaskListContent);

        foreach (ProgressionTaskDefinition task in group.Tasks)
        {
            if (task == null)
                continue;

            PlayerProfile.TaskProgressState taskState = state.tasks.Find(entry => entry.id == task.Id);
            int current = taskState != null ? taskState.current : task.Current;
            bool claimed = taskState != null && taskState.claimed;
            bool complete = current >= task.Target;

            ProgressionTaskRowView prefab = GetTaskPrefab(task.RowStyle);
            ProgressionTaskRowView instance = Instantiate(prefab, view.TaskListContent);
            if (instance == null)
                continue;

            if (instance.DescriptionText != null)
                instance.DescriptionText.text = task.Description;
            if (instance.ProgressText != null)
                instance.ProgressText.text = $"{Mathf.Min(current, task.Target)}/{task.Target}";
            if (instance.ProgressSlider != null)
                instance.ProgressSlider.value = task.Target > 0 ? Mathf.Clamp01(current / (float)task.Target) : 0f;
            if (instance.IconImage != null && task.Icon != null)
                instance.IconImage.sprite = task.Icon;
            if (instance.RewardText != null)
                instance.RewardText.text = task.RewardLabel;
            if (instance.RewardIcon != null && task.RewardIcon != null)
                instance.RewardIcon.sprite = task.RewardIcon;
            if (instance.CompleteOverlay != null)
                instance.CompleteOverlay.SetActive(claimed);
            if (instance.CompleteIndicator != null)
            {
                instance.CompleteIndicator.gameObject.SetActive(claimed || complete);
                instance.CompleteIndicator.text = claimed
                    ? LocalizationService.Get("ui.task_claimed", "Claimed")
                    : complete
                        ? LocalizationService.Get("ui.task_complete", "Completed")
                        : string.Empty;
            }

            string actionLabel = claimed
                ? LocalizationService.Get("ui.task_action_claimed", "Claimed")
                : complete
                    ? LocalizationService.Get("ui.task_action_claim", "Claim")
                    : LocalizationService.Get("ui.task_action_in_progress", "In Progress");

            instance.SetAction(actionLabel, complete && !claimed, () =>
            {
                if (profile.TryClaimTaskReward(group.Cadence, cycleStart, group, task.Id))
                    HandleStateChanged();
            });
        }
    }

    private void HandleStateChanged()
    {
        Refresh();
        FindFirstObjectByType<MainMenuController>()?.RefreshHubChrome();
        FindFirstObjectByType<DailyLoginRewardPreviewView>()?.Refresh();
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
            Destroy(parent.GetChild(i).gameObject);
    }

    private static DateTime GetCycleStartUtc(ProgressionCadence cadence, DateTime nowUtc)
    {
        DateTime date = nowUtc.Date;
        return cadence switch
        {
            ProgressionCadence.Daily => date,
            ProgressionCadence.Weekly => date.AddDays(-((7 + ((int)date.DayOfWeek - (int)DayOfWeek.Monday)) % 7)),
            ProgressionCadence.Monthly => new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            _ => date
        };
    }

    private static int GetRemainingSeconds(ProgressionCadence cadence, DateTime nowUtc)
    {
        DateTime next = cadence switch
        {
            ProgressionCadence.Daily => nowUtc.Date.AddDays(1),
            ProgressionCadence.Weekly => GetCycleStartUtc(ProgressionCadence.Weekly, nowUtc).AddDays(7),
            ProgressionCadence.Monthly => new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1),
            _ => nowUtc.Date.AddDays(1)
        };

        return Mathf.Max(0, Mathf.RoundToInt((float)(next - nowUtc).TotalSeconds));
    }

    private static string FormatDuration(int totalSeconds)
    {
        if (totalSeconds < 0)
            totalSeconds = 0;

        int days = totalSeconds / 86400;
        int hours = (totalSeconds % 86400) / 3600;
        int minutes = (totalSeconds % 3600) / 60;

        if (days > 0)
            return $"Time remaining: {days}d {hours:00}h";

        return $"Time remaining: {hours:00}:{minutes:00}";
    }
}
