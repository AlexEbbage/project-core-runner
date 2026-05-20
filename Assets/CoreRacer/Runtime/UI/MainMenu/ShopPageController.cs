using CoreRacer.Bootstrap;
using CoreRacer.Meta.Economy;
using CoreRacer.Meta.Shop;
using CoreRacer.UI.Shared;
using UnityEngine;

namespace CoreRacer.UI.MainMenu
{
    public sealed class ShopPageController : UiView
    {
        [SerializeField] private ShopCatalog fallbackCatalog;
        private ShopService _shopService;
        private PurchaseService _purchaseService;

        private void Awake()
        {
            GameServices.TryGet(out _shopService);
            GameServices.TryGet(out _purchaseService);
        }

        public void Buy(string itemId)
        {
            if (_shopService != null)
            {
                var result = _shopService.TryPurchase(itemId);
                Debug.Log(result.Success ? $"Purchased {itemId}" : $"Purchase failed {itemId}: {result.FailureReason}");
                return;
            }

            Debug.LogWarning("ShopService is not registered. Ensure GameBootstrapper is present.");
        }

        public void BuyUnlockFallback(string itemId, int softPrice)
        {
            if (_purchaseService == null) return;
            var result = _purchaseService.TryPurchaseUnlock(itemId, new CurrencyAmount(CurrencyType.Soft, softPrice));
            Debug.Log(result.Success ? $"Purchased {itemId}" : $"Purchase failed {itemId}: {result.FailureReason}");
        }
    }
}
