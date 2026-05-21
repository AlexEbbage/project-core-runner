#if UNITY_EDITOR
using System.Collections.Generic;
using CoreRacer.FTUE;
using CoreRacer.Localization;
using CoreRacer.UI.Debugging;
using CoreRacer.UI.FTUE;
using UnityEditor;
using UnityEngine;

namespace CoreRacer.Editor.Validation
{
    public static class CoreRacerFtueTutorialValidator
    {
        private static readonly string[] ExpectedStepIds =
        {
            "welcome",
            "move",
            "dodge_first_obstacle",
            "collect_currency",
            "collect_powerup",
            "crash_continue_explanation",
            "first_upgrade_prompt",
            "daily_task_reward_prompt",
            "complete"
        };

        [MenuItem("Tools/Core Racer/Validate FTUE Tutorial")]
        public static void ValidateFtueTutorial()
        {
            var issues = new List<string>();
            ValidateConfig(issues);
            ValidateScene(issues);

            if (issues.Count == 0)
            {
                Debug.Log("Core Racer FTUE tutorial validation passed.");
                return;
            }

            Debug.LogWarning($"Core Racer FTUE tutorial validation found {issues.Count} issue(s):\n- " + string.Join("\n- ", issues));
        }

        private static void ValidateConfig(List<string> issues)
        {
            var config = LoadFirst<TutorialConfig>();
            if (config == null)
            {
                issues.Add("No TutorialConfig found.");
                return;
            }

            if (config.Steps == null || config.Steps.Count != ExpectedStepIds.Length)
                issues.Add($"TutorialConfig should contain {ExpectedStepIds.Length} steps.");

            var table = LoadFirst<StringTable>();
            for (int i = 0; i < ExpectedStepIds.Length && i < config.Steps.Count; i++)
            {
                var step = config.Steps[i];
                if (step == null)
                {
                    issues.Add($"Tutorial step {i} is null.");
                    continue;
                }

                if (step.Id != ExpectedStepIds[i])
                    issues.Add($"Tutorial step {i} expected id '{ExpectedStepIds[i]}' but found '{step.Id}'.");
                if (table != null && table.Get(step.TitleKey) == step.TitleKey)
                    issues.Add($"Missing FTUE title localization key '{step.TitleKey}'.");
                if (table != null && table.Get(step.BodyKey) == step.BodyKey)
                    issues.Add($"Missing FTUE body localization key '{step.BodyKey}'.");
            }
        }

        private static void ValidateScene(List<string> issues)
        {
            if (Object.FindObjectOfType<TutorialDirector>() == null)
                issues.Add("Scene is missing TutorialDirector.");
            if (Object.FindObjectOfType<TutorialOverlayController>(true) == null)
                issues.Add("Scene is missing TutorialOverlayController.");

            var support = Object.FindObjectOfType<SupportDebugPanel>(true);
            if (support == null)
            {
                issues.Add("Scene is missing SupportDebugPanel.");
                return;
            }

            var so = new SerializedObject(support);
            var resetButton = so.FindProperty("resetTutorialButton");
            if (resetButton == null || resetButton.objectReferenceValue == null)
                issues.Add("SupportDebugPanel resetTutorialButton is not wired.");
        }

        private static T LoadFirst<T>() where T : Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            for (int i = 0; i < guids.Length; i++)
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (asset != null)
                    return asset;
            }

            return null;
        }
    }
}
#endif
