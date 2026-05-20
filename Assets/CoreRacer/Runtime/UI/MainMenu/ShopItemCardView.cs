using System;
using CoreRacer.Meta.Shop;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.MainMenu
{
    public sealed class ShopItemCardView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text priceText;
        [SerializeField] private GameObject featuredBadge;
        [SerializeField] private Button buyButton;

        private string _itemId;
        private Action<string> _onBuy;

        public void Bind(ShopItemDefinition item, Action<string> onBuy)
        {
            _itemId = item != null ? item.Id : string.Empty;
            _onBuy = onBuy;
            if (icon != null) icon.sprite = item != null ? item.Icon : null;
            if (titleText != null) titleText.text = item != null ? item.DisplayName : "Missing item";
            if (descriptionText != null) descriptionText.text = item != null ? item.Description : string.Empty;
            if (priceText != null) priceText.text = item != null ? FormatPrice(item) : string.Empty;
            if (featuredBadge != null) featuredBadge.SetActive(item != null && item.IsFeatured);
        }

        private void OnEnable()
        {
            if (buyButton != null) buyButton.onClick.AddListener(HandleBuy);
        }

        private void OnDisable()
        {
            if (buyButton != null) buyButton.onClick.RemoveListener(HandleBuy);
        }

        private void HandleBuy()
        {
            if (!string.IsNullOrWhiteSpace(_itemId))
                _onBuy?.Invoke(_itemId);
        }

        private string FormatPrice(ShopItemDefinition item)
        {
            if (item.Kind == ShopItemKind.PremiumUser) return "Premium";
            if (item.Kind == ShopItemKind.RestorePurchases) return "Restore";
            if (item.Price.Amount <= 0) return "Free";
            return $"{item.Price.Amount:N0} {item.Price.Type}";
        }
    }
}
