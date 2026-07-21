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
            if (run == null || references == null || tree == null || panelSettings == null)
            {
                Debug.LogError($"[CoreRacer.UI] Install failed. Run={run != null}, References={references != null}, UXML={tree != null}, PanelSettings={panelSettings != null}.");
                return;
            }

            var root = GameObject.Find(RootName);
            if (root == null) root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Install Core Racer UI Toolkit root");
            var document = root.GetComponent<UIDocument>() ?? Undo.AddComponent<UIDocument>(root);
            var controller = root.GetComponent<CoreRacerUiController>() ?? Undo.AddComponent<CoreRacerUiController>(root);
            document.panelSettings = panelSettings;
            document.visualTreeAsset = tree;
            document.sortingOrder = 100;

            var controllerObject = new SerializedObject(controller);
            Assign(controllerObject, "document", document);
            Assign(controllerObject, "runController", run);
            Assign(controllerObject, "runReferences", references);
            Copy(run, "levelRoadmap", controllerObject, "levelRoadmap");
            Copy(run, "boosterCatalog", controllerObject, "boosterCatalog");
            Copy(run, "shipDatabase", controllerObject, "shipDatabase");

            Assign(controllerObject, "shopCatalog", LoadFirst<ShopCatalog>());
            Assign(controllerObject, "powerupUpgrades", LoadFirst<PowerupUpgradeConfigV2>());
            if (controllerObject.FindProperty("powerupUpgrades").objectReferenceValue == null)
            {
                var powerups = Object.FindObjectOfType<PowerupRuntimeController>(true);
                if (powerups != null) Copy(powerups, "upgradeConfig", controllerObject, "powerupUpgrades");
            }
            controllerObject.ApplyModifiedPropertiesWithoutUndo();

            Undo.RecordObject(references, "Wire Core Racer run UI");
            references.RunUiBehaviour = controller;
            EditorUtility.SetDirty(references);
            EditorUtility.SetDirty(document);
            EditorUtility.SetDirty(controller);

            var canvases = Object.FindObjectsOfType<Canvas>(true);
            for (var i = 0; i < canvases.Length; i++)
            {
                if (canvases[i] != null)
                    Undo.DestroyObjectImmediate(canvases[i].gameObject);
            }

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
            var valid = documents.Length == 1 && canvases.Length == 0 && controller != null && references != null && references.RunUiBehaviour == controller;
            if (valid)
                Debug.Log("[CoreRacer.UI] Final UI validated: one UIDocument, no legacy Canvas, and run presentation is wired.", controller);
            else
                Debug.LogError($"[CoreRacer.UI] Validation failed. UIDocuments={documents.Length}, Canvases={canvases.Length}, Controller={controller != null}, RunUiWired={references != null && references.RunUiBehaviour == controller}.");
        }

        private static void Assign(SerializedObject target, string propertyName, Object value)
        {
            var property = target.FindProperty(propertyName);
            if (property != null) property.objectReferenceValue = value;
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
                if (asset != null) return asset;
            }
            return null;
        }
    }
}
