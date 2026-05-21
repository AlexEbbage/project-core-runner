using System;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.MainMenu
{
    public sealed class LevelSelectCardView : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text actionLabelText;
        [SerializeField] private GameObject selectedBadge;
        [SerializeField] private Button selectButton;

        private string _levelId;
        private Action<string> _onSelected;

        private void OnEnable()
        {
            if (selectButton != null)
                selectButton.onClick.AddListener(Select);
        }

        private void OnDisable()
        {
            if (selectButton != null)
                selectButton.onClick.RemoveListener(Select);
        }

        public void Bind(string levelId, string title, string description, string status, string actionLabel, bool interactable, bool selected, Action<string> onSelected)
        {
            _levelId = levelId ?? string.Empty;
            _onSelected = onSelected;

            if (titleText != null) titleText.text = title ?? string.Empty;
            if (descriptionText != null) descriptionText.text = description ?? string.Empty;
            if (statusText != null) statusText.text = status ?? string.Empty;
            if (actionLabelText != null) actionLabelText.text = actionLabel ?? string.Empty;
            if (selectedBadge != null) selectedBadge.SetActive(selected);
            if (selectButton != null) selectButton.interactable = interactable;
        }

        private void Select()
        {
            if (!string.IsNullOrWhiteSpace(_levelId))
                _onSelected?.Invoke(_levelId);
        }
    }
}
