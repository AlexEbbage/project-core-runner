using System.Collections.Generic;
using CoreRacer.Bootstrap;
using CoreRacer.Meta.Achievements;
using CoreRacer.Meta.DailyRewards;
using CoreRacer.Meta.Tasks;
using CoreRacer.UI.GameOver;
using CoreRacer.UI.MainMenu;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoreRacer.Editor.Verticals
{
    public static class ProgressionEconomyRetentionVerticalInstaller
    {
        private const string MainScenePath = "Assets/CoreRacer/Scenes/CoreRacer_Main.unity";
        private const string ConfigRoot = "Assets/CoreRacer/Generated/Configs";
        private const string DailyCalendarPath = ConfigRoot + "/DailyRewardCalendar.asset";
        private const string RotatingTaskPoolPath = ConfigRoot + "/RotatingTaskPool.asset";

        [MenuItem("Tools/Core Racer/Vertical 6/Apply Progression Economy Retention")]
        public static void ApplyProgressionEconomyRetention()
        {
            var scene = EnsureMainSceneOpen();
            var bootstrapper = Object.FindObjectOfType<GameBootstrapper>();
            var dailyCalendar = AssetDatabase.LoadAssetAtPath<DailyRewardCalendarConfig>(DailyCalendarPath);
            var taskPool = AssetDatabase.LoadAssetAtPath<TaskPoolDefinition>(RotatingTaskPoolPath);
            var achievements = FindGeneratedAchievements();

            if (bootstrapper != null)
            {
                AssignObject(bootstrapper, "dailyRewardCalendar", dailyCalendar);
                AssignObject(bootstrapper, "rotatingTaskPool", taskPool);
                AssignObjectList(bootstrapper, "achievementDefinitions", achievements);
                EditorUtility.SetDirty(bootstrapper);
            }

            MarkDirty(
                bootstrapper,
                Object.FindObjectOfType<TopBarController>(),
                Object.FindObjectOfType<LabPageController>(),
                Object.FindObjectOfType<ProgressionPageController>(),
                Object.FindObjectOfType<ProgressionHubController>(),
                Object.FindObjectOfType<DailyLoginPageController>(),
                Object.FindObjectOfType<AchievementsPageController>(),
                Object.FindObjectOfType<GameOverController>());

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Vertical 6 progression/economy/retention wiring applied. Validate next, then test run rewards, Lab upgrades, daily reward, tasks, and achievements.");
        }

        [MenuItem("Tools/Core Racer/Vertical 6/Validate Progression Economy Retention")]
        public static void ValidateProgressionEconomyRetention()
        {
            EnsureMainSceneOpen();
            var issues = 0;
            var bootstrapper = Object.FindObjectOfType<GameBootstrapper>();
            var dailyCalendar = AssetDatabase.LoadAssetAtPath<DailyRewardCalendarConfig>(DailyCalendarPath);
            var taskPool = AssetDatabase.LoadAssetAtPath<TaskPoolDefinition>(RotatingTaskPoolPath);
            var achievements = FindGeneratedAchievements();

            Check(bootstrapper != null, "GameBootstrapper exists.", "GameBootstrapper is missing.", ref issues);
            Check(dailyCalendar != null, "DailyRewardCalendar config exists.", "DailyRewardCalendar config is missing.", ref issues);
            Check(taskPool != null, "RotatingTaskPool config exists.", "RotatingTaskPool config is missing.", ref issues);
            Check(achievements.Count > 0, "Generated achievements exist.", "No generated AchievementDefinition assets found.", ref issues);
            Check(Object.FindObjectOfType<TopBarController>() != null, "TopBarController exists.", "TopBarController is missing.", ref issues);
            Check(Object.FindObjectOfType<LabPageController>() != null, "LabPageController exists.", "LabPageController is missing.", ref issues);
            Check(Object.FindObjectOfType<ProgressionPageController>() != null, "ProgressionPageController exists.", "ProgressionPageController is missing.", ref issues);
            Check(Object.FindObjectOfType<ProgressionHubController>() != null, "ProgressionHubController exists.", "ProgressionHubController is missing.", ref issues);
            Check(Object.FindObjectOfType<DailyLoginPageController>() != null, "DailyLoginPageController exists.", "DailyLoginPageController is missing.", ref issues);
            Check(Object.FindObjectOfType<AchievementsPageController>() != null, "AchievementsPageController exists.", "AchievementsPageController is missing.", ref issues);
            Check(Object.FindObjectOfType<GameOverController>() != null, "GameOverController exists.", "GameOverController is missing.", ref issues);

            if (dailyCalendar != null)
                Check(dailyCalendar.Days.Count > 0, "DailyRewardCalendar has reward days.", "DailyRewardCalendar has no reward days.", ref issues);

            if (taskPool != null)
            {
                Check(taskPool.Tasks.Count > 0, "RotatingTaskPool has task definitions.", "RotatingTaskPool has no tasks.", ref issues);
                Check(taskPool.DailySlots > 0, "Daily task slots enabled.", "Daily task slots should be greater than zero.", ref issues);
            }

            if (issues == 0)
                Debug.Log("Vertical 6 Progression Economy Retention validation passed.");
            else
                Debug.LogError($"Vertical 6 Progression Economy Retention validation failed with {issues} issue(s). Fix these before Vertical 7.");
        }

        private static List<AchievementDefinition> FindGeneratedAchievements()
        {
            var result = new List<AchievementDefinition>();
            var guids = AssetDatabase.FindAssets("t:AchievementDefinition", new[] { ConfigRoot });
            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var achievement = AssetDatabase.LoadAssetAtPath<AchievementDefinition>(path);
                if (achievement != null && !result.Contains(achievement))
                    result.Add(achievement);
            }
            return result;
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

        private static void AssignObjectList<T>(Object target, string propertyName, IReadOnlyList<T> values) where T : Object
        {
            if (target == null)
                return;

            var serialized = new SerializedObject(target);
            var prop = serialized.FindProperty(propertyName);
            if (prop == null || !prop.isArray)
            {
                Debug.LogWarning($"Could not assign {propertyName} on {target.name}; serialized list field was not found.");
                return;
            }

            prop.arraySize = values != null ? values.Count : 0;
            for (int i = 0; i < prop.arraySize; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
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
