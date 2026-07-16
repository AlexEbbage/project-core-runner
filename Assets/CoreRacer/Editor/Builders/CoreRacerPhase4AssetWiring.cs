#if UNITY_EDITOR
using System.Collections.Generic;
using CoreRacer.Bootstrap;
using CoreRacer.Gameplay.Environment;
using CoreRacer.Gameplay.Obstacles;
using CoreRacer.Gameplay.Pickups;
using CoreRacer.Gameplay.Powerups;
using CoreRacer.Gameplay.Vfx;
using CoreRacer.Services.Audio;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoreRacer.Editor.Builders
{
    public static class CoreRacerPhase4AssetWiring
    {
        private const string GeneratedPrefabFolder = "Assets/CoreRacer/Generated/Prefabs";
        private const string GeneratedConfigFolder = "Assets/CoreRacer/Generated/Configs";

        private const string PlayerVisualPath = GeneratedPrefabFolder + "/PlayerVisual_AssetWired.prefab";
        private const string ObstacleSegmentPath = GeneratedPrefabFolder + "/ObstacleSegment_AssetWired.prefab";
        private const string ObstacleRingPath = GeneratedPrefabFolder + "/ObstacleRing_AssetWired.prefab";
        private const string PickupCoinPath = GeneratedPrefabFolder + "/PickupCoin_AssetWired.prefab";
        private const string PickupPowerupPath = GeneratedPrefabFolder + "/PickupPowerup_AssetWired.prefab";

        private const string RunZoneCatalogPath = GeneratedConfigFolder + "/RunZoneCatalog.asset";
        private const string AudioEventLibraryPath = GeneratedConfigFolder + "/AudioEventLibrary.asset";
        private const string VfxLibraryPath = GeneratedConfigFolder + "/VfxLibrary.asset";

        [MenuItem("Tools/Core Racer/Phase 4 Wire Existing Assets")]
        public static void WireExistingAssets()
        {
            EnsureFolder("Assets/CoreRacer/Generated");
            EnsureFolder(GeneratedPrefabFolder);
            EnsureFolder(GeneratedConfigFolder);

            CreatePlayerVisualPrefab();
            CreateObstaclePrefabs();
            CreatePickupPrefabs();
            var audioLibrary = CreateAudioLibrary();
            var vfxLibrary = CreateVfxLibrary();
            var zoneCatalog = CreateRunZoneCatalog();

            WireGenerationConfigs();
            WireOpenScene(audioLibrary, vfxLibrary, zoneCatalog);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Core Racer Phase 4 existing asset wiring completed.");
        }

        private static void CreatePlayerVisualPrefab()
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Reworked/Player.prefab");
            if (source == null)
            {
                Debug.LogWarning("Player visual source prefab missing. Skipping player visual wrapper.");
                return;
            }

            var contents = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(source));
            try
            {
                var visualRoot = FindChild(contents.transform, "VisualRoot");
                if (visualRoot == null)
                {
                    Debug.LogWarning("Player visual source has no VisualRoot. Skipping player visual wrapper.");
                    return;
                }

                var root = new GameObject("PlayerVisual_AssetWired");
                var copy = Object.Instantiate(visualRoot.gameObject, root.transform);
                copy.name = "VisualRoot";
                copy.transform.localPosition = Vector3.zero;
                copy.transform.localRotation = Quaternion.identity;
                copy.transform.localScale = Vector3.one;
                StripNonVisualComponents(copy);
                AssignMaterial(copy, "Assets/Materials/PlayerMaterial.mat", false);
                PrefabUtility.SaveAsPrefabAsset(root, PlayerVisualPath);
                Object.DestroyImmediate(root);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static void CreateObstaclePrefabs()
        {
            var root = new GameObject("ObstacleSegment_AssetWired");
            root.tag = "Obstacle";
            root.transform.localPosition = new Vector3(3f, 0f, 0f);
            root.transform.localScale = new Vector3(1.35f, 0.55f, 1.35f);

            var mesh = FindMesh("Assets/Prefabs/Obstacles/RingSegment.prefab");
            if (mesh != null)
                root.AddComponent<MeshFilter>().sharedMesh = mesh;

            var renderer = root.AddComponent<MeshRenderer>();
            var materials = FindMaterials("Assets/Prefabs/Obstacles/RingSegment.prefab");
            renderer.sharedMaterials = materials.Length > 0 ? materials : new[] { Load<Material>("Assets/Materials/ObstacleMaterial.mat") };

            var collider = root.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            PrefabUtility.SaveAsPrefabAsset(root, ObstacleSegmentPath);
            Object.DestroyImmediate(root);

            var ring = new GameObject("ObstacleRing_AssetWired");
            var view = ring.AddComponent<ObstacleRingView>();
            var segmentsRoot = new GameObject("SegmentsRoot");
            segmentsRoot.transform.SetParent(ring.transform, false);
            SetSerializedObject(view, "segmentsRoot", segmentsRoot.transform);
            SetSerializedObject(view, "segmentPrefab", Load<GameObject>(ObstacleSegmentPath));
            PrefabUtility.SaveAsPrefabAsset(ring, ObstacleRingPath);
            Object.DestroyImmediate(ring);
        }

        private static void CreatePickupPrefabs()
        {
            CreatePickupPrefab(
                PickupCoinPath,
                "PickupCoin_AssetWired",
                PickupType.Coin,
                PowerupType.Magnet,
                "Assets/Prefabs/Release/CoinPickup.prefab",
                "Assets/Materials/CoinMaterial.mat");

            CreatePickupPrefab(
                PickupPowerupPath,
                "PickupPowerup_AssetWired",
                PickupType.Powerup,
                PowerupType.Shield,
                "Assets/Prefabs/Pickups/Pickup.prefab",
                "Assets/Materials/CoreMaterial.mat");
        }

        private static void CreatePickupPrefab(string path, string name, PickupType type, PowerupType powerupType, string visualSourcePath, string fallbackMaterialPath)
        {
            var root = new GameObject(name);
            var view = root.AddComponent<PickupView>();
            view.Type = type;
            view.PowerupType = powerupType;
            view.Amount = 1;

            var collider = root.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.45f;

            var source = Load<GameObject>(visualSourcePath);
            if (source != null)
            {
                var visual = PrefabUtility.InstantiatePrefab(source) as GameObject;
                if (visual != null)
                {
                    visual.name = "Visual";
                    visual.transform.SetParent(root.transform, false);
                    StripNonVisualComponents(visual);
                    AssignMaterial(visual, fallbackMaterialPath, true);
                }
            }
            else
            {
                var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                visual.name = "Visual";
                visual.transform.SetParent(root.transform, false);
                StripNonVisualComponents(visual);
                AssignMaterial(visual, fallbackMaterialPath, true);
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static AudioEventLibrary CreateAudioLibrary()
        {
            var library = LoadOrCreate<AudioEventLibrary>(AudioEventLibraryPath);
            CoreRacerAudioFeedbackBuilder.Populate(library);
            return library;
        }

        private static VfxLibrary CreateVfxLibrary()
        {
            var library = LoadOrCreate<VfxLibrary>(VfxLibraryPath);
            library.Effects = new List<VfxDefinition>
            {
                Vfx(VfxEventId.PickupBurst, "Assets/Prefabs/VFX/PickupEffectVfxPrefab.prefab"),
                Vfx(VfxEventId.CoinTrail, "Assets/Prefabs/VFX/CoinPickupVfxPrefab.prefab"),
                Vfx(VfxEventId.PowerupPulse, "Assets/Prefabs/VFX/BoosterPickupVfxPrefab.prefab"),
                Vfx(VfxEventId.ShieldShell, "Assets/Prefabs/VFX/ShieldDuringVfxPrefab.prefab"),
                Vfx(VfxEventId.ShieldBreak, "Assets/Prefabs/VFX/ShieldEndVfxPrefab.prefab"),
                Vfx(VfxEventId.CrashSparks, "Assets/Prefabs/VFX/HitSparksVfxPrefab.prefab"),
                Vfx(VfxEventId.ContinueRespawnWarp, "Assets/Prefabs/VFX/DeathImpactVfxPrefab.prefab")
            };
            EditorUtility.SetDirty(library);
            return library;
        }

        private static RunZoneCatalog CreateRunZoneCatalog()
        {
            var catalog = LoadOrCreate<RunZoneCatalog>(RunZoneCatalogPath);
            catalog.Zones = new List<RunZoneDefinition>
            {
                new RunZoneDefinition
                {
                    Id = "neon_hex",
                    DisplayName = "Neon Hex",
                    TunnelMaterial = Load<Material>("Assets/Materials/WallMaterial.mat"),
                    AmbientColor = Color.cyan,
                    FogColor = Color.black,
                    FogDensity = 0.01f
                }
            };
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        private static void WireGenerationConfigs()
        {
            var obstacleConfig = Load<ObstacleGenerationConfig>(GeneratedConfigFolder + "/ObstacleGeneration.asset");
            if (obstacleConfig != null)
            {
                obstacleConfig.RingPrefab = Load<ObstacleRingView>(ObstacleRingPath);
                EditorUtility.SetDirty(obstacleConfig);
            }

            var pickupConfig = Load<PickupGenerationConfig>(GeneratedConfigFolder + "/PickupGeneration.asset");
            if (pickupConfig != null)
            {
                pickupConfig.CoinPrefab = Load<PickupView>(PickupCoinPath);
                pickupConfig.PowerupPrefab = Load<PickupView>(PickupPowerupPath);
                EditorUtility.SetDirty(pickupConfig);
            }
        }

        private static void WireOpenScene(AudioEventLibrary audioLibrary, VfxLibrary vfxLibrary, RunZoneCatalog zoneCatalog)
        {
            var scene = SceneManager.GetActiveScene();
            var bootstrap = Object.FindObjectOfType<GameBootstrapper>();
            if (bootstrap != null)
            {
                SetSerializedObject(bootstrap, "audioEventLibrary", audioLibrary);
                SetSerializedObject(bootstrap, "vfxLibrary", vfxLibrary);
            }

            var player = GameObject.Find("PlayerShip_Prototype");
            if (player != null)
            {
                RemoveComponent<MeshFilter>(player);
                RemoveComponent<MeshRenderer>(player);
                RemoveExistingChild(player.transform, "PlayerVisual_AssetWired");
                var visualPrefab = Load<GameObject>(PlayerVisualPath);
                if (visualPrefab != null)
                {
                    var visual = PrefabUtility.InstantiatePrefab(visualPrefab, player.transform) as GameObject;
                    if (visual != null)
                    {
                        visual.name = "PlayerVisual_AssetWired";
                        visual.transform.localPosition = Vector3.zero;
                        visual.transform.localRotation = Quaternion.identity;
                        visual.transform.localScale = Vector3.one;
                    }
                }
            }

            var tunnelRoot = GameObject.Find("TunnelRoot");
            if (tunnelRoot != null)
            {
                RemoveExistingChild(tunnelRoot.transform, "TunnelV2Prefab");
                var tunnelPrefab = Load<GameObject>("Assets/Prefabs/Reworked/TunnelV2Prefab.prefab");
                GameObject tunnel = null;
                if (tunnelPrefab != null)
                    tunnel = PrefabUtility.InstantiatePrefab(tunnelPrefab, tunnelRoot.transform) as GameObject;

                if (tunnel != null)
                {
                    tunnel.name = "TunnelV2Prefab";
                    tunnel.transform.localPosition = Vector3.zero;
                    tunnel.transform.localRotation = Quaternion.identity;
                    tunnel.transform.localScale = Vector3.one;
                    AssignMaterial(tunnel, "Assets/Materials/WallMaterial.mat", false);
                }

                var zoneManager = tunnelRoot.GetComponent<RunZoneManagerV2>() ?? tunnelRoot.AddComponent<RunZoneManagerV2>();
                SetSerializedObject(zoneManager, "catalog", zoneCatalog);
                var tunnelRenderer = tunnelRoot.GetComponentInChildren<MeshRenderer>(true);
                if (tunnelRenderer != null)
                    SetSerializedObject(zoneManager, "tunnelRenderer", tunnelRenderer);
                zoneManager.ApplyDefaultZone();
                EditorUtility.SetDirty(tunnelRoot);
            }

            var vfxRoot = GameObject.Find("VfxRoot");
            if (vfxRoot == null)
                vfxRoot = new GameObject("VfxRoot");
            var vfxManager = vfxRoot.GetComponent<VfxManager>() ?? vfxRoot.AddComponent<VfxManager>();
            SetSerializedObject(vfxManager, "poolRoot", vfxRoot.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static VfxDefinition Vfx(VfxEventId id, string legacyPrefabPath)
        {
            return new VfxDefinition
            {
                Id = id,
                Prefab = CreateVfxWrapper(id, legacyPrefabPath),
                InitialPoolSize = 4,
                DisableOnLowQuality = false
            };
        }

        private static VfxPooledInstance CreateVfxWrapper(VfxEventId id, string legacyPrefabPath)
        {
            var path = GeneratedPrefabFolder + "/Vfx_" + id + "_AssetWired.prefab";
            var root = new GameObject("Vfx_" + id + "_AssetWired");
            var pooled = root.AddComponent<VfxPooledInstance>();
            var source = Load<GameObject>(legacyPrefabPath);
            if (source != null)
            {
                var visual = PrefabUtility.InstantiatePrefab(source, root.transform) as GameObject;
                if (visual != null)
                {
                    visual.name = "Visual";
                    visual.transform.localPosition = Vector3.zero;
                    visual.transform.localRotation = Quaternion.identity;
                    visual.transform.localScale = Vector3.one;
                }
            }

            SetSerializedObject(pooled, "particles", root.GetComponentsInChildren<ParticleSystem>(true));
            SetSerializedFloat(pooled, "fallbackLifetime", 2f);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return Load<VfxPooledInstance>(path);
        }

        private static Mesh FindMesh(string prefabPath)
        {
            var source = Load<GameObject>(prefabPath);
            if (source == null) return null;
            var filter = source.GetComponentInChildren<MeshFilter>(true);
            return filter != null ? filter.sharedMesh : null;
        }

        private static Material[] FindMaterials(string prefabPath)
        {
            var source = Load<GameObject>(prefabPath);
            if (source == null) return new Material[0];
            var renderer = source.GetComponentInChildren<Renderer>(true);
            return renderer != null ? renderer.sharedMaterials : new Material[0];
        }

        private static T Load<T>(string path) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = Load<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            var name = System.IO.Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static Transform FindChild(Transform root, string name)
        {
            var children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
                if (children[i].name == name)
                    return children[i];
            return null;
        }

        private static void StripNonVisualComponents(GameObject root)
        {
            RemoveMissingMonoBehaviours(root);

            var components = root.GetComponentsInChildren<Component>(true);
            for (int i = components.Length - 1; i >= 0; i--)
            {
                var component = components[i];
                if (component == null || component is Transform)
                    continue;

                var type = component.GetType();
                if (component is Collider || component is Rigidbody || type.FullName == "UnityEngine.InputSystem.PlayerInput")
                    Object.DestroyImmediate(component);
            }

            RemoveMissingMonoBehaviours(root);
        }

        private static void RemoveMissingMonoBehaviours(GameObject root)
        {
            var transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transforms[i].gameObject);
        }

        private static void AssignMaterial(GameObject root, string materialPath, bool onlyWhenMissing)
        {
            var material = Load<Material>(materialPath);
            if (material == null) return;

            var renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;
                if (onlyWhenMissing && renderers[i].sharedMaterial != null) continue;
                renderers[i].sharedMaterial = material;
            }
        }

        private static void RemoveExistingChild(Transform parent, string childName)
        {
            var child = parent.Find(childName);
            if (child != null)
                Object.DestroyImmediate(child.gameObject);
        }

        private static void RemoveComponent<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            if (component != null)
                Object.DestroyImmediate(component);
        }

        private static void SetSerializedObject(Object target, string propertyName, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(propertyName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(target);
            }
        }

        private static void SetSerializedObject(Object target, string propertyName, Object[] values)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(propertyName);
            if (prop != null && prop.isArray)
            {
                prop.arraySize = values.Length;
                for (int i = 0; i < values.Length; i++)
                    prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(target);
            }
        }

        private static void SetSerializedFloat(Object target, string propertyName, float value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(propertyName);
            if (prop != null)
            {
                prop.floatValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(target);
            }
        }
    }
}
#endif
