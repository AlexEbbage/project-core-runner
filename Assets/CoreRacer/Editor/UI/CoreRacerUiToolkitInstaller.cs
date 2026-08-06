using System.Collections.Generic;
using CoreRacer.Gameplay.Powerups;
using CoreRacer.Gameplay.Run;
using CoreRacer.Meta.Shop;
using CoreRacer.UI.Toolkit;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace CoreRacer.Editor.UI
{
    public static class CoreRacerUiToolkitInstaller
    {
        private const string RootName = "GameUiRoot";
        private const string UxmlPath = "Assets/CoreRacer/Runtime/UI/Toolkit/CoreRacerUiRoot.uxml";
        private const string PanelSettingsPath = "Assets/CoreRacer/Runtime/UI/Toolkit/CoreRacerPanelSettings.asset";
        private const string AnimationSettingsPath = "Assets/CoreRacer/Generated/Configs/UiAnimationSettings.asset";

        private static readonly string[] RequiredElementNames =
        {
            "GameUiRoot", "SafeArea", "HudLayer", "ScreenLayer", "OverlayLayer", "PopupLayer", "ToastLayer",
            "MainMenuScreen", "TopBar", "BottomNav", "PlayScreen", "ShopScreen", "HangarScreen", "LabScreen",
            "ProgressionScreen", "SettingsScreen", "PlayButton", "NavPlay", "NavShop", "NavHangar", "NavLab",
            "NavProgression", "PauseButton", "PauseOverlay", "GameOverPopup", "GenericModal", "ComponentGallery"
        };

        [MenuItem("Tools/Core Racer/UI Toolkit/Install Final UI")]
        public static void Install()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogError("[CoreRacer.UI] No loaded scene is available for UI installation.");
                return;
            }

            var run = Object.FindObjectOfType<RunController>(true);
            var references = Object.FindObjectOfType<RunSceneReferences>(true);
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            var animationSettings = LoadOrCreateAnimationSettings();
            if (run == null || references == null || tree == null || panelSettings == null || animationSettings == null)
            {
                Debug.LogError($"[CoreRacer.UI] Install failed. Run={run != null}, References={references != null}, UXML={tree != null}, PanelSettings={panelSettings != null}, AnimationSettings={animationSettings != null}.");
                return;
            }

            if (!ValidateTreeContract(tree, out var contractError))
            {
                Debug.LogError("[CoreRacer.UI] Install aborted because the UXML contract is invalid: " + contractError);
                return;
            }

            var root = GameObject.Find(RootName);
            if (root == null)
            {
                root = new GameObject(RootName);
                Undo.RegisterCreatedObjectUndo(root, "Install Core Racer UI Toolkit root");
            }

            var document = root.GetComponent<UIDocument>() ?? Undo.AddComponent<UIDocument>(root);
            var controller = root.GetComponent<CoreRacerUiController>() ?? Undo.AddComponent<CoreRacerUiController>(root);
            Undo.RecordObject(document, "Configure Core Racer UI Toolkit document");
            document.panelSettings = panelSettings;
            document.visualTreeAsset = tree;
            document.sortingOrder = 100;

            var controllerObject = new SerializedObject(controller);
            Assign(controllerObject, "document", document);
            Assign(controllerObject, "runController", run);
            Assign(controllerObject, "runReferences", references);
            Assign(controllerObject, "animationSettings", animationSettings);
            Copy(run, "levelRoadmap", controllerObject, "levelRoadmap");
            Copy(run, "boosterCatalog", controllerObject, "boosterCatalog");
            Copy(run, "shipDatabase", controllerObject, "shipDatabase");

            Assign(controllerObject, "shopCatalog", LoadFirst<ShopCatalog>());
            Assign(controllerObject, "powerupUpgrades", LoadFirst<PowerupUpgradeConfigV2>());
            var powerupProperty = controllerObject.FindProperty("powerupUpgrades");
            if (powerupProperty != null && powerupProperty.objectReferenceValue == null)
            {
                var powerups = Object.FindObjectOfType<PowerupRuntimeController>(true);
                if (powerups != null)
                    Copy(powerups, "upgradeConfig", controllerObject, "powerupUpgrades");
            }
            controllerObject.ApplyModifiedPropertiesWithoutUndo();

            Undo.RecordObject(references, "Wire Core Racer run UI");
            references.RunUiBehaviour = controller;
            EditorUtility.SetDirty(references);
            EditorUtility.SetDirty(document);
            EditorUtility.SetDirty(controller);

            RemoveSupersededCanvases(root);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Validate();
        }

        [MenuItem("Tools/Core Racer/UI Toolkit/Validate Final UI")]
        public static void Validate()
        {
            var documents = Object.FindObjectsOfType<UIDocument>(true);
            var canvases = Object.FindObjectsOfType<Canvas>(true);
            var references = Object.FindObjectOfType<RunSceneReferences>(true);
            var controller = Object.FindObjectOfType<CoreRacerUiController>(true);
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            var contractValid = ValidateTreeContract(tree, out var contractError);
            var valid = documents.Length == 1 && canvases.Length == 0 && controller != null && references != null &&
                        references.RunUiBehaviour == controller && contractValid;
            if (valid)
            {
                Debug.Log("[CoreRacer.UI] Final UI validated: one UIDocument, no legacy Canvas, modular UXML contract valid, and run presentation wired.", controller);
            }
            else
            {
                Debug.LogError($"[CoreRacer.UI] Validation failed. UIDocuments={documents.Length}, Canvases={canvases.Length}, Controller={controller != null}, RunUiWired={references != null && references.RunUiBehaviour == controller}, UxmlContract={contractValid}. {contractError}");
            }
        }

        private static UiAnimationSettings LoadOrCreateAnimationSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<UiAnimationSettings>(AnimationSettingsPath);
            if (settings != null)
                return settings;

            EnsureFolder("Assets/CoreRacer/Generated/Configs");
            settings = ScriptableObject.CreateInstance<UiAnimationSettings>();
            AssetDatabase.CreateAsset(settings, AnimationSettingsPath);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            return settings;
        }

        private static bool ValidateTreeContract(VisualTreeAsset tree, out string error)
        {
            if (tree == null)
            {
                error = "Root VisualTreeAsset is missing.";
                return false;
            }

            var clone = tree.CloneTree();
            var missing = new List<string>();
            for (var i = 0; i < RequiredElementNames.Length; i++)
            {
                if (clone.Q<VisualElement>(RequiredElementNames[i]) == null)
                    missing.Add(RequiredElementNames[i]);
            }

            if (missing.Count > 0)
            {
                error = "Missing required elements: " + string.Join(", ", missing);
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static void RemoveSupersededCanvases(GameObject uiRoot)
        {
            var canvases = Object.FindObjectsOfType<Canvas>(true);
            for (var i = 0; i < canvases.Length; i++)
            {
                if (canvases[i] == null || canvases[i].gameObject == uiRoot)
                    continue;
                Undo.DestroyObjectImmediate(canvases[i].gameObject);
            }
        }

        private static void EnsureFolder(string path)
        {
            var parts = path.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static void Assign(SerializedObject target, string propertyName, Object value)
        {
            var property = target.FindProperty(propertyName);
            if (property != null)
                property.objectReferenceValue = value;
        }

        private static void Copy(Object source, string sourceName, SerializedObject target, string targetName)
        {
            var sourceProperty = new SerializedObject(source).FindProperty(sourceName);
            var targetProperty = target.FindProperty(targetName);
            if (sourceProperty != null && targetProperty != null)
                targetProperty.objectReferenceValue = sourceProperty.objectReferenceValue;
        }

        private static T LoadFirst<T>() where T : Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            for (var i = 0; i < guids.Length; i++)
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (asset != null)
                    return asset;
            }
            return null;
        }
    }
}
