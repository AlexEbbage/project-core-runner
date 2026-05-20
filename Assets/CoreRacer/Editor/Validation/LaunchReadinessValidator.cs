using System.IO;
using UnityEditor;
using UnityEngine;

namespace CoreRacer.Editor.Validation
{
    public static class LaunchReadinessValidator
    {
        [MenuItem("Tools/Core Racer/Validate Launch Readiness")]
        public static void ValidateLaunchReadiness()
        {
            var issues = 0;
            issues += CheckExists("Packages/manifest.json", "Package manifest missing.");
            issues += CheckExists("docs/store/01-google-play-listing-checklist.md", "Store listing checklist missing.");
            issues += CheckExists("docs/rewrite/15-launch-readiness-pass-summary.md", "Launch readiness summary missing.");
            issues += CheckPlaceholder("Assets", "YOUR_" + "LEVELPLAY", "LevelPlay placeholder still present.");
            issues += CheckPlaceholder("Assets", "TODO_" + "PRIVACY", "Privacy placeholder still present.");
            issues += CheckPlaceholder("Assets", "example" + ".com", "Example privacy/terms URL still present.");
            Debug.Log(issues == 0 ? "Launch readiness validation passed." : $"Launch readiness validation found {issues} issue(s). See console warnings.");
        }

        private static int CheckExists(string path, string warning)
        {
            if (File.Exists(path) || Directory.Exists(path)) return 0;
            Debug.LogWarning(warning);
            return 1;
        }

        private static int CheckPlaceholder(string root, string token, string warning)
        {
            if (!Directory.Exists(root)) return 0;
            foreach (var file in Directory.GetFiles(root, "*.*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".meta") || file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".mp3")) continue;
                try
                {
                    if (File.ReadAllText(file).Contains(token))
                    {
                        Debug.LogWarning(warning + " File: " + file);
                        return 1;
                    }
                }
                catch { }
            }
            return 0;
        }
    }
}
