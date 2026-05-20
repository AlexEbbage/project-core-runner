#if UNITY_EDITOR
using CoreRacer.Gameplay.Run;
using UnityEditor;
using UnityEngine;

namespace CoreRacer.Editor.Validation
{
    public static class CoreRacerSceneWiringValidator
    {
        [MenuItem("Tools/Core Racer/Validate Open Scene Wiring")]
        public static void ValidateOpenScene()
        {
            var issues = 0;
            var runController = Object.FindObjectOfType<RunController>();
            var references = Object.FindObjectOfType<RunSceneReferences>();

            if (runController == null)
            {
                Debug.LogError("Core Racer scene wiring: missing RunController.");
                issues++;
            }

            if (references == null)
            {
                Debug.LogError("Core Racer scene wiring: missing RunSceneReferences.");
                issues++;
            }
            else
            {
                var result = references.ValidateReferences();
                if (!result.IsValid)
                {
                    issues += result.Errors.Count;
                    foreach (var message in result.Errors)
                        Debug.LogError("Core Racer scene wiring: " + message);
                    foreach (var message in result.Warnings)
                        Debug.LogWarning("Core Racer scene wiring: " + message);
                }
            }

            if (issues == 0)
                Debug.Log("Core Racer scene wiring validation passed.");
        }
    }
}
#endif
