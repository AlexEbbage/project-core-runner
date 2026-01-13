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
        public GameObject obstacleInstance;
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
        ClearPickups(ring);
        bool isObstacleRing = ShouldSpawnObstacleRing();
        if (!isObstacleRing)
        {
            ConfigurePickupRing(ring);
            return;
        }

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
        ConfigureObstacleRingPickups(ring);
    }

    #region Type-run state

    private void EnsurePatternState()
    {
        if (_patternRingsRemaining > 0 || _wedgeRunRingsRemaining > 0)
            return;

        float difficulty = GetDifficulty01();
        int minRun = Mathf.RoundToInt(Mathf.Lerp(minPatternRunLength, minPatternRunLengthAtMaxDifficulty, difficulty));
        int maxRun = Mathf.RoundToInt(Mathf.Lerp(maxPatternRunLength, maxPatternRunLengthAtMaxDifficulty, difficulty));
        maxRun = Mathf.Max(maxRun, minRun);
        float shiftedChance = Mathf.Lerp(shiftedPatternChance, shiftedPatternChanceAtMaxDifficulty, difficulty);

        // Choose new obstacle type
        _currentPatternType = ChooseRandomObstacleType(difficulty);

        // Choose pattern mode for non-wedge types
        _currentPatternMode = (RandomValue() < shiftedChance)
            ? PatternMode.ShiftedOrientation
            : PatternMode.RandomOrientationEachRing;

        _patternRingsRemaining = RandomRange(minRun, maxRun + 1);

        _currentRotationStep = RandomRange(0, Mathf.Max(1, sideCount));
        _rotationStepDelta = RandomRange(-2, 3);
        if (_rotationStepDelta == 0)
            _rotationStepDelta = 1;
    }

    private ObstacleRingType ChooseRandomObstacleType(float difficulty)
    {
        var available = new List<ObstacleRingPrefab>();
        if (obstaclePrefabs != null)
        {
            foreach (var e in obstaclePrefabs)
            {
                if (e != null && e.prefab != null)
                    available.Add(e);
            }
        }

        if (available.Count == 0)
            return ObstacleRingType.Wedge; // fallback

        int totalWeight = 0;
        foreach (var entry in available)
        {
            int baseWeight = Mathf.Max(0, entry.weight);
            int bonus = Mathf.RoundToInt(Mathf.Max(0f, entry.weightBonusAtMaxDifficulty) * difficulty);
            totalWeight += Mathf.Max(0, baseWeight + bonus);
        }

        if (totalWeight <= 0)
            return available[0].type;

        int roll = RandomRange(0, totalWeight);
        int cumulative = 0;
        foreach (var entry in available)
        {
            int baseWeight = Mathf.Max(0, entry.weight);
            int bonus = Mathf.RoundToInt(Mathf.Max(0f, entry.weightBonusAtMaxDifficulty) * difficulty);
            int w = Mathf.Max(0, baseWeight + bonus);
            cumulative += w;
            if (roll < cumulative)
                return entry.type;
        }

        return available[0].type;
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
            if (enableDifficultyScaling)
            {
                float difficulty = GetDifficulty01();
                float multiplier = Mathf.Lerp(1f, wedgeRotationStepMultiplierAtMaxDifficulty, difficulty);
                maxStep = Mathf.RoundToInt(maxStep * multiplier);
            }
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

    #region Pickups

    private void ConfigurePickupRing(RingInstance ring)
    {
        if (ring == null || ring.root == null || pickupPrefab == null)
            return;

        if (!EnsurePickupChain())
            return;

        int slotCount = Mathf.Max(1, pickupSlotsPerRing);
        int slotsToFill = RandomRange(pickupSlotsToFillMin, pickupSlotsToFillMax + 1);
        slotsToFill = Mathf.Clamp(slotsToFill, 0, slotCount);

        var usedSlots = new HashSet<int>();
        for (int i = 0; i < slotsToFill; i++)
        {
            float spawnChance = Mathf.Clamp01(pickupSlotSpawnChance * _pickupSpawnChanceMultiplier);
            if (RandomValue() > spawnChance)
                continue;

            bool spawned = TrySpawnPickup(ring, slotCount, usedSlots, ShouldSpawnPowerupInChain());
            if (!spawned)
                break;
        }

        AdvancePickupChain();
    }

    private void ConfigureObstacleRingPickups(RingInstance ring)
    {
        if (ring == null || ring.root == null || pickupPrefab == null)
            return;

        float obstacleChance = Mathf.Clamp01(obstacleRingPickupChance * _pickupSpawnChanceMultiplier);
        if (RandomValue() > obstacleChance)
            return;

        int slotCount = Mathf.Max(1, pickupSlotsPerRing);
        var usedSlots = new HashSet<int>();
        TrySpawnPickup(ring, slotCount, usedSlots, ShouldSpawnPowerupInChain());
    }

    private bool TrySpawnPickup(RingInstance ring, int slotCount, HashSet<int> usedSlots, bool allowPowerup)
    {
        int attempts = Mathf.Max(1, pickupPlacementAttempts);
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            int slotIndex = RandomRange(0, slotCount);
            if (!usedSlots.Add(slotIndex))
                continue;

            float angleStep = 360f / slotCount;
            float angleDeg = slotIndex * angleStep;
            float radius = pickupSlotRadiusOverride > 0f ? pickupSlotRadiusOverride : GetDefaultPickupRadius();

            float angleRad = angleDeg * Mathf.Deg2Rad;
            Vector3 localPos = new Vector3(Mathf.Cos(angleRad) * radius, Mathf.Sin(angleRad) * radius, 0f);
            Vector3 worldPos = ring.root.TransformPoint(localPos);
            if (IsPickupBlocked(worldPos))
                continue;

            var pickup = Instantiate(pickupPrefab, ring.root);
            pickup.transform.localPosition = localPos;
            pickup.transform.localRotation = Quaternion.LookRotation(Vector3.forward, -localPos.normalized);

            bool spawnPowerup = allowPowerup && RandomValue() <= powerupSpawnChance;
            if (spawnPowerup)
            {
                pickup.Configure(PickupType.Powerup, ChooseRandomPowerup());
            }
            else
            {
                pickup.Configure(PickupType.Coin, PowerupType.CoinMultiplier);
            }

            ring.pickups.Add(pickup.gameObject);
            return true;
        }

        return false;
    }

    private bool EnsurePickupChain()
    {
        if (_pickupChainRemaining > 0)
            return true;

        if (_pickupChainGapRemaining > 0)
        {
            _pickupChainGapRemaining--;
            return false;
        }

        if (RandomValue() > pickupChainStartChance)
            return false;

        _pickupChainLength = RandomRange(pickupChainMinRings, pickupChainMaxRings + 1);
        _pickupChainRemaining = _pickupChainLength;
        return true;
    }

    private void AdvancePickupChain()
    {
        if (_pickupChainRemaining <= 0)
            return;

        _pickupChainRemaining--;
        if (_pickupChainRemaining <= 0)
        {
            _pickupChainGapRemaining = RandomRange(pickupChainGapMin, pickupChainGapMax + 1);
        }
    }

    private bool ShouldSpawnPowerupInChain()
    {
        if (_pickupChainRemaining <= 0)
            return false;

        int chainIndex = _pickupChainLength - _pickupChainRemaining;
        int midIndex = _pickupChainLength / 2;
        return chainIndex == 0 || chainIndex == midIndex || _pickupChainRemaining == 1;
    }

    private PowerupType ChooseRandomPowerup()
    {
        if (powerupEntries == null || powerupEntries.Length == 0)
            return PowerupType.CoinMultiplier;

        int totalWeight = 0;
        foreach (var entry in powerupEntries)
        {
            if (entry == null)
                continue;
            totalWeight += Mathf.Max(0, entry.weight);
        }

        if (totalWeight <= 0)
            return powerupEntries[0].type;

        int roll = RandomRange(0, totalWeight);
        int cumulative = 0;
        foreach (var entry in powerupEntries)
        {
            if (entry == null)
                continue;
            int w = Mathf.Max(0, entry.weight);
            cumulative += w;
            if (roll < cumulative)
                return entry.type;
        }

        return powerupEntries[0].type;
    }

    private float GetDefaultPickupRadius()
    {
        return playerController != null ? playerController.TubeRadius : 5f;
    }

    private void ClearPickups(RingInstance ring)
    {
        if (ring == null)
            return;

        foreach (var pickup in ring.pickups)
        {
            if (pickup != null)
                Destroy(pickup);
        }
        ring.pickups.Clear();
    }

    private bool ShouldSpawnObstacleRing()
    {
        int interval = Mathf.RoundToInt(Mathf.Lerp(obstacleRingIntervalStart, obstacleRingIntervalAtMaxDifficulty, GetDifficulty01()));
        interval = Mathf.Max(1, interval);
        _ringSequenceIndex++;
        return _ringSequenceIndex % interval == 0;
    }

    public void SetPickupSpawnChanceMultiplier(float multiplier)
    {
        _pickupSpawnChanceMultiplier = Mathf.Max(0f, multiplier);
    }

    private bool IsPickupBlocked(Vector3 worldPosition)
    {
        return Physics.CheckSphere(worldPosition, pickupClearanceRadius, obstacleOverlapMask);
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
        foreach (var r in _rings)
        {
            if (r != null && r.root != null)
            {
                Destroy(r.root.gameObject);
            }
        }
        _rings.Clear();

        _patternRingsRemaining = 0;
        _nextSpawnZ = player.position.z + ringSpacing * 4.2f;
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

    #endregion
}
