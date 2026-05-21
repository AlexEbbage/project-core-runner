using System.Collections.Generic;
using CoreRacer.Bootstrap;
using CoreRacer.Localization;
using CoreRacer.Meta.Profile;
using CoreRacer.Meta.Ships;
using CoreRacer.UI.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.MainMenu
{
    public sealed class HangarPageController : UiView
    {
        private enum HangarSection
        {
            Ships,
            Skins,
            Trails,
            CoreFx,
            Upgrades
        }

        [SerializeField] private ShipDatabase shipDatabase;
        [SerializeField] private Button shipsButton;
        [SerializeField] private Button skinsButton;
        [SerializeField] private Button trailsButton;
        [SerializeField] private Button coreFxButton;
        [SerializeField] private Button upgradesButton;
        [SerializeField] private GameObject shipsPanel;
        [SerializeField] private GameObject skinsPanel;
        [SerializeField] private GameObject trailsPanel;
        [SerializeField] private GameObject coreFxPanel;
        [SerializeField] private GameObject upgradesPanel;
        [SerializeField] private Transform shipsRoot;
        [SerializeField] private Transform skinsRoot;
        [SerializeField] private Transform trailsRoot;
        [SerializeField] private Transform coreFxRoot;
        [SerializeField] private Transform upgradesRoot;
        [SerializeField] private HangarCosmeticItemView cosmeticPrefab;
        [SerializeField] private HangarUpgradeItemView upgradePrefab;
        [SerializeField] private HangarStatRowView statRowPrefab;
        [SerializeField] private Transform statsRoot;
        [SerializeField] private Text previewShipText;
        [SerializeField] private Text previewCosmeticsText;
        [SerializeField] private Text statusText;

        private readonly List<HangarCosmeticItemView> _shipRows = new List<HangarCosmeticItemView>();
        private readonly List<HangarCosmeticItemView> _skinRows = new List<HangarCosmeticItemView>();
        private readonly List<HangarCosmeticItemView> _trailRows = new List<HangarCosmeticItemView>();
        private readonly List<HangarCosmeticItemView> _coreFxRows = new List<HangarCosmeticItemView>();
        private readonly List<HangarUpgradeItemView> _upgradeRows = new List<HangarUpgradeItemView>();
        private readonly List<HangarStatRowView> _statRows = new List<HangarStatRowView>();
        private PlayerProfileService _profile;
        private LocalizationServiceV2 _localization;
        private HangarSection _currentSection;

        private void Awake()
        {
            GameServices.TryGet(out _profile);
            GameServices.TryGet(out _localization);
            if (shipsButton != null) shipsButton.onClick.AddListener(() => ShowSection(HangarSection.Ships));
            if (skinsButton != null) skinsButton.onClick.AddListener(() => ShowSection(HangarSection.Skins));
            if (trailsButton != null) trailsButton.onClick.AddListener(() => ShowSection(HangarSection.Trails));
            if (coreFxButton != null) coreFxButton.onClick.AddListener(() => ShowSection(HangarSection.CoreFx));
            if (upgradesButton != null) upgradesButton.onClick.AddListener(() => ShowSection(HangarSection.Upgrades));
        }

        public override void Show()
        {
            base.Show();
            Refresh();
            ShowSection(_currentSection);
        }

        public void EquipShip(string shipId)
        {
            if (_profile == null || !_profile.State.Inventory.IsUnlocked(shipId)) return;
            _profile.State.SelectedShipId = shipId;
            _profile.Save();
            Refresh();
        }

        public void Refresh()
        {
            if (_profile == null || shipDatabase == null)
                return;

            BindUnlockables(shipDatabase.Ships, shipsRoot, _shipRows, _profile.State.SelectedShipId, EquipShip);
            BindUnlockables(shipDatabase.Skins, skinsRoot, _skinRows, _profile.State.SelectedSkinId, EquipSkin);
            BindUnlockables(shipDatabase.Trails, trailsRoot, _trailRows, _profile.State.SelectedTrailId, EquipTrail);
            BindUnlockables(shipDatabase.CoreFx, coreFxRoot, _coreFxRows, _profile.State.SelectedCoreFxId, EquipCoreFx);
            BindUpgradeRows();
            BindStats();

            var selectedShip = shipDatabase.GetShip(_profile.State.SelectedShipId);
            if (previewShipText != null)
                previewShipText.text = selectedShip != null ? selectedShip.DisplayName : Localize("ui.hangar_no_ship");
            if (previewCosmeticsText != null)
                previewCosmeticsText.text = $"{ResolveName(shipDatabase.GetSkin(_profile.State.SelectedSkinId))} / {ResolveName(shipDatabase.GetTrail(_profile.State.SelectedTrailId))} / {ResolveName(shipDatabase.GetCoreFx(_profile.State.SelectedCoreFxId))}";
            if (statusText != null)
                statusText.text = "Ship upgrades remain disabled until the clean run runtime consumes them.";
        }

        private void BindUnlockables<T>(List<T> definitions, Transform root, List<HangarCosmeticItemView> rows, string selectedId, System.Action<string> onSelect) where T : UnlockableDefinition
        {
            if (definitions == null || root == null || cosmeticPrefab == null)
                return;

            while (rows.Count < definitions.Count)
            {
                var row = Instantiate(cosmeticPrefab, root);
                row.gameObject.SetActive(false);
                rows.Add(row);
            }

            for (int i = 0; i < rows.Count; i++)
            {
                var active = i < definitions.Count;
                rows[i].gameObject.SetActive(active);
                if (!active)
                    continue;

                var definition = definitions[i];
                var unlocked = definition != null && _profile.State.Inventory.IsUnlocked(definition.Id);
                var selected = definition != null && definition.Id == selectedId;
                var actionLabel = !unlocked
                    ? Localize("ui.hangar_action_locked")
                    : selected ? Localize("ui.hangar_action_equipped") : Localize("ui.hangar_action_equip");
                rows[i].Bind(definition, unlocked, selected, actionLabel, onSelect);
            }
        }

        private void BindUpgradeRows()
        {
            if (shipDatabase == null || upgradesRoot == null || upgradePrefab == null)
                return;

            while (_upgradeRows.Count < shipDatabase.Upgrades.Count)
            {
                var row = Instantiate(upgradePrefab, upgradesRoot);
                row.gameObject.SetActive(false);
                _upgradeRows.Add(row);
            }

            for (int i = 0; i < _upgradeRows.Count; i++)
            {
                var active = i < shipDatabase.Upgrades.Count;
                _upgradeRows[i].gameObject.SetActive(active);
                if (!active)
                    continue;

                var definition = shipDatabase.Upgrades[i];
                var level = _profile.GetUpgradeLevel(_profile.State.ShipUpgradeLevels, definition.UpgradeType.ToString());
                _upgradeRows[i].Bind(definition, level, "Runtime Pending", false, AttemptUpgrade);
            }
        }

        private void BindStats()
        {
            if (statsRoot == null || statRowPrefab == null || shipDatabase == null)
                return;

            var ship = shipDatabase.GetShip(_profile.State.SelectedShipId);
            var statTypes = new[]
            {
                ShipStatType.Speed,
                ShipStatType.Handling,
                ShipStatType.Stability,
                ShipStatType.Boost,
                ShipStatType.Energy
            };

            while (_statRows.Count < statTypes.Length)
            {
                var row = Instantiate(statRowPrefab, statsRoot);
                row.gameObject.SetActive(false);
                _statRows.Add(row);
            }

            for (int i = 0; i < _statRows.Count; i++)
            {
                var active = i < statTypes.Length;
                _statRows[i].gameObject.SetActive(active);
                if (!active)
                    continue;

                _statRows[i].Bind(statTypes[i], ship != null ? ship.BaseStats.GetValue(statTypes[i]) : 0f);
            }
        }

        private void EquipSkin(string id)
        {
            if (!CanEquip(id))
                return;
            _profile.State.SelectedSkinId = id;
            _profile.Save();
            Refresh();
        }

        private void EquipTrail(string id)
        {
            if (!CanEquip(id))
                return;
            _profile.State.SelectedTrailId = id;
            _profile.Save();
            Refresh();
        }

        private void EquipCoreFx(string id)
        {
            if (!CanEquip(id))
                return;
            _profile.State.SelectedCoreFxId = id;
            _profile.Save();
            Refresh();
        }

        private bool CanEquip(string id)
        {
            return _profile != null && _profile.State.Inventory.IsUnlocked(id);
        }

        private void AttemptUpgrade(UpgradeType upgradeType)
        {
            if (statusText != null)
                statusText.text = $"{upgradeType} upgrades are not connected to the clean run runtime yet.";
        }

        private void ShowSection(HangarSection section)
        {
            _currentSection = section;
            SetActive(shipsPanel, section == HangarSection.Ships);
            SetActive(skinsPanel, section == HangarSection.Skins);
            SetActive(trailsPanel, section == HangarSection.Trails);
            SetActive(coreFxPanel, section == HangarSection.CoreFx);
            SetActive(upgradesPanel, section == HangarSection.Upgrades);
        }

        private string ResolveName(UnlockableDefinition definition)
        {
            return definition != null ? definition.DisplayName : Localize("ui.hangar_none");
        }

        private string Localize(string key)
        {
            return _localization != null ? _localization.Get(key) : key;
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
                target.SetActive(active);
        }
    }
}
