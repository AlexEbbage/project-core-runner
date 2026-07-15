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
                    // Currency packs must be fulfilled by a validated store receipt. Never grant catalog currency directly.
                    return PurchaseResult.Fail(item.Id, PurchaseFailureReason.StoreUnavailable);
                case ShopItemKind.PremiumUser:
                    if (_premium != null && _premium.HasPremium)
                        return PurchaseResult.Fail(item.Id, PurchaseFailureReason.AlreadyOwned);
                    if (_iap == null || !_iap.TryBuyPremium())
                        return PurchaseResult.Fail(item.Id, PurchaseFailureReason.StoreUnavailable);
                    return PurchaseResult.Pending(item.Id);
                case ShopItemKind.RestorePurchases:
                    if (_iap == null || !_iap.TryRestorePurchases())
                        return PurchaseResult.Fail(item.Id, PurchaseFailureReason.StoreUnavailable);
                    return PurchaseResult.Pending(item.Id);
                default:
                    return PurchaseResult.Fail(itemId, PurchaseFailureReason.InvalidItem);
            }
        }

        private PurchaseResult PurchaseUnlock(ShopItemDefinition item)
        {
            var grantId = string.IsNullOrWhiteSpace(item.GrantItemId) ? item.Id : item.GrantItemId;
            var failure = PurchaseFailureReason.None;
            var committed = _profile.TryMutate(state =>
            {
                if (state.Inventory.IsUnlocked(grantId))
                {
                    failure = PurchaseFailureReason.AlreadyOwned;
                    return false;
                }

                if (!state.Wallet.TrySpend(item.Price))
                {
                    failure = PurchaseFailureReason.InsufficientCurrency;
                    return false;
                }

                _rewards.ApplyToState(state, RewardGrant.Unlock(grantId));
                return true;
            });

            return committed ? PurchaseResult.Ok(item.Id) : PurchaseResult.Fail(item.Id, failure);
        }
    }
}
