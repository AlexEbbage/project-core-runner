using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CoreRacer.Bootstrap;
using CoreRacer.Gameplay.Run;
using CoreRacer.Meta.Shop;
using CoreRacer.Monetisation.Commercial;
using CoreRacer.Services.Compliance;
using CoreRacer.Services.Diagnostics;
using CoreRacer.UI.GameOver;
using CoreRacer.UI.Hud;
using CoreRacer.UI.MainMenu;
using CoreRacer.Editor.Validation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoreRacer.Editor.Verticals
{
    public static class ClosedTestingHardeningVerticalInstaller
    {
        private const string MainScenePath = ClosedTestingReadinessRules.ExpectedMainScenePath;
        private const string PrivacyLinksPath = "Assets/CoreRacer/Generated/Configs/PrivacyLinks.asset";
        private const string ShopCatalogPath = "Assets/CoreRacer/Generated/Configs/ShopCatalog.asset";
        private const string ObstacleGenerationPath = "Assets/CoreRacer/Generated/Configs/ObstacleGeneration.asset";
        private const string PickupGenerationPath = "Assets/CoreRacer/Generated/Configs/PickupGeneration.asset";
        private const string PowerupUpgradesPath = "Assets/CoreRacer/Generated/Configs/PowerupUpgrades.asset";
        private const string RunFeelProfilePath = "Assets/CoreRacer/Generated/Configs/RunFeelProfile.asset";

        [MenuItem("Tools/Core Racer/Vertical 8/Apply Closed Testing Hardening")]
        public static void ApplyClosedTestingHardening()
        {
            var scene = EnsureMainSceneOpen();
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainScenePath, true)
            };

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Vertical 8 closed-testing hardening applied. Build Settings now contains only CoreRacer_Main. Run the Vertical 8 validator for remaining Android/store blockers.");
        }

        [MenuItem("Tools/Core Racer/Vertical 8/Validate Closed Testing Hardening")]
        public static void ValidateClosedTestingHardening()
        {
            EnsureMainSceneOpen();

            var blockers = new List<string>();
            var warnings = new List<string>();

            ValidateBuildSettings(blockers);
            ValidateAndroidPlayerSettings(blockers, warnings);
            ValidateRequiredAssets(blockers);
            ValidateCommercialCompliance(blockers, warnings);
            ValidateSceneWiring(blockers, warnings);
            ValidateMissingScripts(blockers);
            ValidateTestsAndDocs(blockers, warnings);
            ValidateSdkStatus(warnings);

            if (blockers.Count == 0 && warnings.Count == 0)
            {
                Debug.Log("Vertical 8 Closed Testing Hardening validation passed. The project is ready for an Android closed-testing build pass.");
                return;
            }

            if (blockers.Count > 0)
                Debug.LogError("Vertical 8 Closed Testing Hardening found " + blockers.Count + " blocker(s):\n- " + string.Join("\n- ", blockers));

            if (warnings.Count > 0)
                Debug.LogWarning("Vertical 8 Closed Testing Hardening found " + warnings.Count + " warning(s):\n- " + string.Join("\n- ", warnings));
        }

        private static Scene EnsureMainSceneOpen()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.path == MainScenePath)
                return scene;

            return EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
        }

        private static void ValidateBuildSettings(List<string> blockers)
        {
            var scenes = new List<BuildSceneReadinessInfo>();
            foreach (var scene in EditorBuildSettings.scenes)
                scenes.Add(new BuildSceneReadinessInfo(scene.path, scene.enabled));

            if (!ClosedTestingReadinessRules.HasOnlyExpectedEnabledScene(scenes))
                blockers.Add("Build Settings must contain exactly one enabled scene: " + MainScenePath + ". Run Tools/Core Racer/Vertical 8/Apply Closed Testing Hardening.");
        }

        private static void ValidateAndroidPlayerSettings(List<string> blockers, List<string> warnings)
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                blockers.Add("Active build target is not Android. Switch Build Target to Android before closed testing.");

            var bundleId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
            if (!ClosedTestingReadinessRules.IsProductionBundleIdentifier(bundleId))
                blockers.Add("Android application identifier is missing/default/placeholder. Current value: " + (string.IsNullOrEmpty(bundleId) ? "<empty>" : bundleId));

            if (!ClosedTestingReadinessRules.IsReleaseVersionReady(PlayerSettings.bundleVersion))
                warnings.Add("Bundle version looks like a prototype/default value. Current value: " + PlayerSettings.bundleVersion);

            if (!ClosedTestingReadinessRules.IsClosedTestingVersionCodeReady(PlayerSettings.Android.bundleVersionCode))
                blockers.Add("Android bundle version code must be at least 2 for the first closed-testing upload after local prototypes. Current value: " + PlayerSettings.Android.bundleVersionCode);
        }

        private static void ValidateRequiredAssets(List<string> blockers)
        {
            RequireAsset(PrivacyLinksPath, "Privacy links config", blockers);
            RequireAsset(ShopCatalogPath, "Shop catalog", blockers);
            RequireAsset(ObstacleGenerationPath, "Vertical 2 obstacle generation config", blockers);
            RequireAsset(PickupGenerationPath, "Vertical 3 pickup generation config", blockers);
            RequireAsset(PowerupUpgradesPath, "Vertical 3 powerup upgrades config", blockers);
            RequireAsset(RunFeelProfilePath, "Vertical 4 run feel profile", blockers);
        }

        private static void ValidateCommercialCompliance(List<string> blockers, List<string> warnings)
        {
            var privacyLinks = AssetDatabase.LoadAssetAtPath<PrivacyLinksConfig>(PrivacyLinksPath);
            if (privacyLinks == null)
                return;

            if (!CommercialComplianceRules.HasProductionSafeUrl(privacyLinks.PrivacyPolicyUrl))
                blockers.Add("Privacy policy URL is missing or placeholder/example. Update " + PrivacyLinksPath + ".");
            if (!CommercialComplianceRules.HasProductionSafeUrl(privacyLinks.TermsUrl))
                blockers.Add("Terms URL is missing or placeholder/example. Update " + PrivacyLinksPath + ".");
            if (!CommercialComplianceRules.HasProductionSafeUrl(privacyLinks.DataDeletionUrl))
                blockers.Add("Data deletion URL is missing or placeholder/example. Update " + PrivacyLinksPath + ".");
            if (privacyLinks.TreatAsChildDirected)
                warnings.Add("PrivacyLinksConfig is marked child-directed. Confirm ad SDK, analytics, store questionnaire, and age-rating choices match this.");

            var shopCatalog = AssetDatabase.LoadAssetAtPath<ShopCatalog>(ShopCatalogPath);
            if (shopCatalog != null)
            {
                if (shopCatalog.Get("premium_user") == null)
                    blockers.Add("ShopCatalog is missing premium_user/remove-ads product.");
                if (shopCatalog.Get("restore_purchases") == null)
                    blockers.Add("ShopCatalog is missing restore_purchases action.");
            }
        }

        private static void ValidateSceneWiring(List<string> blockers, List<string> warnings)
        {
            if (Object.FindObjectOfType<GameBootstrapper>(true) == null)
                blockers.Add("GameBootstrapper is missing from CoreRacer_Main.");

            if (Object.FindObjectOfType<RunController>(true) == null)
                blockers.Add("RunController is missing from CoreRacer_Main.");

            var references = Object.FindObjectOfType<RunSceneReferences>(true);
            if (references == null)
            {
                blockers.Add("RunSceneReferences is missing from CoreRacer_Main.");
            }
            else
            {
                var validation = references.ValidateReferences();
                foreach (var error in validation.Errors)
                    blockers.Add("RunSceneReferences: " + error);
                foreach (var warning in validation.Warnings)
                    warnings.Add("RunSceneReferences: " + warning);
            }

            if (Object.FindObjectOfType<MainMenuShell>(true) == null)
                blockers.Add("MainMenuShell is missing from CoreRacer_Main.");
            if (Object.FindObjectOfType<HudController>(true) == null)
                blockers.Add("HudController is missing from CoreRacer_Main.");
            if (Object.FindObjectOfType<GameOverController>(true) == null)
                blockers.Add("GameOverController is missing from CoreRacer_Main.");
        }

        private static void ValidateMissingScripts(List<string> blockers)
        {
            var roots = new[]
            {
                "Assets/CoreRacer",
                "Assets/Config",
                "Assets/Prefabs",
                "Assets/Scenes"
            };
            var reported = new HashSet<string>();
            var scriptReference = new Regex(@"m_Script:\s*\{fileID:\s*11500000,\s*guid:\s*([0-9a-fA-F]{32}),\s*type:\s*3\}", RegexOptions.Compiled);

            foreach (var root in roots)
            {
                if (!Directory.Exists(root))
                    continue;

                foreach (var file in Directory.GetFiles(root, "*.*", SearchOption.AllDirectories))
                {
                    if (!IsYamlUnityAsset(file))
                        continue;

                    string text;
                    try
                    {
                        text = File.ReadAllText(file);
                    }
                    catch
                    {
                        continue;
                    }

                    var normalized = file.Replace('\\', '/');
                    if (text.Contains("m_Script: {fileID: 0}") && reported.Add(normalized + ":file0"))
                        blockers.Add("Null script reference found in " + normalized);

                    foreach (Match match in scriptReference.Matches(text))
                    {
                        var guid = match.Groups[1].Value;
                        if (string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guid)) && reported.Add(normalized + ":" + guid))
                            blockers.Add("Unresolved MonoScript GUID " + guid + " found in " + normalized);
                    }
                }
            }
        }

        private static void ValidateTestsAndDocs(List<string> blockers, List<string> warnings)
        {
            WarnIfMissing("Assets/CoreRacer/Tests/EditMode/Vertical1RunStateMachineTests.cs", "Vertical 1 EditMode tests were not included in the supplied patch archive.", warnings);
            WarnIfMissing("Assets/CoreRacer/Tests/EditMode/Vertical2ObstacleIdentityTests.cs", "Vertical 2 EditMode tests were not included in the supplied patch archive.", warnings);
            WarnIfMissing("Assets/CoreRacer/Tests/EditMode/Vertical3PowerupTests.cs", "Vertical 3 EditMode tests were not included in the supplied patch archive.", warnings);
            WarnIfMissing("Assets/CoreRacer/Tests/EditMode/Vertical4RunFeelTests.cs", "Vertical 4 EditMode tests were not included in the supplied patch archive.", warnings);
            RequireFile("Assets/CoreRacer/Tests/EditMode/Vertical5FinalMenuSetTests.cs", "Vertical 5 EditMode tests", blockers);
            RequireFile("Assets/CoreRacer/Tests/EditMode/Vertical6ProgressionEconomyTests.cs", "Vertical 6 EditMode tests", blockers);
            RequireFile("Assets/CoreRacer/Tests/EditMode/Vertical7CommercialComplianceTests.cs", "Vertical 7 EditMode tests", blockers);
            RequireFile("Assets/CoreRacer/Tests/EditMode/Vertical8ClosedTestingRulesTests.cs", "Vertical 8 EditMode tests", blockers);

            RequireFile("docs/verticals/vertical-roadmap.md", "vertical roadmap", blockers);
            RequireFile("docs/bdd/acceptance-backlog.md", "BDD acceptance backlog", blockers);
            RequireFile("docs/menus/final-menu-set.md", "final menu set", blockers);
            RequireFile("docs/testing/closed-testing-smoke-test-plan.md", "closed-testing smoke plan", blockers);
            RequireFile("docs/store/google-play-closed-testing-gate.md", "Google Play closed-testing gate", blockers);

            for (var vertical = 1; vertical <= 7; vertical++)
            {
                var report = "docs/rewrite/" + (38 + vertical) + "-vertical-" + vertical;
                if (!HasFileStartingWith(report))
                    warnings.Add("Could not find a rewrite report beginning with " + report + ". Keep the implementation handoff reports with the project source.");
            }
        }

        private static void ValidateSdkStatus(List<string> warnings)
        {
            var sdkWarnings = new List<string>();
            var sdkErrors = new List<string>();
            CoreRacerSdkStatusValidator.AppendReadinessMessages(CoreRacerSdkStatusValidator.GetStatuses(), sdkWarnings, sdkErrors);

            foreach (var error in sdkErrors)
                warnings.Add("SDK setup: " + error);
            foreach (var warning in sdkWarnings)
                warnings.Add("SDK setup: " + warning);
        }

        private static void RequireAsset(string path, string displayName, List<string> blockers)
        {
            if (AssetDatabase.LoadMainAssetAtPath(path) == null)
                blockers.Add(displayName + " is missing at " + path + ".");
        }

        private static void RequireFile(string path, string displayName, List<string> blockers)
        {
            if (!File.Exists(path))
                blockers.Add(displayName + " is missing at " + path + ".");
        }


        private static void WarnIfMissing(string path, string message, List<string> warnings)
        {
            if (!File.Exists(path))
                warnings.Add(message + " Expected path: " + path);
        }

        private static bool HasFileStartingWith(string prefix)
        {
            var directory = Path.GetDirectoryName(prefix);
            var name = Path.GetFileName(prefix);
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return false;

            foreach (var file in Directory.GetFiles(directory))
            {
                if (Path.GetFileName(file).StartsWith(name, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool IsYamlUnityAsset(string file)
        {
            var extension = Path.GetExtension(file).ToLowerInvariant();
            return extension == ".unity" || extension == ".prefab" || extension == ".asset";
        }
    }
}
