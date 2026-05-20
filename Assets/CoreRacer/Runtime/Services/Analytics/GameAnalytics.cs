using System.Collections.Generic;
using CoreRacer.Gameplay.Run;
using CoreRacer.Monetisation.Ads;

namespace CoreRacer.Services.Analytics
{
    public sealed class GameAnalytics
    {
        private readonly IAnalyticsService _analytics;

        public GameAnalytics(IAnalyticsService analytics)
        {
            _analytics = analytics;
        }

        public void RunStarted(string levelId, string shipId)
        {
            _analytics.Track(AnalyticsEventNames.RunStarted, new Dictionary<string, object>
            {
                ["level_id"] = levelId,
                ["ship_id"] = shipId
            });
        }

        public void RunEnded(RunResult result)
        {
            _analytics.Track(AnalyticsEventNames.RunEnded, new Dictionary<string, object>
            {
                ["score"] = result.Score,
                ["coins"] = result.Coins,
                ["distance"] = result.Distance,
                ["duration"] = result.DurationSeconds,
                ["end_reason"] = result.EndReason.ToString()
            });
        }

        public void AdRequested(AdPlacement placement)
        {
            _analytics.Track(AnalyticsEventNames.AdRequested, new Dictionary<string, object>
            {
                ["placement"] = placement.ToString()
            });
        }

        public void AdCompleted(AdPlacement placement, string result)
        {
            _analytics.Track(AnalyticsEventNames.AdCompleted, new Dictionary<string, object>
            {
                ["placement"] = placement.ToString(),
                ["result"] = result
            });
        }
    }
}
