using CoreRacer.Gameplay.Powerups;
using CoreRacer.Meta.Economy;
using CoreRacer.Meta.Profile;
using CoreRacer.UI.Shared;
using UnityEngine;

namespace CoreRacer.UI.MainMenu
{
    public sealed class LabPageController : UiView
    {
        [SerializeField] private PowerupUpgradeConfigV2 upgradeConfig;
        private PlayerProfileService _profile;
        private void Awake() => CoreRacer.Bootstrap.GameServices.TryGet(out _profile);

        public void UpgradePowerup(PowerupType type)
        {
            if (_profile == null || upgradeConfig == null) return;
            var id = type.ToString();
            var current = _profile.GetUpgradeLevel(_profile.State.PowerupUpgradeLevels, id);
            var entry = upgradeConfig.GetEntry(type);
            if (current >= entry.MaxLevel) return;
            var cost = new CurrencyAmount(CurrencyType.Soft, entry.GetCostForLevel(current));
            if (!_profile.TrySpend(cost)) return;
            _profile.SetUpgradeLevel(_profile.State.PowerupUpgradeLevels, id, current + 1);
        }
    }
}
