using System.Collections.Generic;
using CoreRacer.Monetisation.Ads;
using CoreRacer.Services.Analytics;

namespace CoreRacer.Services.Metrics
{
    public sealed class AdIapAnalytics
    {
        private readonly IAnalyticsService _analytics;

        public AdIapAnalytics(IAnalyticsService analytics)
        {
            _analytics = analytics;
        }

        public void RewardedAdFailed(AdPlacement placement, string reason)
        {
            _analytics.Track(AnalyticsEventNames.RewardedAdFailed, new Dictionary<string, object>
            {
                ["placement"] = placement.ToString(),
                ["reason"] = reason
            });
        }

        public void AdBypassedByPremium(AdPlacement placement)
        {
            _analytics.Track(AnalyticsEventNames.AdBypassedByPremium, new Dictionary<string, object>
            {
                ["placement"] = placement.ToString()
            });
        }

        public void PurchaseStarted(string productId)
        {
            _analytics.Track(AnalyticsEventNames.PurchaseStarted, new Dictionary<string, object>
            {
                ["product_id"] = productId
            });
        }

        public void PurchaseFailed(string productId, string reason)
        {
            _analytics.Track(AnalyticsEventNames.PurchaseFailed, new Dictionary<string, object>
            {
                ["product_id"] = productId,
                ["reason"] = reason
            });
        }
    }
}
