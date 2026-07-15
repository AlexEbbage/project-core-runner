using CoreRacer.Meta.Profile;

namespace CoreRacer.Meta.Progression
{
    public readonly struct ProgressionSnapshot
    {
        public readonly int Level;
        public readonly int Experience;
        public readonly int ExperienceForNextLevel;
        public readonly int SoftCurrency;
        public readonly int PremiumCurrency;
        public readonly int TotalRuns;
        public readonly int BestScore;
        public readonly float BestDistance;
        public readonly int TotalCoinsCollected;
        public readonly int TotalPowerupsCollected;
        public readonly int ReadyTasks;
        public readonly int ClaimedTasks;
        public readonly int TotalTasks;

        public float LevelProgress => ExperienceForNextLevel <= 0 ? 0f : UnityEngine.Mathf.Clamp01(Experience / (float)ExperienceForNextLevel);
        public bool HasClaimableTasks => ReadyTasks > 0;

        public ProgressionSnapshot(PlayerProfileState state, int experienceForNextLevel, ProgressionTaskSummary taskSummary)
        {
            Level = state != null ? state.Level : 1;
            Experience = state != null ? state.Experience : 0;
            ExperienceForNextLevel = UnityEngine.Mathf.Max(1, experienceForNextLevel);
            SoftCurrency = state != null ? state.Wallet.Soft : 0;
            PremiumCurrency = state != null ? state.Wallet.Premium : 0;
            TotalRuns = state != null ? state.TotalRuns : 0;
            BestScore = state != null ? state.BestScore : 0;
            BestDistance = state != null ? state.BestDistance : 0f;
            TotalCoinsCollected = state != null ? state.TotalCoinsCollected : 0;
            TotalPowerupsCollected = state != null ? state.TotalPowerupsCollected : 0;
            ReadyTasks = taskSummary.ReadyToClaim;
            ClaimedTasks = taskSummary.Claimed;
            TotalTasks = taskSummary.Total;
        }
    }
}
