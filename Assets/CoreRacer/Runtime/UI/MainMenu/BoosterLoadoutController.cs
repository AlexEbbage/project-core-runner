using System.Collections.Generic;
using CoreRacer.Bootstrap;
using CoreRacer.Meta.Boosters;
using CoreRacer.Meta.Profile;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.MainMenu
{
    public sealed class BoosterLoadoutController : MonoBehaviour
    {
        [SerializeField] private BoosterCatalog catalog;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private BoosterOptionView optionPrefab;
        [SerializeField] private Text summaryText;

        private readonly List<BoosterOptionView> _options = new List<BoosterOptionView>();
        private PlayerProfileService _profile;
        private BoosterLoadoutService _loadout;

        public int VisibleOptionCount => _options.Count;
        public BoosterCatalog Catalog => catalog;

        private void Awake()
        {
            ResolveDependencies();
        }

        private void OnEnable()
        {
            ResolveDependencies();
            if (_profile != null)
                _profile.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (_profile != null)
                _profile.Changed -= Refresh;
        }

        public void Refresh()
        {
            ResolveDependencies();
            if (catalog == null || catalog.Boosters == null || contentRoot == null || optionPrefab == null)
            {
                if (summaryText != null)
                    summaryText.text = "Boosters unavailable";
                return;
            }

            EnsureOptions(catalog.Boosters.Count);
            var equippedCount = 0;
            for (var i = 0; i < _options.Count; i++)
            {
                var active = i < catalog.Boosters.Count;
                _options[i].gameObject.SetActive(active);
                if (!active)
                    continue;

                var booster = catalog.Boosters[i];
                var equipped = booster != null && _loadout != null && _loadout.IsEquipped(booster.Id);
                if (equipped)
                    equippedCount++;
                _options[i].Bind(booster, equipped, Toggle);
            }

            if (summaryText != null)
                summaryText.text = $"{equippedCount} equipped • one choice per family";
        }

        private void Toggle(string boosterId)
        {
            if (_loadout == null || !_loadout.TryToggle(boosterId))
            {
                Debug.LogError($"[CoreRacer.Boosters] Could not toggle booster '{boosterId}'.", this);
                return;
            }

            Refresh();
        }

        private void EnsureOptions(int count)
        {
            while (_options.Count < count)
            {
                var option = Instantiate(optionPrefab, contentRoot);
                option.gameObject.SetActive(false);
                _options.Add(option);
            }
        }

        private void ResolveDependencies()
        {
            if (_profile == null)
                GameServices.TryGet(out _profile);
            if (_loadout == null && _profile != null && catalog != null)
                _loadout = new BoosterLoadoutService(_profile, catalog);
        }
    }
}
