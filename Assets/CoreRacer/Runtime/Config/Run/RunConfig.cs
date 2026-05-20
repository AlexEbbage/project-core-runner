using UnityEngine;

namespace CoreRacer.Config.Run
{
    [System.Serializable]
    public sealed class ContinueConfig
    {
        public int MaxContinuesPerRun = 3;
        public float RespawnBackDistance = 8f;
        public float RespawnHeightOffset = 0.5f;
        public float InvulnerabilitySeconds = 2f;
    }

    [System.Serializable]
    public sealed class RunRewardConfig
    {
        public float XpPerScorePoint = 1f;
        public int PremiumCurrencyPerCoins = 100;
        public float RewardGrantCooldownSeconds = 2f;
    }

    [CreateAssetMenu(menuName = "Core Racer/Run/Run Config")]
    public sealed class RunConfig : ScriptableObject
    {
        public ContinueConfig Continues = new ContinueConfig();
        public RunRewardConfig Rewards = new RunRewardConfig();
    }
}
