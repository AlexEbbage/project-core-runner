using System;
using CoreRacer.Services.Analytics;
using CoreRacer.Services.Metrics;

namespace CoreRacer.Monetisation.Ads
{
    public sealed class InterstitialAdController
    {
        private readonly IInterstitialAdService _ads;
        private readonly AdPolicyService _policy;
        private readonly GameAnalytics _analytics;
        private readonly AdIapAnalytics _adIapAnalytics;

        public InterstitialAdController(IInterstitialAdService ads, AdPolicyService policy, GameAnalytics analytics, AdIapAnalytics adIapAnalytics = null)
        {
            _ads = ads;
            _policy = policy;
            _analytics = analytics;
            _adIapAnalytics = adIapAnalytics;
        }

        public bool HasProvider => _ads != null;

        public void ShowIfAllowed(Action<InterstitialAdResult> completed = null)
        {
            const AdPlacement placement = AdPlacement.Interstitial;

            if (!_policy.RequiresAd(placement))
            {
                _adIapAnalytics?.AdBypassedByPremium(placement);
                completed?.Invoke(InterstitialAdResult.BypassedByPremium);
                return;
            }

            if (_ads == null || !_ads.IsInterstitialAdReady())
            {
                completed?.Invoke(InterstitialAdResult.NotReady);
                return;
            }

            _analytics?.AdRequested(placement);
            _ads.ShowInterstitialAd(placement, result =>
            {
                _analytics?.AdCompleted(placement, result.ToString());
                completed?.Invoke(result);
            });
        }
    }
}
