using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns and recycles obstacle rings in an endless tube.
/// Each ring hosts a single obstacle prefab (fan, laser, wedge, etc.)
/// centered at the tube origin and rotated around Z.
///
/// Features:
/// - Obstacle types: Laser, Fan, WedgeFull, Wedge75, Wedge50, Wedge25
/// - For non-wedge types:
///     * Type X for Y rings, random orientations or shifted orientations
/// - For wedge types:
///     * Uses WedgePatternSet to define "runs" of rings:
///         - local segment pattern (every-other, single gap, etc.)
///         - run length (minRings..maxRings)
///         - max rotation step per ring
///         - one-direction-only vs both directions
///         - weight (how often this set appears)
/// - Color: gradient with alternating light/dark per ring.
/// </summary>
public partial class ObstacleRingGenerator : MonoBehaviour
{
    public enum ObstacleRingType
    {
        Laser,
        Fan,
        Wedge
    }

    [System.Serializable]
    public class ObstacleRingPrefab
    {
        public ObstacleRingType type;
        public GameObject prefab;
        [Min(0)] public int weight = 1;
        [Tooltip("Extra weight added when difficulty ramps to 1.")]
        [Min(0f)] public float weightBonusAtMaxDifficulty = 0f;
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
        public ObstacleRingType type;
        public bool isObstacleRing;
        public bool isDissolving;
        public GameObject obstacleInstance;
        public Renderer[] renderers;
        public readonly List<GameObject> pickups = new List<GameObject>();
    }

    private enum PatternMode
    {
        RandomOrientationEachRing,
        ShiftedOrientation
    }

    [System.Serializable]
    public class WedgePatternSet
    {
        public string id;

        [Tooltip("Local segment pattern for this wedge run.")]
        public WedgeObstacle.LocalPatternType localPattern;

        [Tooltip("Minimum rings in this wedge run.")]
        public int minRings = 3;

        [Tooltip("Maximum rings in this wedge run.")]
        public int maxRings = 7;

        [Tooltip("Max side steps pattern can rotate per subsequent ring.")]
        public int maxRotationStepPerRing = 1;

        [Tooltip("If true, rotation only moves in one direction; otherwise can move left or right.")]
        public bool oneDirectionOnly = false;

        [Tooltip("Relative weight for picking this set among all wedge sets.")]
        public int weight = 1;
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

    [Header("Global Pattern Settings (All Types)")]
    [Tooltip("Minimum rings in a type run (Laser/Fan/WedgeX).")]
    [SerializeField] private int minPatternRunLength = 3;

    [Tooltip("Maximum rings in a type run.")]
    [SerializeField] private int maxPatternRunLength = 7;

    [Header("Difficulty Scaling")]
    [SerializeField] private bool enableDifficultyScaling = true;
    [Tooltip("Distance (in Z) over which difficulty ramps from 0 to 1.")]
    [SerializeField] private float difficultyRampDistance = 600f;
    [SerializeField] private int minPatternRunLengthAtMaxDifficulty = 2;
    [SerializeField] private int maxPatternRunLengthAtMaxDifficulty = 5;
    [Range(0f, 1f)]
    [SerializeField] private float shiftedPatternChanceAtMaxDifficulty = 0.8f;
    [Tooltip("Multiplier applied to wedge rotation steps at max difficulty.")]
    [SerializeField] private float wedgeRotationStepMultiplierAtMaxDifficulty = 1.5f;

    [Tooltip("Chance to use shifted orientation vs random orientation for non-wedge types.")]
    [Range(0f, 1f)]
    [SerializeField] private float shiftedPatternChance = 0.5f;

    [Header("Colors")]
    [SerializeField] private Gradient obstacleColorGradient;
    [SerializeField] private float colorCycleSpeed = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float darkenFactor = 0.35f;

    [Header("Random")]
    [SerializeField] private int randomSeed = 0;

    [Header("Wedge Pattern Sets (Runs)")]
    [Tooltip("List of wedge pattern sets that define how wedges behave over multiple rings.")]
    [SerializeField] private WedgePatternSet[] wedgePatternSets;

    private readonly List<RingInstance> _rings = new List<RingInstance>();
    private System.Random _rng;
    private float _nextSpawnZ;
    private float _colorTime;
    private float _startZ;
    private int _ringSequenceIndex;
    private int _pickupChainRemaining;
    private int _pickupChainLength;
    private int _pickupChainGapRemaining;
    private float _pickupSpawnChanceMultiplier = 1f;

    // Global type-run pattern state (applies to all types)
    private ObstacleRingType _currentPatternType;
    private PatternMode _currentPatternMode;
    private int _patternRingsRemaining;
    private int _currentRotationStep;
    private int _rotationStepDelta; // -2..+2

    // Wedge run state (per wedge "set of rings")
    private bool _inWedgeRun;
    private WedgePatternSet _currentWedgeSet;
    private int _wedgeRunRingsRemaining;
    private int _wedgeCurrentRotationStep;
    private int _wedgeRotationDirectionSign; // +1, -1, or 0 (0 = free both directions)
    private MaterialPropertyBlock _colorPropertyBlock;
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");

    public event System.Action<ObstacleRingType> OnObstacleRingPassed;

    // Debug properties
    public bool InWedgeRunDebug => _inWedgeRun;
    public int WedgeRunRingsRemainingDebug => _wedgeRunRingsRemaining;
    public int WedgeCurrentRotationStepDebug => _wedgeCurrentRotationStep;
    public int WedgeRotationDirectionSignDebug => _wedgeRotationDirectionSign;
    public WedgeObstacle.LocalPatternType WedgeCurrentLocalPatternDebug =>
        _currentWedgeSet != null ? _currentWedgeSet.localPattern : WedgeObstacle.LocalPatternType.SingleGap;
    public int MaxWedgeRotationStepsPerRingDebug =>
        _currentWedgeSet != null ? _currentWedgeSet.maxRotationStepPerRing : 0;

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

        _startZ = player != null ? player.position.z : 0f;
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
                    OnObstacleRingPassed?.Invoke(ring.type);
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
            ring.type = default;
            ConfigurePickupRing(ring);
            return;
        }

        EnsurePatternState();
        ObstacleRingType type = _currentPatternType;
        ring.type = type;

        bool isWedgeType = type == ObstacleRingType.Wedge;

        if (isWedgeType)
        {
            ConfigureWedgeRing(ring, type);
        }
        else
        {
            ConfigureNonWedgeRing(ring, type);
        }

        // Decrement type-run counter (how long we keep this obstacle type)
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

    private float GetDifficulty01()
    {
        if (!enableDifficultyScaling || player == null || difficultyRampDistance <= 0f)
            return 0f;

        float distance = Mathf.Max(0f, player.position.z - _startZ);
        return Mathf.Clamp01(distance / difficultyRampDistance);
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

        _patternRingsRemaining = 0;
        _nextSpawnZ = player.position.z + ringSpacing;
        _startZ = player.position.z;
        _ringSequenceIndex = 0;
        _pickupChainRemaining = 0;
        _pickupChainLength = 0;
        _pickupChainGapRemaining = 0;
        _pickupSpawnChanceMultiplier = 1f;

        _inWedgeRun = false;
        _currentWedgeSet = null;
        _wedgeRunRingsRemaining = 0;

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
