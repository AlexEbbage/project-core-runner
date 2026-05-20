namespace CoreRacer.Monetisation.Ads
{
    public struct AdPlacementPolicy
    {
        public bool RequiresAd;
        public bool BypassedByPremium;
        public bool GrantRewardWhenBypassed;

        public AdPlacementPolicy(bool requiresAd, bool bypassedByPremium, bool grantRewardWhenBypassed)
        {
            RequiresAd = requiresAd;
            BypassedByPremium = bypassedByPremium;
            GrantRewardWhenBypassed = grantRewardWhenBypassed;
        }
    }
}
