using UnityEngine;

namespace CoreRacer.Services.Compliance
{
    [CreateAssetMenu(menuName = "Core Racer/Compliance/Privacy Links")]
    public sealed class PrivacyLinksConfig : ScriptableObject
    {
        public string PrivacyPolicyUrl = "https://example.com/privacy";
        public string TermsUrl = "https://example.com/terms";
        public string DataDeletionUrl = "https://example.com/data-deletion";
        public string PrivacyVersion = "1.0";
        public string TermsVersion = "1.0";
        public bool RequireConsentBeforeAnalytics = true;
        public bool TreatAsChildDirected = false;
    }
}
