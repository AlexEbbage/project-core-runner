using System.Collections.Generic;
using CoreRacer.Gameplay.Powerups;
using CoreRacer.Meta.Economy;
using CoreRacer.Meta.Profile;
using CoreRacer.Meta.Progression;
using CoreRacer.Meta.Ships;
using UnityEngine;

namespace CoreRacer.UI.Toolkit
{
    public sealed class LabScreenPresenter : UiScreenPresenterBase
    {
        private readonly LabScreenView _view;
        private readonly CoreRacerUiContext _context;
        private readonly IUiAnimationService _animations;
        private readonly UiToastService _toast;

        public LabScreenPresenter(LabScreenView view, CoreRacerUiContext context, IUiAnimationService animations, UiToastService toast)
            : base(CoreRacerScreenId.Lab, view.Root, animations)
        {
            _view = view;
            _context = context;
            _animations = animations;
            _toast = toast;
        }

        protected override void OnInitialize()
        {
            if (_context.Profile != null)
                _context.Profile.Changed += Refresh;
        }

        protected override void OnDispose()
        {
            if (_context.Profile != null)
                _context.Profile.Changed -= Refresh;
        }

        public override void Refresh()
        {
            RenderBoosters();
            RenderPassiveUpgrades();
            RenderExperiments();
        }

        private void RenderBoosters()
        {
            _view.BoosterList.Clear();
            var config = _context.PowerupUpgrades;
            var profile = _context.Profile;
            if (config == null || profile == null || config.Upgrades == null)
            {
                _view.BoosterList.Add(UiDynamicElements.EmptyState("Run booster upgrades are not configured."));
                return;
            }

            for (var i = 0; i < config.Upgrades.Count; i++)
            {
                var entry = config.Upgrades[i];
                if (entry == null)
                    continue;
                var id = entry.Type.ToString();
                var level = profile.GetUpgradeLevel(profile.State.PowerupUpgradeLevels, id);
                var max = entry.MaxLevel;
                var cost = entry.GetCostForLevel(level);
                var row = new ActionListItemElement();
                row.Bind(
                    entry.Icon,
                    entry.DisplayName,
                    $"Level {level}/{max} · improve duration and strength",
                    level >= max ? "MAXIMUM" : $"{cost:N0} CREDITS",
                    max > 0 ? (float)level / max : 1f,
                    level >= max ? "MAX" : "UPGRADE",
                    () => UpgradePowerup(entry),
                    level < max,
                    level >= max ? UiClassNames.Success : null);
                _view.BoosterList.Add(row);
            }
        }

        private void RenderPassiveUpgrades()
        {
            _view.PassiveList.Clear();
            var upgrades = _context.ShipDatabase?.Upgrades;
            var profile = _context.Profile;
            if (upgrades == null || profile == null || upgrades.Count == 0)
            {
                _view.PassiveList.Add(UiDynamicElements.EmptyState("Passive upgrades are not configured."));
                return;
            }

            for (var i = 0; i < upgrades.Count; i++)
            {
                var upgrade = upgrades[i];
                if (upgrade == null)
                    continue;
                var id = upgrade.UpgradeType.ToString();
                var level = profile.GetUpgradeLevel(profile.State.ShipUpgradeLevels, id);
                var cost = upgrade.GetCostForLevel(level);
                var row = new ActionListItemElement();
                row.Bind(
                    upgrade.Icon,
                    upgrade.DisplayName,
                    $"Permanent ship upgrade · level {level}/{upgrade.MaxLevel}",
                    level >= upgrade.MaxLevel ? "MAXIMUM" : $"{cost:N0} {upgrade.Currency}",
                    upgrade.MaxLevel > 0 ? (float)level / upgrade.MaxLevel : 1f,
                    level >= upgrade.MaxLevel ? "MAX" : "UPGRADE",
                    () => UpgradeShip(upgrade),
                    level < upgrade.MaxLevel,
                    level >= upgrade.MaxLevel ? UiClassNames.Success : null);
                _view.PassiveList.Add(row);
            }
        }

        private void RenderExperiments()
        {
            _view.ExperimentList.Clear();
            var levels = _context.LevelRoadmap?.Levels;
            if (levels == null || levels.Count == 0)
            {
                _view.ExperimentList.Add(UiDynamicElements.EmptyState("Core experiments are not configured."));
                return;
            }

            for (var i = 0; i < Mathf.Min(2, levels.Count); i++)
            {
                var level = levels[i];
                if (level == null)
                    continue;
                var unlocked = i == 0 || (_context.Profile != null && _context.Profile.State.Level >= level.RequiredPlayerLevel);
                var row = new ActionListItemElement();
                row.Bind(
                    null,
                    level.DisplayName,
                    level.Description,
                    unlocked ? "AVAILABLE" : $"REQUIRES LV {level.RequiredPlayerLevel}",
                    unlocked ? 0.65f : 0f,
                    string.Empty,
                    null,
                    false,
                    unlocked ? UiClassNames.Attention : UiClassNames.Locked);
                _view.ExperimentList.Add(row);
            }
        }

        private void UpgradePowerup(PowerupUpgradeEntry entry)
        {
            var profile = _context.Profile;
            var id = entry.Type.ToString();
            var current = profile.GetUpgradeLevel(profile.State.PowerupUpgradeLevels, id);
            if (current >= entry.MaxLevel)
                return;
            var cost = new CurrencyAmount(CurrencyType.Soft, entry.GetCostForLevel(current));
            var success = profile.TryMutate(state =>
            {
                if (!state.Wallet.TrySpend(cost))
                    return false;
                SetLevel(state.PowerupUpgradeLevels, id, current + 1);
                return true;
            });
            CompleteUpgrade(entry.DisplayName, current + 1, success);
        }

        private void UpgradeShip(ShipUpgradeDefinition upgrade)
        {
            var profile = _context.Profile;
            var id = upgrade.UpgradeType.ToString();
            var current = profile.GetUpgradeLevel(profile.State.ShipUpgradeLevels, id);
            if (current >= upgrade.MaxLevel)
                return;
            var cost = new CurrencyAmount(upgrade.Currency, upgrade.GetCostForLevel(current));
            var success = profile.TryMutate(state =>
            {
                if (!state.Wallet.TrySpend(cost))
                    return false;
                SetLevel(state.ShipUpgradeLevels, id, current + 1);
                return true;
            });
            CompleteUpgrade(upgrade.DisplayName, current + 1, success);
        }

        private void CompleteUpgrade(string name, int level, bool success)
        {
            if (!success)
            {
                _view.Status.text = "Not enough currency.";
                _toast.Show(_view.Status.text, true);
                _animations.PlayInvalidAction(_view.Root);
                return;
            }

            _view.Status.text = $"{name} upgraded to level {level}.";
            _toast.Show(_view.Status.text);
            _context.Tutorial?.Notify(CoreRacer.FTUE.TutorialStepKind.WaitForUpgradePurchased, "lab");
            Refresh();
        }

        private static void SetLevel(List<SerializableIntById> levels, string id, int value)
        {
            for (var i = 0; i < levels.Count; i++)
            {
                if (levels[i].Id != id)
                    continue;
                levels[i] = new SerializableIntById(id, value);
                return;
            }
            levels.Add(new SerializableIntById(id, value));
        }
    }
}
