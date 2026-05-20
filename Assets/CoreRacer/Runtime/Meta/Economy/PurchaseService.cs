using CoreRacer.Meta.Profile;

namespace CoreRacer.Meta.Economy
{
    public enum PurchaseFailureReason
    {
        None,
        AlreadyOwned,
        InsufficientCurrency,
        InvalidItem
    }

    public struct PurchaseResult
    {
        public bool Success;
        public PurchaseFailureReason FailureReason;
        public string ItemId;

        public static PurchaseResult Ok(string itemId) => new PurchaseResult { Success = true, ItemId = itemId };
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

            if (_profile.State.Inventory.IsUnlocked(itemId))
                return PurchaseResult.Fail(itemId, PurchaseFailureReason.AlreadyOwned);

            if (!_profile.State.Wallet.CanSpend(price))
                return PurchaseResult.Fail(itemId, PurchaseFailureReason.InsufficientCurrency);

            _profile.TrySpend(price);
            _rewards.Grant(RewardGrant.Unlock(itemId));
            return PurchaseResult.Ok(itemId);
        }
    }
}
