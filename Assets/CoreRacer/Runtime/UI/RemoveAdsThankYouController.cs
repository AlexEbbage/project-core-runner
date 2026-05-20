using CoreRacer.Bootstrap;
using CoreRacer.Monetisation.Premium;
using CoreRacer.UI.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI
{
    public sealed class RemoveAdsThankYouController : UiView
    {
        [SerializeField] private Text messageText;
        [SerializeField] private string message = "Premium active — respawn, double reward and interstitial ads are removed.";
        private PremiumEntitlementService _premium;

        private void Awake()
        {
            GameServices.TryGet(out _premium);
            if (messageText != null) messageText.text = message;
        }

        private void OnEnable()
        {
            if (_premium != null) _premium.PremiumChanged += HandlePremiumChanged;
            Refresh();
        }

        private void OnDisable()
        {
            if (_premium != null) _premium.PremiumChanged -= HandlePremiumChanged;
        }

        private void HandlePremiumChanged(bool active) => Refresh();

        public void Refresh()
        {
            if (_premium != null && _premium.HasPremium)
                Show();
            else
                Hide();
        }
    }
}
