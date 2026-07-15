using CoreRacer.Bootstrap;
using CoreRacer.Services.Compliance;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.Compliance
{
    public sealed class PrivacySettingsController : MonoBehaviour
    {
        [SerializeField] private Button privacyPolicyButton;
        [SerializeField] private Button termsButton;
        [SerializeField] private Button dataDeletionButton;
        [SerializeField] private Button deleteLocalProgressButton;
        [SerializeField] private Button grantAnalyticsConsentButton;
        [SerializeField] private Button denyAnalyticsConsentButton;
        [SerializeField] private Button grantPersonalizedAdsButton;
        [SerializeField] private Button denyPersonalizedAdsButton;
        [SerializeField] private Text exportSummaryText;
        [SerializeField] private Text consentStatusText;

        private ConsentService _consent;
        private DataControlsService _dataControls;

        private void Awake()
        {
            if (privacyPolicyButton != null) privacyPolicyButton.onClick.AddListener(OpenPrivacyPolicy);
            if (termsButton != null) termsButton.onClick.AddListener(OpenTerms);
            if (dataDeletionButton != null) dataDeletionButton.onClick.AddListener(OpenDataDeletion);
            if (deleteLocalProgressButton != null) deleteLocalProgressButton.onClick.AddListener(DeleteLocalProgress);
            if (grantAnalyticsConsentButton != null) grantAnalyticsConsentButton.onClick.AddListener(() => SetAnalyticsConsent(TrackingConsentState.Granted));
            if (denyAnalyticsConsentButton != null) denyAnalyticsConsentButton.onClick.AddListener(() => SetAnalyticsConsent(TrackingConsentState.Denied));
            if (grantPersonalizedAdsButton != null) grantPersonalizedAdsButton.onClick.AddListener(() => SetPersonalizedAdsConsent(TrackingConsentState.Granted));
            if (denyPersonalizedAdsButton != null) denyPersonalizedAdsButton.onClick.AddListener(() => SetPersonalizedAdsConsent(TrackingConsentState.Denied));
        }

        private void OnEnable()
        {
            GameServices.TryGet(out _consent);
            GameServices.TryGet(out _dataControls);
            if (_consent != null)
                _consent.Changed += OnConsentChanged;
            Refresh();
        }

        private void OnDisable()
        {
            if (_consent != null)
                _consent.Changed -= OnConsentChanged;
        }

        public void OpenPrivacyPolicy()
        {
            var url = _consent?.Links != null ? _consent.Links.PrivacyPolicyUrl : string.Empty;
            if (!string.IsNullOrWhiteSpace(url)) Application.OpenURL(url);
        }

        public void OpenTerms()
        {
            var url = _consent?.Links != null ? _consent.Links.TermsUrl : string.Empty;
            if (!string.IsNullOrWhiteSpace(url)) Application.OpenURL(url);
        }

        public void OpenDataDeletion()
        {
            var url = _consent?.Links != null ? _consent.Links.DataDeletionUrl : string.Empty;
            if (!string.IsNullOrWhiteSpace(url)) Application.OpenURL(url);
        }

        public void DeleteLocalProgress()
        {
            _dataControls?.DeleteLocalProgress();
            Refresh();
        }

        public void SetAnalyticsConsent(TrackingConsentState consent)
        {
            _consent?.SetAnalyticsConsent(consent);
            _consent?.AcceptPolicies();
            Refresh();
        }

        public void SetPersonalizedAdsConsent(TrackingConsentState consent)
        {
            _consent?.SetAdsPersonalizationConsent(consent);
            _consent?.AcceptPolicies();
            Refresh();
        }

        private void OnConsentChanged(ConsentState state)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (exportSummaryText != null)
                exportSummaryText.text = _dataControls != null ? _dataControls.ExportLocalDataSummary() : string.Empty;

            if (consentStatusText != null)
            {
                if (_consent == null)
                {
                    consentStatusText.text = string.Empty;
                    return;
                }

                consentStatusText.text = $"Analytics: {_consent.State.AnalyticsConsent}\nAds Personalisation: {_consent.State.AdsPersonalizationConsent}\nPrivacy Version: {_consent.State.AcceptedPrivacyVersion}";
            }
        }
    }
}
