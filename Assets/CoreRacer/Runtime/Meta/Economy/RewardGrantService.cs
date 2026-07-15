using System.Collections.Generic;
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

            _profile.Mutate(state => ApplyToState(state, reward));
        }

        public void GrantMany(IReadOnlyList<RewardGrant> rewards)
        {
            if (rewards == null || rewards.Count == 0)
                return;

            _profile.Mutate(state => ApplyManyToState(state, rewards));
        }

        /// <summary>Applies a reward without saving. The owning operation must commit the profile once.</summary>
        public void ApplyToState(PlayerProfileState state, RewardGrant reward)
        {
            if (state == null || reward == null)
                return;

            switch (reward.Type)
            {
                case RewardGrantType.SoftCurrency:
                    state.Wallet.Add(CurrencyType.Soft, reward.Amount);
                    break;
                case RewardGrantType.PremiumCurrency:
                    state.Wallet.Add(CurrencyType.Premium, reward.Amount);
                    break;
                case RewardGrantType.Experience:
                    _profile.ApplyExperience(state, reward.Amount);
                    break;
                case RewardGrantType.UnlockItem:
                    state.Inventory.Unlock(reward.ItemId);
                    break;
            }
        }

        public void ApplyManyToState(PlayerProfileState state, IReadOnlyList<RewardGrant> rewards)
        {
            if (state == null || rewards == null)
                return;

            for (var i = 0; i < rewards.Count; i++)
                ApplyToState(state, rewards[i]);
        }
    }
}
