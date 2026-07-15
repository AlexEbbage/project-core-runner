using CoreRacer.UI.MainMenu;
using UnityEditor;
using UnityEngine;

namespace CoreRacer.Editor.Playability
{
    public static class CoreRacerPlayabilityMenu
    {
        private const string MenuPath = "Tools/Core Racer/Playability/Start Core Run";
        private const string PendingQuickPlayKey = "CoreRacer.Playability.PendingQuickPlay";

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;

            if (EditorApplication.isPlaying && SessionState.GetBool(PendingQuickPlayKey, false))
                EditorApplication.delayCall += StartPendingCoreRun;
        }

        [MenuItem(MenuPath)]
        public static void StartCoreRun()
        {
            Time.timeScale = 1f;
            if (EditorApplication.isPlaying)
            {
                StartCoreRunInPlayMode();
                return;
            }

            SessionState.SetBool(PendingQuickPlayKey, true);
            EditorApplication.isPlaying = true;
        }

        private static void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode)
                return;

            if (SessionState.GetBool(PendingQuickPlayKey, false))
                EditorApplication.delayCall += StartPendingCoreRun;
        }

        private static void StartPendingCoreRun()
        {
            SessionState.SetBool(PendingQuickPlayKey, false);
            StartCoreRunInPlayMode();
        }

        private static void StartCoreRunInPlayMode()
        {
            Time.timeScale = 1f;
            var levelSelect = Object.FindObjectOfType<LevelSelectPageController>(true);
            if (levelSelect == null)
            {
                Debug.LogError("Quick Play failed: no active LevelSelectPageController exists in the loaded scene.");
                return;
            }

            if (!levelSelect.TryQuickPlayCoreRun())
                Debug.LogError("Quick Play failed. See the preceding validation errors for the missing run setup.", levelSelect);
        }
    }
}
