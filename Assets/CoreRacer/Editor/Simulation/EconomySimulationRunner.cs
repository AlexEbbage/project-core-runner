using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CoreRacer.Editor.Simulation
{
    public static class EconomySimulationRunner
    {
        [MenuItem("Tools/Core Racer/Run Default Economy Simulation")]
        public static void RunDefaultSimulation()
        {
            var settings = ScriptableObject.CreateInstance<EconomySimulationSettings>();
            var report = Run(settings);
            var dir = "docs/reports";
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, "economy-simulation-report.md");
            File.WriteAllText(path, report.ToMarkdown());
            AssetDatabase.Refresh();
            Debug.Log("Economy simulation report written to " + path);
        }

        public static EconomySimulationReport Run(EconomySimulationSettings s)
        {
            var ads = Mathf.RoundToInt(s.Runs * s.RewardedAdUsageRate);
            var soft = s.Runs * s.AverageSoftCurrencyPerRun + ads * s.RewardedAdSoftReward;
            soft += Mathf.CeilToInt(s.Runs / 10f) * s.DailyTaskSoftReward;
            soft += Mathf.CeilToInt(s.Runs / 70f) * s.WeeklyTaskSoftReward;
            soft += Mathf.CeilToInt(s.Runs / 300f) * s.MonthlyTaskSoftReward;

            return new EconomySimulationReport
            {
                Runs = s.Runs,
                SoftCurrencyEarned = soft,
                PremiumCurrencyEarned = Mathf.CeilToInt(s.Runs / 10f) * s.AveragePremiumCurrencyPerDay,
                EstimatedRewardedAdsWatched = ads,
                RunsToFirstUpgrade = Math.Max(1, Mathf.CeilToInt((float)s.FirstUpgradeCost / Math.Max(1, s.AverageSoftCurrencyPerRun + s.RewardedAdSoftReward * s.RewardedAdUsageRate))),
                RunsToSecondShip = Math.Max(1, Mathf.CeilToInt((float)s.SecondShipCost / Math.Max(1, s.AverageSoftCurrencyPerRun + s.RewardedAdSoftReward * s.RewardedAdUsageRate)))
            };
        }
    }
}
