using System.Collections.Generic;
using CoreRacer.Meta.Economy;
using UnityEngine;

namespace CoreRacer.Config.Monetisation
{
    public enum RewardedOfferPlacement
    {
        MidRun,
        GameOver,
        DailyLogin,
        ShopBonus
    }

    [System.Serializable]
    public sealed class RewardedOfferDefinition
    {
        public string Id;
        public RewardedOfferPlacement Placement;
        public string DisplayName;
        [TextArea] public string Description;
        public CurrencyAmount Reward;
        public float MinimumSecondsBetweenOffers = 90f;
    }

    [CreateAssetMenu(menuName = "Core Racer/Monetisation/Rewarded Offer Config V2")]
    public sealed class RewardedOfferConfigV2 : ScriptableObject
    {
        public List<RewardedOfferDefinition> Offers = new List<RewardedOfferDefinition>();
    }
}
