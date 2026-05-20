using System;

namespace CoreRacer.Services.Compliance
{
    [Serializable]
    public sealed class ConsentState
    {
        public int Version = 1;
        public TrackingConsentState AnalyticsConsent = TrackingConsentState.Unknown;
        public TrackingConsentState AdsPersonalizationConsent = TrackingConsentState.Unknown;
        public string AcceptedPrivacyVersion;
        public string AcceptedTermsVersion;
        public string LastUpdatedUtcIso;
    }
}
