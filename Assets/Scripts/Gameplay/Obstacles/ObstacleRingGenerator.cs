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
public class ObstacleRingGenerator : MonoBehaviour
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
    }

    private class RingInstance
    {
        public Transform root;
        public ObstacleRingType type;
        public GameObject obstacleInstance;
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

    [Header("Global Pattern Settings (All Types)")]
    [Tooltip("Minimum rings in a type run (Laser/Fan/WedgeX).")]
    [SerializeField] private int minPatternRunLength = 3;

    [Tooltip("Maximum rings in a type run.")]
    [SerializeField] private int maxPatternRunLength = 7;

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
                player = pc.transform;
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

            float z = ring.root.position.z;

            if (playerZ - z > recycleBehindDistance)
            {
                ring.root.position = new Vector3(0f, 0f, _nextSpawnZ);
                _nextSpawnZ += ringSpacing;

                ConfigureRing(ring, i);
            }

            ApplyColor(ring, i, _colorTime);
        }
    }

    private void ConfigureRing(RingInstance ring, int ringIndex)
    {
        EnsurePatternState();

        ObstacleRingType type = _currentPatternType;

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
    }

    #region Type-run state

    private void EnsurePatternState()
    {
        if (_patternRingsRemaining > 0 || _wedgeRunRingsRemaining > 0)
            return;

        // Choose new obstacle type
        _currentPatternType = ChooseRandomObstacleType();

        // Choose pattern mode for non-wedge types
        _currentPatternMode = (RandomValue() < shiftedPatternChance)
            ? PatternMode.ShiftedOrientation
            : PatternMode.RandomOrientationEachRing;

        _patternRingsRemaining = RandomRange(minPatternRunLength, maxPatternRunLength + 1);

        _currentRotationStep = RandomRange(0, Mathf.Max(1, sideCount));
        _rotationStepDelta = RandomRange(-2, 3);
        if (_rotationStepDelta == 0)
            _rotationStepDelta = 1;
    }

    private ObstacleRingType ChooseRandomObstacleType()
    {
        var available = new List<ObstacleRingType>();
        if (obstaclePrefabs != null)
        {
            foreach (var e in obstaclePrefabs)
            {
                if (e != null && e.prefab != null)
                    available.Add(e.type);
            }
        }

        if (available.Count == 0)
            return ObstacleRingType.Wedge; // fallback

        //int idx = RandomRange(0, available.Count);
        int idx = RandomRange(0, 100) <= 40 ? 0 : 1;
        return available[idx];
    }

    private GameObject GetPrefabForType(ObstacleRingType type)
    {
        if (obstaclePrefabs == null) return null;

        foreach (var e in obstaclePrefabs)
        {
            if (e != null && e.prefab != null && e.type == type)
                return e.prefab;
        }
        return null;
    }

    #endregion

    #region Non-wedge configuration

    private void ConfigureNonWedgeRing(RingInstance ring, ObstacleRingType type)
    {
        // Determine orientation step for this ring
        int rotationStep;
        if (_currentPatternMode == PatternMode.RandomOrientationEachRing)
        {
            rotationStep = RandomRange(0, sideCount);
        }
        else
        {
            // Shifted orientation
            rotationStep = _currentRotationStep;
            // Advance for next ring
            int stepChange = RandomRange(-2, 3); // -2..+2
            if (stepChange == 0)
                stepChange = 1; // avoid no movement
            _rotationStepDelta = stepChange;
            _currentRotationStep = Mod(_currentRotationStep + _rotationStepDelta, sideCount);
        }

        float angleStep = 360f / Mathf.Max(3, sideCount);
        float zAngle = rotationStep * angleStep;

        GameObject prefab = GetPrefabForType(type);

        // If type changed or no instance, respawn
        if (ring.obstacleInstance != null && ring.type != type)
        {
            Destroy(ring.obstacleInstance);
            ring.obstacleInstance = null;
        }

        if (prefab != null && ring.obstacleInstance == null)
        {
            ring.obstacleInstance = Instantiate(prefab, ring.root);
        }

        ring.type = type;

        if (ring.obstacleInstance != null)
        {
            var t = ring.obstacleInstance.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.Euler(0f, 0f, zAngle);

            ConfigureObstacleInstanceNonWedge(ring.obstacleInstance, type);
        }
    }

    private void ConfigureObstacleInstanceNonWedge(GameObject instance, ObstacleRingType type)
    {
        if (instance == null) return;

        // Fan
        var fan = instance.GetComponent<FanObstacle>();
        if (fan != null)
        {
            fan.SetSideCount(sideCount);
        }

        // Laser
        var laser = instance.GetComponent<LaserObstacle>();
        if (laser != null)
        {
            laser.SetSideCount(sideCount);
        }

        // Do NOT configure WedgeObstacle here; wedges are handled separately.
    }

    #endregion

    #region Wedge configuration using WedgePatternSet

    private void ConfigureWedgeRing(RingInstance ring, ObstacleRingType type)
    {
        bool isWedgeType = type == ObstacleRingType.Wedge;

        if (!isWedgeType)
        {
            _inWedgeRun = false;
            _currentWedgeSet = null;
            return;
        }

        // Start a new wedge run if none active
        if (!_inWedgeRun || _wedgeRunRingsRemaining <= 0 || _currentWedgeSet == null)
        {
            _currentWedgeSet = ChooseWedgePatternSet();
            if (_currentWedgeSet == null)
            {
                // Fallback: just treat this as a non-wedge ring
                ConfigureNonWedgeRing(ring, type);
                return;
            }

            _inWedgeRun = true;
            _wedgeRunRingsRemaining = RandomRange(_currentWedgeSet.minRings, _currentWedgeSet.maxRings + 1);
            _wedgeCurrentRotationStep = RandomRange(0, Mathf.Max(1, sideCount));

            if (_currentWedgeSet.oneDirectionOnly)
            {
                _wedgeRotationDirectionSign = RandomValue() > 0.5f ? 1 : -1;
            }
            else
            {
                _wedgeRotationDirectionSign = 0; // can go both directions
            }
        }

        // Orientation in side steps -> degrees
        float angleStep = 360f / Mathf.Max(3, sideCount);
        float zAngle = _wedgeCurrentRotationStep * angleStep;

        GameObject prefab = GetPrefabForType(type);
        if (prefab != null && ring.obstacleInstance == null)
        {
            ring.obstacleInstance = Instantiate(prefab, ring.root);
        }

        ring.type = type;

        if (ring.obstacleInstance != null)
        {
            Transform t = ring.obstacleInstance.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.Euler(0f, 0f, zAngle);

            var wedge = ring.obstacleInstance.GetComponent<WedgeObstacle>();
            if (wedge != null)
            {
                wedge.SetSideCount(sideCount);
                wedge.SetLocalPattern(_currentWedgeSet.localPattern);
                wedge.RegeneratePattern();
            }
        }

        // Prepare next ring in this wedge run
        _wedgeRunRingsRemaining--;

        if (_wedgeRunRingsRemaining > 0)
        {
            int maxStep = Mathf.Max(0, _currentWedgeSet.maxRotationStepPerRing);
            int delta = 0;

            if (maxStep > 0)
            {
                if (_currentWedgeSet.oneDirectionOnly)
                {
                    int steps = RandomRange(0, maxStep + 1);
                    int sign = (_wedgeRotationDirectionSign == 0) ? 1 : _wedgeRotationDirectionSign;
                    delta = steps * sign;
                }
                else
                {
                    delta = RandomRange(-maxStep, maxStep + 1);
                }
            }

            _wedgeCurrentRotationStep = Mod(_wedgeCurrentRotationStep + delta, sideCount);
        }
        else
        {
            _inWedgeRun = false;
            _currentWedgeSet = null;
        }
    }

    private WedgePatternSet ChooseWedgePatternSet()
    {
        if (wedgePatternSets == null || wedgePatternSets.Length == 0)
            return null;

        int totalWeight = 0;
        foreach (var set in wedgePatternSets)
        {
            if (set == null) continue;
            totalWeight += Mathf.Max(0, set.weight);
        }

        if (totalWeight <= 0)
        {
            // fallback: just pick the first non-null
            foreach (var set in wedgePatternSets)
            {
                if (set != null) return set;
            }
            return null;
        }

        int roll = RandomRange(0, totalWeight);
        int cumulative = 0;
        foreach (var set in wedgePatternSets)
        {
            if (set == null) continue;
            int w = Mathf.Max(0, set.weight);
            cumulative += w;
            if (roll < cumulative)
                return set;
        }

        // Fallback
        return wedgePatternSets[0];
    }

    #endregion

    #region Color / random / utility

    private void ApplyColor(RingInstance ring, int index, float time)
    {
        if (ring == null || ring.obstacleInstance == null || obstacleColorGradient == null)
            return;

        float t = 0.5f + 0.5f * Mathf.Sin(time);
        Color baseColor = obstacleColorGradient.Evaluate(t);
        Color darkColor = Color.Lerp(baseColor, Color.black, darkenFactor);
        bool isEven = (index % 2) == 0;
        Color c = isEven ? baseColor : darkColor;

        var renderers = ring.obstacleInstance.GetComponentsInChildren<Renderer>();
        foreach (var rend in renderers)
        {
            if (rend != null)
                rend.material.color = c;
        }
    }

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
        foreach (var r in _rings)
        {
            if (r != null && r.root != null)
            {
                Destroy(r.root.gameObject);
            }
        }
        _rings.Clear();

        _patternRingsRemaining = 0;
        _nextSpawnZ = player.position.z + ringSpacing * 4;

        _inWedgeRun = false;
        _currentWedgeSet = null;
        _wedgeRunRingsRemaining = 0;

        for (int i = 0; i < ringBufferCount; i++)
        {
            SpawnRing();
        }
    }

    #endregion
}
