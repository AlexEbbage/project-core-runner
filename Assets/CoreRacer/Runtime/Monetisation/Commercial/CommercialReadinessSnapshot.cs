namespace CoreRacer.Monetisation.Commercial
{
    public struct CommercialReadinessSnapshot
    {
        public bool HasPrivacyPolicyUrl;
        public bool HasTermsUrl;
        public bool HasDataDeletionUrl;
        public bool ConsentResolved;
        public bool CanUseAnalytics;
        public bool CanUsePersonalizedAds;
        public bool HasPremium;
        public bool RewardedAdsConfigured;
        public bool InterstitialAdsConfigured;
        public bool IapConfigured;
        public bool StoreLinksConfigured => HasPrivacyPolicyUrl && HasTermsUrl && HasDataDeletionUrl;
        public bool IsCommerciallySafe => StoreLinksConfigured && ConsentResolved && IapConfigured;

        public string ToSummary()
        {
            return $"privacy={HasPrivacyPolicyUrl}, terms={HasTermsUrl}, deletion={HasDataDeletionUrl}, " +
                   $"consent={ConsentResolved}, analytics={CanUseAnalytics}, personalisedAds={CanUsePersonalizedAds}, " +
                   $"premium={HasPremium}, rewardedAds={RewardedAdsConfigured}, interstitialAds={InterstitialAdsConfigured}, iap={IapConfigured}";
        }
    }
}
