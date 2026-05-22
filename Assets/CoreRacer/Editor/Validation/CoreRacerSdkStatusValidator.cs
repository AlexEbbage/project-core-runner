using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreRacer.Bootstrap;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CoreRacer.Editor.Validation
{
    public enum SdkAdapterKind
    {
        UnityPurchasing,
        LevelPlay,
        FirebaseAnalytics,
        FirebaseCrashlytics,
        Addressables,
        MobileNotifications
    }

    public sealed class SdkStatus
    {
        public SdkAdapterKind Kind;
        public string DisplayName;
        public bool PackageOrPluginInstalled;
        public bool ReflectedApiPresent;
        public bool CompileSymbolPresent;
        public bool AdapterAssignedOrPresent;
        public bool AdapterEnabled => PackageOrPluginInstalled && ReflectedApiPresent && CompileSymbolPresent && AdapterAssignedOrPresent;
        public string ManualSetup;
    }

    public static class CoreRacerSdkStatusValidator
    {
        private const string IapSymbol = "CORE_RACER_UNITY_IAP";
        private const string LevelPlaySymbol = "CORE_RACER_LEVELPLAY";
        private const string FirebaseSymbol = "CORE_RACER_FIREBASE";
        private const string CrashlyticsSymbol = "CORE_RACER_FIREBASE_CRASHLYTICS";
        private const string AddressablesSymbol = "CORE_RACER_ADDRESSABLES";
        private const string MobileNotificationsSymbol = "CORE_RACER_MOBILE_NOTIFICATIONS";

        [MenuItem("Tools/Core Racer/Validate SDK Status")]
        public static void ValidateSdkStatus()
        {
            var statuses = GetStatuses();
            var warnings = new List<string>();
            var errors = new List<string>();
            AppendReadinessMessages(statuses, warnings, errors);

            var report = string.Join("\n", statuses.Select(FormatStatus));
            Debug.Log("Core Racer SDK status report:\n" + report);
            if (errors.Count > 0)
                Debug.LogError($"Core Racer SDK status found {errors.Count} blocking issue(s):\n- {string.Join("\n- ", errors)}\n\n{report}");
            else if (warnings.Count > 0)
                Debug.LogWarning($"Core Racer SDK status found {warnings.Count} warning(s):\n- {string.Join("\n- ", warnings)}\n\n{report}");
            else
                Debug.Log("Core Racer SDK status validation passed.\n\n" + report);
        }

        [MenuItem("Tools/Core Racer/Wire Safe SDK Adapters")]
        public static void WireSafeSdkAdapters()
        {
            var bootstrapper = UnityEngine.Object.FindObjectOfType<GameBootstrapper>();
            if (bootstrapper == null)
            {
                Debug.LogError("Core Racer SDK adapter wiring failed: GameBootstrapper is missing from the open scene.");
                return;
            }

            var statuses = GetStatuses();
            var host = FindOrCreateAdapterHost(bootstrapper.transform);
            var serialized = new SerializedObject(bootstrapper);
            var changed = false;

            if (IsReadyForSceneWiring(statuses, SdkAdapterKind.UnityPurchasing))
            {
                EnsureComponent(host, "CoreRacer.Monetisation.Iap.UnityPurchasingAdapter, CoreRacer.Runtime", ref changed);
            }

            if (IsReadyForSceneWiring(statuses, SdkAdapterKind.FirebaseAnalytics))
            {
                var component = EnsureComponent(host, "CoreRacer.Services.Analytics.FirebaseAnalyticsServiceAdapter, CoreRacer.Runtime", ref changed);
                changed |= AssignObjectReference(serialized, "analyticsServiceBehaviour", component);
            }

            if (IsReadyForSceneWiring(statuses, SdkAdapterKind.MobileNotifications))
            {
                var component = EnsureComponent(host, "CoreRacer.Services.Notifications.MobilePushNotificationService, CoreRacer.Runtime", ref changed);
                changed |= AssignObjectReference(serialized, "pushNotificationServiceBehaviour", component);
            }

            if (IsReadyForSceneWiring(statuses, SdkAdapterKind.FirebaseCrashlytics))
            {
                var component = EnsureComponent(host, "CoreRacer.Services.Crash.FirebaseCrashlyticsAdapter, CoreRacer.Runtime", ref changed);
                changed |= AssignObjectReference(serialized, "crashReportingServiceBehaviour", component);
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            if (changed)
            {
                EditorUtility.SetDirty(bootstrapper);
                EditorSceneManager.MarkSceneDirty(bootstrapper.gameObject.scene);
                EditorSceneManager.SaveScene(bootstrapper.gameObject.scene);
                Debug.Log("Core Racer SDK adapters wired for verified installed SDKs. Disabled SDKs were left unassigned.");
            }
            else
            {
                Debug.Log("Core Racer SDK adapter wiring made no changes.");
            }
        }

        public static List<SdkStatus> GetStatuses()
        {
            var bootstrapper = UnityEngine.Object.FindObjectOfType<GameBootstrapper>();
            var serialized = bootstrapper != null ? new SerializedObject(bootstrapper) : null;

            return new List<SdkStatus>
            {
                new SdkStatus
                {
                    Kind = SdkAdapterKind.UnityPurchasing,
                    DisplayName = "Unity IAP / Unity Purchasing",
                    PackageOrPluginInstalled = ManifestContains("com.unity.purchasing"),
                    ReflectedApiPresent = HasType("UnityEngine.Purchasing.UnityPurchasing") && HasType("UnityEngine.Purchasing.IDetailedStoreListener"),
                    CompileSymbolPresent = HasSymbol(IapSymbol),
                    AdapterAssignedOrPresent = HasComponent("CoreRacer.Monetisation.Iap.UnityPurchasingAdapter"),
                    ManualSetup = "Install Unity Purchasing, enable CORE_RACER_UNITY_IAP, and keep premium_user routed through IapPurchaseService."
                },
                new SdkStatus
                {
                    Kind = SdkAdapterKind.LevelPlay,
                    DisplayName = "LevelPlay / IronSource",
                    PackageOrPluginInstalled = Directory.Exists("Assets/LevelPlay") || ManifestContains("com.unity.services.levelplay"),
                    ReflectedApiPresent = HasType("Unity.Services.LevelPlay.LevelPlaySDK") || HasType("Unity.Services.LevelPlay.LevelPlayRewardedAd") || HasType("IronSource.Agent"),
                    CompileSymbolPresent = HasSymbol(LevelPlaySymbol),
                    AdapterAssignedOrPresent = IsAssigned(serialized, "rewardedAdServiceBehaviour") || IsAssigned(serialized, "interstitialAdServiceBehaviour"),
                    ManualSetup = "Install the LevelPlay C# SDK/API, verify exact rewarded/interstitial callbacks, then enable CORE_RACER_LEVELPLAY and assign the adapters."
                },
                new SdkStatus
                {
                    Kind = SdkAdapterKind.FirebaseAnalytics,
                    DisplayName = "Firebase Analytics",
                    PackageOrPluginInstalled = File.Exists("Assets/Firebase/Plugins/Firebase.Analytics.dll"),
                    ReflectedApiPresent = HasType("Firebase.Analytics.FirebaseAnalytics") && HasType("Firebase.FirebaseApp"),
                    CompileSymbolPresent = HasSymbol(FirebaseSymbol),
                    AdapterAssignedOrPresent = IsAssignedTo(serialized, "analyticsServiceBehaviour", "CoreRacer.Services.Analytics.FirebaseAnalyticsServiceAdapter"),
                    ManualSetup = "Install Firebase Analytics, verify FirebaseApp and FirebaseAnalytics APIs, enable CORE_RACER_FIREBASE, and assign the analytics adapter."
                },
                new SdkStatus
                {
                    Kind = SdkAdapterKind.FirebaseCrashlytics,
                    DisplayName = "Firebase Crashlytics",
                    PackageOrPluginInstalled = File.Exists("Assets/Firebase/Plugins/Firebase.Crashlytics.dll"),
                    ReflectedApiPresent = HasType("Firebase.Crashlytics.Crashlytics"),
                    CompileSymbolPresent = HasSymbol(CrashlyticsSymbol),
                    AdapterAssignedOrPresent = IsAssignedTo(serialized, "crashReportingServiceBehaviour", "CoreRacer.Services.Crash.FirebaseCrashlyticsAdapter"),
                    ManualSetup = "Install Firebase Crashlytics, verify Firebase.Crashlytics.Crashlytics APIs, then enable CORE_RACER_FIREBASE_CRASHLYTICS and assign the adapter."
                },
                new SdkStatus
                {
                    Kind = SdkAdapterKind.Addressables,
                    DisplayName = "Addressables",
                    PackageOrPluginInstalled = ManifestContains("com.unity.addressables"),
                    ReflectedApiPresent = HasType("UnityEngine.AddressableAssets.Addressables"),
                    CompileSymbolPresent = HasSymbol(AddressablesSymbol),
                    AdapterAssignedOrPresent = HasComponent("CoreRacer.Services.Assets.AddressablesAssetProvider"),
                    ManualSetup = "Install com.unity.addressables, verify Addressables APIs, enable CORE_RACER_ADDRESSABLES, and use AddressablesAssetProvider where content needs it."
                },
                new SdkStatus
                {
                    Kind = SdkAdapterKind.MobileNotifications,
                    DisplayName = "Mobile Notifications",
                    PackageOrPluginInstalled = ManifestContains("com.unity.mobile.notifications"),
                    ReflectedApiPresent = HasType("Unity.Notifications.Android.AndroidNotificationCenter") && HasType("Unity.Notifications.iOS.iOSNotificationCenter"),
                    CompileSymbolPresent = HasSymbol(MobileNotificationsSymbol),
                    AdapterAssignedOrPresent = IsAssignedTo(serialized, "pushNotificationServiceBehaviour", "CoreRacer.Services.Notifications.MobilePushNotificationService"),
                    ManualSetup = "Install Unity Mobile Notifications, verify Android/iOS notification APIs, enable CORE_RACER_MOBILE_NOTIFICATIONS, and assign the notification adapter."
                }
            };
        }

        public static void AppendReadinessMessages(IReadOnlyList<SdkStatus> statuses, List<string> warnings, List<string> errors)
        {
            foreach (var status in statuses)
            {
                if (status.CompileSymbolPresent && !status.PackageOrPluginInstalled)
                {
                    errors.Add($"{status.DisplayName} compile symbol is set but the SDK package/plugin is not installed.");
                    continue;
                }

                if (status.CompileSymbolPresent && !status.ReflectedApiPresent)
                {
                    errors.Add($"{status.DisplayName} compile symbol is set but the expected API type was not found by reflection.");
                    continue;
                }

                if (status.PackageOrPluginInstalled && status.ReflectedApiPresent && !status.CompileSymbolPresent)
                {
                    warnings.Add($"{status.DisplayName} is installed and reflected, but its Core Racer compile symbol is not set.");
                    continue;
                }

                if (status.PackageOrPluginInstalled && !status.ReflectedApiPresent)
                {
                    warnings.Add($"{status.DisplayName} assets/package were found, but no supported C# API type was reflected. {status.ManualSetup}");
                    continue;
                }

                if (!status.PackageOrPluginInstalled)
                {
                    warnings.Add($"{status.DisplayName} is not installed. {status.ManualSetup}");
                    continue;
                }

                if (status.CompileSymbolPresent && status.ReflectedApiPresent && !status.AdapterAssignedOrPresent)
                    warnings.Add($"{status.DisplayName} is enabled but no clean-scene adapter is assigned/present.");
            }
        }

        private static string FormatStatus(SdkStatus status)
        {
            return $"{status.DisplayName}: installed={status.PackageOrPluginInstalled}, api={status.ReflectedApiPresent}, symbol={status.CompileSymbolPresent}, adapter={status.AdapterAssignedOrPresent}, enabled={status.AdapterEnabled}";
        }

        private static bool IsReadyForSceneWiring(IEnumerable<SdkStatus> statuses, SdkAdapterKind kind)
        {
            var status = statuses.FirstOrDefault(item => item.Kind == kind);
            return status != null && status.PackageOrPluginInstalled && status.ReflectedApiPresent && status.CompileSymbolPresent;
        }

        private static GameObject FindOrCreateAdapterHost(Transform bootstrapper)
        {
            var existing = bootstrapper.Find("SdkAdapters");
            if (existing != null)
                return existing.gameObject;

            var host = new GameObject("SdkAdapters");
            Undo.RegisterCreatedObjectUndo(host, "Create SDK Adapters");
            host.transform.SetParent(bootstrapper, false);
            return host;
        }

        private static Component EnsureComponent(GameObject host, string assemblyQualifiedTypeName, ref bool changed)
        {
            var type = Type.GetType(assemblyQualifiedTypeName);
            if (type == null || !typeof(Component).IsAssignableFrom(type))
            {
                Debug.LogWarning("Cannot wire SDK component because the type is unavailable: " + assemblyQualifiedTypeName);
                return null;
            }

            var component = host.GetComponent(type);
            if (component != null)
                return component;

            changed = true;
            return Undo.AddComponent(host, type);
        }

        private static bool AssignObjectReference(SerializedObject serialized, string fieldName, UnityEngine.Object value)
        {
            if (serialized == null || value == null)
                return false;

            var property = serialized.FindProperty(fieldName);
            if (property == null || property.objectReferenceValue == value)
                return false;

            property.objectReferenceValue = value;
            return true;
        }

        private static bool ManifestContains(string packageName)
        {
            const string manifestPath = "Packages/manifest.json";
            return File.Exists(manifestPath) && File.ReadAllText(manifestPath).Contains("\"" + packageName + "\"");
        }

        private static bool HasSymbol(string symbol)
        {
            return HasSymbol(EditorUserBuildSettings.selectedBuildTargetGroup, symbol) ||
                   HasSymbol(BuildTargetGroup.Android, symbol) ||
                   HasSymbol(BuildTargetGroup.Standalone, symbol);
        }

        private static bool HasSymbol(BuildTargetGroup group, string symbol)
        {
            if (group == BuildTargetGroup.Unknown)
                return false;

            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            return defines.Split(';').Any(item => string.Equals(item.Trim(), symbol, StringComparison.Ordinal));
        }

        private static bool HasType(string fullName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Any(assembly => assembly.GetType(fullName, false) != null);
        }

        private static bool HasComponent(string fullName)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(fullName, false))
                .FirstOrDefault(candidate => candidate != null && typeof(Component).IsAssignableFrom(candidate));
            return type != null && UnityEngine.Object.FindObjectOfType(type) != null;
        }

        private static bool IsAssigned(SerializedObject serialized, string fieldName)
        {
            if (serialized == null)
                return false;

            var property = serialized.FindProperty(fieldName);
            return property != null && property.objectReferenceValue != null;
        }

        private static bool IsAssignedTo(SerializedObject serialized, string fieldName, string typeName)
        {
            if (serialized == null)
                return false;

            var property = serialized.FindProperty(fieldName);
            return property?.objectReferenceValue != null && property.objectReferenceValue.GetType().FullName == typeName;
        }
    }
}
