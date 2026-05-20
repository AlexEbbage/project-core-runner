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
        [SerializeField] private Button deleteLocalProgressButton;
        [SerializeField] private Text exportSummaryText;

        private ConsentService _consent;
        private DataControlsService _dataControls;

        private void Awake()
        {
            if (privacyPolicyButton != null) privacyPolicyButton.onClick.AddListener(OpenPrivacyPolicy);
            if (termsButton != null) termsButton.onClick.AddListener(OpenTerms);
            if (deleteLocalProgressButton != null) deleteLocalProgressButton.onClick.AddListener(DeleteLocalProgress);
        }

        private void OnEnable()
        {
            GameServices.TryGet(out _consent);
            GameServices.TryGet(out _dataControls);
            RefreshExportSummary();
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

        public void DeleteLocalProgress()
        {
            _dataControls?.DeleteLocalProgress();
            RefreshExportSummary();
        }

        private void RefreshExportSummary()
        {
            if (exportSummaryText != null)
                exportSummaryText.text = _dataControls != null ? _dataControls.ExportLocalDataSummary() : string.Empty;
        }
    }
}
