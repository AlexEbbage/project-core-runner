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
            if (_config == null || _config.Days.Count == 0)
                return false;

            if (!_config.LoopAfterFinalDay && _profile.State.DailyLoginStreak >= _config.Days.Count)
                return false;

            return _profile.State.LastDailyRewardDateUtc != TodayKey();
        }

        public int GetCurrentCalendarIndex()
        {
            if (_config == null || _config.Days.Count == 0)
                return 0;

            if (_config.LoopAfterFinalDay)
                return Math.Max(0, _profile.State.DailyLoginStreak % _config.Days.Count);

            return Math.Min(Math.Max(0, _profile.State.DailyLoginStreak), _config.Days.Count - 1);
        }

        public IReadOnlyList<DailyRewardDay> GetCalendarPreview()
        {
            return _config != null ? _config.Days : new List<DailyRewardDay>();
        }

        public bool TryClaim(bool doubled)
        {
            if (!CanClaimToday())
                return false;

            var today = TodayKey();
            var claimedIndex = -1;
            var committed = _profile.TryMutate(state =>
            {
                if (state.LastDailyRewardDateUtc == today)
                    return false;

                ApplyMissedDayPolicy(state);
                if (!_config.LoopAfterFinalDay && state.DailyLoginStreak >= _config.Days.Count)
                    return false;

                claimedIndex = _config.LoopAfterFinalDay
                    ? Math.Max(0, state.DailyLoginStreak % _config.Days.Count)
                    : Math.Min(Math.Max(0, state.DailyLoginStreak), _config.Days.Count - 1);

                var day = _config.Days[claimedIndex];
                _rewards.ApplyManyToState(state, day.Rewards);
                if (doubled)
                    _rewards.ApplyManyToState(state, day.Rewards);

                state.DailyLoginStreak++;
                state.LastDailyRewardDateUtc = today;
                return true;
            });

            if (!committed)
                return false;

            _analytics?.Track(AnalyticsEventNames.DailyRewardClaimed, new Dictionary<string, object>
            {
                ["day_index"] = claimedIndex + 1,
                ["doubled"] = doubled
            });
            _logger?.Info(LogCategory.DailyRewards, $"Claimed daily reward day {claimedIndex + 1}.");
            return true;
        }

        private void ApplyMissedDayPolicy(PlayerProfileState state)
        {
            if (_config == null || !_config.ResetStreakOnMissedDay || string.IsNullOrEmpty(state.LastDailyRewardDateUtc))
                return;

            if (!DateTime.TryParse(state.LastDailyRewardDateUtc, out var lastClaimDate))
                return;

            var daysMissed = (_clock.UtcNow.Date - lastClaimDate.Date).Days - 1;
            if (daysMissed > _config.GraceDays)
                state.DailyLoginStreak = 0;
        }

        private string TodayKey()
        {
            return _clock.UtcNow.Date.ToString("yyyy-MM-dd");
        }
    }
}
