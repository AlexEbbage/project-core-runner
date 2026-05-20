using System;

namespace CoreRacer.Services.LiveOps
{
    [Serializable]
    public sealed class BalanceOverrideSnapshot
    {
        public float ObstacleDifficultyMultiplier = 1f;
        public float CoinRewardMultiplier = 1f;
        public float UpgradeCostMultiplier = 1f;
        public float PowerupDurationMultiplier = 1f;
        public int RewardedAdBonusSoftCurrency = 100;
        public int InterstitialCooldownSeconds = 120;
    }
}
