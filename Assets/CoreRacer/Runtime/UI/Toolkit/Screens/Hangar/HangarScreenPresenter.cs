using System;
using System.Collections.Generic;
using CoreRacer.Meta.Economy;
using CoreRacer.Meta.Profile;
using CoreRacer.Meta.Progression;
using CoreRacer.Meta.Ships;
using UnityEngine;
using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public sealed class HangarScreenPresenter : UiScreenPresenterBase
    {
        private readonly HangarScreenView _view;
        private readonly CoreRacerUiContext _context;
        private readonly UiToastService _toast;
        private readonly IUiAnimationService _animations;
        private string _section = "ships";
        private int _selectedIndex;
        private UnlockableDefinition _selection;

        public HangarScreenPresenter(HangarScreenView view, CoreRacerUiContext context, IUiAnimationService animations, UiToastService toast)
            : base(CoreRacerScreenId.Hangar, view.Root, animations)
        {
            _view = view;
            _context = context;
            _toast = toast;
            _animations = animations;
        }

        protected override void OnInitialize()
        {
            _view.PreviousButton.clicked += Previous;
            _view.NextButton.clicked += Next;
            _view.EquipButton.clicked += Equip;
            _view.UpgradeButton.clicked += Upgrade;
            _view.ShipsTab.clicked += ShowShips;
            _view.SkinsTab.clicked += ShowSkins;
            _view.TrailsTab.clicked += ShowTrails;
            _view.CoreFxTab.clicked += ShowCoreFx;
            if (_context.Profile != null)
                _context.Profile.Changed += Refresh;
        }

        protected override void OnDispose()
        {
            _view.PreviousButton.clicked -= Previous;
            _view.NextButton.clicked -= Next;
            _view.EquipButton.clicked -= Equip;
            _view.UpgradeButton.clicked -= Upgrade;
            _view.ShipsTab.clicked -= ShowShips;
            _view.SkinsTab.clicked -= ShowSkins;
            _view.TrailsTab.clicked -= ShowTrails;
            _view.CoreFxTab.clicked -= ShowCoreFx;
            if (_context.Profile != null)
                _context.Profile.Changed -= Refresh;
        }

        public override void Refresh()
        {
            _view.SetSection(_section);
            var items = CurrentItems();
            if (items.Count == 0 || _context.Profile == null)
            {
                _selection = null;
                _view.SelectionTitle.text = "NO EQUIPMENT";
                _view.SelectionStatus.text = "NOT CONFIGURED";
                _view.EquipButton.SetEnabled(false);
                _view.UpgradeButton.SetEnabled(false);
                _view.List.Clear();
                _view.List.Add(UiDynamicElements.EmptyState("Hangar content is not configured."));
                return;
            }

            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, items.Count - 1);
            _selection = items[_selectedIndex];
            RenderSelection(items);
            RenderList(items);
        }

        private void RenderSelection(IReadOnlyList<UnlockableDefinition> items)
        {
            var profile = _context.Profile;
            var unlocked = profile.State.Inventory.IsUnlocked(_selection.Id);
            var selected = IsSelected(_selection.Id);
            _view.SelectionTitle.text = (_selection.DisplayName ?? _selection.name).ToUpperInvariant();
            _view.SelectionStatus.text = selected ? "EQUIPPED" : unlocked ? "UNLOCKED" : "LOCKED";
            _view.SelectionStatus.EnableInClassList(UiClassNames.Success, selected);
            _view.SelectionStatus.EnableInClassList(UiClassNames.Locked, !unlocked);
            _view.EquipButton.text = selected ? "EQUIPPED" : unlocked ? "EQUIP" : "LOCKED";
            _view.EquipButton.SetEnabled(unlocked && !selected);
            UiVisibility.SetAvailable(_view.PreviousButton, _selectedIndex > 0, true);
            UiVisibility.SetAvailable(_view.NextButton, _selectedIndex < items.Count - 1, true);

            if (_selection.Icon != null)
            {
                _view.Preview.style.backgroundImage = new StyleBackground(_selection.Icon);
                _view.Preview.AddToClassList("has-image");
            }
            else
            {
                _view.Preview.style.backgroundImage = StyleKeyword.None;
                _view.Preview.RemoveFromClassList("has-image");
            }

            var ship = _selection as ShipDefinition ?? _context.ShipDatabase?.GetShip(profile.State.SelectedShipId);
            RenderStats(ship);
            RenderUpgrade();
        }

        private void RenderStats(ShipDefinition ship)
        {
            var stats = ship != null ? ship.BaseStats : default;
            SetStat(_view.Speed, stats.Speed, "SPEED");
            SetStat(_view.Handling, stats.Handling, "HANDLING");
            SetStat(_view.Shield, stats.Stability, "SHIELD");
            SetStat(_view.Boost, stats.Boost, "BOOST");
        }

        private void RenderUpgrade()
        {
            var upgrades = _context.ShipDatabase?.Upgrades;
            if (_section != "ships" || upgrades == null || upgrades.Count == 0 || upgrades[0] == null)
            {
                _view.UpgradeButton.text = "UPGRADES IN LAB";
                _view.UpgradeButton.SetEnabled(false);
                return;
            }

            var upgrade = upgrades[0];
            var level = _context.Profile.GetUpgradeLevel(_context.Profile.State.ShipUpgradeLevels, upgrade.UpgradeType.ToString());
            if (level >= upgrade.MaxLevel)
            {
                _view.UpgradeButton.text = "MAX LEVEL";
                _view.UpgradeButton.SetEnabled(false);
                return;
            }

            _view.UpgradeButton.text = $"UPGRADE  {upgrade.GetCostForLevel(level):N0}";
            _view.UpgradeButton.SetEnabled(true);
        }

        private void RenderList(IReadOnlyList<UnlockableDefinition> items)
        {
            _view.List.Clear();
            for (var i = 0; i < items.Count; i++)
            {
                var index = i;
                var item = items[i];
                var tile = new Button(() => Select(index));
                tile.AddToClassList("hangar-thumbnail");
                tile.EnableInClassList(UiClassNames.Selected, i == _selectedIndex);
                tile.EnableInClassList(UiClassNames.Locked, !_context.Profile.State.Inventory.IsUnlocked(item.Id));
                var icon = UiDynamicElements.CreateIcon(item.Icon, "▲", "hangar-thumbnail__icon");
                var label = new Label(item.DisplayName ?? item.name);
                label.AddToClassList("hangar-thumbnail__label");
                tile.Add(icon);
                tile.Add(label);
                _view.List.Add(tile);
            }
        }

        private IReadOnlyList<UnlockableDefinition> CurrentItems()
        {
            var result = new List<UnlockableDefinition>();
            var database = _context.ShipDatabase;
            if (database == null)
                return result;

            if (_section == "ships") Add(result, database.Ships);
            else if (_section == "skins") Add(result, database.Skins);
            else if (_section == "trails") Add(result, database.Trails);
            else Add(result, database.CoreFx);
            return result;
        }

        private static void Add<T>(List<UnlockableDefinition> target, IList<T> source) where T : UnlockableDefinition
        {
            if (source == null)
                return;
            for (var i = 0; i < source.Count; i++)
                if (source[i] != null)
                    target.Add(source[i]);
        }

        private void Select(int index)
        {
            _selectedIndex = index;
            Refresh();
            _animations.ShowScreen(_view.Preview);
        }

        private void Previous() => Select(Mathf.Max(0, _selectedIndex - 1));
        private void Next() => Select(_selectedIndex + 1);

        private void Equip()
        {
            if (_selection == null || _context.Profile == null)
                return;
            if (!_context.Profile.State.Inventory.IsUnlocked(_selection.Id))
            {
                _toast.Show("This item is still locked.", true);
                return;
            }

            _context.Profile.Mutate(state =>
            {
                if (_section == "ships") state.SelectedShipId = _selection.Id;
                else if (_section == "skins") state.SelectedSkinId = _selection.Id;
                else if (_section == "trails") state.SelectedTrailId = _selection.Id;
                else state.SelectedCoreFxId = _selection.Id;
            });
            _toast.Show($"{_selection.DisplayName} equipped.");
            Refresh();
        }

        private void Upgrade()
        {
            var upgrades = _context.ShipDatabase?.Upgrades;
            if (_section != "ships" || upgrades == null || upgrades.Count == 0 || upgrades[0] == null || _context.Profile == null)
                return;

            var upgrade = upgrades[0];
            var id = upgrade.UpgradeType.ToString();
            var current = _context.Profile.GetUpgradeLevel(_context.Profile.State.ShipUpgradeLevels, id);
            if (current >= upgrade.MaxLevel)
                return;
            var cost = new CurrencyAmount(upgrade.Currency, upgrade.GetCostForLevel(current));
            var success = _context.Profile.TryMutate(state =>
            {
                if (!state.Wallet.TrySpend(cost))
                    return false;
                SetLevel(state.ShipUpgradeLevels, id, current + 1);
                return true;
            });
            if (!success)
            {
                _toast.Show("Not enough currency for this upgrade.", true);
                _animations.PlayInvalidAction(_view.UpgradeButton);
                return;
            }
            _toast.Show($"{upgrade.DisplayName} upgraded to level {current + 1}.");
            Refresh();
        }

        private bool IsSelected(string id)
        {
            var state = _context.Profile.State;
            if (_section == "ships") return state.SelectedShipId == id;
            if (_section == "skins") return state.SelectedSkinId == id;
            if (_section == "trails") return state.SelectedTrailId == id;
            return state.SelectedCoreFxId == id;
        }

        private static void SetStat(ProgressBar bar, float value, string title)
        {
            bar.lowValue = 0f;
            bar.highValue = 10f;
            bar.value = Mathf.Clamp(value, 0f, 10f);
            bar.title = $"{title}  {value:0.0}";
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

        private void SwitchSection(string section)
        {
            _section = section;
            _selectedIndex = 0;
            Refresh();
        }

        private void ShowShips() => SwitchSection("ships");
        private void ShowSkins() => SwitchSection("skins");
        private void ShowTrails() => SwitchSection("trails");
        private void ShowCoreFx() => SwitchSection("corefx");
    }
}
