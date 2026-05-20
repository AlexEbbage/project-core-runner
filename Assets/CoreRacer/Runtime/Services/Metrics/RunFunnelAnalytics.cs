using System.Collections.Generic;
using CoreRacer.Services.Analytics;

namespace CoreRacer.Services.Metrics
{
    public sealed class RunFunnelAnalytics
    {
        private readonly IAnalyticsService _analytics;

        public RunFunnelAnalytics(IAnalyticsService analytics)
        {
            _analytics = analytics;
        }

        public void FirstRunStarted()
        {
            _analytics.Track(AnalyticsEventNames.FirstRunStarted);
        }

        public void RunAbandoned(string state, float durationSeconds)
        {
            _analytics.Track(AnalyticsEventNames.RunAbandoned, new Dictionary<string, object>
            {
                ["state"] = state,
                ["duration_seconds"] = durationSeconds
            });
        }
    }
}
