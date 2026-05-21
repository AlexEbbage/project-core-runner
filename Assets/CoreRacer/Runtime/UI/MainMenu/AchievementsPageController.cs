using System.Collections.Generic;
using CoreRacer.Bootstrap;
using CoreRacer.Localization;
using CoreRacer.Meta.Achievements;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.MainMenu
{
    public sealed class AchievementsPageController : MonoBehaviour
    {
        [SerializeField] private Transform contentRoot;
        [SerializeField] private AchievementRowView rowPrefab;
        [SerializeField] private Text statusText;

        private readonly List<AchievementRowView> _rows = new List<AchievementRowView>();
        private AchievementService _service;
        private LocalizationServiceV2 _localization;

        private void OnEnable()
        {
            GameServices.TryGet(out _service);
            GameServices.TryGet(out _localization);
            Refresh();
        }

        public void Refresh()
        {
            var definitions = _service != null ? _service.Definitions : null;
            if (definitions == null || definitions.Count == 0 || rowPrefab == null || contentRoot == null)
            {
                if (statusText != null)
                    statusText.text = "Achievement content is not wired.";
                return;
            }

            EnsureRows(definitions.Count);
            for (int i = 0; i < _rows.Count; i++)
            {
                var active = i < definitions.Count;
                _rows[i].gameObject.SetActive(active);
                if (!active)
                    continue;

                var definition = definitions[i];
                var progress = Mathf.Min(_service.GetProgress(definition), definition.RequiredValue);
                var claimed = _service.IsClaimed(definition.Id);
                var actionLabel = claimed
                    ? Localize("ui.achievement_claimed")
                    : _service.IsComplete(definition) ? Localize("ui.achievement_claim") : Localize("ui.achievement_locked");
                _rows[i].Bind(
                    definition.Id,
                    definition.DisplayName,
                    definition.Description,
                    $"{progress:N0}/{definition.RequiredValue:N0}",
                    actionLabel,
                    _service.IsComplete(definition) && !claimed,
                    Claim);
            }

            if (statusText != null)
                statusText.text = "Achievements update from profile progress.";
        }

        private void EnsureRows(int count)
        {
            while (_rows.Count < count)
            {
                var row = Instantiate(rowPrefab, contentRoot);
                row.gameObject.SetActive(false);
                _rows.Add(row);
            }
        }

        private void Claim(string achievementId)
        {
            if (_service != null && _service.TryClaim(achievementId))
                Refresh();
        }

        private string Localize(string key)
        {
            return _localization != null ? _localization.Get(key) : key;
        }
    }
}
