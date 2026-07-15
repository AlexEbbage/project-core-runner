using CoreRacer.Gameplay.Run;

namespace CoreRacer.Meta.Economy
{
    public static class ProgressionEconomyRules
    {
        public const int MinimumRunSoftCurrency = 0;
        public const int MinimumRunExperience = 0;

        public static bool IsValidRunReward(RunResult result)
        {
            return result.Coins >= MinimumRunSoftCurrency
                   && result.Experience >= MinimumRunExperience
                   && result.PremiumCurrency >= 0
                   && result.Score >= 0
                   && result.Distance >= 0f
                   && result.DurationSeconds >= 0f
                   && result.PowerupsCollected >= 0;
        }

        public static int ClampRewardAmount(int amount)
        {
            return amount < 0 ? 0 : amount;
        }
    }
}
