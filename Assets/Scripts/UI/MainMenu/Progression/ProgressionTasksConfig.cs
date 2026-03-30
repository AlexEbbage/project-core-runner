using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Progression Tasks", fileName = "ProgressionTasksConfig")]
public class ProgressionTasksConfig : ScriptableObject
{
    [SerializeField] private List<ProgressionTaskGroupDefinition> taskGroups = new();

    public IReadOnlyList<ProgressionTaskGroupDefinition> TaskGroups => taskGroups;

    public ProgressionTaskGroupDefinition GetGroup(ProgressionCadence cadence)
    {
        return taskGroups.Find(group => group.Cadence == cadence);
    }
}

[Serializable]
public class ProgressionTaskGroupDefinition
{
    [SerializeField] private ProgressionCadence cadence;
    [SerializeField] private string pointsLabel = "Progress Points";
    [SerializeField] private int currentPoints = 0;
    [SerializeField] private int targetPoints = 100;
    [SerializeField] private int timeRemainingSeconds = 0;
    [SerializeField] private int activeTaskCount = 3;
    [SerializeField] private List<ProgressionRewardDefinition> rewards = new();
    [SerializeField] private List<ProgressionTaskDefinition> tasks = new();

    public ProgressionCadence Cadence => cadence;
    public string PointsLabel => pointsLabel;
    public int CurrentPoints => currentPoints;
    public int TargetPoints => targetPoints;
    public int TimeRemainingSeconds => timeRemainingSeconds;
    public int ActiveTaskCount => Mathf.Clamp(activeTaskCount, 1, tasks != null && tasks.Count > 0 ? tasks.Count : 1);
    public IReadOnlyList<ProgressionRewardDefinition> Rewards => rewards;
    public IReadOnlyList<ProgressionTaskDefinition> Tasks => tasks;

    public ProgressionTaskDefinition FindTask(string taskId)
    {
        if (tasks == null || string.IsNullOrWhiteSpace(taskId))
            return null;

        for (int i = 0; i < tasks.Count; i++)
        {
            ProgressionTaskDefinition task = tasks[i];
            if (task != null && task.Id == taskId)
                return task;
        }

        return null;
    }

    public List<string> BuildActiveTaskIds(DateTime cycleStartUtcDate)
    {
        List<string> activeIds = new();
        if (tasks == null || tasks.Count == 0)
            return activeIds;

        List<int> indices = new(tasks.Count);
        for (int i = 0; i < tasks.Count; i++)
        {
            if (tasks[i] != null && !string.IsNullOrWhiteSpace(tasks[i].Id))
                indices.Add(i);
        }

        if (indices.Count == 0)
            return activeIds;

        int count = Mathf.Min(ActiveTaskCount, indices.Count);
        int seed = unchecked((int)(cycleStartUtcDate.Date.Ticks ^ ((long)cadence + 1) * 486187739L));
        System.Random random = new(seed);

        for (int i = indices.Count - 1; i > 0; i--)
        {
            int swapIndex = random.Next(i + 1);
            (indices[i], indices[swapIndex]) = (indices[swapIndex], indices[i]);
        }

        indices.Sort(0, count, Comparer<int>.Default);
        for (int i = 0; i < count; i++)
            activeIds.Add(tasks[indices[i]].Id);

        return activeIds;
    }

    public List<string> GetLegacyFallbackActiveTaskIds()
    {
        List<string> fallbackIds = new();
        if (tasks == null || tasks.Count == 0)
            return fallbackIds;

        for (int i = 0; i < tasks.Count && fallbackIds.Count < ActiveTaskCount; i++)
        {
            ProgressionTaskDefinition task = tasks[i];
            if (task != null && !string.IsNullOrWhiteSpace(task.Id))
                fallbackIds.Add(task.Id);
        }

        return fallbackIds;
    }

    public List<ProgressionTaskDefinition> ResolveActiveTasks(IReadOnlyList<string> activeTaskIds)
    {
        List<ProgressionTaskDefinition> activeTasks = new();
        if (tasks == null || tasks.Count == 0)
            return activeTasks;

        IReadOnlyList<string> ids = activeTaskIds != null && activeTaskIds.Count > 0
            ? activeTaskIds
            : GetLegacyFallbackActiveTaskIds();

        for (int i = 0; i < ids.Count; i++)
        {
            ProgressionTaskDefinition task = FindTask(ids[i]);
            if (task != null)
                activeTasks.Add(task);
        }

        return activeTasks;
    }
}

[Serializable]
public class ProgressionTaskDefinition
{
    [SerializeField] private string id;
    [SerializeField] private string description = "Complete 3 runs";
    [SerializeField] private ProgressionTaskMetricType metricType = ProgressionTaskMetricType.RunCount;
    [SerializeField] private int current = 0;
    [SerializeField] private int target = 3;
    [SerializeField] private Sprite icon;
    [SerializeField] private ProgressionTaskRowStyle rowStyle = ProgressionTaskRowStyle.MultiStep;
    [SerializeField] private string rewardLabel = "50 Coins";
    [SerializeField] private Sprite rewardIcon;
    [SerializeField] private ProfileGrantType rewardType = ProfileGrantType.SoftCurrency;
    [SerializeField] private int rewardAmount = 50;
    [SerializeField] private string rewardItemId;
    [SerializeField] private int progressPointsReward = 25;

    public string Id => id;
    public string Description => description;
    public ProgressionTaskMetricType MetricType => metricType;
    public int Current => current;
    public int Target => target;
    public Sprite Icon => icon;
    public ProgressionTaskRowStyle RowStyle => rowStyle;
    public string RewardLabel => rewardLabel;
    public Sprite RewardIcon => rewardIcon;
    public ProfileGrantType RewardType => rewardType;
    public int RewardAmount => rewardAmount;
    public string RewardItemId => rewardItemId;
    public int ProgressPointsReward => progressPointsReward;

    public int GetProgressDelta(ProgressionTaskRunResult runResult)
    {
        return metricType switch
        {
            ProgressionTaskMetricType.RunCount => Mathf.Max(0, runResult.runCount),
            ProgressionTaskMetricType.DistanceMeters => Mathf.Max(0, Mathf.RoundToInt(runResult.distanceMeters)),
            ProgressionTaskMetricType.Score => Mathf.Max(0, Mathf.RoundToInt(runResult.score)),
            ProgressionTaskMetricType.CoinsCollected => Mathf.Max(0, runResult.coinsCollected),
            ProgressionTaskMetricType.XpEarned => Mathf.Max(0, runResult.xpEarned),
            ProgressionTaskMetricType.NoContinueRuns => runResult.completedWithoutContinue ? Mathf.Max(1, runResult.runCount) : 0,
            _ => 0
        };
    }
}

[Serializable]
public class ProgressionRewardDefinition
{
    [SerializeField] private string rewardLabel = "50 Coins";
    [SerializeField] private int pointsRequired = 50;
    [SerializeField] private Sprite icon;
    [SerializeField] private ProgressionRewardState state = ProgressionRewardState.Locked;
    [SerializeField] private ProfileGrantType rewardType = ProfileGrantType.SoftCurrency;
    [SerializeField] private int rewardAmount = 50;
    [SerializeField] private string rewardItemId;

    public string RewardLabel => rewardLabel;
    public int PointsRequired => pointsRequired;
    public Sprite Icon => icon;
    public ProgressionRewardState DefaultState => state;
    public ProfileGrantType RewardType => rewardType;
    public int RewardAmount => rewardAmount;
    public string RewardItemId => rewardItemId;
}

public enum ProgressionRewardState
{
    Locked,
    Claimable,
    Claimed
}

public readonly struct ProgressionTaskRunResult
{
    public ProgressionTaskRunResult(int runCount, float distanceMeters, float score, int coinsCollected, int xpEarned, bool completedWithoutContinue)
    {
        this.runCount = Mathf.Max(0, runCount);
        this.distanceMeters = Mathf.Max(0f, distanceMeters);
        this.score = Mathf.Max(0f, score);
        this.coinsCollected = Mathf.Max(0, coinsCollected);
        this.xpEarned = Mathf.Max(0, xpEarned);
        this.completedWithoutContinue = completedWithoutContinue;
    }

    public int runCount { get; }
    public float distanceMeters { get; }
    public float score { get; }
    public int coinsCollected { get; }
    public int xpEarned { get; }
    public bool completedWithoutContinue { get; }
}

public enum ProgressionTaskRowStyle
{
    MultiStep,
    SliderOnly,
    Completed
}

public enum ProgressionTaskMetricType
{
    RunCount,
    DistanceMeters,
    Score,
    CoinsCollected,
    XpEarned,
    NoContinueRuns
}

public enum ProgressionCadence
{
    Daily,
    Weekly,
    Monthly
}

public static class ProgressionTaskCycleUtility
{
    public static DateTime GetCycleStartUtc(ProgressionCadence cadence, DateTime nowUtc)
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
}
