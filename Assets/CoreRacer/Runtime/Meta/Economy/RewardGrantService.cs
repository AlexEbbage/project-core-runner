using CoreRacer.Meta.Profile;

namespace CoreRacer.Meta.Economy
{
    public enum RewardGrantType
    {
        SoftCurrency,
        PremiumCurrency,
        Experience,
        UnlockItem
    }

    [System.Serializable]
    public sealed class RewardGrant
    {
        public RewardGrantType Type;
        public int Amount;
        public string ItemId;

        public static RewardGrant Soft(int amount) => new RewardGrant { Type = RewardGrantType.SoftCurrency, Amount = amount };
        public static RewardGrant Premium(int amount) => new RewardGrant { Type = RewardGrantType.PremiumCurrency, Amount = amount };
        public static RewardGrant Experience(int amount) => new RewardGrant { Type = RewardGrantType.Experience, Amount = amount };
        public static RewardGrant Unlock(string itemId) => new RewardGrant { Type = RewardGrantType.UnlockItem, ItemId = itemId };

        public static RewardGrant Currency(CurrencyAmount amount)
        {
            return amount.Currency == CurrencyType.Premium
                ? Premium(amount.Amount)
                : Soft(amount.Amount);
        }
    }

    public sealed class RewardGrantService
    {
        private readonly PlayerProfileService _profile;

        public RewardGrantService(PlayerProfileService profile)
        {
            _profile = profile;
        }

        public void Grant(RewardGrant reward)
        {
            if (reward == null)
                return;

            switch (reward.Type)
            {
                case RewardGrantType.SoftCurrency:
                    _profile.AddCurrency(CurrencyType.Soft, reward.Amount);
                    break;
                case RewardGrantType.PremiumCurrency:
                    _profile.AddCurrency(CurrencyType.Premium, reward.Amount);
                    break;
                case RewardGrantType.Experience:
                    _profile.AddExperience(reward.Amount);
                    break;
                case RewardGrantType.UnlockItem:
                    _profile.UnlockItem(reward.ItemId);
                    break;
            }
        }
    }
}
