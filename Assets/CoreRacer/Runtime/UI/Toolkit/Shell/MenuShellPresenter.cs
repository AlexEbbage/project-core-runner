using System;
using CoreRacer.Meta.Profile;
using UnityEngine;

namespace CoreRacer.UI.Toolkit
{
    public sealed class MenuShellPresenter : IDisposable
    {
        private readonly MenuShellView _view;
        private readonly PlayerProfileService _profile;
        private readonly ICoreRacerScreenRouter _router;

        public MenuShellPresenter(MenuShellView view, PlayerProfileService profile, ICoreRacerScreenRouter router)
        {
            _view = view;
            _profile = profile;
            _router = router;
        }

        public void Initialize()
        {
            _view.ProfileButton.clicked += ShowProgression;
            _view.SettingsButton.clicked += ShowSettings;
            _view.Navigation[CoreRacerScreenId.Play].clicked += ShowPlay;
            _view.Navigation[CoreRacerScreenId.Shop].clicked += ShowShop;
            _view.Navigation[CoreRacerScreenId.Hangar].clicked += ShowHangar;
            _view.Navigation[CoreRacerScreenId.Lab].clicked += ShowLab;
            _view.Navigation[CoreRacerScreenId.Progression].clicked += ShowProgression;
            if (_profile != null)
                _profile.Changed += Refresh;
            Refresh();
        }

        public void Refresh()
        {
            if (_profile == null)
            {
                _view.LevelLabel.text = "LV 1";
                _view.XpLabel.text = "0 / 500 XP";
                _view.XpBar.value = 0f;
                _view.SoftCurrencyLabel.text = "0";
                _view.PremiumCurrencyLabel.text = "0";
                return;
            }

            var state = _profile.State;
            var required = Mathf.Max(1, _profile.ExperienceForNextLevel(state.Level));
            _view.LevelLabel.text = $"LV {state.Level}";
            _view.XpLabel.text = $"{state.Experience:N0} / {required:N0} XP";
            _view.XpBar.value = Mathf.Clamp01((float)state.Experience / required) * 100f;
            _view.SoftCurrencyLabel.text = state.Wallet.Soft.ToString("N0");
            _view.PremiumCurrencyLabel.text = state.Wallet.Premium.ToString("N0");
        }

        public void Dispose()
        {
            _view.ProfileButton.clicked -= ShowProgression;
            _view.SettingsButton.clicked -= ShowSettings;
            _view.Navigation[CoreRacerScreenId.Play].clicked -= ShowPlay;
            _view.Navigation[CoreRacerScreenId.Shop].clicked -= ShowShop;
            _view.Navigation[CoreRacerScreenId.Hangar].clicked -= ShowHangar;
            _view.Navigation[CoreRacerScreenId.Lab].clicked -= ShowLab;
            _view.Navigation[CoreRacerScreenId.Progression].clicked -= ShowProgression;
            if (_profile != null)
                _profile.Changed -= Refresh;
        }

        private void ShowPlay() => _router.Show(CoreRacerScreenId.Play);
        private void ShowShop() => _router.Show(CoreRacerScreenId.Shop);
        private void ShowHangar() => _router.Show(CoreRacerScreenId.Hangar);
        private void ShowLab() => _router.Show(CoreRacerScreenId.Lab);
        private void ShowProgression() => _router.Show(CoreRacerScreenId.Progression);
        private void ShowSettings() => _router.Show(CoreRacerScreenId.Settings);
    }
}
