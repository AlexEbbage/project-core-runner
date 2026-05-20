using System;
using CoreRacer.Services.Analytics;

namespace CoreRacer.Monetisation.Ads
{
    public sealed class RewardedAdController
    {
        private readonly IRewardedAdService _ads;
        private readonly AdPolicyService _policy;
        private readonly GameAnalytics _analytics;

        public RewardedAdController(IRewardedAdService ads, AdPolicyService policy, GameAnalytics analytics)
        {
            _ads = ads;
            _policy = policy;
            _analytics = analytics;
        }

        public bool CanShow(AdPlacement placement)
        {
            return !_policy.RequiresAd(placement) || (_ads != null && _ads.IsRewardedAdReady());
        }

        public void ShowOrBypass(AdPlacement placement, Action<RewardedAdResult> completed)
        {
            if (!_policy.RequiresAd(placement))
            {
                completed?.Invoke(RewardedAdResult.BypassedByPremium);
                return;
            }

            if (_ads == null || !_ads.IsRewardedAdReady())
            {
                completed?.Invoke(RewardedAdResult.NotReady);
                return;
            }

            _analytics?.AdRequested(placement);
            _ads.ShowRewardedAd(placement, result =>
            {
                _analytics?.AdCompleted(placement, result.ToString());
                completed?.Invoke(result);
            });
        }
    }
}
