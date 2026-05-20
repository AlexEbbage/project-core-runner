using UnityEngine;

namespace CoreRacer.Config.Gameplay
{
    [CreateAssetMenu(menuName = "Core Racer/Gameplay/Game Balance V2")]
    public sealed class GameBalanceConfigV2 : ScriptableObject
    {
        [Header("Player")]
        public float MaxHealth = 2f;
        public float SideScrapeDamage = 1f;
        public float SideScrapeCooldown = 0.25f;

        [Header("Score")]
        public float DistanceScoreMultiplier = 1f;
        public int PickupBaseScore = 10;
        public float ComboIncreasePerPickup = 1f;
        public float MaxComboValue = 10f;
        public float ComboDecayPerSecond = 1f;
        public float ComboToMultiplierFactor = 0.1f;

        [Header("Currency")]
        public int CoinValue = 1;
    }
}
