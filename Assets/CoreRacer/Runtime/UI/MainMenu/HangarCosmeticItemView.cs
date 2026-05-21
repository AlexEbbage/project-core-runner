using System;
using CoreRacer.Meta.Ships;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.MainMenu
{
    public sealed class HangarCosmeticItemView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Text titleText;
        [SerializeField] private Text actionLabelText;
        [SerializeField] private GameObject selectedBadge;
        [SerializeField] private GameObject lockedBadge;
        [SerializeField] private Button selectButton;
        private string _id;
        private Action<string> _onSelected;

        public void Bind(UnlockableDefinition definition, bool unlocked, bool selected, string actionLabel, Action<string> onSelected)
        {
            _id = definition != null ? definition.Id : string.Empty;
            _onSelected = onSelected;
            if (icon != null) icon.sprite = definition != null ? definition.Icon : null;
            if (titleText != null) titleText.text = definition != null ? definition.DisplayName : "Missing";
            if (actionLabelText != null) actionLabelText.text = actionLabel ?? string.Empty;
            if (selectedBadge != null) selectedBadge.SetActive(selected);
            if (lockedBadge != null) lockedBadge.SetActive(!unlocked);
            if (selectButton != null) selectButton.interactable = unlocked;
        }

        private void OnEnable()
        {
            if (selectButton != null) selectButton.onClick.AddListener(Select);
        }

        private void OnDisable()
        {
            if (selectButton != null) selectButton.onClick.RemoveListener(Select);
        }

        private void Select()
        {
            if (!string.IsNullOrWhiteSpace(_id))
                _onSelected?.Invoke(_id);
        }
    }
}
