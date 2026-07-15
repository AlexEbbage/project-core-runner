using System;
using CoreRacer.Monetisation.Commercial;
using CoreRacer.Services.Analytics;
using CoreRacer.Services.Metrics;

namespace CoreRacer.Monetisation.Ads
{
    public sealed class RewardedAdController
    {
        private readonly IRewardedAdService _ads;
        private readonly AdPolicyService _policy;
        private readonly GameAnalytics _analytics;
        private readonly AdIapAnalytics _adIapAnalytics;

        public RewardedAdController(IRewardedAdService ads, AdPolicyService policy, GameAnalytics analytics, AdIapAnalytics adIapAnalytics = null)
        {
            _ads = ads;
            _policy = policy;
            _analytics = analytics;
            _adIapAnalytics = adIapAnalytics;
        }

        public bool HasProvider => _ads != null;

        public bool CanShow(AdPlacement placement)
        {
            return !_policy.RequiresAd(placement) || (_ads != null && _ads.IsRewardedAdReady());
        }

        public bool ShouldGrantReward(RewardedAdResult result)
        {
            return CommercialComplianceRules.ShouldGrantReward(result);
        }

        public void ShowOrBypass(AdPlacement placement, Action<RewardedAdResult> completed)
        {
            if (!_policy.RequiresAd(placement))
            {
                _adIapAnalytics?.AdBypassedByPremium(placement);
                completed?.Invoke(RewardedAdResult.BypassedByPremium);
                return;
            }

            if (_ads == null)
            {
                _adIapAnalytics?.RewardedAdFailed(placement, "provider_missing");
                completed?.Invoke(RewardedAdResult.NotReady);
                return;
            }

            if (!_ads.IsRewardedAdReady())
            {
                _adIapAnalytics?.RewardedAdFailed(placement, "not_ready");
                completed?.Invoke(RewardedAdResult.NotReady);
                return;
            }

            _analytics?.AdRequested(placement);
            _ads.ShowRewardedAd(placement, result =>
            {
                _analytics?.AdCompleted(placement, result.ToString());
                if (CommercialComplianceRules.ShouldTreatAsUnavailable(result))
                    _adIapAnalytics?.RewardedAdFailed(placement, result.ToString());
                completed?.Invoke(result);
            });
        }
    }
}
