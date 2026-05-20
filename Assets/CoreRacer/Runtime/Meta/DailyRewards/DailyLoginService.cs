using System;
using System.Collections.Generic;
using CoreRacer.Common.Time;
using CoreRacer.Meta.Economy;
using CoreRacer.Meta.Profile;

namespace CoreRacer.Meta.DailyRewards
{
    public sealed class DailyLoginService
    {
        private readonly PlayerProfileService _profile;
        private readonly RewardGrantService _rewards;
        private readonly IGameClock _clock;
        private readonly List<RewardGrant> _rewardCycle;

        public DailyLoginService(PlayerProfileService profile, RewardGrantService rewards, IGameClock clock, List<RewardGrant> rewardCycle)
        {
            _profile = profile;
            _rewards = rewards;
            _clock = clock;
            _rewardCycle = rewardCycle ?? new List<RewardGrant>();
        }

        public bool CanClaimToday()
        {
            return _profile.State.LastDailyRewardDateUtc != _clock.UtcNow.Date.ToString("yyyy-MM-dd");
        }

        public bool TryClaim(bool doubled)
        {
            if (!CanClaimToday() || _rewardCycle.Count == 0)
                return false;

            var index = Math.Max(0, _profile.State.DailyLoginStreak % _rewardCycle.Count);
            var reward = _rewardCycle[index];
            _rewards.Grant(reward);
            if (doubled)
                _rewards.Grant(reward);

            _profile.State.DailyLoginStreak++;
            _profile.State.LastDailyRewardDateUtc = _clock.UtcNow.Date.ToString("yyyy-MM-dd");
            _profile.Save();
            return true;
        }
    }
}
