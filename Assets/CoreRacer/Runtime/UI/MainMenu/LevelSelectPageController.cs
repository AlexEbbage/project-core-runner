using System;
using System.Collections.Generic;
using CoreRacer.Bootstrap;
using CoreRacer.Gameplay.Run;
using CoreRacer.Localization;
using CoreRacer.Meta.Levels;
using CoreRacer.Meta.Profile;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.MainMenu
{
    public sealed class LevelSelectPageController : MonoBehaviour
    {
        [SerializeField] private LevelRoadmapConfigV2 roadmap;
        [SerializeField] private RunController runController;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private LevelSelectCardView cardPrefab;
        [SerializeField] private Text selectedTitleText;
        [SerializeField] private Text selectedDescriptionText;
        [SerializeField] private Text selectedStatusText;
        [SerializeField] private Text playButtonLabelText;
        [SerializeField] private Button playButton;

        private readonly List<LevelSelectCardView> _cards = new List<LevelSelectCardView>();
        private PlayerProfileService _profile;
        private LocalizationServiceV2 _localization;
        private int _selectedIndex;

        private void Awake()
        {
            GameServices.TryGet(out _profile);
            GameServices.TryGet(out _localization);
        }

        private void OnEnable()
        {
            if (playButton != null)
                playButton.onClick.AddListener(PlaySelected);
            Refresh();
        }

        private void OnDisable()
        {
            if (playButton != null)
                playButton.onClick.RemoveListener(PlaySelected);
        }

        public void Refresh()
        {
            if (roadmap == null || roadmap.Levels == null || roadmap.Levels.Count == 0)
            {
                SetText(selectedTitleText, Localize("ui.no_levels"));
                SetText(selectedDescriptionText, Localize("ui.level_select_empty"));
                SetText(selectedStatusText, string.Empty);
                SetText(playButtonLabelText, "Play");
                if (playButton != null) playButton.interactable = false;
                return;
            }

            _selectedIndex = Mathf.Clamp(_profile != null ? _profile.State.SelectedLevelIndex : 0, 0, roadmap.Levels.Count - 1);
            var selected = roadmap.Levels[_selectedIndex];
            EnsureCards(roadmap.Levels.Count);

            for (int i = 0; i < _cards.Count; i++)
            {
                var active = i < roadmap.Levels.Count;
                _cards[i].gameObject.SetActive(active);
                if (!active)
                    continue;

                var level = roadmap.Levels[i];
                var unlocked = IsUnlocked(level);
                var isSelected = i == _selectedIndex;
                var status = unlocked
                    ? string.Format(Localize("ui.level_select_unlocked"), level.RequiredPlayerLevel)
                    : string.Format(Localize("ui.level_select_locked"), level.RequiredPlayerLevel);

                _cards[i].Bind(
                    level.Id,
                    level.DisplayName,
                    string.Format(Localize("ui.level_select_description"), level.TunnelSides),
                    status,
                    isSelected ? Localize("ui.level_select_selected") : (unlocked ? Localize("ui.level_select_select") : Localize("ui.level_select_locked_short")),
                    unlocked,
                    isSelected,
                    HandleSelected);
            }

            SyncRunController(selected);
            SetText(selectedTitleText, selected.DisplayName);
            SetText(selectedDescriptionText, selected.Description);
            SetText(selectedStatusText, IsUnlocked(selected)
                ? string.Format(Localize("ui.level_select_unlocked"), selected.RequiredPlayerLevel)
                : string.Format(Localize("ui.level_select_locked"), selected.RequiredPlayerLevel));
            SetText(playButtonLabelText, "Play");
            if (playButton != null)
                playButton.interactable = IsUnlocked(selected);
        }

        private void EnsureCards(int count)
        {
            if (contentRoot == null || cardPrefab == null)
                return;

            while (_cards.Count < count)
            {
                var card = Instantiate(cardPrefab, contentRoot);
                card.gameObject.SetActive(false);
                _cards.Add(card);
            }
        }

        private void HandleSelected(string levelId)
        {
            if (roadmap == null || roadmap.Levels == null)
                return;

            for (int i = 0; i < roadmap.Levels.Count; i++)
            {
                var level = roadmap.Levels[i];
                if (level == null || level.Id != levelId)
                    continue;

                if (!IsUnlocked(level))
                    return;

                _selectedIndex = i;
                if (_profile != null)
                {
                    _profile.State.SelectedLevelIndex = i;
                    _profile.Save();
                }

                SyncRunController(level);
                Refresh();
                return;
            }
        }

        private void PlaySelected()
        {
            if (roadmap == null || _selectedIndex < 0 || _selectedIndex >= roadmap.Levels.Count)
                return;

            var level = roadmap.Levels[_selectedIndex];
            if (!IsUnlocked(level))
                return;

            SyncRunController(level);
            runController?.StartRun();
        }

        private void SyncRunController(LevelDefinition level)
        {
            if (level != null)
                runController?.SetSelectedLevelId(level.Id);
        }

        private bool IsUnlocked(LevelDefinition level)
        {
            if (level == null)
                return false;
            return _profile == null || _profile.State.Level >= Mathf.Max(1, level.RequiredPlayerLevel);
        }

        private string Localize(string key)
        {
            return _localization != null ? _localization.Get(key) : key;
        }

        private static void SetText(Text target, string value)
        {
            if (target != null)
                target.text = value ?? string.Empty;
        }
    }
}
