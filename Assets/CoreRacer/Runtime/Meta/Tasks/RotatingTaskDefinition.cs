using System.Collections.Generic;
using CoreRacer.Meta.Economy;
using CoreRacer.Meta.Progression;
using UnityEngine;

namespace CoreRacer.Meta.Tasks
{
    [CreateAssetMenu(menuName = "Core Racer/Progression/Rotating Task")]
    public sealed class RotatingTaskDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
        public TaskCadence Cadence = TaskCadence.Daily;
        public ProgressionTaskMetric Metric = ProgressionTaskMetric.RunsCompleted;
        public int TargetValue = 1;
        public int Weight = 100;
        public List<RewardGrant> Rewards = new List<RewardGrant>();
    }
}
