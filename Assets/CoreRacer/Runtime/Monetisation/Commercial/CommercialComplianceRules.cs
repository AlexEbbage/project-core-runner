using CoreRacer.Monetisation.Ads;
using CoreRacer.Services.Compliance;

namespace CoreRacer.Monetisation.Commercial
{
    public static class CommercialComplianceRules
    {
        public static bool HasUsableUrl(string url)
        {
            return !string.IsNullOrWhiteSpace(url) &&
                   url.Trim().StartsWith("https://", System.StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsPlaceholderUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return true;

            var lower = url.Trim().ToLowerInvariant();
            return lower.Contains("example.com") ||
                   lower.Contains("localhost") ||
                   lower.Contains("placeholder") ||
                   lower.Contains("your-") ||
                   lower.Contains("todo");
        }

        public static bool HasProductionSafeUrl(string url)
        {
            return HasUsableUrl(url) && !IsPlaceholderUrl(url);
        }

        public static bool IsConsentResolved(ConsentState state)
        {
            return state != null &&
                   state.AnalyticsConsent != TrackingConsentState.Unknown &&
                   state.AdsPersonalizationConsent != TrackingConsentState.Unknown &&
                   !string.IsNullOrWhiteSpace(state.AcceptedPrivacyVersion);
        }

        public static bool ShouldGrantReward(RewardedAdResult result)
        {
            return result == RewardedAdResult.Rewarded || result == RewardedAdResult.BypassedByPremium;
        }

        public static bool ShouldTreatAsUnavailable(RewardedAdResult result)
        {
            return result == RewardedAdResult.NotReady || result == RewardedAdResult.FailedToShow;
        }
    }
}
