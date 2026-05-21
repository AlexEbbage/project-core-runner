using System;
using CoreRacer.Meta.Ships;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.MainMenu
{
    public sealed class HangarUpgradeItemView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Text titleText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text costText;
        [SerializeField] private Text actionLabelText;
        [SerializeField] private Button upgradeButton;
        private UpgradeType _upgradeType;
        private Action<UpgradeType> _onUpgrade;

        public void Bind(ShipUpgradeDefinition definition, int currentLevel, string actionLabel, bool interactable, Action<UpgradeType> onUpgrade)
        {
            _upgradeType = definition != null ? definition.UpgradeType : default;
            _onUpgrade = onUpgrade;
            if (icon != null) icon.sprite = definition != null ? definition.Icon : null;
            if (titleText != null) titleText.text = definition != null ? definition.DisplayName : "Missing upgrade";
            if (levelText != null) levelText.text = definition != null ? $"Lv {currentLevel}/{definition.MaxLevel}" : string.Empty;
            if (costText != null) costText.text = definition != null && currentLevel < definition.MaxLevel ? $"{definition.GetCostForLevel(currentLevel):N0}" : "MAX";
            if (actionLabelText != null) actionLabelText.text = actionLabel ?? string.Empty;
            if (upgradeButton != null) upgradeButton.interactable = interactable && definition != null && currentLevel < definition.MaxLevel;
        }

        private void OnEnable()
        {
            if (upgradeButton != null) upgradeButton.onClick.AddListener(Upgrade);
        }

        private void OnDisable()
        {
            if (upgradeButton != null) upgradeButton.onClick.RemoveListener(Upgrade);
        }

        private void Upgrade() => _onUpgrade?.Invoke(_upgradeType);
    }
}
