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
        private PlayerProfileService _profile;

        private void Awake()
        {
            CoreRacer.Bootstrap.GameServices.TryGet(out _profile);
        }

        private void OnEnable()
        {
            if (_profile != null) _profile.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (_profile != null) _profile.Changed -= Refresh;
        }

        public void Refresh()
        {
            if (_profile == null) return;
            UiTextBinder.SetText(softCurrencyText, _profile.State.Wallet.Soft.ToString("N0"));
            UiTextBinder.SetText(premiumCurrencyText, _profile.State.Wallet.Premium.ToString("N0"));
            UiTextBinder.SetText(levelText, $"Lv {_profile.State.Level}");
        }
    }
}
