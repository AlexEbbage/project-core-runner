using System.Collections.Generic;
using CoreRacer.Bootstrap;
using CoreRacer.FTUE;
using CoreRacer.Gameplay.Powerups;
using CoreRacer.Meta.Economy;
using CoreRacer.Meta.Profile;
using CoreRacer.UI.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.MainMenu
{
    public sealed class LabPageController : UiView
    {
        [SerializeField] private PowerupUpgradeConfigV2 upgradeConfig;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private LabUpgradeItemView rowPrefab;
        [SerializeField] private Text statusText;

        private readonly List<LabUpgradeItemView> _rows = new List<LabUpgradeItemView>();
        private PlayerProfileService _profile;
        private TutorialService _tutorial;

        private void Awake()
        {
            GameServices.TryGet(out _profile);
            GameServices.TryGet(out _tutorial);
        }

        public override void Show()
        {
            base.Show();
            if (_tutorial == null) GameServices.TryGet(out _tutorial);
            _tutorial?.Notify(TutorialStepKind.WaitForUpgradePromptOpened, "lab");
            Refresh();
        }

        public void UpgradePowerup(PowerupType type)
        {
            if (_profile == null || upgradeConfig == null) return;
            var id = type.ToString();
            var current = _profile.GetUpgradeLevel(_profile.State.PowerupUpgradeLevels, id);
            var entry = upgradeConfig.GetEntry(type);
            if (current >= entry.MaxLevel) return;
            var cost = new CurrencyAmount(CurrencyType.Soft, entry.GetCostForLevel(current));
            if (!_profile.TrySpend(cost))
            {
                if (statusText != null)
                    statusText.text = $"Not enough coins for {entry.DisplayName}.";
                return;
            }
            _profile.SetUpgradeLevel(_profile.State.PowerupUpgradeLevels, id, current + 1);
            if (_tutorial == null) GameServices.TryGet(out _tutorial);
            _tutorial?.Notify(TutorialStepKind.WaitForUpgradePurchased, "lab");
            if (statusText != null)
                statusText.text = $"{entry.DisplayName} upgraded to Lv {current + 1}.";
            Refresh();
        }

        public void Refresh()
        {
            if (upgradeConfig == null || contentRoot == null || rowPrefab == null || _profile == null)
                return;

            while (_rows.Count < upgradeConfig.Upgrades.Count)
            {
                var row = Instantiate(rowPrefab, contentRoot);
                row.gameObject.SetActive(false);
                _rows.Add(row);
            }

            for (int i = 0; i < _rows.Count; i++)
            {
                var active = i < upgradeConfig.Upgrades.Count;
                _rows[i].gameObject.SetActive(active);
                if (!active)
                    continue;

                var entry = upgradeConfig.Upgrades[i];
                var current = _profile.GetUpgradeLevel(_profile.State.PowerupUpgradeLevels, entry.Type.ToString());
                var maxLevel = entry.MaxLevel;
                var canUpgrade = current < maxLevel && _profile.State.Wallet.CanSpend(new CurrencyAmount(CurrencyType.Soft, entry.GetCostForLevel(current)));
                var actionLabel = current >= maxLevel ? "MAX" : "Upgrade";
                var status = current >= maxLevel ? "Fully upgraded." : canUpgrade ? "Ready to upgrade." : "Need more coins.";
                _rows[i].Bind(
                    entry.Type.ToString(),
                    entry.Icon,
                    entry.DisplayName,
                    $"Lv {current}/{maxLevel}",
                    current >= maxLevel ? "MAX" : $"{entry.GetCostForLevel(current):N0} Coins",
                    status,
                    actionLabel,
                    current < maxLevel,
                    UpgradeById);
            }
        }

        private void UpgradeById(string powerupId)
        {
            if (System.Enum.TryParse(powerupId, out PowerupType type))
                UpgradePowerup(type);
        }
    }
}
