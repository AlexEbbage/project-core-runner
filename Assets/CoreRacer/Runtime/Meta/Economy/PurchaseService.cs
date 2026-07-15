using CoreRacer.Meta.Profile;

namespace CoreRacer.Meta.Economy
{
    public enum PurchaseFailureReason
    {
        None,
        AlreadyOwned,
        InsufficientCurrency,
        InvalidItem,
        Pending,
        StoreUnavailable
    }

    public struct PurchaseResult
    {
        public bool Success;
        public bool IsPending;
        public PurchaseFailureReason FailureReason;
        public string ItemId;

        public static PurchaseResult Ok(string itemId) => new PurchaseResult { Success = true, ItemId = itemId };
        public static PurchaseResult Pending(string itemId) => new PurchaseResult { IsPending = true, ItemId = itemId, FailureReason = PurchaseFailureReason.Pending };
        public static PurchaseResult Fail(string itemId, PurchaseFailureReason reason) => new PurchaseResult { Success = false, ItemId = itemId, FailureReason = reason };
    }

    public sealed class PurchaseService
    {
        private readonly PlayerProfileService _profile;
        private readonly RewardGrantService _rewards;

        public PurchaseService(PlayerProfileService profile, RewardGrantService rewards)
        {
            _profile = profile;
            _rewards = rewards;
        }

        public PurchaseResult TryPurchaseUnlock(string itemId, CurrencyAmount price)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return PurchaseResult.Fail(itemId, PurchaseFailureReason.InvalidItem);

            var failure = PurchaseFailureReason.None;
            var committed = _profile.TryMutate(state =>
            {
                if (state.Inventory.IsUnlocked(itemId))
                {
                    failure = PurchaseFailureReason.AlreadyOwned;
                    return false;
                }

                if (!state.Wallet.TrySpend(price))
                {
                    failure = PurchaseFailureReason.InsufficientCurrency;
                    return false;
                }

                _rewards.ApplyToState(state, RewardGrant.Unlock(itemId));
                return true;
            });

            return committed ? PurchaseResult.Ok(itemId) : PurchaseResult.Fail(itemId, failure);
        }
    }
}
