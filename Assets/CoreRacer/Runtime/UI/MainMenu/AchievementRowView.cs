using System;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.MainMenu
{
    public sealed class AchievementRowView : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text progressText;
        [SerializeField] private Text actionLabelText;
        [SerializeField] private Button claimButton;

        private string _achievementId;
        private Action<string> _onClaim;

        private void OnEnable()
        {
            if (claimButton != null)
                claimButton.onClick.AddListener(Claim);
        }

        private void OnDisable()
        {
            if (claimButton != null)
                claimButton.onClick.RemoveListener(Claim);
        }

        public void Bind(string achievementId, string title, string description, string progress, string actionLabel, bool interactable, Action<string> onClaim)
        {
            _achievementId = achievementId ?? string.Empty;
            _onClaim = onClaim;
            if (titleText != null) titleText.text = title ?? string.Empty;
            if (descriptionText != null) descriptionText.text = description ?? string.Empty;
            if (progressText != null) progressText.text = progress ?? string.Empty;
            if (actionLabelText != null) actionLabelText.text = actionLabel ?? string.Empty;
            if (claimButton != null) claimButton.interactable = interactable;
        }

        private void Claim()
        {
            if (!string.IsNullOrWhiteSpace(_achievementId))
                _onClaim?.Invoke(_achievementId);
        }
    }
}
