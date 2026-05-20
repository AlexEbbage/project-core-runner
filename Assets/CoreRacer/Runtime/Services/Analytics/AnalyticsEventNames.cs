namespace CoreRacer.Services.Analytics
{
    public static class AnalyticsEventNames
    {
        public const string AppOpen = "app_open";
        public const string SessionStarted = "session_started";
        public const string SessionEnded = "session_ended";
        public const string FirstRunStarted = "first_run_started";

        public const string RunStarted = "run_started";
        public const string RunEnded = "run_ended";
        public const string RunContinued = "run_continued";
        public const string RunAbandoned = "run_abandoned";
        public const string DistanceReached = "distance_reached";
        public const string PowerupCollected = "powerup_collected";

        public const string RewardGranted = "reward_granted";
        public const string CurrencyEarned = "currency_earned";
        public const string CurrencySpent = "currency_spent";
        public const string UpgradePurchased = "upgrade_purchased";
        public const string ShipUnlocked = "ship_unlocked";
        public const string CosmeticEquipped = "cosmetic_equipped";

        public const string AdRequested = "ad_requested";
        public const string AdCompleted = "ad_completed";
        public const string RewardedAdFailed = "rewarded_ad_failed";
        public const string InterstitialShown = "interstitial_shown";
        public const string AdBypassedByPremium = "ad_bypassed_by_premium";

        public const string PurchaseStarted = "purchase_started";
        public const string PurchaseCompleted = "purchase_completed";
        public const string PurchaseFailed = "purchase_failed";
        public const string RestoreStarted = "restore_started";
        public const string RestoreCompleted = "restore_completed";
        public const string RestoreFailed = "restore_failed";
        public const string PremiumPurchased = "premium_purchased";

        public const string DailyRewardClaimed = "daily_reward_claimed";
        public const string TaskCompleted = "task_completed";
        public const string TaskClaimed = "task_claimed";
        public const string AchievementClaimed = "achievement_claimed";
        public const string ConsentUpdated = "consent_updated";
    }
}
