using CoreRacer.Monetisation.Ads;
using CoreRacer.Monetisation.Iap;
using CoreRacer.Monetisation.Premium;
using CoreRacer.Services.Compliance;

namespace CoreRacer.Monetisation.Commercial
{
    public sealed class CommercialReadinessService
    {
        private readonly ConsentService _consent;
        private readonly PremiumEntitlementService _premium;
        private readonly RewardedAdController _rewardedAds;
        private readonly InterstitialAdController _interstitialAds;
        private readonly IapPurchaseService _iap;

        public CommercialReadinessService(
            ConsentService consent,
            PremiumEntitlementService premium,
            RewardedAdController rewardedAds,
            InterstitialAdController interstitialAds,
            IapPurchaseService iap)
        {
            _consent = consent;
            _premium = premium;
            _rewardedAds = rewardedAds;
            _interstitialAds = interstitialAds;
            _iap = iap;
        }

        public CommercialReadinessSnapshot BuildSnapshot()
        {
            var links = _consent?.Links;
            return new CommercialReadinessSnapshot
            {
                HasPrivacyPolicyUrl = CommercialComplianceRules.HasProductionSafeUrl(links != null ? links.PrivacyPolicyUrl : null),
                HasTermsUrl = CommercialComplianceRules.HasProductionSafeUrl(links != null ? links.TermsUrl : null),
                HasDataDeletionUrl = CommercialComplianceRules.HasProductionSafeUrl(links != null ? links.DataDeletionUrl : null),
                ConsentResolved = _consent != null && CommercialComplianceRules.IsConsentResolved(_consent.State),
                CanUseAnalytics = _consent == null || _consent.CanUseAnalytics(),
                CanUsePersonalizedAds = _consent != null && _consent.CanUsePersonalizedAds(),
                HasPremium = _premium != null && _premium.HasPremium,
                RewardedAdsConfigured = _rewardedAds != null && _rewardedAds.HasProvider,
                InterstitialAdsConfigured = _interstitialAds != null && _interstitialAds.HasProvider,
                IapConfigured = _iap != null && _iap.HasStoreAdapter
            };
        }
    }
}
