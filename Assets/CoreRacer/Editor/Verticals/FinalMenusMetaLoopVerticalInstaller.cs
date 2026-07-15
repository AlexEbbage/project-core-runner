using CoreRacer.Gameplay.Run;
using CoreRacer.UI.GameOver;
using CoreRacer.UI.MainMenu;
using CoreRacer.UI.Pause;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoreRacer.Editor.Verticals
{
    public static class FinalMenusMetaLoopVerticalInstaller
    {
        private const string MainScenePath = "Assets/CoreRacer/Scenes/CoreRacer_Main.unity";

        [MenuItem("Tools/Core Racer/Vertical 5/Apply Final Menus Meta Loop")]
        public static void ApplyFinalMenusMetaLoop()
        {
            var scene = EnsureMainSceneOpen();
            var run = FindSceneObject<RunController>();
            var router = FindSceneObject<MainMenuPageRouter>();
            var shell = FindSceneObject<MainMenuShell>();
            var topBar = FindSceneObject<TopBarController>();
            var bottomNav = FindSceneObject<BottomNavBarController>();
            var play = FindSceneObject<PlayPageController>();
            var levelSelect = FindSceneObject<LevelSelectPageController>();
            var gameOver = FindSceneObject<GameOverController>();
            var pause = FindSceneObject<PauseMenuController>();

            AssignObject(shell, "router", router);
            AssignObject(shell, "topBar", topBar);
            AssignObject(topBar, "router", router);
            AssignObject(bottomNav, "router", router);
            AssignObject(play, "runController", run);
            AssignObject(levelSelect, "runController", run);
            AssignObject(gameOver, "runController", run);
            AssignObject(pause, "runController", run);

            if (router != null)
            {
                AssignEnum(router, "defaultPage", (int)MainMenuPage.Play);
                EditorUtility.SetDirty(router);
            }

            MarkDirty(shell, topBar, bottomNav, play, levelSelect, gameOver, pause, run);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Vertical 5 final menu/meta-loop wiring applied. Validate next, then test hub -> play -> game over -> retry/hub and all bottom nav pages.");
        }

        [MenuItem("Tools/Core Racer/Vertical 5/Validate Final Menus Meta Loop")]
        public static void ValidateFinalMenusMetaLoop()
        {
            EnsureMainSceneOpen();
            var issues = 0;
            var run = FindSceneObject<RunController>();
            var router = FindSceneObject<MainMenuPageRouter>();
            var shell = FindSceneObject<MainMenuShell>();
            var topBar = FindSceneObject<TopBarController>();
            var bottomNav = FindSceneObject<BottomNavBarController>();
            var play = FindSceneObject<PlayPageController>();
            var hangar = FindSceneObject<HangarPageController>();
            var lab = FindSceneObject<LabPageController>();
            var shop = FindSceneObject<ShopPageController>();
            var progression = FindSceneObject<ProgressionPageController>();
            var gameOver = FindSceneObject<GameOverController>();
            var pause = FindSceneObject<PauseMenuController>();

            Check(run != null, "RunController exists.", "RunController is missing.", ref issues);
            Check(router != null, "MainMenuPageRouter exists.", "MainMenuPageRouter is missing.", ref issues);
            Check(shell != null, "MainMenuShell exists.", "MainMenuShell is missing.", ref issues);
            Check(topBar != null, "TopBarController exists.", "TopBarController is missing.", ref issues);
            Check(bottomNav != null, "BottomNavBarController exists.", "BottomNavBarController is missing.", ref issues);
            Check(play != null, "PlayPageController exists.", "PlayPageController is missing.", ref issues);
            Check(hangar != null, "HangarPageController exists.", "HangarPageController is missing.", ref issues);
            Check(lab != null, "LabPageController exists.", "LabPageController is missing.", ref issues);
            Check(shop != null, "ShopPageController exists.", "ShopPageController is missing.", ref issues);
            Check(progression != null, "ProgressionPageController exists.", "ProgressionPageController is missing.", ref issues);
            Check(gameOver != null, "GameOverController exists.", "GameOverController is missing.", ref issues);
            Check(pause != null, "PauseMenuController exists.", "PauseMenuController is missing.", ref issues);

            if (router != null)
            {
                var bottomPages = FinalMenuSetRules.BottomNavigationPages;
                for (int i = 0; i < bottomPages.Length; i++)
                {
                    var page = bottomPages[i];
                    Check(router.HasPage(page), $"Router has {page} page binding.", $"Router is missing required {page} page binding.", ref issues);
                }

                Check(!FinalMenuSetRules.IsBottomNavigationPage(MainMenuPage.Settings), "Settings is excluded from bottom navigation.", "Settings must not be a bottom navigation page.", ref issues);
            }

            if (issues == 0)
                Debug.Log("Vertical 5 Final Menus Meta Loop validation passed.");
            else
                Debug.LogError($"Vertical 5 Final Menus Meta Loop validation failed with {issues} issue(s). Fix warnings/errors before moving to Vertical 6.");
        }

        private static T FindSceneObject<T>() where T : Object
        {
            var candidates = Resources.FindObjectsOfTypeAll<T>();
            for (int i = 0; i < candidates.Length; i++)
            {
                var candidate = candidates[i];
                if (candidate == null || EditorUtility.IsPersistent(candidate))
                    continue;

                if (candidate is Component component && component.gameObject.scene.IsValid())
                    return candidate;

                if (candidate is GameObject gameObject && gameObject.scene.IsValid())
                    return candidate;
            }

            return null;
        }

        private static Scene EnsureMainSceneOpen()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.path == MainScenePath)
                return scene;

            return EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
        }

        private static void AssignObject(Object target, string propertyName, Object value)
        {
            if (target == null)
                return;

            var serialized = new SerializedObject(target);
            var prop = serialized.FindProperty(propertyName);
            if (prop == null)
            {
                Debug.LogWarning($"Could not assign {propertyName} on {target.name}; serialized field was not found.");
                return;
            }

            prop.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignEnum(Object target, string propertyName, int value)
        {
            if (target == null)
                return;

            var serialized = new SerializedObject(target);
            var prop = serialized.FindProperty(propertyName);
            if (prop == null)
                return;

            prop.enumValueIndex = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void MarkDirty(params Object[] objects)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                    EditorUtility.SetDirty(objects[i]);
            }
        }

        private static void Check(bool condition, string ok, string fail, ref int issues)
        {
            if (condition)
            {
                Debug.Log(ok);
                return;
            }

            issues++;
            Debug.LogError(fail);
        }
    }
}
