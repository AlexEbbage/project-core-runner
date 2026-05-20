using CoreRacer.Monetisation.Premium;

namespace CoreRacer.Monetisation.Ads
{
    /// <summary>
    /// Encodes the product promise:
    /// Premium bypasses continue, double-run-reward and interstitial ads.
    /// Premium still watches true rewarded ads such as mid-run offers and daily double reward.
    /// </summary>
    public sealed class AdPolicyService
    {
        private readonly PremiumEntitlementService _premium;

        public AdPolicyService(PremiumEntitlementService premium)
        {
            _premium = premium;
        }

        public AdPlacementPolicy GetPolicy(AdPlacement placement)
        {
            switch (placement)
            {
                case AdPlacement.ContinueRun:
                    return new AdPlacementPolicy(true, true, true);
                case AdPlacement.DoubleRunRewards:
                    return new AdPlacementPolicy(true, true, true);
                case AdPlacement.Interstitial:
                    return new AdPlacementPolicy(true, true, false);
                case AdPlacement.DailyLoginDoubleReward:
                    return new AdPlacementPolicy(true, false, false);
                case AdPlacement.MidRunRewardedOffer:
                    return new AdPlacementPolicy(true, false, false);
                default:
                    return new AdPlacementPolicy(true, false, false);
            }
        }

        public bool RequiresAd(AdPlacement placement)
        {
            var policy = GetPolicy(placement);
            return policy.RequiresAd && !(policy.BypassedByPremium && _premium.HasPremium);
        }

        public bool ShouldGrantRewardWhenBypassed(AdPlacement placement)
        {
            var policy = GetPolicy(placement);
            return policy.BypassedByPremium && policy.GrantRewardWhenBypassed && _premium.HasPremium;
        }
    }
}
