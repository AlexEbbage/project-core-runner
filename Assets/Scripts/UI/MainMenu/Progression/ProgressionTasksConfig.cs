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
    [SerializeField] private List<ProgressionRewardDefinition> rewards = new();
    [SerializeField] private List<ProgressionTaskDefinition> tasks = new();

    public ProgressionCadence Cadence => cadence;
    public string PointsLabel => pointsLabel;
    public int CurrentPoints => currentPoints;
    public int TargetPoints => targetPoints;
    public int TimeRemainingSeconds => timeRemainingSeconds;
    public IReadOnlyList<ProgressionRewardDefinition> Rewards => rewards;
    public IReadOnlyList<ProgressionTaskDefinition> Tasks => tasks;
}

[Serializable]
public class ProgressionTaskDefinition
{
    [SerializeField] private string id;
    [SerializeField] private string description = "Complete 3 runs";
    [SerializeField] private int current = 0;
    [SerializeField] private int target = 3;
    [SerializeField] private Sprite icon;
    [SerializeField] private ProgressionTaskRowStyle rowStyle = ProgressionTaskRowStyle.MultiStep;
    [SerializeField] private string rewardLabel = "50 Coins";
    [SerializeField] private Sprite rewardIcon;

    public string Id => id;
    public string Description => description;
    public int Current => current;
    public int Target => target;
    public Sprite Icon => icon;
    public ProgressionTaskRowStyle RowStyle => rowStyle;
    public string RewardLabel => rewardLabel;
    public Sprite RewardIcon => rewardIcon;
}

[Serializable]
public class ProgressionRewardDefinition
{
    [SerializeField] private string rewardLabel = "50 Coins";
    [SerializeField] private int pointsRequired = 50;
    [SerializeField] private Sprite icon;
    [SerializeField] private ProgressionRewardState state = ProgressionRewardState.Locked;

    public string RewardLabel => rewardLabel;
    public int PointsRequired => pointsRequired;
    public Sprite Icon => icon;
    public ProgressionRewardState State => state;
}

public enum ProgressionRewardState
{
    Locked,
    Claimable,
    Claimed
}

public enum ProgressionTaskRowStyle
{
    MultiStep,
    SliderOnly,
    Completed
}

public enum ProgressionCadence
{
    Daily,
    Weekly,
    Monthly
}
