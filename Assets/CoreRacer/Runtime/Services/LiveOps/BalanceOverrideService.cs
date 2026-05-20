namespace CoreRacer.Services.LiveOps
{
    public sealed class BalanceOverrideService
    {
        private readonly IRemoteConfigService _remote;
        public BalanceOverrideService(IRemoteConfigService remote) { _remote = remote; }

        public BalanceOverrideSnapshot GetSnapshot()
        {
            return new BalanceOverrideSnapshot
            {
                ObstacleDifficultyMultiplier = _remote?.GetFloat("balance_obstacle_difficulty_multiplier", 1f) ?? 1f,
                CoinRewardMultiplier = _remote?.GetFloat("balance_coin_reward_multiplier", 1f) ?? 1f,
                UpgradeCostMultiplier = _remote?.GetFloat("balance_upgrade_cost_multiplier", 1f) ?? 1f,
                PowerupDurationMultiplier = _remote?.GetFloat("balance_powerup_duration_multiplier", 1f) ?? 1f,
                RewardedAdBonusSoftCurrency = _remote?.GetInt("rewarded_ad_bonus_soft_currency", 100) ?? 100,
                InterstitialCooldownSeconds = _remote?.GetInt("interstitial_cooldown_seconds", 120) ?? 120
            };
        }
    }
}
