using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CoreRacer.Editor.Verticals
{
    /// <summary>
    /// Applies the non-destructive super-patch wiring once per project checkout after scripts compile.
    /// This exists because copying C# files alone does not update serialized scene references.
    /// </summary>
    [InitializeOnLoad]
    public static class CoreRacerSuperPatchAutoInstaller
    {
        private const string PatchVersion = "1.1.0-playability";
        private const string MainScenePath = "Assets/CoreRacer/Scenes/CoreRacer_Main.unity";
        private static bool _queued;

        static CoreRacerSuperPatchAutoInstaller()
        {
            QueueApply();
        }

        [MenuItem("Tools/Core Racer/Super Patch/Force Auto-Wiring Reapply")]
        public static void ForceReapply()
        {
            EditorPrefs.DeleteKey(GetPreferenceKey());
            QueueApply();
        }

        private static void QueueApply()
        {
            if (_queued)
                return;

            _queued = true;
            EditorApplication.delayCall += TryApply;
        }

        private static void TryApply()
        {
            _queued = false;

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                QueueApply();
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (!File.Exists(ToAbsoluteProjectPath(MainScenePath)))
                return;

            var key = GetPreferenceKey();
            if (string.Equals(EditorPrefs.GetString(key), PatchVersion, StringComparison.Ordinal))
                return;

            // Set this before touching assets so any import refresh cannot re-enter the installer.
            EditorPrefs.SetString(key, PatchVersion);
            try
            {
                Debug.Log("Core Racer: applying Super Patch 1.1 playability wiring to CoreRacer_Main...");
                CoreRacerSuperPatchInstaller.ApplyAll();
                CoreRacerSuperPatchInstaller.ValidateIntegration();
                Debug.Log("Core Racer: Super Patch 1.1 playability wiring completed and the scene was saved.");
            }
            catch (Exception exception)
            {
                EditorPrefs.DeleteKey(key);
                Debug.LogException(exception);
                Debug.LogError("Core Racer auto-wiring failed. Run Tools > Core Racer > Super Patch > Repair Playability Wiring after fixing the first Console error.");
            }
        }

        private static string GetPreferenceKey()
        {
            return "CoreRacer.SuperPatch.AutoInstall." + Application.dataPath.Replace('\\', '/');
        }

        private static string ToAbsoluteProjectPath(string assetPath)
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            return Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
