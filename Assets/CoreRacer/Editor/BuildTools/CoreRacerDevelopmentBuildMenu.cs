using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CoreRacer.Editor.Build
{
    public static class CoreRacerDevelopmentBuildMenu
    {
        public const string MenuPath = "Tools/Core Racer/Build/Android Development APK";

        [MenuItem(MenuPath)]
        public static void BuildAndroidDevelopmentApk()
        {
            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled && File.Exists(scene.path))
                .Select(scene => scene.path)
                .ToArray();
            if (scenes.Length == 0)
                throw new BuildFailedException("Core Racer Android build requires at least one enabled Build Settings scene.");

            var outputPath = GetAndroidDevelopmentOutputPath();
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? throw new InvalidOperationException("Build output directory is invalid."));

            var previousCustomKeystore = PlayerSettings.Android.useCustomKeystore;
            var previousBuildAppBundle = EditorUserBuildSettings.buildAppBundle;
            try
            {
                // Development APKs use Android debug signing so release credentials never need
                // to be exposed to editor automation or committed to the project.
                PlayerSettings.Android.useCustomKeystore = false;
                EditorUserBuildSettings.buildAppBundle = false;
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = outputPath,
                    target = BuildTarget.Android,
                    targetGroup = BuildTargetGroup.Android,
                    subtarget = (int)StandaloneBuildSubtarget.Player,
                    options = BuildOptions.Development |
                              BuildOptions.AllowDebugging |
                              BuildOptions.ConnectWithProfiler |
                              BuildOptions.CompressWithLz4 |
                              BuildOptions.StrictMode |
                              BuildOptions.DetailedBuildReport
                });

                if (report.summary.result != BuildResult.Succeeded)
                    throw new BuildFailedException($"Core Racer Android Development APK failed: {report.summary.result} ({report.summary.totalErrors} errors).");

                Debug.Log($"[CoreRacer.Build] Android Development APK built: {outputPath} ({report.summary.totalSize / (1024f * 1024f):F1} MB)");
            }
            finally
            {
                PlayerSettings.Android.useCustomKeystore = previousCustomKeystore;
                EditorUserBuildSettings.buildAppBundle = previousBuildAppBundle;
            }
        }

        public static string GetAndroidDevelopmentOutputPath()
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                              ?? throw new InvalidOperationException("Unity project root could not be resolved.");
            return Path.Combine(projectRoot, "Builds", "Android", $"CoreRacer-{PlayerSettings.bundleVersion}-dev.apk");
        }
    }
}
