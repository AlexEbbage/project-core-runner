using UnityEngine;

namespace CoreRacer.Editor.Simulation
{
    [CreateAssetMenu(menuName = "Core Racer/Simulation/Economy Simulation Settings")]
    public sealed class EconomySimulationSettings : ScriptableObject
    {
        public int Runs = 100;
        public int AverageSoftCurrencyPerRun = 75;
        public int AveragePremiumCurrencyPerDay = 5;
        public int DailyTaskSoftReward = 150;
        public int WeeklyTaskSoftReward = 1000;
        public int MonthlyTaskSoftReward = 5000;
        public int FirstUpgradeCost = 250;
        public int SecondShipCost = 2500;
        public float RewardedAdUsageRate = 0.25f;
        public int RewardedAdSoftReward = 100;
    }
}
