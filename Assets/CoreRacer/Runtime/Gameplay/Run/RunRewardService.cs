using CoreRacer.Config.Run;
using CoreRacer.Meta.Economy;
using CoreRacer.Meta.Profile;

namespace CoreRacer.Gameplay.Run
{
    public sealed class RunRewardService
    {
        private readonly PlayerProfileService _profile;
        private readonly RunRewardConfig _config;

        public RunRewardService(PlayerProfileService profile, RunRewardConfig config)
        {
            _profile = profile;
            _config = config ?? new RunRewardConfig();
        }

        public RunResult BuildResult(int score, int coins, float distance, float duration, int powerups, RunEndReason reason, bool doubled)
        {
            var multiplier = doubled ? 2 : 1;
            var premium = _config.PremiumCurrencyPerCoins <= 0 ? 0 : coins / _config.PremiumCurrencyPerCoins;
            return new RunResult
            {
                Score = score,
                Coins = coins * multiplier,
                Experience = UnityEngine.Mathf.RoundToInt(score * _config.XpPerScorePoint) * multiplier,
                PremiumCurrency = premium * multiplier,
                Distance = distance,
                DurationSeconds = duration,
                PowerupsCollected = powerups,
                EndReason = reason
            };
        }

        public void Grant(RunResult result)
        {
            _profile.AddCurrency(CurrencyType.Soft, result.Coins);
            if (result.PremiumCurrency > 0)
                _profile.AddCurrency(CurrencyType.Premium, result.PremiumCurrency);
            _profile.AddExperience(result.Experience);
            _profile.RecordRun(result.Score, result.Coins, result.Distance, result.PowerupsCollected);
        }
    }
}
