using System;
using System.Collections.Generic;
using CoreRacer.Common.Time;
using CoreRacer.Meta.Economy;
using CoreRacer.Meta.Profile;
using CoreRacer.Services.Analytics;
using CoreRacer.Services.Logging;

namespace CoreRacer.Meta.DailyRewards
{
    public sealed class DailyRewardCalendarService
    {
        private readonly PlayerProfileService _profile;
        private readonly RewardGrantService _rewards;
        private readonly IGameClock _clock;
        private readonly DailyRewardCalendarConfig _config;
        private readonly IAnalyticsService _analytics;
        private readonly IGameLogger _logger;

        public DailyRewardCalendarService(PlayerProfileService profile, RewardGrantService rewards, IGameClock clock, DailyRewardCalendarConfig config, IAnalyticsService analytics = null, IGameLogger logger = null)
        {
            _profile = profile;
            _rewards = rewards;
            _clock = clock;
            _config = config;
            _analytics = analytics;
            _logger = logger;
        }

        public bool CanClaimToday()
        {
            return _profile.State.LastDailyRewardDateUtc != TodayKey();
        }

        public int GetCurrentCalendarIndex()
        {
            if (_config == null || _config.Days.Count == 0)
                return 0;
            return Math.Max(0, _profile.State.DailyLoginStreak % _config.Days.Count);
        }

        public IReadOnlyList<DailyRewardDay> GetCalendarPreview()
        {
            return _config != null ? _config.Days : new List<DailyRewardDay>();
        }

        public bool TryClaim(bool doubled)
        {
            if (!CanClaimToday() || _config == null || _config.Days.Count == 0)
                return false;

            ApplyMissedDayPolicy();
            int index = GetCurrentCalendarIndex();
            var day = _config.Days[index];
            Grant(day.Rewards);
            if (doubled)
                Grant(day.Rewards);

            _profile.State.DailyLoginStreak++;
            if (!_config.LoopAfterFinalDay && _profile.State.DailyLoginStreak >= _config.Days.Count)
                _profile.State.DailyLoginStreak = _config.Days.Count - 1;
            _profile.State.LastDailyRewardDateUtc = TodayKey();
            _profile.Save();

            _analytics?.Track(AnalyticsEventNames.DailyRewardClaimed, new Dictionary<string, object>
            {
                ["day_index"] = index + 1,
                ["doubled"] = doubled
            });
            _logger.Info(LogCategory.DailyRewards, $"Claimed daily reward day {index + 1}.");
            return true;
        }

        private void ApplyMissedDayPolicy()
        {
            if (_config == null || !_config.ResetStreakOnMissedDay || string.IsNullOrEmpty(_profile.State.LastDailyRewardDateUtc))
                return;

            if (!DateTime.TryParse(_profile.State.LastDailyRewardDateUtc, out var lastClaimDate))
                return;

            var daysMissed = (_clock.UtcNow.Date - lastClaimDate.Date).Days - 1;
            if (daysMissed > _config.GraceDays)
                _profile.State.DailyLoginStreak = 0;
        }

        private void Grant(List<RewardGrant> rewards)
        {
            for (int i = 0; i < rewards.Count; i++)
                _rewards.Grant(rewards[i]);
        }

        private string TodayKey()
        {
            return _clock.UtcNow.Date.ToString("yyyy-MM-dd");
        }
    }
}
