using System;
using CoreRacer.Meta.Shop;
using CoreRacer.UI.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.MainMenu
{
    public sealed class ShopItemDetailsModal : UiView
    {
        [SerializeField] private Image icon;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text priceText;
        [SerializeField] private Button buyButton;
        [SerializeField] private Button closeButton;
        private string _itemId;
        private Action<string> _onBuy;

        private void OnEnable()
        {
            if (buyButton != null) buyButton.onClick.AddListener(Buy);
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
        }

        private void OnDisable()
        {
            if (buyButton != null) buyButton.onClick.RemoveListener(Buy);
            if (closeButton != null) closeButton.onClick.RemoveListener(Hide);
        }

        public void Open(ShopItemDefinition item, Action<string> onBuy)
        {
            _itemId = item != null ? item.Id : string.Empty;
            _onBuy = onBuy;
            if (icon != null) icon.sprite = item != null ? item.Icon : null;
            if (titleText != null) titleText.text = item != null ? item.DisplayName : "Missing item";
            if (descriptionText != null) descriptionText.text = item != null ? item.Description : string.Empty;
            if (priceText != null) priceText.text = item != null && item.Price.Amount > 0 ? $"{item.Price.Amount:N0} {item.Price.Type}" : "Free";
            Show();
        }

        private void Buy()
        {
            if (!string.IsNullOrWhiteSpace(_itemId))
                _onBuy?.Invoke(_itemId);
            Hide();
        }
    }
}
