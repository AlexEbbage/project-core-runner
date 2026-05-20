using System;
using CoreRacer.Meta.Economy;
using CoreRacer.Meta.Profile;
using CoreRacer.Monetisation.Iap;
using CoreRacer.Monetisation.Premium;

namespace CoreRacer.Meta.Shop
{
    public sealed class ShopService
    {
        private readonly ShopCatalog _catalog;
        private readonly PlayerProfileService _profile;
        private readonly RewardGrantService _rewards;
        private readonly PremiumEntitlementService _premium;
        private readonly IapPurchaseService _iap;

        public ShopService(ShopCatalog catalog, PlayerProfileService profile, RewardGrantService rewards, PremiumEntitlementService premium, IapPurchaseService iap)
        {
            _catalog = catalog;
            _profile = profile;
            _rewards = rewards;
            _premium = premium;
            _iap = iap;
        }

        public PurchaseResult TryPurchase(string itemId)
        {
            var item = _catalog != null ? _catalog.Get(itemId) : null;
            if (item == null)
                return PurchaseResult.Fail(itemId, PurchaseFailureReason.InvalidItem);

            switch (item.Kind)
            {
                case ShopItemKind.Unlock:
                    return PurchaseUnlock(item);
                case ShopItemKind.CurrencyPack:
                    _rewards.Grant(RewardGrant.Currency(item.CurrencyGrant));
                    return PurchaseResult.Ok(item.Id);
                case ShopItemKind.PremiumUser:
                    _iap.BuyPremium();
                    return PurchaseResult.Ok(item.Id);
                case ShopItemKind.RestorePurchases:
                    _iap.RestorePurchases();
                    return PurchaseResult.Ok(item.Id);
                default:
                    return PurchaseResult.Fail(itemId, PurchaseFailureReason.InvalidItem);
            }
        }

        private PurchaseResult PurchaseUnlock(ShopItemDefinition item)
        {
            var grantId = string.IsNullOrWhiteSpace(item.GrantItemId) ? item.Id : item.GrantItemId;
            if (_profile.State.Inventory.IsUnlocked(grantId))
                return PurchaseResult.Fail(item.Id, PurchaseFailureReason.AlreadyOwned);

            if (!_profile.State.Wallet.CanSpend(item.Price))
                return PurchaseResult.Fail(item.Id, PurchaseFailureReason.InsufficientCurrency);

            _profile.TrySpend(item.Price);
            _rewards.Grant(RewardGrant.Unlock(grantId));
            return PurchaseResult.Ok(item.Id);
        }
    }
}
