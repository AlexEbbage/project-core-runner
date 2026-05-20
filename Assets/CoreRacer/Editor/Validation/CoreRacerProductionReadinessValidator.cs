using System.Collections.Generic;
using CoreRacer.Meta.DailyRewards;
using CoreRacer.Meta.Tasks;
using CoreRacer.Services.Compliance;
using CoreRacer.Services.LiveOps;
using UnityEditor;
using UnityEngine;

namespace CoreRacer.Editor.Validation
{
    public static class CoreRacerProductionReadinessValidator
    {
        [MenuItem("Tools/Core Racer/Validate Production Readiness")]
        public static void ValidateProductionReadiness()
        {
            var issues = new List<string>();
            var warnings = new List<string>();

            ValidateCompliance(issues, warnings);
            ValidateTaskPools(issues, warnings);
            ValidateDailyRewards(issues, warnings);
            ValidateRemoteConfig(warnings);
            ValidateSdkSymbols(warnings);
            ValidateBuildSettings(warnings);

            if (issues.Count == 0 && warnings.Count == 0)
            {
                Debug.Log("Core Racer production readiness validation passed.");
                return;
            }

            if (issues.Count > 0)
                Debug.LogError($"Core Racer production readiness found {issues.Count} blocking issue(s):\n- " + string.Join("\n- ", issues));
            if (warnings.Count > 0)
                Debug.LogWarning($"Core Racer production readiness found {warnings.Count} warning(s):\n- " + string.Join("\n- ", warnings));
        }

        private static void ValidateCompliance(List<string> issues, List<string> warnings)
        {
            var configs = LoadAll<PrivacyLinksConfig>();
            if (configs.Count == 0)
            {
                issues.Add("No PrivacyLinksConfig found. Create one and wire it to GameBootstrapper before release.");
                return;
            }

            foreach (var config in configs)
            {
                if (config.PrivacyPolicyUrl.Contains("example.com")) issues.Add($"{config.name} still uses placeholder privacy policy URL.");
                if (config.TermsUrl.Contains("example.com")) warnings.Add($"{config.name} still uses placeholder terms URL.");
                if (config.DataDeletionUrl.Contains("example.com")) warnings.Add($"{config.name} still uses placeholder data deletion URL.");
                if (config.TreatAsChildDirected) warnings.Add($"{config.name} is marked child-directed. Confirm ad/analytics SDK setup matches store policy.");
            }
        }

        private static void ValidateTaskPools(List<string> issues, List<string> warnings)
        {
            var pools = LoadAll<TaskPoolDefinition>();
            if (pools.Count == 0)
            {
                warnings.Add("No TaskPoolDefinition found. Daily/weekly/monthly rotating tasks will not be populated.");
                return;
            }

            foreach (var pool in pools)
            {
                if (pool.DailySlots > 0 && pool.GetTasksFor(TaskCadence.Daily).Count < pool.DailySlots)
                    issues.Add($"{pool.name} has fewer daily tasks than DailySlots.");
                if (pool.WeeklySlots > 0 && pool.GetTasksFor(TaskCadence.Weekly).Count < pool.WeeklySlots)
                    issues.Add($"{pool.name} has fewer weekly tasks than WeeklySlots.");
                if (pool.MonthlySlots > 0 && pool.GetTasksFor(TaskCadence.Monthly).Count < pool.MonthlySlots)
                    issues.Add($"{pool.name} has fewer monthly tasks than MonthlySlots.");
            }
        }

        private static void ValidateDailyRewards(List<string> issues, List<string> warnings)
        {
            var calendars = LoadAll<DailyRewardCalendarConfig>();
            if (calendars.Count == 0)
            {
                warnings.Add("No DailyRewardCalendarConfig found. Enhanced daily login calendar will be inactive.");
                return;
            }

            foreach (var calendar in calendars)
            {
                if (calendar.Days.Count < 7)
                    warnings.Add($"{calendar.name} has fewer than 7 daily reward days.");
                for (int i = 0; i < calendar.Days.Count; i++)
                    if (calendar.Days[i].Rewards == null || calendar.Days[i].Rewards.Count == 0)
                        issues.Add($"{calendar.name} day {i + 1} has no rewards.");
            }
        }

        private static void ValidateRemoteConfig(List<string> warnings)
        {
            var configs = LoadAll<RemoteConfigDefaultsConfig>();
            if (configs.Count == 0)
                warnings.Add("No RemoteConfigDefaultsConfig found. LiveOps values will use hard-coded fallbacks.");
        }

        private static void ValidateSdkSymbols(List<string> warnings)
        {
#if !CORE_RACER_LEVELPLAY
            warnings.Add("CORE_RACER_LEVELPLAY scripting define is not set. LevelPlay adapter remains placeholder-only.");
#endif
#if !CORE_RACER_FIREBASE
            warnings.Add("CORE_RACER_FIREBASE scripting define is not set. Firebase analytics/crash integration remains placeholder-only.");
#endif
#if !CORE_RACER_UNITY_IAP
            warnings.Add("CORE_RACER_UNITY_IAP scripting define is not set. Unity IAP adapter remains placeholder-only until you wire your installed Unity IAP package version.");
#endif
        }

        private static void ValidateBuildSettings(List<string> warnings)
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                warnings.Add("Active build target is not Android.");
            if (PlayerSettings.bundleVersion == "1.0")
                warnings.Add("Bundle version is still the Unity default 1.0. Confirm release versioning.");
            if (string.IsNullOrWhiteSpace(PlayerSettings.applicationIdentifier) || PlayerSettings.applicationIdentifier.Contains("DefaultCompany"))
                warnings.Add("Application identifier/bundle ID still looks like a default value.");
        }

        private static List<T> LoadAll<T>() where T : Object
        {
            var result = new List<T>();
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null) result.Add(asset);
            }
            return result;
        }
    }
}
