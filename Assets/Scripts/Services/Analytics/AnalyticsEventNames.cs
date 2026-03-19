public static class AnalyticsEventNames
{
    public const string ObstacleHit = "obstacle_hit";
    public const string PowerupPickup = "powerup_pickup";
    public const string UpgradePurchased = "upgrade_purchased";
    public const string ShopPurchase = "shop_purchase";
    public const string RewardedOfferShown = "rewarded_offer_shown";
    public const string RewardedOfferTapped = "rewarded_offer_tapped";
    public const string RewardedOfferIgnored = "rewarded_offer_ignored";
    public const string RewardedOfferTimedOut = "rewarded_offer_timed_out";
    public const string RewardedOfferRewardGranted = "rewarded_offer_reward_granted";

    public static class Params
    {
        public const string Type = "type";
        public const string Id = "id";
        public const string Price = "price";
        public const string Source = "source";
        public const string RewardKind = "reward_kind";
        public const string Amount = "amount";
    }
}
