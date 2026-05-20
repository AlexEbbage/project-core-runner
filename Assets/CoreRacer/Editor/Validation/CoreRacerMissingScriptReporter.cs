#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoreRacer.Editor.Validation
{
    public static class CoreRacerMissingScriptReporter
    {
        [MenuItem("Tools/Core Racer/Report Missing Scripts In Open Scene")]
        public static void ReportMissingScripts()
        {
            var count = 0;
            var scene = SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
                count += ReportRecursive(root.transform);

            if (count == 0)
                Debug.Log("No missing scripts found in the open scene.");
            else
                Debug.LogWarning($"Found {count} missing script reference(s). Use Unity's prefab/scene tools to remove or remap them during migration.");
        }

        private static int ReportRecursive(Transform transform)
        {
            var count = 0;
            var components = transform.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Debug.LogWarning("Missing script on: " + GetPath(transform), transform);
                    count++;
                }
            }

            for (int i = 0; i < transform.childCount; i++)
                count += ReportRecursive(transform.GetChild(i));
            return count;
        }

        private static string GetPath(Transform transform)
        {
            var path = transform.name;
            var current = transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }
    }
}
#endif
