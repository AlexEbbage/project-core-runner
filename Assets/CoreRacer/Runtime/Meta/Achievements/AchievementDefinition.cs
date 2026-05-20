using System.Collections.Generic;
using CoreRacer.Meta.Economy;
using UnityEngine;

namespace CoreRacer.Meta.Achievements
{
    public enum AchievementMetricType
    {
        ProfileLevel,
        TotalRuns,
        BestScore,
        TotalCoinsCollected,
        TotalPowerupsCollected,
        UnlockedItems
    }

    [CreateAssetMenu(menuName = "Core Racer/Progression/Achievement")]
    public sealed class AchievementDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public string Description;
        public AchievementMetricType Metric;
        public int RequiredValue;
        public List<RewardGrant> Rewards = new List<RewardGrant>();
    }
}
