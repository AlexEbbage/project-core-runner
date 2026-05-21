using CoreRacer.Bootstrap;
using CoreRacer.Localization;
using CoreRacer.Meta.Economy;
using CoreRacer.Meta.Profile;
using CoreRacer.Meta.Shop;
using CoreRacer.UI.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.MainMenu
{
    public sealed class ShopPageController : UiView
    {
        [SerializeField] private ShopCatalog fallbackCatalog;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private ShopItemCardView cardPrefab;
        [SerializeField] private ShopItemDetailsModal detailsModal;
        [SerializeField] private Text statusText;

        private readonly System.Collections.Generic.List<ShopItemCardView> _cards = new System.Collections.Generic.List<ShopItemCardView>();
        private ShopService _shopService;
        private PurchaseService _purchaseService;
        private PlayerProfileService _profile;
        private LocalizationServiceV2 _localization;

        private void Awake()
        {
            GameServices.TryGet(out _shopService);
            GameServices.TryGet(out _purchaseService);
            GameServices.TryGet(out _profile);
            GameServices.TryGet(out _localization);
        }

        public override void Show()
        {
            base.Show();
            Refresh();
        }

        public void Buy(string itemId)
        {
            if (_shopService != null)
            {
                var result = _shopService.TryPurchase(itemId);
                if (statusText != null)
                    statusText.text = result.Success ? $"Purchased {itemId}." : $"Purchase failed: {result.FailureReason}.";
                Refresh();
                return;
            }

            Debug.LogWarning("ShopService is not registered. Ensure GameBootstrapper is present.");
        }

        public void BuyUnlockFallback(string itemId, int softPrice)
        {
            if (_purchaseService == null) return;
            var result = _purchaseService.TryPurchaseUnlock(itemId, new CurrencyAmount(CurrencyType.Soft, softPrice));
            if (statusText != null)
                statusText.text = result.Success ? $"Purchased {itemId}." : $"Purchase failed: {result.FailureReason}.";
            Refresh();
        }

        public void Refresh()
        {
            var catalog = fallbackCatalog;
            if (catalog == null || contentRoot == null || cardPrefab == null)
                return;

            EnsureCards(catalog.Items.Count);
            for (int i = 0; i < _cards.Count; i++)
            {
                var active = i < catalog.Items.Count;
                _cards[i].gameObject.SetActive(active);
                if (!active)
                    continue;

                var item = catalog.Items[i];
                var actionLabel = ResolveCardActionLabel(item);
                var itemStatus = ResolveStatus(item);
                _cards[i].Bind(item, itemStatus, actionLabel, OpenDetails);
            }
        }

        private void EnsureCards(int count)
        {
            while (_cards.Count < count)
            {
                var card = Instantiate(cardPrefab, contentRoot);
                card.gameObject.SetActive(false);
                _cards.Add(card);
            }
        }

        private void OpenDetails(string itemId)
        {
            var item = fallbackCatalog != null ? fallbackCatalog.Get(itemId) : null;
            if (item == null || detailsModal == null)
                return;

            var actionLabel = ResolveModalActionLabel(item);
            var interactable = IsPurchaseActionAvailable(item);
            detailsModal.Open(item, ResolveStatus(item), actionLabel, interactable, Buy);
        }

        private bool IsPurchaseActionAvailable(ShopItemDefinition item)
        {
            if (item == null)
                return false;

            if (item.Kind == ShopItemKind.RestorePurchases || item.Kind == ShopItemKind.PremiumUser)
                return true;

            if (_profile == null)
                return true;

            var ownedId = string.IsNullOrWhiteSpace(item.GrantItemId) ? item.Id : item.GrantItemId;
            if (_profile.State.Inventory.IsUnlocked(ownedId))
                return false;

            return _profile.State.Wallet.CanSpend(item.Price);
        }

        private string ResolveCardActionLabel(ShopItemDefinition item)
        {
            if (item == null)
                return Localize("ui.shop_action_default");
            if (item.Kind == ShopItemKind.PremiumUser)
                return Localize("ui.shop_action_remove_ads");
            if (item.Kind == ShopItemKind.RestorePurchases)
                return Localize("ui.shop_action_restore");
            if (_profile != null)
            {
                var ownedId = string.IsNullOrWhiteSpace(item.GrantItemId) ? item.Id : item.GrantItemId;
                if (_profile.State.Inventory.IsUnlocked(ownedId))
                    return Localize("ui.shop_action_owned");
                if (!_profile.State.Wallet.CanSpend(item.Price))
                    return Localize("ui.shop_action_need_more");
            }
            return Localize("ui.shop_action_buy");
        }

        private string ResolveModalActionLabel(ShopItemDefinition item)
        {
            return ResolveCardActionLabel(item);
        }

        private string ResolveStatus(ShopItemDefinition item)
        {
            if (item == null)
                return string.Empty;
            if (item.Kind == ShopItemKind.PremiumUser)
                return Localize("ui.shop_status_remove_ads");
            if (item.Kind == ShopItemKind.RestorePurchases)
                return Localize("ui.shop_status_restore");
            if (_profile == null)
                return Localize("ui.shop_status_ready");

            var ownedId = string.IsNullOrWhiteSpace(item.GrantItemId) ? item.Id : item.GrantItemId;
            if (_profile.State.Inventory.IsUnlocked(ownedId))
                return Localize("ui.shop_status_owned");

            if (!_profile.State.Wallet.CanSpend(item.Price))
            {
                return item.Price.Type == CurrencyType.Premium
                    ? Localize("ui.shop_status_need_premium")
                    : Localize("ui.shop_status_need_soft");
            }

            return Localize("ui.shop_status_ready");
        }

        private string Localize(string key)
        {
            return _localization != null ? _localization.Get(key) : key;
        }
    }
}
