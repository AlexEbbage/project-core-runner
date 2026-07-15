using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CoreRacer.Services.Diagnostics
{
    public readonly struct BuildSceneReadinessInfo
    {
        public readonly string Path;
        public readonly bool Enabled;

        public BuildSceneReadinessInfo(string path, bool enabled)
        {
            Path = path;
            Enabled = enabled;
        }
    }

    public sealed class ClosedTestingReadinessSnapshot
    {
        public bool BuildSceneIsCorrect;
        public bool AndroidBuildTargetSelected;
        public bool BundleIdentifierReady;
        public bool BundleVersionReady;
        public bool BundleVersionCodeReady;
        public bool StoreLinksReady;
        public bool RequiredConfigsReady;
        public bool SceneWiringReady;
        public bool MissingScriptsClear;
        public bool SmokeTestsPresent;
        public bool VerticalDocsPresent;

        public bool IsReady => BuildSceneIsCorrect &&
                               AndroidBuildTargetSelected &&
                               BundleIdentifierReady &&
                               BundleVersionReady &&
                               BundleVersionCodeReady &&
                               StoreLinksReady &&
                               RequiredConfigsReady &&
                               SceneWiringReady &&
                               MissingScriptsClear &&
                               SmokeTestsPresent &&
                               VerticalDocsPresent;
    }

    public static class ClosedTestingReadinessRules
    {
        public const string ExpectedMainScenePath = "Assets/CoreRacer/Scenes/CoreRacer_Main.unity";

        private static readonly Regex AndroidPackagePattern = new Regex(
            @"^[a-zA-Z][a-zA-Z0-9_]*(\.[a-zA-Z][a-zA-Z0-9_]*){2,}$",
            RegexOptions.Compiled);

        public static bool IsExpectedMainScene(string path)
        {
            return string.Equals(path, ExpectedMainScenePath, System.StringComparison.OrdinalIgnoreCase);
        }

        public static bool HasOnlyExpectedEnabledScene(IReadOnlyList<BuildSceneReadinessInfo> scenes)
        {
            if (scenes == null)
                return false;

            var enabledCount = 0;
            for (var i = 0; i < scenes.Count; i++)
            {
                if (!scenes[i].Enabled)
                    continue;

                enabledCount++;
                if (!IsExpectedMainScene(scenes[i].Path))
                    return false;
            }

            return enabledCount == 1;
        }

        public static bool IsProductionBundleIdentifier(string bundleIdentifier)
        {
            if (string.IsNullOrWhiteSpace(bundleIdentifier))
                return false;

            var trimmed = bundleIdentifier.Trim();
            if (!AndroidPackagePattern.IsMatch(trimmed))
                return false;

            var lower = trimmed.ToLowerInvariant();
            if (lower.Contains("defaultcompany") ||
                lower.Contains("companyname") ||
                lower.Contains("example") ||
                lower == "com.company.product" ||
                lower == "com.yourcompany.coreracer")
            {
                return false;
            }

            return true;
        }

        public static bool IsReleaseVersionReady(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                return false;

            var trimmed = version.Trim();
            if (trimmed == "1.0" || trimmed == "0.1" || trimmed == "0.0.1")
                return false;

            var parts = trimmed.Split('.');
            if (parts.Length < 2 || parts.Length > 4)
                return false;

            for (var i = 0; i < parts.Length; i++)
            {
                if (!int.TryParse(parts[i], out var value) || value < 0)
                    return false;
            }

            return true;
        }

        public static bool IsClosedTestingVersionCodeReady(int versionCode)
        {
            return versionCode >= 2;
        }

        public static bool IsProductionSafeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            var trimmed = url.Trim();
            if (!trimmed.StartsWith("https://", System.StringComparison.OrdinalIgnoreCase))
                return false;

            var lower = trimmed.ToLowerInvariant();
            return !lower.Contains("example.com") &&
                   !lower.Contains("localhost") &&
                   !lower.Contains("todo") &&
                   !lower.Contains("your-") &&
                   !lower.Contains("placeholder");
        }
    }
}
