using System;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.MainMenu
{
    public sealed class LabUpgradeItemView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Text titleText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text costText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text actionLabelText;
        [SerializeField] private Button upgradeButton;

        private string _upgradeId;
        private Action<string> _onUpgrade;

        private void OnEnable()
        {
            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(Upgrade);
        }

        private void OnDisable()
        {
            if (upgradeButton != null)
                upgradeButton.onClick.RemoveListener(Upgrade);
        }

        public void Bind(string upgradeId, Sprite iconSprite, string title, string level, string cost, string status, string actionLabel, bool interactable, Action<string> onUpgrade)
        {
            _upgradeId = upgradeId ?? string.Empty;
            _onUpgrade = onUpgrade;
            if (icon != null) icon.sprite = iconSprite;
            if (titleText != null) titleText.text = title ?? string.Empty;
            if (levelText != null) levelText.text = level ?? string.Empty;
            if (costText != null) costText.text = cost ?? string.Empty;
            if (statusText != null) statusText.text = status ?? string.Empty;
            if (actionLabelText != null) actionLabelText.text = actionLabel ?? string.Empty;
            if (upgradeButton != null) upgradeButton.interactable = interactable;
        }

        private void Upgrade()
        {
            if (!string.IsNullOrWhiteSpace(_upgradeId))
                _onUpgrade?.Invoke(_upgradeId);
        }
    }
}
