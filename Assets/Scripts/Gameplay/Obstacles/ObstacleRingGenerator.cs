using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns and recycles obstacle rings in an endless tube.
/// Each ring hosts a single obstacle prefab (fan, laser, wedge, etc.)
/// centered at the tube origin and rotated around Z.
///
/// Features:
/// - Obstacle variants defined per prefab with difficulty gating and run lengths.
/// - Rotation steps configured per difficulty for each prefab.
/// - Color: gradient with alternating light/dark per ring.
/// </summary>
public partial class ObstacleRingGenerator : MonoBehaviour
{
    [System.Serializable]
    public class RotationDifficultyConfig
    {
        [Min(0)] public int difficultyLevel;
        [Min(0)] public int minRotationSteps = 0;
        [Min(0)] public int maxRotationSteps = 0;
        [Tooltip("When enabled, rotation steps can be negative or positive.")]
        public bool allowBothDirections = false;
    }

    [System.Serializable]
    public class ObstacleRingPrefab
    {
        public string id;
        public GameObject prefab;
        [Min(0)] public int weight = 1;
        [Tooltip("Minimum difficulty level required for this prefab.")]
        [Min(0)] public int minDifficulty = 0;
        [Tooltip("Maximum difficulty level allowed for this prefab.")]
        [Min(0)] public int maxDifficulty = 10;
        [Tooltip("Minimum rings in a run for this prefab.")]
        [Min(1)] public int minRunLength = 1;
        [Tooltip("Maximum rings in a run for this prefab.")]
        [Min(1)] public int maxRunLength = 3;
        [Tooltip("Difficulty modifier added to min/max run length per level.")]
        [Min(0)] public int runLengthDifficultyBonus = 0;
        [Tooltip("Optional pickup slot indices to use for this prefab (0-based).")]
        public int[] pickupSlots;
        [Tooltip("Rotation step settings by difficulty level.")]
        public RotationDifficultyConfig[] rotationByDifficulty;
    }

    [System.Serializable]
    public class PowerupEntry
    {
        public PowerupType type;
        [Min(0)] public int weight = 1;
    }

    private class RingInstance
    {
        public Transform root;
        public bool isObstacleRing;
        public bool isDissolving;
        public GameObject obstacleInstance;
        public Renderer[] renderers;
        public ObstacleRingPrefab obstacleConfig;
        public readonly List<GameObject> pickups = new List<GameObject>();
    }

    [System.Serializable]
    public class LaserBehaviorSettings
    {
        public bool enableRotation = false;
        public float rotationSpeed = 45f;
        public bool enableBeamCycling = false;
        public float pulseDuration = 2f;
        [Range(0f, 1f)] public float dutyCycle = 0.6f;
        public bool startBeamsOn = true;
        public bool randomizeBeamCyclePhase = false;
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerController playerController;

    [Header("Shape")]
    [Tooltip("Number of sides for the tunnel polygon.")]
    [SerializeField] private int sideCount = 6;

    [Tooltip("Z spacing between rings.")]
    [SerializeField] private float ringSpacing = 18f;

    [Tooltip("Number of ring instances to keep in memory.")]
    [SerializeField] private int ringBufferCount = 20;

    [Tooltip("How far behind the player a ring can be before we recycle it.")]
    [SerializeField] private float recycleBehindDistance = 40f;

    [Header("Obstacle Types")]
    [SerializeField] private ObstacleRingPrefab[] obstaclePrefabs;

    [Header("Pickups")]
    [SerializeField] private Pickup pickupPrefab;
    [SerializeField] private int pickupSlotsPerRing = 6;
    [SerializeField] private float pickupSlotRadiusOverride = 0f;
    [SerializeField] private int pickupSlotsToFillMin = 1;
    [SerializeField] private int pickupSlotsToFillMax = 2;
    [SerializeField] private float pickupFloatHeight = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float pickupSlotSpawnChance = 0.8f;
    [Range(0f, 1f)]
    [SerializeField] private float obstacleRingPickupChance = 0.35f;
    [Range(0f, 1f)]
    [SerializeField] private float powerupSpawnChance = 0.15f;
    [SerializeField] private PowerupEntry[] powerupEntries;
    [SerializeField] private float pickupClearanceRadius = 0.35f;
    [SerializeField] private int pickupPlacementAttempts = 3;
    [SerializeField] private LayerMask obstacleOverlapMask = ~0;

    [Header("Pickup Chains")]
    [Range(0f, 1f)]
    [SerializeField] private float pickupChainStartChance = 0.6f;
    [SerializeField] private int pickupChainMinRings = 3;
    [SerializeField] private int pickupChainMaxRings = 6;
    [SerializeField] private int pickupChainGapMin = 1;
    [SerializeField] private int pickupChainGapMax = 3;

    [Header("Ring Sequence")]
    [SerializeField] private int obstacleRingIntervalStart = 4;
    [SerializeField] private int obstacleRingIntervalAtMaxDifficulty = 2;

    [Header("Difficulty Scaling")]
    [SerializeField] private bool enableDifficultyScaling = true;
    [Tooltip("Minutes to ramp from difficulty 0 to 1 for normalized scaling.")]
    [SerializeField] private float difficultyRampMinutes = 5f;

    [Header("Laser Behavior")]
    [SerializeField] private LaserBehaviorSettings laserRandomOrientationSettings = new LaserBehaviorSettings();

    [Header("Laser Difficulty Scaling")]
    [SerializeField] private bool scaleLaserWithDifficulty = true;
    [SerializeField] private float laserRotationSpeedAtMaxDifficulty = 90f;
    [SerializeField] private float laserPulseDurationAtMaxDifficulty = 1.2f;
    [Range(0f, 1f)]
    [SerializeField] private float laserDutyCycleAtMaxDifficulty = 0.7f;

    [Header("Colors")]
    [SerializeField] private Gradient obstacleColorGradient;
    [SerializeField] private float colorCycleSpeed = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float darkenFactor = 0.35f;

    [Header("Random")]
    [SerializeField] private int randomSeed = 0;

    private readonly List<RingInstance> _rings = new List<RingInstance>();
    private System.Random _rng;
    private float _nextSpawnZ;
    private float _colorTime;
    private float _difficultyStartTime;
    private int _ringSequenceIndex;
    private int _pickupChainRemaining;
    private int _pickupChainLength;
    private int _pickupChainGapRemaining;
    private float _pickupSpawnChanceMultiplier = 1f;
    private float _pickupRadiusMultiplier = 1f;

    // Global type-run pattern state (applies to all types)
    private ObstacleRingPrefab _currentPatternPrefab;
    private RotationDifficultyConfig _currentRotationConfig;
    private int _patternRingsRemaining;
    private int _patternIndex;
    private int _currentRotationStep;
    private int _rotationDirectionSign;
    private MaterialPropertyBlock _colorPropertyBlock;
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");
    private float _baseDarkenFactor;
    private float _baseColorCycleSpeed;

    public event System.Action OnObstacleRingPassed;

    private void Awake()
    {
        if (player == null)
        {
            var pc = FindFirstObjectByType<PlayerController>();
            if (pc != null)
            {
                player = pc.transform;
                playerController = pc;
            }
        }
        else if (playerController == null)
        {
            playerController = player.GetComponent<PlayerController>();
        }

        if (player == null)
        {
            Debug.LogError("ObstacleRingGenerator: missing player reference.", this);
            enabled = false;
            return;
        }

        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            Debug.LogWarning("ObstacleRingGenerator: no obstacle prefabs configured.", this);
        }

        _rng = (randomSeed != 0)
            ? new System.Random(randomSeed)
            : new System.Random(Random.Range(int.MinValue, int.MaxValue));

        _difficultyStartTime = Time.time;
        _baseDarkenFactor = darkenFactor;
        _baseColorCycleSpeed = colorCycleSpeed;
    }

    private void Start()
    {
        //_nextSpawnZ = player.position.z + ringSpacing;

        //for (int i = 0; i < ringBufferCount; i++)
        //{
        //    SpawnRing();
        //}
    }

    private void Update()
    {
        _colorTime += Time.deltaTime * colorCycleSpeed;
        RecycleRingsAndApplyColors();
    }

    private void SpawnRing()
    {
        float z = _nextSpawnZ;
        _nextSpawnZ += ringSpacing;

        GameObject rootGO = new GameObject($"ObstacleRingRoot_{_rings.Count}");
        rootGO.transform.SetParent(transform, false);
        rootGO.transform.position = new Vector3(0f, 0f, z);

        var ring = new RingInstance
        {
            root = rootGO.transform
        };

        ConfigureRing(ring, _rings.Count);
        _rings.Add(ring);
    }

    private void RecycleRingsAndApplyColors()
    {
        if (player == null) return;

        float playerZ = player.position.z;

        for (int i = 0; i < _rings.Count; i++)
        {
            RingInstance ring = _rings[i];
            if (ring == null || ring.root == null) continue;
            if (ring.isDissolving)
            {
                ApplyColor(ring, i, _colorTime);
                continue;
            }

            float z = ring.root.position.z;
        if (playerZ - z > recycleBehindDistance)
        {
            if (ring.isObstacleRing)
            {
                OnObstacleRingPassed?.Invoke();
            }

                ring.root.position = new Vector3(0f, 0f, _nextSpawnZ);
                _nextSpawnZ += ringSpacing;

                ConfigureRing(ring, i);
            }

            ApplyColor(ring, i, _colorTime);
        }
    }

    private void ConfigureRing(RingInstance ring, int ringIndex)
    {
        ClearPickups(ring);
        bool isObstacleRing = ShouldSpawnObstacleRing();
        ring.isObstacleRing = isObstacleRing;
        if (!isObstacleRing)
        {
            ring.obstacleConfig = null;
            if (ring.obstacleInstance != null)
                ring.obstacleInstance.SetActive(false);
            ConfigurePickupRing(ring);
            return;
        }

        EnsurePatternState();
        ConfigureObstacleRing(ring);

        _patternRingsRemaining--;
        ConfigureObstacleRingPickups(ring);
    }

    private void SetObstacleInstance(RingInstance ring, GameObject instance)
    {
        if (ring == null)
            return;

        ring.obstacleInstance = instance;
        ring.renderers = instance != null ? instance.GetComponentsInChildren<Renderer>() : null;
    }

    private void ReactivateObstacleInstance(GameObject instance)
    {
        if (instance == null)
            return;

        if (!instance.activeSelf)
            instance.SetActive(true);

        var dissolvers = instance.GetComponentsInChildren<ObstacleDissolver>(true);
        foreach (var dissolver in dissolvers)
        {
            if (dissolver != null && !dissolver.gameObject.activeSelf)
                dissolver.gameObject.SetActive(true);
        }
    }

    #region Color / random / utility

    private int RandomRange(int minInclusive, int maxExclusive)
    {
        return _rng.Next(minInclusive, maxExclusive);
    }

    private float RandomValue()
    {
        return (float)_rng.NextDouble();
    }

    private int Mod(int x, int m)
    {
        int r = x % m;
        return r < 0 ? r + m : r;
    }

    private float GetDifficultyMinutes()
    {
        return Mathf.Max(0f, (Time.time - _difficultyStartTime) / 60f);
    }

    private int GetDifficultyLevel()
    {
        if (!enableDifficultyScaling)
            return 0;

        return Mathf.Max(0, Mathf.FloorToInt(GetDifficultyMinutes()));
    }

    private float GetDifficulty01()
    {
        if (!enableDifficultyScaling || difficultyRampMinutes <= 0f)
            return 0f;

        return Mathf.Clamp01(GetDifficultyMinutes() / difficultyRampMinutes);
    }

    public void RebuildAll(int sides)
    {
        sideCount = Mathf.Max(3, sides);

        foreach (var r in _rings)
        {
            if (r != null && r.root != null)
            {
                Destroy(r.root.gameObject);
            }
        }
        _rings.Clear();

        _nextSpawnZ = player.position.z + ringSpacing;
        _difficultyStartTime = Time.time;
        _ringSequenceIndex = 0;
        _pickupChainRemaining = 0;
        _pickupChainLength = 0;
        _pickupChainGapRemaining = 0;
        _pickupSpawnChanceMultiplier = 1f;
        _pickupRadiusMultiplier = 1f;
        _currentPatternPrefab = null;
        _currentRotationConfig = null;
        _patternRingsRemaining = 0;
        _patternIndex = 0;

        for (int i = 0; i < ringBufferCount; i++)
        {
            SpawnRing();
        }
    }

    public void SetColorGradient(Gradient gradient)
    {
        if (gradient == null) return;
        obstacleColorGradient = gradient;
    }

    public void SetColorStyle(float contrastMultiplier, float colorCycleSpeedMultiplier)
    {
        darkenFactor = Mathf.Clamp01(_baseDarkenFactor * NormalizeMultiplier(contrastMultiplier));
        colorCycleSpeed = Mathf.Max(0f, _baseColorCycleSpeed * NormalizeMultiplier(colorCycleSpeedMultiplier));
    }

    public void SetColorStyle(Gradient gradient, float contrastMultiplier, float colorCycleSpeedMultiplier)
    {
        SetColorGradient(gradient);
        SetColorStyle(contrastMultiplier, colorCycleSpeedMultiplier);
    }

    private static float NormalizeMultiplier(float value)
    {
        return value > 0f ? value : 1f;
    }

    /// <summary>
    /// Dissolves the next N rings in front of the player using ObstacleDissolver (if present).
    /// Used for start & continue "safe" zones.
    /// </summary>
    public void DissolveNextRings(int ringCount, float dissolveDuration)
    {
        if (player == null)
            return;

        int clampedCount = Mathf.Max(0, ringCount);
        if (clampedCount == 0)
            return;

        float playerZ = player.position.z;
        var ringsAhead = new List<RingInstance>();
        foreach (var ring in _rings)
        {
            if (ring != null && ring.root != null && ring.root.position.z > playerZ)
            {
                ringsAhead.Add(ring);
            }
        }
        ringsAhead.Sort((a, b) => a.root.position.z.CompareTo(b.root.position.z));

        int dissolveCount = Mathf.Min(clampedCount, ringsAhead.Count);
        for (int i = 0; i < dissolveCount; i++)
        {
            RingInstance ring = ringsAhead[i];
            if (ring == null || ring.isDissolving)
                continue;

            ring.isObstacleRing = false;
            ring.isDissolving = true;
            ClearPickups(ring);

            if (ring.obstacleInstance == null)
            {
                ring.isDissolving = false;
                continue;
            }

            var dissolvers = ring.obstacleInstance.GetComponentsInChildren<ObstacleDissolver>(true);
            if (dissolvers.Length > 0)
            {
                foreach (var dissolver in dissolvers)
                {
                    if (dissolver != null)
                        dissolver.Dissolve(dissolveDuration);
                }
            }
            else
            {
                ring.obstacleInstance.SetActive(false);
            }

            StartCoroutine(FinalizeDissolvedRing(ring, ring.obstacleInstance, dissolveDuration));
        }
    }

    private IEnumerator FinalizeDissolvedRing(RingInstance ring, GameObject obstacleInstance, float dissolveDuration)
    {
        if (dissolveDuration > 0f)
            yield return new WaitForSeconds(dissolveDuration);

        if (obstacleInstance == null)
        {
            if (ring != null)
                ring.isDissolving = false;
            yield break;
        }

        if (ring != null && ring.obstacleInstance == obstacleInstance)
        {
            ring.isDissolving = false;
            obstacleInstance.SetActive(false);
        }
    }

    #endregion
}
