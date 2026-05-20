using System.Collections.Generic;
using CoreRacer.Gameplay.Obstacles;
using CoreRacer.Gameplay.Pickups;
using CoreRacer.Gameplay.Powerups;
using CoreRacer.Meta.Shop;
using UnityEditor;
using UnityEngine;

namespace CoreRacer.Editor.Validation
{
    public static class CoreRacerProjectValidator
    {
        [MenuItem("Tools/Core Racer/Validate Project")]
        public static void ValidateProject()
        {
            var issues = new List<string>();
            ValidateGeneratedConfigs(issues);
            ValidateAdPlaceholders(issues);
            ValidatePowerupConfigs(issues);
            ValidateShopCatalogs(issues);
            ValidateObstacleConfigs(issues);
            ValidatePickupConfigs(issues);

            if (issues.Count == 0)
            {
                Debug.Log("Core Racer validation passed.");
                return;
            }

            Debug.LogWarning($"Core Racer validation found {issues.Count} issue(s):\n- " + string.Join("\n- ", issues));
        }

        private static void ValidateGeneratedConfigs(List<string> issues)
        {
            if (!AssetDatabase.IsValidFolder("Assets/CoreRacer/Generated/Configs"))
                issues.Add("Default config folder is missing. Run Tools/Core Racer/Generate Default Config Assets.");
        }

        private static void ValidateAdPlaceholders(List<string> issues)
        {
            var guids = AssetDatabase.FindAssets("t:MonoScript LevelPlayRewardedAdServiceAdapter");
            if (guids.Length == 0)
                issues.Add("LevelPlay rewarded ad adapter script is missing.");
        }

        private static void ValidatePowerupConfigs(List<string> issues)
        {
            foreach (var config in LoadAll<PowerupUpgradeConfigV2>())
            {
                if (config.Upgrades == null || config.Upgrades.Count == 0)
                    issues.Add($"Powerup upgrade config {config.name} has no authored upgrade entries.");
            }
        }

        private static void ValidateShopCatalogs(List<string> issues)
        {
            foreach (var catalog in LoadAll<ShopCatalog>())
            {
                var ids = new HashSet<string>();
                foreach (var item in catalog.Items)
                {
                    if (item == null) continue;
                    if (string.IsNullOrWhiteSpace(item.Id)) issues.Add($"Shop catalog {catalog.name} has an item with an empty id.");
                    else if (!ids.Add(item.Id)) issues.Add($"Shop catalog {catalog.name} has duplicate item id {item.Id}.");
                    if (item.Kind == ShopItemKind.PremiumUser && item.Id != CoreRacer.Monetisation.Iap.IapProductIds.PremiumUser)
                        issues.Add($"Premium shop item id should match IAP product id {CoreRacer.Monetisation.Iap.IapProductIds.PremiumUser}.");
                }
            }
        }

        private static void ValidateObstacleConfigs(List<string> issues)
        {
            foreach (var config in LoadAll<ObstacleGenerationConfig>())
            {
                if (config.TunnelSides < 3) issues.Add($"Obstacle config {config.name} has tunnel sides below 3.");
                if (config.RingSpacing <= 0f) issues.Add($"Obstacle config {config.name} has invalid ring spacing.");
                if (config.Patterns == null || config.Patterns.Count == 0) issues.Add($"Obstacle config {config.name} has no obstacle patterns.");
            }
        }

        private static void ValidatePickupConfigs(List<string> issues)
        {
            foreach (var config in LoadAll<PickupGenerationConfig>())
            {
                if (config.RingSpacing <= 0f) issues.Add($"Pickup config {config.name} has invalid ring spacing.");
                if (config.TunnelSides < 3) issues.Add($"Pickup config {config.name} has tunnel sides below 3.");
            }
        }

        private static IEnumerable<T> LoadAll<T>() where T : Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null) yield return asset;
            }
        }
    }
}
