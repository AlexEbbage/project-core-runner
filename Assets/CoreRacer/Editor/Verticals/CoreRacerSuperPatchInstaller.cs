using System.IO;
using CoreRacer.Config.Gameplay;
using CoreRacer.Config.Run;
using CoreRacer.Gameplay.Camera;
using CoreRacer.Gameplay.Environment;
using CoreRacer.Gameplay.Pickups;
using CoreRacer.Gameplay.Player;
using CoreRacer.Gameplay.Powerups;
using CoreRacer.Gameplay.Run;
using CoreRacer.Meta.Levels;
using CoreRacer.Meta.Ships;
using CoreRacer.UI.GameOver;
using CoreRacer.UI.MainMenu;
using CoreRacer.Editor.Builders;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CoreRacer.Editor.Verticals
{
    /// <summary>
    /// Applies the supplied Verticals 5-8 and then wires the integration/safety additions made by the super patch.
    /// Safe to run repeatedly: components/assets/buttons are reused when already present.
    /// </summary>
    public static class CoreRacerSuperPatchInstaller
    {
        private const string MainScenePath = "Assets/CoreRacer/Scenes/CoreRacer_Main.unity";
        private const string ConfigRoot = "Assets/CoreRacer/Generated/Configs";
        private const string ShipRoot = "Assets/CoreRacer/Generated/Ships";
        private const string ShipDatabasePath = ConfigRoot + "/ShipDatabase.asset";

        [MenuItem("Tools/Core Racer/Super Patch/Apply Verticals 5-8 + Corrections")]
        public static void ApplyAll()
        {
            FinalMenusMetaLoopVerticalInstaller.ApplyFinalMenusMetaLoop();
            ProgressionEconomyRetentionVerticalInstaller.ApplyProgressionEconomyRetention();
            CommercialServicesComplianceVerticalInstaller.ApplyCommercialServicesCompliance();

            var scene = EnsureMainSceneOpen();
            ApplyRuntimeWiring();
            EnsureGameOverActions();
            EnsureMenuActions();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ClosedTestingHardeningVerticalInstaller.ApplyClosedTestingHardening();
            Debug.Log("Core Racer super patch wiring applied and saved to CoreRacer_Main. Enter Play Mode or run Validate Integration for a detailed report.");
        }

        [MenuItem("Tools/Core Racer/Super Patch/Repair Playability Wiring")]
        public static void RepairPlayabilityWiring()
        {
            ApplyAll();
            ValidateIntegration();
        }

        [MenuItem("Tools/Core Racer/Super Patch/Rebuild Generated UI + Reapply (Destructive)")]
        public static void RebuildGeneratedUiAndReapply()
        {
            if (!EditorUtility.DisplayDialog(
                    "Rebuild Core Racer generated UI?",
                    "This deletes and recreates Canvas/MainMenu from the Phase 5 generator, so manual edits under that object will be lost. Continue only if the existing generated UI is visually corrupted.",
                    "Rebuild UI",
                    "Cancel"))
                return;

            EnsureMainSceneOpen();
            CoreRacerPhase5UiBuilder.BuildMainUiFlow();
            ApplyAll();
            ValidateIntegration();
        }

        [MenuItem("Tools/Core Racer/Super Patch/Validate Integration")]
        public static void ValidateIntegration()
        {
            EnsureMainSceneOpen();
            var errors = 0;

            var run = FindSceneObject<RunController>();
            var references = FindSceneObject<RunSceneReferences>();
            var gameOver = FindSceneObject<GameOverController>();
            var levelSelect = FindSceneObject<LevelSelectPageController>();
            var zoneManager = FindSceneObject<RunZoneManagerV2>();
            var contextBuilder = FindSceneObject<PowerupContextBuilder>();
            var router = FindSceneObject<MainMenuPageRouter>();
            var shell = FindSceneObject<MainMenuShell>();
            var bottomNav = FindSceneObject<BottomNavBarController>();
            var eventSystem = FindSceneObject<EventSystem>();
            var canvas = FindSceneObject<Canvas>();

            Check(run != null, "RunController", ref errors);
            Check(references != null, "RunSceneReferences", ref errors);
            Check(gameOver != null, "GameOverController", ref errors);
            Check(levelSelect != null, "LevelSelectPageController", ref errors);
            Check(zoneManager != null, "RunZoneManagerV2", ref errors);
            Check(contextBuilder != null, "PowerupContextBuilder", ref errors);
            Check(router != null, "MainMenuPageRouter", ref errors);
            Check(shell != null, "MainMenuShell", ref errors);
            Check(bottomNav != null, "BottomNavBarController", ref errors);
            Check(eventSystem != null && eventSystem.enabled, "enabled EventSystem", ref errors);
            Check(canvas != null && canvas.GetComponent<GraphicRaycaster>() != null, "Canvas GraphicRaycaster", ref errors);

            if (run != null)
            {
                Check(HasObjectReference(run, "references"), "RunController.references", ref errors);
                Check(HasObjectReference(run, "config"), "RunController.config", ref errors);
                Check(HasObjectReference(run, "levelRoadmap"), "RunController.levelRoadmap", ref errors);
                Check(HasObjectReference(run, "speedScalingConfig"), "RunController.speedScalingConfig", ref errors);
                Check(HasObjectReference(run, "zoneManager"), "RunController.zoneManager", ref errors);
                Check(HasObjectReference(run, "shipDatabase"), "RunController.shipDatabase", ref errors);
            }

            if (references != null)
            {
                var validation = references.ValidateReferences();
                Check(validation.IsValid, "required RunSceneReferences", ref errors);
                Check(references.PlayerCosmetics != null, "RunSceneReferences.PlayerCosmetics", ref errors);
            }

            if (levelSelect != null)
            {
                Check(HasObjectReference(levelSelect, "roadmap"), "LevelSelectPageController.roadmap", ref errors);
                Check(HasObjectReference(levelSelect, "runController"), "LevelSelectPageController.runController", ref errors);
                var playButton = FindButton(levelSelect.transform, "PlayButton");
                Check(playButton != null, "main Play button", ref errors);
                Check(playButton != null && HasPersistentListener(playButton, levelSelect, "PlaySelected"), "main Play button listener", ref errors);
            }

            if (gameOver != null)
            {
                Check(HasObjectReference(gameOver, "runController"), "GameOverController.runController", ref errors);
                Check(HasObjectReference(gameOver, "retryButton"), "GameOver Retry button", ref errors);
                Check(HasObjectReference(gameOver, "hubButton"), "GameOver Hub button", ref errors);
                Check(HasObjectReference(gameOver, "doubleRewardsButton"), "GameOver Double Rewards button", ref errors);
            }

            if (router != null)
            {
                var requiredPages = FinalMenuSetRules.BottomNavigationPages;
                for (var i = 0; i < requiredPages.Length; i++)
                    Check(router.HasPage(requiredPages[i]), "router page " + requiredPages[i], ref errors);
            }

            var player = FindSceneObject<PlayerController>();
            Check(player != null, "PlayerController", ref errors);
            if (player != null)
            {
                Check(player.GetComponent<PickupMagnetController>() != null, "PickupMagnetController on Player", ref errors);
                Check(player.GetComponent<AutoPilotSteeringController>() != null, "AutoPilotSteeringController on Player", ref errors);
                Check(player.GetComponent<PlayerCosmeticsController>() != null, "PlayerCosmeticsController on Player", ref errors);
            }

            Check(AssetDatabase.LoadAssetAtPath<LevelRoadmapConfigV2>(ConfigRoot + "/LevelRoadmap.asset") != null, "LevelRoadmap asset", ref errors);
            Check(AssetDatabase.LoadAssetAtPath<SpeedScalingConfigV2>(ConfigRoot + "/SpeedScaling.asset") != null, "SpeedScaling asset", ref errors);
            Check(AssetDatabase.LoadAssetAtPath<RunZoneCatalog>(ConfigRoot + "/RunZoneCatalog.asset") != null, "RunZoneCatalog asset", ref errors);
            Check(AssetDatabase.LoadAssetAtPath<ShipDatabase>(ShipDatabasePath) != null, "ShipDatabase asset", ref errors);

            if (errors == 0)
                Debug.Log("Core Racer playability integration validation passed.");
            else
                Debug.LogError($"Core Racer playability integration found {errors} issue(s). Run Repair Playability Wiring, then send the complete validation output if any remain.");
        }

        private static void ApplyRuntimeWiring()
        {
            var run = FindSceneObject<RunController>();
            var references = FindSceneObject<RunSceneReferences>();
            var play = FindSceneObject<PlayPageController>();
            var levelSelect = FindSceneObject<LevelSelectPageController>();
            var gameOver = FindSceneObject<GameOverController>();
            var zoneManager = FindSceneObject<RunZoneManagerV2>();
            var cameraFov = FindSceneObject<CameraFovController>();
            var contextBuilder = FindSceneObject<PowerupContextBuilder>();
            var pickupWorld = FindSceneObject<PickupWorldController>();
            var hangar = FindSceneObject<HangarPageController>();
            var player = FindSceneObject<PlayerController>();

            var runConfig = AssetDatabase.LoadAssetAtPath<RunConfig>(ConfigRoot + "/RunConfig.asset");
            var roadmap = AssetDatabase.LoadAssetAtPath<LevelRoadmapConfigV2>(ConfigRoot + "/LevelRoadmap.asset");
            var speed = AssetDatabase.LoadAssetAtPath<SpeedScalingConfigV2>(ConfigRoot + "/SpeedScaling.asset");
            var zones = AssetDatabase.LoadAssetAtPath<RunZoneCatalog>(ConfigRoot + "/RunZoneCatalog.asset");
            var ships = EnsureShipDatabase();

            AssignObject(run, "config", runConfig);
            AssignObject(run, "levelRoadmap", roadmap);
            AssignObject(run, "speedScalingConfig", speed);
            AssignObject(run, "cameraFovController", cameraFov);
            AssignObject(run, "zoneManager", zoneManager);
            AssignObject(run, "shipDatabase", ships);
            AssignObject(play, "runController", run);
            AssignObject(play, "levelSelect", levelSelect);
            AssignObject(levelSelect, "roadmap", roadmap);
            AssignObject(levelSelect, "runController", run);
            AssignObject(gameOver, "runController", run);
            AssignObject(zoneManager, "catalog", zones);
            AssignObject(hangar, "shipDatabase", ships);

            if (player != null)
            {
                var magnet = GetOrAdd<PickupMagnetController>(player.gameObject);
                var autoPilot = GetOrAdd<AutoPilotSteeringController>(player.gameObject);
                var cosmetics = GetOrAdd<PlayerCosmeticsController>(player.gameObject);
                var visualRoot = FindOrCreateChild(player.transform, "ShipVisualRoot");

                AssignObject(cosmetics, "shipRoot", visualRoot);
                AssignObject(cosmetics, "shipDatabase", ships);
                AssignObject(contextBuilder, "Player", references != null ? references.Player : player);
                AssignObject(contextBuilder, "Health", references != null ? references.PlayerHealth : player.GetComponent<PlayerHealth>());
                AssignObject(contextBuilder, "ScoreTracker", references != null ? references.ScoreTracker : null);
                AssignObject(contextBuilder, "CurrencyTracker", references != null ? references.CurrencyTracker : null);
                AssignObject(contextBuilder, "Magnet", magnet);
                AssignObject(contextBuilder, "AutoPilotSteering", autoPilot);
                AssignObject(references, "PlayerCosmetics", cosmetics);
            }

            if (references != null)
                AssignObject(pickupWorld, "statsTracker", references.StatsTracker);

            MarkDirty(run, references, play, levelSelect, gameOver, zoneManager, contextBuilder, pickupWorld, hangar, player);
        }

        private static void EnsureMenuActions()
        {
            var bottomNav = FindSceneObject<BottomNavBarController>();
            if (bottomNav != null)
            {
                EnsurePersistentListener(FindButton(bottomNav.transform, "PlayButton"), bottomNav, "ShowPlay", bottomNav.ShowPlay);
                EnsurePersistentListener(FindButton(bottomNav.transform, "ShopButton"), bottomNav, "ShowShop", bottomNav.ShowShop);
                EnsurePersistentListener(FindButton(bottomNav.transform, "HangarButton"), bottomNav, "ShowHangar", bottomNav.ShowHangar);
                EnsurePersistentListener(FindButton(bottomNav.transform, "LabButton"), bottomNav, "ShowLab", bottomNav.ShowLab);
                EnsurePersistentListener(FindButton(bottomNav.transform, "ProgressionButton"), bottomNav, "ShowProgression", bottomNav.ShowProgression);
                EnsurePersistentListener(FindButton(bottomNav.transform, "SettingsButton"), bottomNav, "ShowSettings", bottomNav.ShowSettings);
            }

            var levelSelect = FindSceneObject<LevelSelectPageController>();
            if (levelSelect != null)
                EnsurePersistentListener(FindButton(levelSelect.transform, "PlayButton"), levelSelect, "PlaySelected", levelSelect.PlaySelected);
        }

        private static void EnsureGameOverActions()
        {
            var gameOver = FindSceneObject<GameOverController>();
            if (gameOver == null)
                return;

            var finalActions = FindChildRecursive(gameOver.transform, "FinalActions");
            var continueActions = FindChildRecursive(gameOver.transform, "ContinueActions");
            var menuButton = FindButton(finalActions, "MenuButton");
            var retryButton = EnsureClonedButton(menuButton, finalActions, "RetryButton", "Retry", 0);
            var doubleButton = EnsureClonedButton(menuButton, finalActions, "DoubleRewardsButton", "Double Rewards", 1);
            var continueButton = FindButton(continueActions, "ContinueButton");
            var declineButton = FindButton(continueActions, "EndRunButton");

            AssignObject(gameOver, "retryButton", retryButton);
            AssignObject(gameOver, "hubButton", menuButton);
            AssignObject(gameOver, "continueButton", continueButton);
            AssignObject(gameOver, "declineContinueButton", declineButton);
            AssignObject(gameOver, "doubleRewardsButton", doubleButton);
            MarkDirty(gameOver, retryButton, doubleButton, menuButton, continueButton, declineButton);
        }

        private static Button EnsureClonedButton(Button template, Transform parent, string objectName, string label, int siblingIndex)
        {
            if (parent == null)
                return null;

            var existing = FindButton(parent, objectName);
            if (existing != null)
                return existing;
            if (template == null)
            {
                Debug.LogWarning($"Could not create {objectName}: GameOver/MenuButton template is missing.");
                return null;
            }

            var clone = Object.Instantiate(template, parent);
            clone.name = objectName;
            clone.onClick = new Button.ButtonClickedEvent();
            clone.transform.SetSiblingIndex(Mathf.Clamp(siblingIndex, 0, parent.childCount - 1));
            var text = clone.GetComponentInChildren<Text>(true);
            if (text != null)
                text.text = label;
            EditorUtility.SetDirty(clone);
            return clone;
        }

        private static ShipDatabase EnsureShipDatabase()
        {
            EnsureFolder("Assets/CoreRacer/Generated");
            EnsureFolder(ShipRoot);
            var database = AssetDatabase.LoadAssetAtPath<ShipDatabase>(ShipDatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<ShipDatabase>();
                AssetDatabase.CreateAsset(database, ShipDatabasePath);
            }

            EnsureUnlockable(database.Ships, ShipRoot + "/Ship_StarterRunner.asset", "starter_runner", "Starter Runner", new ShipStats
            {
                Speed = 50f,
                Handling = 50f,
                Stability = 50f,
                Boost = 50f,
                Energy = 50f
            });
            EnsureUnlockable(database.Skins, ShipRoot + "/Skin_ClassicWhite.asset", "classic_white", "Classic White");
            EnsureUnlockable(database.Trails, ShipRoot + "/Trail_PulseWake.asset", "pulse_wake", "Pulse Wake");
            EnsureUnlockable(database.CoreFx, ShipRoot + "/CoreFx_StarterGlow.asset", "starter_glow", "Starter Glow");

            EnsureUpgrade(database, UpgradeType.ComboMultiplier, "Combo Multiplier", 150, 100);
            EnsureUpgrade(database, UpgradeType.PickupRadius, "Pickup Radius", 125, 90);
            EnsureUpgrade(database, UpgradeType.Handling, "Handling", 125, 90);
            EnsureUpgrade(database, UpgradeType.ShieldRecharge, "Shield Capacity", 200, 125);

            EditorUtility.SetDirty(database);
            return database;
        }

        private static void EnsureUnlockable<T>(System.Collections.Generic.List<T> list, string path, string id, string displayName, ShipStats? stats = null)
            where T : UnlockableDefinition
        {
            var definition = AssetDatabase.LoadAssetAtPath<T>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(definition, path);
            }

            definition.Id = id;
            definition.DisplayName = displayName;
            if (definition is ShipDefinition ship && stats.HasValue)
                ship.BaseStats = stats.Value;
            if (!list.Contains(definition))
                list.Add(definition);
            EditorUtility.SetDirty(definition);
        }

        private static void EnsureUpgrade(ShipDatabase database, UpgradeType type, string displayName, int baseCost, int increase)
        {
            var path = ShipRoot + "/Upgrade_" + type + ".asset";
            var definition = AssetDatabase.LoadAssetAtPath<ShipUpgradeDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<ShipUpgradeDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            definition.UpgradeType = type;
            definition.DisplayName = displayName;
            definition.MaxLevel = 5;
            definition.BaseCost = baseCost;
            definition.CostIncrease = increase;
            if (!database.Upgrades.Contains(definition))
                database.Upgrades.Add(definition);
            EditorUtility.SetDirty(definition);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static Scene EnsureMainSceneOpen()
        {
            var scene = SceneManager.GetActiveScene();
            return scene.path == MainScenePath ? scene : EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
        }

        private static T FindSceneObject<T>() where T : Object
        {
            var candidates = Resources.FindObjectsOfTypeAll<T>();
            for (var i = 0; i < candidates.Length; i++)
            {
                var candidate = candidates[i];
                if (candidate == null || EditorUtility.IsPersistent(candidate))
                    continue;
                if (candidate is Component component && component.gameObject.scene.IsValid())
                    return candidate;
            }
            return null;
        }

        private static T GetOrAdd<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            return component != null ? component : Undo.AddComponent<T>(target);
        }

        private static Transform FindOrCreateChild(Transform parent, string name)
        {
            var child = FindChildRecursive(parent, name);
            if (child != null)
                return child;

            var gameObject = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create " + name);
            gameObject.transform.SetParent(parent, false);
            return gameObject.transform;
        }

        private static Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent == null)
                return null;
            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == name)
                    return child;
                var nested = FindChildRecursive(child, name);
                if (nested != null)
                    return nested;
            }
            return null;
        }

        private static Button FindButton(Transform root, string name)
        {
            if (root == null)
                return null;
            var buttons = root.GetComponentsInChildren<Button>(true);
            for (var i = 0; i < buttons.Length; i++)
                if (buttons[i] != null && buttons[i].name == name)
                    return buttons[i];
            return null;
        }

        private static void EnsurePersistentListener(Button button, Object target, string methodName, UnityAction action)
        {
            if (button == null || target == null || action == null || HasPersistentListener(button, target, methodName))
                return;

            UnityEventTools.AddPersistentListener(button.onClick, action);
            EditorUtility.SetDirty(button);
        }

        private static bool HasPersistentListener(Button button, Object target, string methodName)
        {
            if (button == null || target == null)
                return false;

            for (var i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            {
                if (button.onClick.GetPersistentTarget(i) == target
                    && button.onClick.GetPersistentMethodName(i) == methodName)
                    return true;
            }

            return false;
        }

        private static bool HasObjectReference(Object target, string propertyName)
        {
            if (target == null)
                return false;

            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(propertyName);
            return property != null && property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null;
        }

        private static void AssignObject(Object target, string propertyName, Object value)
        {
            if (target == null)
                return;
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning($"Could not assign {propertyName} on {target.name}; serialized field was not found.");
                return;
            }
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void MarkDirty(params Object[] objects)
        {
            for (var i = 0; i < objects.Length; i++)
                if (objects[i] != null)
                    EditorUtility.SetDirty(objects[i]);
        }

        private static void Check(bool condition, string label, ref int errors)
        {
            if (condition)
                Debug.Log("OK: " + label);
            else
            {
                errors++;
                Debug.LogError("Missing/unwired: " + label);
            }
        }
    }
}
