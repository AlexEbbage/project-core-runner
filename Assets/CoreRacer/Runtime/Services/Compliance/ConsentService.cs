using System;
using CoreRacer.Services.Analytics;
using CoreRacer.Services.Save;

namespace CoreRacer.Services.Compliance
{
    public sealed class ConsentService
    {
        private readonly ISaveStorage _storage;
        private readonly JsonSaveSerializer _serializer;
        private readonly IAnalyticsService _analytics;

        public ConsentState State { get; private set; }
        public PrivacyLinksConfig Links { get; }
        public event Action<ConsentState> Changed;

        public ConsentService(ISaveStorage storage, JsonSaveSerializer serializer, PrivacyLinksConfig links, IAnalyticsService analytics = null)
        {
            _storage = storage;
            _serializer = serializer;
            Links = links;
            _analytics = analytics;
            State = Load();
        }

        public bool CanUseAnalytics()
        {
            if (Links != null && !Links.RequireConsentBeforeAnalytics)
                return true;
            return State.AnalyticsConsent == TrackingConsentState.Granted || State.AnalyticsConsent == TrackingConsentState.NotRequired;
        }

        public bool CanUsePersonalizedAds()
        {
            return State.AdsPersonalizationConsent == TrackingConsentState.Granted || State.AdsPersonalizationConsent == TrackingConsentState.NotRequired;
        }

        public void SetAnalyticsConsent(TrackingConsentState consent)
        {
            State.AnalyticsConsent = consent;
            Commit();
        }

        public void SetAdsPersonalizationConsent(TrackingConsentState consent)
        {
            State.AdsPersonalizationConsent = consent;
            Commit();
        }

        public void AcceptPolicies(string privacyVersion = null, string termsVersion = null)
        {
            State.AcceptedPrivacyVersion = privacyVersion ?? Links?.PrivacyVersion ?? string.Empty;
            State.AcceptedTermsVersion = termsVersion ?? Links?.TermsVersion ?? string.Empty;
            Commit();
        }

        private ConsentState Load()
        {
            if (!_storage.Exists(SaveKeys.Consent))
                return new ConsentState();
            return _serializer.Deserialize<ConsentState>(_storage.Load(SaveKeys.Consent));
        }

        private void Commit()
        {
            State.LastUpdatedUtcIso = DateTimeOffset.UtcNow.ToString("o");
            _storage.Save(SaveKeys.Consent, _serializer.Serialize(State));
            _analytics?.Track(AnalyticsEventNames.ConsentUpdated);
            Changed?.Invoke(State);
        }
    }
}
