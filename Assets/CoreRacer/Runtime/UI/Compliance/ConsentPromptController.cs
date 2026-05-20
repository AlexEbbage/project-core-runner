using CoreRacer.Bootstrap;
using CoreRacer.Services.Compliance;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.Compliance
{
    public sealed class ConsentPromptController : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Button acceptAllButton;
        [SerializeField] private Button rejectPersonalizedAdsButton;
        [SerializeField] private Button privacyPolicyButton;
        [SerializeField] private Button termsButton;

        private ConsentService _consent;

        private void Awake()
        {
            if (acceptAllButton != null) acceptAllButton.onClick.AddListener(AcceptAll);
            if (rejectPersonalizedAdsButton != null) rejectPersonalizedAdsButton.onClick.AddListener(RejectPersonalizedAds);
            if (privacyPolicyButton != null) privacyPolicyButton.onClick.AddListener(OpenPrivacyPolicy);
            if (termsButton != null) termsButton.onClick.AddListener(OpenTerms);
        }

        private void Start()
        {
            GameServices.TryGet(out _consent);
            Refresh();
        }

        public void Refresh()
        {
            if (root == null || _consent == null)
                return;
            root.SetActive(_consent.State.AnalyticsConsent == TrackingConsentState.Unknown || string.IsNullOrEmpty(_consent.State.AcceptedPrivacyVersion));
        }

        private void AcceptAll()
        {
            _consent?.SetAnalyticsConsent(TrackingConsentState.Granted);
            _consent?.SetAdsPersonalizationConsent(TrackingConsentState.Granted);
            _consent?.AcceptPolicies();
            Refresh();
        }

        private void RejectPersonalizedAds()
        {
            _consent?.SetAnalyticsConsent(TrackingConsentState.Granted);
            _consent?.SetAdsPersonalizationConsent(TrackingConsentState.Denied);
            _consent?.AcceptPolicies();
            Refresh();
        }

        private void OpenPrivacyPolicy()
        {
            if (_consent?.Links != null) Application.OpenURL(_consent.Links.PrivacyPolicyUrl);
        }

        private void OpenTerms()
        {
            if (_consent?.Links != null) Application.OpenURL(_consent.Links.TermsUrl);
        }
    }
}
