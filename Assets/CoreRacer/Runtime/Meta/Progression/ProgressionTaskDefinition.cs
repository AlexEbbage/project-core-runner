using System.Collections.Generic;
using CoreRacer.Meta.Economy;
using UnityEngine;

namespace CoreRacer.Meta.Progression
{
    public enum ProgressionTaskMetric
    {
        RunsCompleted,
        CoinsCollected,
        PowerupsCollected,
        BestScore
    }

    [CreateAssetMenu(menuName = "Core Racer/Progression/Task")]
    public sealed class ProgressionTaskDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public string Description;
        public ProgressionTaskMetric Metric;
        public int TargetValue;
        public List<RewardGrant> Rewards = new List<RewardGrant>();
    }
}
