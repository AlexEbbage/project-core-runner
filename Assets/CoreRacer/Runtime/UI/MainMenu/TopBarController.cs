using CoreRacer.Bootstrap;
using CoreRacer.Common;
using CoreRacer.Meta.Profile;
using CoreRacer.UI.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.MainMenu
{
    public sealed class TopBarController : MonoBehaviour
    {
        [SerializeField] private Text softCurrencyText;
        [SerializeField] private Text premiumCurrencyText;
        [SerializeField] private Text levelText;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button profileButton;
        [SerializeField] private MainMenuPageRouter router;
        private PlayerProfileService _profile;

        private void Awake()
        {
            if (router == null)
                router = GetComponentInParent<MainMenuPageRouter>();
        }

        private void OnEnable()
        {
            GameServices.RegistryChanged += HandleRegistryChanged;
            ResolveProfile();
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (profileButton != null) profileButton.onClick.AddListener(OpenProgression);
            Refresh();
        }

        private void OnDisable()
        {
            GameServices.RegistryChanged -= HandleRegistryChanged;
            if (_profile != null) _profile.Changed -= Refresh;
            if (settingsButton != null) settingsButton.onClick.RemoveListener(OpenSettings);
            if (profileButton != null) profileButton.onClick.RemoveListener(OpenProgression);
        }

        public void Refresh()
        {
            if (_profile == null)
            {
                ResolveProfile();
                if (_profile == null)
                    return;
            }

            UiTextBinder.SetText(softCurrencyText, _profile.State.Wallet.Soft.ToString("N0"));
            UiTextBinder.SetText(premiumCurrencyText, _profile.State.Wallet.Premium.ToString("N0"));
            UiTextBinder.SetText(levelText, $"Lv {_profile.State.Level}");
        }

        public void OpenSettings() => router?.Show(MainMenuPage.Settings);
        public void OpenProgression() => router?.Show(MainMenuPage.Progression);

        private void HandleRegistryChanged(ServiceRegistry _)
        {
            ResolveProfile();
            Refresh();
        }

        private void ResolveProfile()
        {
            if (_profile != null)
                _profile.Changed -= Refresh;

            GameServices.TryGet(out _profile);
            if (isActiveAndEnabled && _profile != null)
                _profile.Changed += Refresh;
        }
    }
}
