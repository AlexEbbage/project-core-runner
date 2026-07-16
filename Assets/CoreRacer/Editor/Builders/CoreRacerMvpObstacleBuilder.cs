using System.Collections.Generic;
using System.IO;
using CoreRacer.Gameplay.Obstacles;
using UnityEditor;
using UnityEngine;

namespace CoreRacer.Editor.Builders
{
    public static class CoreRacerMvpObstacleBuilder
    {
        private const string PrefabFolder = "Assets/CoreRacer/Generated/Prefabs/ObstacleVariants";
        private const string LegacyPrefabFolder = "Assets/CoreRacer/Generated/Prefabs/ObstacleS";
        private const string PatternFolder = "Assets/CoreRacer/Generated/Configs/Obstacles";
        private const string ConfigPath = "Assets/CoreRacer/Generated/Configs/ObstacleGeneration.asset";
        private const string ObstacleMaterialPath = "Assets/Materials/ObstacleMaterial.mat";
        private const float MvpTunnelRadius = 4f;
        private const float AuthoredTunnelInnerRadius = 7f;
        private const float MvpObstacleScale = MvpTunnelRadius / AuthoredTunnelInnerRadius;
        private const float MvpObstacleRotationOffset = -30f;

        [MenuItem("Tools/Core Racer/Playability/Rebuild MVP Obstacles")]
        public static void Rebuild()
        {
            MigrateLegacyPrefabFolder();
            EnsureFolder(PrefabFolder);
            EnsureFolder(PatternFolder);

            var wallOne = BuildCleanPrefab("Assets/Prefabs/Release/Wall_1_Prefab.prefab", "Obstacle_WedgeGate_Easy", false, false);
            var wallThree = BuildCleanPrefab("Assets/Prefabs/Release/Wall_3_Prefab.prefab", "Obstacle_WedgeGate_Medium", false, false);
            var wallFive = BuildCleanPrefab("Assets/Prefabs/Release/Wall_5_Prefab.prefab", "Obstacle_WedgeGate_Hard", false, false);
            var fan = BuildCleanPrefab("Assets/Prefabs/Release/Fan_2_Prefab.prefab", "Obstacle_Fan", true, false);
            var door = BuildCleanPrefab("Assets/Prefabs/Release/Door_Prefab.prefab", "Obstacle_Door", false, true);

            var patterns = new List<ObstaclePatternDefinition>
            {
                BuildPattern("wedge_easy", "Wedge Gate", ObstacleType.Walls, wallOne, 0f, 2.2f, 6f, 2, 3, 1.15f, 0f, 0f),
                BuildPattern("wedge_medium", "Split Wedges", ObstacleType.Walls, wallThree, 0.9f, 4.5f, 4f, 2, 3, 1.25f, 0f, 0f),
                BuildPattern("fan", "Rotating Fan", ObstacleType.Fan, fan, 1.5f, 999f, 2.5f, 1, 2, 1.65f, 22f, 40f),
                BuildPattern("door", "Sliding Door", ObstacleType.Doors, door, 2.25f, 999f, 2f, 1, 2, 1.8f, 0f, 0f),
                BuildPattern("wedge_hard", "Narrow Wedge Gate", ObstacleType.Walls, wallFive, 3f, 999f, 2f, 2, 3, 1.35f, 0f, 0f)
            };

            var config = AssetDatabase.LoadAssetAtPath<ObstacleGenerationConfig>(ConfigPath);
            if (config == null)
            {
                Debug.LogError($"[CoreRacer.Obstacles] Missing generation config at {ConfigPath}.");
                return;
            }

            config.RingSpacing = 12f;
            config.SpawnStartZ = 36f;
            config.TunnelSides = 6;
            config.BaseDifficulty = 0f;
            config.DifficultyPerSecond = 0.05f;
            config.Patterns.Clear();
            config.Patterns.AddRange(patterns);
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CoreRacer.Obstacles] Rebuilt five clean MVP obstacle prefabs and difficulty-scaled pattern groups.");
        }

        private static GameObject BuildCleanPrefab(string sourcePath, string name, bool rotating, bool door)
        {
            var sourceRoot = PrefabUtility.LoadPrefabContents(sourcePath);
            try
            {
                sourceRoot.name = name;
                foreach (var transform in sourceRoot.GetComponentsInChildren<Transform>(true))
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject);

                foreach (var body in sourceRoot.GetComponentsInChildren<Rigidbody>(true))
                {
                    body.useGravity = false;
                    body.isKinematic = true;
                    body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                }

                var material = AssetDatabase.LoadAssetAtPath<Material>(ObstacleMaterialPath);
                foreach (var renderer in sourceRoot.GetComponentsInChildren<Renderer>(true))
                {
                    if (material != null)
                        renderer.sharedMaterial = material;
                }

                foreach (var collider in sourceRoot.GetComponentsInChildren<Collider>(true))
                {
                    collider.isTrigger = true;
                    collider.gameObject.tag = "Obstacle";
                }

                if (rotating && sourceRoot.GetComponent<ObstacleRingController>() == null)
                    sourceRoot.AddComponent<ObstacleRingController>();

                if (door)
                    ConfigureDoor(sourceRoot);

                var destination = $"{PrefabFolder}/{name}.prefab";
                return PrefabUtility.SaveAsPrefabAsset(sourceRoot, destination);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(sourceRoot);
            }
        }

        private static void ConfigureDoor(GameObject root)
        {
            var controller = root.GetComponent<DoorObstacle>();
            if (controller == null)
                controller = root.AddComponent<DoorObstacle>();

            var left = FindChild(root.transform, "NegativeDoor");
            var right = FindChild(root.transform, "PositiveDoor");
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("doorLeft").objectReferenceValue = left;
            serialized.FindProperty("doorRight").objectReferenceValue = right;
            serialized.FindProperty("openDistance").floatValue = 2.5f;
            serialized.FindProperty("openSpeed").floatValue = 2f;
            serialized.FindProperty("startsOpen").boolValue = true;
            serialized.FindProperty("cycleAutomatically").boolValue = true;
            serialized.FindProperty("cycleSeconds").floatValue = 3.2f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform FindChild(Transform root, string name)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
                if (child.name == name)
                    return child;
            return null;
        }

        private static ObstaclePatternDefinition BuildPattern(
            string id,
            string displayName,
            ObstacleType type,
            GameObject prefab,
            float minimumDifficulty,
            float maximumDifficulty,
            float weight,
            int minimumIterations,
            int maximumIterations,
            float spacingMultiplier,
            float minimumRotationSpeed,
            float maximumRotationSpeed)
        {
            var path = $"{PatternFolder}/ObstaclePattern_{id}.asset";
            var pattern = AssetDatabase.LoadAssetAtPath<ObstaclePatternDefinition>(path);
            if (pattern == null)
            {
                pattern = ScriptableObject.CreateInstance<ObstaclePatternDefinition>();
                AssetDatabase.CreateAsset(pattern, path);
            }

            pattern.Id = id;
            pattern.DisplayName = displayName;
            pattern.Type = type;
            pattern.MinimumSides = 6;
            pattern.MaximumSides = 6;
            pattern.MinimumDifficulty = minimumDifficulty;
            pattern.MaximumDifficulty = maximumDifficulty;
            pattern.Weight = weight;
            pattern.MinRotationDegrees = 0f;
            pattern.MaxRotationDegrees = 300f;
            pattern.MinIterations = minimumIterations;
            pattern.MaxIterations = maximumIterations;
            pattern.ObstaclePrefab = prefab;
            pattern.ObstacleScale = MvpObstacleScale;
            pattern.RotationOffsetDegrees = MvpObstacleRotationOffset;
            pattern.SpacingMultiplier = spacingMultiplier;
            pattern.MinRotationSpeedDegrees = minimumRotationSpeed;
            pattern.MaxRotationSpeedDegrees = maximumRotationSpeed;
            pattern.Segments.Clear();
            EditorUtility.SetDirty(pattern);
            return pattern;
        }

        private static void EnsureFolder(string path)
        {
            var normalized = path.Replace('\\', '/');
            var current = "Assets";
            var parts = normalized.Split('/');
            for (var i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static void MigrateLegacyPrefabFolder()
        {
            if (!AssetDatabase.IsValidFolder(LegacyPrefabFolder) || AssetDatabase.IsValidFolder(PrefabFolder))
                return;

            var error = AssetDatabase.MoveAsset(LegacyPrefabFolder, PrefabFolder);
            if (!string.IsNullOrEmpty(error))
                Debug.LogError($"[CoreRacer.Obstacles] Could not rename the generated obstacle folder: {error}");
        }
    }
}
