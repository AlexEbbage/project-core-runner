using CoreRacer.Meta.Shop;
using CoreRacer.Monetisation.Commercial;
using CoreRacer.Services.Compliance;
using CoreRacer.UI.Compliance;
using CoreRacer.UI.MainMenu;
using CoreRacer.UI.Settings;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoreRacer.Editor.Verticals
{
    public static class CommercialServicesComplianceVerticalInstaller
    {
        private const string MainScenePath = "Assets/CoreRacer/Scenes/CoreRacer_Main.unity";
        private const string PrivacyLinksPath = "Assets/CoreRacer/Generated/Configs/PrivacyLinks.asset";
        private const string ShopCatalogPath = "Assets/CoreRacer/Generated/Configs/ShopCatalog.asset";

        [MenuItem("Tools/Core Racer/Vertical 7/Apply Commercial Services Compliance")]
        public static void ApplyCommercialServicesCompliance()
        {
            var scene = EnsureMainSceneOpen();
            var privacyLinks = AssetDatabase.LoadAssetAtPath<PrivacyLinksConfig>(PrivacyLinksPath);
            var shopCatalog = AssetDatabase.LoadAssetAtPath<ShopCatalog>(ShopCatalogPath);
            var shopPage = Object.FindObjectOfType<ShopPageController>(true);
            var privacySettings = Object.FindObjectOfType<PrivacySettingsController>(true);
            var consentPrompt = Object.FindObjectOfType<ConsentPromptController>(true);
            var settingsHub = Object.FindObjectOfType<SettingsHubController>(true);

            if (privacyLinks == null)
                Debug.LogWarning("PrivacyLinks asset is missing. Create Assets/CoreRacer/Generated/Configs/PrivacyLinks.asset before closed testing.");

            if (shopCatalog == null)
                Debug.LogWarning("ShopCatalog asset is missing. Restore purchases/remove ads will not appear in the Shop.");
            else
                EnsureShopCommercialItems(shopCatalog);

            if (shopPage == null)
                Debug.LogWarning("ShopPageController was not found. Shop UI must be wired manually.");

            if (privacySettings == null)
                Debug.LogWarning("PrivacySettingsController was not found. Settings > Privacy must be wired manually.");

            if (consentPrompt == null)
                Debug.LogWarning("ConsentPromptController was not found. First-run consent must be wired manually if required.");

            if (settingsHub == null)
                Debug.LogWarning("SettingsHubController was not found. Privacy/support panels must be checked manually.");

            if (shopCatalog != null) EditorUtility.SetDirty(shopCatalog);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Vertical 7 commercial services and compliance pass applied. Replace placeholder privacy/terms/data-deletion URLs before closed testing.");
        }

        [MenuItem("Tools/Core Racer/Vertical 7/Validate Commercial Services Compliance")]
        public static void ValidateCommercialServicesCompliance()
        {
            EnsureMainSceneOpen();
            var issues = 0;
            var warnings = 0;
            var privacyLinks = AssetDatabase.LoadAssetAtPath<PrivacyLinksConfig>(PrivacyLinksPath);
            var shopCatalog = AssetDatabase.LoadAssetAtPath<ShopCatalog>(ShopCatalogPath);
            var shopPage = Object.FindObjectOfType<ShopPageController>(true);
            var privacySettings = Object.FindObjectOfType<PrivacySettingsController>(true);
            var consentPrompt = Object.FindObjectOfType<ConsentPromptController>(true);

            Check(privacyLinks != null, "PrivacyLinks asset exists.", "PrivacyLinks asset is missing.", ref issues);
            if (privacyLinks != null)
            {
                Check(CommercialComplianceRules.HasProductionSafeUrl(privacyLinks.PrivacyPolicyUrl), "Privacy policy URL is production-safe.", "Privacy policy URL is missing or still placeholder/example.", ref issues);
                Check(CommercialComplianceRules.HasProductionSafeUrl(privacyLinks.TermsUrl), "Terms URL is production-safe.", "Terms URL is missing or still placeholder/example.", ref issues);
                Check(CommercialComplianceRules.HasProductionSafeUrl(privacyLinks.DataDeletionUrl), "Data deletion URL is production-safe.", "Data deletion URL is missing or still placeholder/example.", ref issues);
            }

            Check(shopCatalog != null, "ShopCatalog exists.", "ShopCatalog is missing.", ref issues);
            if (shopCatalog != null)
            {
                Check(shopCatalog.Get("premium_user") != null, "Remove Ads/Premium item exists.", "premium_user item is missing from ShopCatalog.", ref issues);
                Check(shopCatalog.Get("restore_purchases") != null, "Restore Purchases item exists.", "restore_purchases item is missing from ShopCatalog.", ref issues);
            }

            Check(shopPage != null, "ShopPageController exists.", "ShopPageController is missing from scene.", ref warnings, true);
            Check(privacySettings != null, "PrivacySettingsController exists.", "PrivacySettingsController is missing from scene.", ref warnings, true);
            Check(consentPrompt != null, "ConsentPromptController exists.", "ConsentPromptController is missing from scene.", ref warnings, true);

            if (issues == 0)
                Debug.Log($"Vertical 7 Commercial Services Compliance validation passed with {warnings} warning(s).");
            else
                Debug.LogError($"Vertical 7 Commercial Services Compliance validation failed with {issues} issue(s) and {warnings} warning(s).");
        }

        private static Scene EnsureMainSceneOpen()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.path == MainScenePath)
                return scene;

            return EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
        }

        private static void EnsureShopCommercialItems(ShopCatalog catalog)
        {
            if (catalog.Get("premium_user") == null)
            {
                catalog.Items.Insert(0, new ShopItemDefinition
                {
                    Id = "premium_user",
                    DisplayName = "Remove Ads",
                    Description = "Remove interstitial ads and bypass continue/double-reward ads where the premium policy allows it.",
                    Kind = ShopItemKind.PremiumUser,
                    IsFeatured = true
                });
            }

            if (catalog.Get("restore_purchases") == null)
            {
                catalog.Items.Insert(Mathf.Min(1, catalog.Items.Count), new ShopItemDefinition
                {
                    Id = "restore_purchases",
                    DisplayName = "Restore Purchases",
                    Description = "Restore non-consumable purchases on this account.",
                    Kind = ShopItemKind.RestorePurchases
                });
            }
        }

        private static void Check(bool condition, string ok, string fail, ref int count, bool warning = false)
        {
            if (condition)
            {
                Debug.Log(ok);
                return;
            }

            count++;
            if (warning) Debug.LogWarning(fail);
            else Debug.LogError(fail);
        }
    }
}
