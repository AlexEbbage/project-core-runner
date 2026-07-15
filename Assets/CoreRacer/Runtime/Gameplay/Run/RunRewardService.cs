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

        public RunResult BuildBonusResult(RunResult settledResult)
        {
            return new RunResult
            {
                Score = 0,
                Coins = UnityEngine.Mathf.Max(0, settledResult.Coins),
                Experience = UnityEngine.Mathf.Max(0, settledResult.Experience),
                PremiumCurrency = UnityEngine.Mathf.Max(0, settledResult.PremiumCurrency),
                Distance = 0f,
                DurationSeconds = 0f,
                PowerupsCollected = 0,
                EndReason = settledResult.EndReason
            };
        }

        /// <summary>Settles one completed run in one profile commit.</summary>
        public void Grant(RunResult result)
        {
            _profile.Mutate(state =>
            {
                state.Wallet.Add(CurrencyType.Soft, result.Coins);
                state.Wallet.Add(CurrencyType.Premium, result.PremiumCurrency);
                _profile.ApplyExperience(state, result.Experience);
                _profile.ApplyRunRecord(state, result.Score, result.Coins, result.Distance, result.PowerupsCollected);
            });
        }

        /// <summary>Grants only the bonus delta. It never records a second run or powerup collection.</summary>
        public void GrantBonus(RunResult bonus)
        {
            _profile.Mutate(state =>
            {
                state.Wallet.Add(CurrencyType.Soft, bonus.Coins);
                state.Wallet.Add(CurrencyType.Premium, bonus.PremiumCurrency);
                _profile.ApplyExperience(state, bonus.Experience);
            });
        }
    }
}
