using System.Collections.Generic;
using CoreRacer.Meta.Economy;
using UnityEngine;

namespace CoreRacer.Meta.DailyRewards
{
    [CreateAssetMenu(menuName = "Core Racer/Progression/Daily Reward Calendar")]
    public sealed class DailyRewardCalendarConfig : ScriptableObject
    {
        public bool LoopAfterFinalDay = true;
        public bool ResetStreakOnMissedDay = false;
        public int GraceDays = 1;
        public List<DailyRewardDay> Days = new List<DailyRewardDay>();
    }

    [System.Serializable]
    public sealed class DailyRewardDay
    {
        public string DisplayName;
        public List<RewardGrant> Rewards = new List<RewardGrant>();
        public bool IsBonusDay;
    }
}
