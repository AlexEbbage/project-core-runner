using System.Collections.Generic;
using CoreRacer.Meta.Economy;
using CoreRacer.Services.Analytics;

namespace CoreRacer.Services.Metrics
{
    public sealed class EconomyAnalytics
    {
        private readonly IAnalyticsService _analytics;

        public EconomyAnalytics(IAnalyticsService analytics)
        {
            _analytics = analytics;
        }

        public void CurrencyEarned(CurrencyType type, int amount, string source)
        {
            _analytics.Track(AnalyticsEventNames.CurrencyEarned, new Dictionary<string, object>
            {
                ["currency"] = type.ToString(),
                ["amount"] = amount,
                ["source"] = source
            });
        }

        public void CurrencySpent(CurrencyType type, int amount, string sink)
        {
            _analytics.Track(AnalyticsEventNames.CurrencySpent, new Dictionary<string, object>
            {
                ["currency"] = type.ToString(),
                ["amount"] = amount,
                ["sink"] = sink
            });
        }
    }
}
