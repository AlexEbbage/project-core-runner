using System.Collections.Generic;
using CoreRacer.Services.Analytics;
using CoreRacer.Services.Save;

namespace CoreRacer.FTUE
{
    public sealed class FirstSessionFunnelTracker
    {
        private const string Prefix = "core_racer_ftue_funnel_";
        private readonly ISaveStorage _storage;
        private readonly IAnalyticsService _analytics;

        public FirstSessionFunnelTracker(ISaveStorage storage, IAnalyticsService analytics)
        {
            _storage = storage;
            _analytics = analytics;
        }

        public void TrackOnce(string eventName, Dictionary<string, object> parameters = null)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                return;

            var key = Prefix + eventName;
            if (_storage != null && _storage.Exists(key))
                return;

            _analytics?.Track(eventName, parameters);
            _storage?.Save(key, "1");
        }

        public void FirstRunStarted() => TrackOnce("first_run_started");
        public void FirstRunFinished() => TrackOnce("first_run_finished");
        public void FirstCrash() => TrackOnce("first_crash");
        public void FirstContinueOfferSeen() => TrackOnce("first_continue_offer_seen");
        public void FirstUpgradePurchased() => TrackOnce("first_upgrade_purchased");
        public void FirstShopOpened() => TrackOnce("first_shop_opened");
        public void FirstTaskClaimed() => TrackOnce("first_task_claimed");
        public void FirstDailyRewardClaimed() => TrackOnce("first_daily_reward_claimed");
        public void FirstAdWatched() => TrackOnce("first_ad_watched");
    }
}
