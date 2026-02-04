using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ObstacleRingGenerator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The player's transform; rings are spawned ahead of this along +Z and recycled behind.")]
    [SerializeField] private Transform playerTransform;

    [Tooltip("All obstacle ring ScriptableObject configs available to this level.")]
    [SerializeField] private List<ObstacleRingConfig> obstacleRingConfigs = new List<ObstacleRingConfig>();

    [Tooltip("Prefab for pickup-only rings (has PickupRing component).")]
    [SerializeField] private PickupRing pickupRingPrefab;

    [Tooltip("Optional parent transform for all spawned rings.")]
    [SerializeField] private Transform ringsParent;

    [Header("Obstacle Ring Placement")]
    [Tooltip("Distance along Z between consecutive obstacle rings.")]
    [SerializeField] private float obstacleRingSpacing = 10f;

    [Tooltip("Number of obstacle rings to maintain ahead of the player.")]
    [SerializeField] private int obstacleRingsAhead = 10;

    [Tooltip("When an obstacle ring is this far behind the player on Z, recycle it.")]
    [SerializeField] private float obstacleRecycleDistanceBehind = 20f;

    [Header("Door Phase")]
    [Tooltip("Seconds of door-cycle delay per unit of distance along +Z. Farther rings get more negative initial time, so they close later.")]
    [SerializeField] private float doorPhaseDelayPerUnitZ = 0.2f;

    [Header("Pickup Ring Placement")]
    [Tooltip("Distance along Z between consecutive pickup rings.")]
    [SerializeField] private float pickupRingSpacing = 7f;

    [Tooltip("Number of pickup rings to maintain ahead of the player.")]
    [SerializeField] private int pickupRingsAhead = 15;

    [Tooltip("When a pickup ring is this far behind the player on Z, recycle it.")]
    [SerializeField] private float pickupRecycleDistanceBehind = 20f;

    [Header("Pickup / Obstacle Separation")]
    [Tooltip("Minimum Z-distance between pickup rings and obstacle rings to avoid overlap.")]
    [SerializeField] private float minPickupObstacleSeparation = 3f;

    [Header("Pickup Ring Difficulty Scaling")]
    [Tooltip("Initial uniform scale for pickup rings at starting difficulty.")]
    [SerializeField] private float startingPickupRingScale = 1.0f;

    [Tooltip("Minimum uniform scale for pickup rings at high difficulty.")]
    [SerializeField] private float minPickupRingScale = 0.5f;

    [Tooltip("Difficulty level at which pickup rings will reach their minimum scale.")]
    [SerializeField] private float difficultyForMinPickupScale = 100f;

    [Header("Obstacle Dissolve")]
    [SerializeField] private float ringSpawnFadeInDuration = 0.35f;

    [Header("Difficulty Progression")]
    [Tooltip("Starting difficulty level for this run.")]
    [SerializeField] private float startingDifficulty = 0f;

    [Tooltip("If true, difficulty increases over time; otherwise based on distance travelled on Z.")]
    [SerializeField] private bool useTimeBasedDifficulty = true;

    [Tooltip("Difficulty increase per second, if using time-based difficulty.")]
    [SerializeField] private float difficultyIncreasePerSecond = 0.1f;

    [Tooltip("Difficulty increase per meter along +Z, if using distance-based difficulty.")]
    [SerializeField] private float difficultyIncreasePerMeter = 0.01f;

    [Header("Pickup Prefab & Spawn")]
    [Tooltip("Pickup prefab (coin/powerup) that will be spawned on rings.")]
    [SerializeField] private Pickup pickupPrefab;

    [Tooltip("Minimum number of pickup slots to attempt to fill on a pickup ring.")]
    [SerializeField] private int pickupSlotsToFillMin = 3;

    [Tooltip("Maximum number of pickup slots to attempt to fill on a pickup ring.")]
    [SerializeField] private int pickupSlotsToFillMax = 6;

    [Tooltip("Chance per chosen slot to actually spawn a pickup, before any powerup logic.")]
    [SerializeField, Range(0f, 1f)] private float pickupSlotSpawnChance = 1f;

    [Tooltip("Chance that an obstacle ring will get pickups at all.")]
    [SerializeField, Range(0f, 1f)] private float obstacleRingPickupChance = 0.35f;

    [Header("Pickup Powerups")]
    [Tooltip("Chance that a spawned pickup will be a powerup instead of a coin.")]
    [SerializeField, Range(0f, 1f)] private float powerupSpawnChance = 0.25f;

    [Tooltip("Weighted list of powerups to choose from when spawning powerup pickups.")]
    [SerializeField] private PowerupEntry[] powerupEntries;

    [Header("Pickup Placement")]
    [Tooltip("Number of attempts to place a pickup on a ring before giving up.")]
    [SerializeField] private int pickupPlacementAttempts = 3;

    [Tooltip("Radius used when checking for collisions around pickup spawn positions.")]
    [SerializeField] private float pickupClearanceRadius = 0.3f;

    [Tooltip("Layer mask for geometry that should block pickup placement.")]
    [SerializeField] private LayerMask obstacleOverlapMask = ~0;

    [Header("Pickup Chains")]
    [Tooltip("Chance that a new pickup chain will start when not currently in one.")]
    [SerializeField, Range(0f, 1f)] private float pickupChainStartChance = 0.3f;

    [Tooltip("Minimum number of rings in a pickup chain.")]
    [SerializeField] private int pickupChainMinRings = 2;

    [Tooltip("Maximum number of rings in a pickup chain.")]
    [SerializeField] private int pickupChainMaxRings = 5;

    [Tooltip("Minimum number of rings between pickup chains.")]
    [SerializeField] private int pickupChainGapMin = 1;

    [Tooltip("Maximum number of rings between pickup chains.")]
    [SerializeField] private int pickupChainGapMax = 3;

    [Header("Pickup Patterns")]
    [Tooltip("Allow pattern where all slots are filled.")]
    [SerializeField] private bool allowFullRingPattern = true;

    [Tooltip("Allow pattern where every second slot is filled.")]
    [SerializeField] private bool allowAlternatingPattern = true;

    [Tooltip("Allow arc patterns (contiguous block of slots).")]
    [SerializeField] private bool allowArcPattern = true;

    [Tooltip("Allow small cluster patterns (short contiguous block).")]
    [SerializeField] private bool allowClusterPattern = true;

    [Tooltip("Minimum size of an arc pattern in slots.")]
    [SerializeField] private int minArcSize = 3;

    [Tooltip("Maximum size of an arc pattern in slots.")]
    [SerializeField] private int maxArcSize = 8;

    [Tooltip("Minimum size of a cluster pattern in slots.")]
    [SerializeField] private int minClusterSize = 2;

    [Tooltip("Maximum size of a cluster pattern in slots.")]
    [SerializeField] private int maxClusterSize = 4;

    // Runtime difficulty
    private float _currentDifficulty;
    private float _startPlayerZ;

    // Next spawn Z positions
    private float _nextObstacleSpawnZ;
    private float _nextPickupSpawnZ;

    // Obstacle rings
    private readonly List<ObstacleRingController> _tempAheadRingsBuffer = new List<ObstacleRingController>();
    private readonly Dictionary<ObstacleRingConfig, Queue<ObstacleRingController>> _obstaclePools
        = new Dictionary<ObstacleRingConfig, Queue<ObstacleRingController>>();
    private readonly List<ObstacleRingController> _activeObstacleRings
        = new List<ObstacleRingController>();

    // Pickup rings
    private readonly Queue<PickupRing> _pickupRingPool = new Queue<PickupRing>();
    private readonly List<PickupRing> _activePickupRings = new List<PickupRing>();

    // Individual pickup objects
    private readonly Queue<Pickup> _pickupObjectPool = new Queue<Pickup>();

    // Pattern state
    private ObstacleRingConfig _currentPatternConfig;
    private ObstacleRingDifficultyConfig _currentPatternDifficultyConfig;
    private int _currentPatternIterationIndex;
    private int _currentPatternIterationCount;
    private float _currentPatternRotationPerIteration;
    private float _currentPatternDirectionSign;
    private float _lastObstacleRingAngle;

    // Scratch buffers to avoid allocations
    private readonly List<ObstacleRingConfig> _validConfigBuffer = new List<ObstacleRingConfig>();
    private readonly List<ObstacleRingDifficultyConfig> _validDifficultyBuffer = new List<ObstacleRingDifficultyConfig>();

    // Pickup chain state
    private int _pickupChainLength;
    private int _pickupChainRemaining;
    private int _pickupChainGapRemaining;

    // Spawn chance multiplier, tweakable externally
    private float _pickupSpawnChanceMultiplier = 1f;

    private enum PickupPatternType
    {
        FullRing,
        Alternating,
        Arc,
        Cluster
    }

    private readonly List<PickupPatternType> _patternTypesBuffer = new List<PickupPatternType>();
    private readonly List<int> _patternSlotBuffer = new List<int>();

    #region Unity Lifecycle

    private void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("[ObstacleRingGenerator] Player transform is not assigned.");
            enabled = false;
            return;
        }

        InitializeForRun();
    }

    private void Update()
    {
        if (playerTransform == null)
            return;

        UpdateDifficulty();

        float playerZ = playerTransform.position.z;

        RecycleObstacleRings(playerZ);
        RecyclePickupRings(playerZ);

        EnsureObstacleRingsAhead(playerZ);
        EnsurePickupRingsAhead(playerZ);
    }

    #endregion

    #region Run Init / Reset

    /// <summary>
    /// Clears all current rings and re-seeds obstacle and pickup rings ahead of the player.
    /// Call this when starting a new run or after a continue, once the player has been positioned.
    /// </summary>
    public void InitializeForRun()
    {
        if (playerTransform == null)
        {
            Debug.LogError("[ObstacleRingGenerator] Player transform is not assigned.");
            enabled = false;
            return;
        }

        ClearAllRingsImmediate();

        _startPlayerZ = playerTransform.position.z;
        _currentDifficulty = startingDifficulty;

        _nextObstacleSpawnZ = _startPlayerZ + obstacleRingSpacing;
        _nextPickupSpawnZ = _startPlayerZ + pickupRingSpacing;

        for (int i = 0; i < obstacleRingsAhead; i++)
        {
            SpawnNextObstacleRing();
        }

        for (int i = 0; i < pickupRingsAhead; i++)
        {
            SpawnNextPickupRing();
        }
    }

    /// <summary>
    /// Immediately clears all obstacle and pickup rings and resets pattern/chain state.
    /// Does not spawn new rings; call InitializeForRun() for that.
    /// </summary>
    public void ClearAllRingsImmediate()
    {
        // Obstacles
        for (int i = _activeObstacleRings.Count - 1; i >= 0; i--)
        {
            var ring = _activeObstacleRings[i];
            if (ring != null)
            {
                ReleaseObstacleRing(ring);
            }
        }
        _activeObstacleRings.Clear();

        // Pickup rings
        for (int i = _activePickupRings.Count - 1; i >= 0; i--)
        {
            var ring = _activePickupRings[i];
            if (ring != null)
            {
                ReleasePickupRing(ring);
            }
        }
        _activePickupRings.Clear();

        // Pickup chain reset
        _pickupChainLength = 0;
        _pickupChainRemaining = 0;
        _pickupChainGapRemaining = 0;

        // Pattern reset
        _currentPatternConfig = null;
        _currentPatternDifficultyConfig = null;
        _currentPatternIterationIndex = 0;
        _currentPatternIterationCount = 0;
        _currentPatternRotationPerIteration = 0f;
        _lastObstacleRingAngle = 0f;
    }

    #endregion

    #region Difficulty

    private void UpdateDifficulty()
    {
        if (useTimeBasedDifficulty)
        {
            _currentDifficulty += difficultyIncreasePerSecond * Time.deltaTime;
        }
        else
        {
            float distance = playerTransform.position.z - _startPlayerZ;
            _currentDifficulty = startingDifficulty + distance * difficultyIncreasePerMeter;
        }

        if (_currentDifficulty < 0f)
            _currentDifficulty = 0f;
    }

    /// <summary>
    /// Optional external override (e.g. from RunSpeedController).
    /// </summary>
    public void SetDifficulty(float newDifficulty)
    {
        _currentDifficulty = Mathf.Max(0f, newDifficulty);
    }

    #endregion

    #region Obstacle Rings

    private void EnsureObstacleRingsAhead(float playerZ)
    {
        float targetMaxZ = playerZ + obstacleRingsAhead * obstacleRingSpacing;

        while (_nextObstacleSpawnZ <= targetMaxZ)
        {
            SpawnNextObstacleRing();
        }
    }

    private void RecycleObstacleRings(float playerZ)
    {
        for (int i = _activeObstacleRings.Count - 1; i >= 0; i--)
        {
            var ring = _activeObstacleRings[i];
            if (ring == null)
            {
                _activeObstacleRings.RemoveAt(i);
                continue;
            }

            float dz = playerZ - ring.transform.position.z;
            if (dz > obstacleRecycleDistanceBehind)
            {
                ReleaseObstacleRing(ring);
                _activeObstacleRings.RemoveAt(i);
            }
        }
    }

    private void SpawnNextObstacleRing()
    {
        if (obstacleRingConfigs == null || obstacleRingConfigs.Count == 0)
        {
            Debug.LogWarning("[ObstacleRingGenerator] No obstacle ring configs assigned.");
            return;
        }

        if (_currentPatternConfig == null || _currentPatternIterationIndex >= _currentPatternIterationCount)
        {
            StartNewPattern();
        }

        if (_currentPatternConfig == null || _currentPatternDifficultyConfig == null)
            return;

        // Choose direction for this iteration.
        float directionSign = _currentPatternDifficultyConfig.rotateInOneDirectionPerIteration
            ? _currentPatternDirectionSign
            : (Random.value < 0.5f ? -1f : 1f);

        // Accumulate rotation for the ring (around tube axis).
        _lastObstacleRingAngle += _currentPatternRotationPerIteration * directionSign;

        var ring = GetObstacleRingInstance(_currentPatternConfig);
        if (ring == null)
            return;

        if (ringsParent != null)
            ring.transform.SetParent(ringsParent, false);

        ring.transform.position = new Vector3(0f, 0f, _nextObstacleSpawnZ);
        ring.transform.rotation = Quaternion.AngleAxis(_lastObstacleRingAngle, Vector3.forward);

        float speed = Random.Range(_currentPatternDifficultyConfig.minSpeed, _currentPatternDifficultyConfig.maxSpeed);
        ring.SetupForPattern(speed, directionSign);

        // Door phasing
        if (ring.Type == ObstacleType.Doors && doorPhaseDelayPerUnitZ > 0f)
        {
            float distanceAhead = ring.transform.position.z - playerTransform.position.z;
            float initialTime = -distanceAhead * doorPhaseDelayPerUnitZ;
            ring.SetInitialDoorTime(initialTime);
        }

        var visuals = ring.GetComponent<ObstacleRingVisuals>();
        if (visuals != null)
        {
            visuals.SetHiddenImmediate();
            visuals.PlayFadeIn(ringSpawnFadeInDuration);
        }

        // Optional: pickups on obstacle rings (currently disabled).
        // ConfigureObstacleRingPickups(ring);

        _activeObstacleRings.Add(ring);

        _currentPatternIterationIndex++;
        _nextObstacleSpawnZ += obstacleRingSpacing;
    }

    private void StartNewPattern()
    {
        _currentPatternConfig = null;
        _currentPatternDifficultyConfig = null;
        _currentPatternIterationIndex = 0;
        _currentPatternIterationCount = 0;
        _currentPatternRotationPerIteration = 0f;

        // Random base direction sign for this pattern
        _currentPatternDirectionSign = (Random.value < 0.5f) ? -1f : 1f;

        _validConfigBuffer.Clear();

        // Find configs that have at least one valid difficulty band.
        for (int i = 0; i < obstacleRingConfigs.Count; i++)
        {
            var cfg = obstacleRingConfigs[i];
            if (cfg == null) continue;

            cfg.GetValidDifficultyConfigs(_currentDifficulty, _validDifficultyBuffer);
            if (_validDifficultyBuffer.Count > 0)
            {
                _validConfigBuffer.Add(cfg);
            }
        }

        // Fallback: if nothing matches the current difficulty, allow all configs.
        if (_validConfigBuffer.Count == 0)
        {
            Debug.LogWarning($"[ObstacleRingGenerator] No valid obstacle configs for difficulty {_currentDifficulty}. Using all configs.");
            _validConfigBuffer.AddRange(obstacleRingConfigs);
        }

        if (_validConfigBuffer.Count == 0)
        {
            Debug.LogWarning("[ObstacleRingGenerator] No obstacle configs available.");
            return;
        }

        // Pick a config for this pattern.
        _currentPatternConfig = _validConfigBuffer[Random.Range(0, _validConfigBuffer.Count)];

        // Now pick a specific difficulty band from that config.
        _currentPatternConfig.GetValidDifficultyConfigs(_currentDifficulty, _validDifficultyBuffer);
        if (_validDifficultyBuffer.Count == 0)
        {
            var allBands = _currentPatternConfig.DifficultyConfigs;
            for (int i = 0; i < allBands.Count; i++)
            {
                _validDifficultyBuffer.Add(allBands[i]);
            }
        }

        if (_validDifficultyBuffer.Count == 0)
        {
            Debug.LogWarning("[ObstacleRingGenerator] Selected obstacle config has no difficulty configs.");
            _currentPatternConfig = null;
            return;
        }

        _currentPatternDifficultyConfig = _validDifficultyBuffer[Random.Range(0, _validDifficultyBuffer.Count)];

        // Choose iteration count within band.
        int minIter = Mathf.Max(1, _currentPatternDifficultyConfig.minIterationCount);
        int maxIter = Mathf.Max(minIter, _currentPatternDifficultyConfig.maxIterationCount);
        _currentPatternIterationCount = Random.Range(minIter, maxIter + 1);

        // Choose rotation amount (degrees) per iteration within band,
        // interpreting min/max as "number of 60° steps".
        float minRot = _currentPatternDifficultyConfig.minRotationsPerIteration;
        float maxRot = _currentPatternDifficultyConfig.maxRotationsPerIteration;

        int minSteps = Mathf.CeilToInt(minRot);
        int maxSteps = Mathf.FloorToInt(maxRot);

        if (maxSteps < minSteps)
        {
            maxSteps = minSteps;
        }

        int stepCount;
        if (minSteps == maxSteps)
        {
            stepCount = minSteps;
        }
        else
        {
            stepCount = Random.Range(minSteps, maxSteps + 1);
        }

        _currentPatternRotationPerIteration = stepCount * 60f;
    }

    private ObstacleRingController GetObstacleRingInstance(ObstacleRingConfig config)
    {
        if (config == null || config.RingPrefab == null)
        {
            Debug.LogWarning("[ObstacleRingGenerator] ObstacleRingConfig has no prefab.");
            return null;
        }

        if (!_obstaclePools.TryGetValue(config, out var queue))
        {
            queue = new Queue<ObstacleRingController>();
            _obstaclePools.Add(config, queue);
        }

        ObstacleRingController instance = null;

        while (queue.Count > 0 && instance == null)
        {
            instance = queue.Dequeue();
        }

        if (instance == null)
        {
            var go = Instantiate(config.RingPrefab);
            instance = go.GetComponent<ObstacleRingController>();
            if (instance == null)
            {
                Debug.LogError("[ObstacleRingGenerator] Ring prefab does not contain ObstacleRingController.");
                Destroy(go);
                return null;
            }

            instance.InitializeFromConfig(config);
        }

        instance.gameObject.SetActive(true);
        return instance;
    }

    private void ReleaseObstacleRing(ObstacleRingController ring)
    {
        if (ring == null)
            return;

        // Clear any pickups parented to this ring
        ClearPickupsUnder(ring.transform);

        var config = ring.SourceConfig;
        if (config == null)
        {
            ring.gameObject.SetActive(false);
            return;
        }

        if (!_obstaclePools.TryGetValue(config, out var queue))
        {
            queue = new Queue<ObstacleRingController>();
            _obstaclePools.Add(config, queue);
        }

        ring.gameObject.SetActive(false);
        queue.Enqueue(ring);
    }

    /// <summary>
    /// Clears obstacle rings within a Z window around the player (ahead and slightly behind) with dissolve visuals.
    /// Can be used at run start/continue to guarantee a safe region.
    /// </summary>
    internal void ClearRingsAroundPlayer(float clearAheadDistance, float clearBehindDistance, float dissolveDuration)
    {
        if (playerTransform == null)
            return;

        float playerZ = playerTransform.position.z;
        _tempAheadRingsBuffer.Clear();

        for (int i = 0; i < _activeObstacleRings.Count; i++)
        {
            var ring = _activeObstacleRings[i];
            if (ring == null)
                continue;

            float dz = ring.transform.position.z - playerZ;
            if (dz >= -clearBehindDistance && dz <= clearAheadDistance)
            {
                _tempAheadRingsBuffer.Add(ring);
            }
        }

        if (_tempAheadRingsBuffer.Count == 0)
            return;

        _tempAheadRingsBuffer.Sort((a, b) =>
            a.transform.position.z.CompareTo(b.transform.position.z));

        for (int i = 0; i < _tempAheadRingsBuffer.Count; i++)
        {
            var ring = _tempAheadRingsBuffer[i];
            if (ring == null)
                continue;

            var visuals = ring.GetComponent<ObstacleRingVisuals>();
            var ringToRelease = ring;

            if (visuals != null && dissolveDuration > 0f)
            {
                visuals.PlayFadeOut(dissolveDuration, disableObjectAtEnd: true);
                _activeObstacleRings.Remove(ringToRelease);
                ReleaseObstacleRing(ringToRelease);
            }
            else
            {
                _activeObstacleRings.Remove(ringToRelease);
                ReleaseObstacleRing(ringToRelease);
            }
        }
    }

    /// <summary>
    /// Legacy API: clears the next N rings ahead of the player.
    /// Kept for compatibility; ClearRingsAroundPlayer is more robust.
    /// </summary>
    internal void DissolveNextRings(int startClearRings, float dissolveDuration)
    {
        if (playerTransform == null || startClearRings <= 0)
            return;

        float playerZ = playerTransform.position.z;

        _tempAheadRingsBuffer.Clear();

        for (int i = 0; i < _activeObstacleRings.Count; i++)
        {
            var ring = _activeObstacleRings[i];
            if (ring == null)
                continue;

            // Slightly expand the window to include rings just behind the player
            if (ring.transform.position.z >= playerZ - obstacleRingSpacing * 0.5f)
            {
                _tempAheadRingsBuffer.Add(ring);
            }
        }

        if (_tempAheadRingsBuffer.Count == 0)
            return;

        _tempAheadRingsBuffer.Sort((a, b) =>
            a.transform.position.z.CompareTo(b.transform.position.z));

        int count = Mathf.Min(startClearRings, _tempAheadRingsBuffer.Count);

        for (int i = 0; i < count; i++)
        {
            var ring = _tempAheadRingsBuffer[i];
            if (ring == null)
                continue;

            var visuals = ring.GetComponent<ObstacleRingVisuals>();
            var ringToRelease = ring;

            if (visuals != null && dissolveDuration > 0f)
            {
                visuals.PlayFadeOut(dissolveDuration, disableObjectAtEnd: true);
                _activeObstacleRings.Remove(ringToRelease);
                ReleaseObstacleRing(ringToRelease);
            }
            else
            {
                _activeObstacleRings.Remove(ringToRelease);
                ReleaseObstacleRing(ringToRelease);
            }
        }
    }

    #endregion

    #region Pickup Rings (Placement & Pooling)

    private void EnsurePickupRingsAhead(float playerZ)
    {
        float targetMaxZ = playerZ + pickupRingsAhead * pickupRingSpacing;

        while (_nextPickupSpawnZ <= targetMaxZ)
        {
            SpawnNextPickupRing();
        }
    }

    private void RecyclePickupRings(float playerZ)
    {
        for (int i = _activePickupRings.Count - 1; i >= 0; i--)
        {
            var ring = _activePickupRings[i];
            if (ring == null)
            {
                _activePickupRings.RemoveAt(i);
                continue;
            }

            float dz = playerZ - ring.transform.position.z;
            if (dz > pickupRecycleDistanceBehind)
            {
                ReleasePickupRing(ring);
                _activePickupRings.RemoveAt(i);
            }
        }
    }

    private void SpawnNextPickupRing()
    {
        if (pickupRingPrefab == null)
            return;

        // Avoid placing a pickup ring too close to an obstacle ring along Z.
        int safety = 0;
        while (IsTooCloseToObstacle(_nextPickupSpawnZ) && safety < 4)
        {
            _nextPickupSpawnZ += pickupRingSpacing;
            safety++;
        }

        var ring = GetPickupRingInstance();
        if (ring == null)
            return;

        if (ringsParent != null)
            ring.transform.SetParent(ringsParent, false);

        ring.transform.position = new Vector3(0f, 0f, _nextPickupSpawnZ);
        ring.transform.rotation = Quaternion.identity;

        float scale = CalculatePickupRingScale();
        ring.transform.localScale = Vector3.one * scale;

        ConfigurePickupRingPickups(ring);

        _activePickupRings.Add(ring);

        _nextPickupSpawnZ += pickupRingSpacing;
    }

    private bool IsTooCloseToObstacle(float z)
    {
        if (minPickupObstacleSeparation <= 0f)
            return false;

        for (int i = 0; i < _activeObstacleRings.Count; i++)
        {
            var ring = _activeObstacleRings[i];
            if (ring == null) continue;

            float dz = Mathf.Abs(ring.transform.position.z - z);
            if (dz < minPickupObstacleSeparation)
                return true;
        }

        return false;
    }

    internal void SetPickupRadiusMultiplier(float pickupMultiplier)
    {
        // TODO: Implement if magnet / pickup radius upgrades need to adjust spacing or spawn patterns.
    }

    private PickupRing GetPickupRingInstance()
    {
        PickupRing ring = null;

        while (_pickupRingPool.Count > 0 && ring == null)
        {
            ring = _pickupRingPool.Dequeue();
        }

        if (ring == null)
        {
            ring = Instantiate(pickupRingPrefab);
        }

        ring.gameObject.SetActive(true);
        return ring;
    }

    private void ReleasePickupRing(PickupRing ring)
    {
        if (ring == null)
            return;

        ClearPickupsUnder(ring.transform);

        ring.gameObject.SetActive(false);
        _pickupRingPool.Enqueue(ring);
    }

    private float CalculatePickupRingScale()
    {
        if (difficultyForMinPickupScale <= 0f)
        {
            return Mathf.Clamp(startingPickupRingScale, minPickupRingScale, startingPickupRingScale);
        }

        float t = Mathf.InverseLerp(startingDifficulty, difficultyForMinPickupScale, _currentDifficulty);
        float scale = Mathf.Lerp(startingPickupRingScale, minPickupRingScale, t);
        return Mathf.Clamp(scale, minPickupRingScale, startingPickupRingScale);
    }

    /// <summary>
    /// Clears pickup rings within a Z window around the player.
    /// </summary>
    public void ClearPickupRingsAroundPlayer(float clearAheadDistance, float clearBehindDistance = 0f)
    {
        if (playerTransform == null)
            return;

        float playerZ = playerTransform.position.z;

        for (int i = _activePickupRings.Count - 1; i >= 0; i--)
        {
            var ring = _activePickupRings[i];
            if (ring == null)
            {
                _activePickupRings.RemoveAt(i);
                continue;
            }

            float dz = ring.transform.position.z - playerZ;
            if (dz >= -clearBehindDistance && dz <= clearAheadDistance)
            {
                ReleasePickupRing(ring);
                _activePickupRings.RemoveAt(i);
            }
        }
    }

    #endregion

    #region Pickup Instantiation

    private void ConfigurePickupRingPickups(PickupRing ring)
    {
        if (ring == null || pickupPrefab == null)
            return;

        var spawnPoints = ring.PickupSpawnPoints;
        if (spawnPoints == null || spawnPoints.Count == 0)
            return;

        // Optional chain logic: if we're not in a chain, skip this ring entirely.
        if (!EnsurePickupChain())
            return;

        int slotCount = spawnPoints.Count;
        _patternSlotBuffer.Clear();

        // Choose a pattern and fill the buffer with slot indices for this ring.
        FillPatternSlots(slotCount, _patternSlotBuffer);

        float spawnChance = Mathf.Clamp01(pickupSlotSpawnChance * _pickupSpawnChanceMultiplier);

        foreach (int slotIndex in _patternSlotBuffer)
        {
            if (slotIndex < 0 || slotIndex >= slotCount)
                continue;

            if (Random.value > spawnChance)
                continue;

            Transform spawn = spawnPoints[slotIndex];
            if (spawn == null)
                continue;

            Vector3 worldPos = spawn.position;
            if (IsPickupBlocked(worldPos))
                continue;

            Vector3 localPos = ring.transform.InverseTransformPoint(worldPos);

            var pickup = GetPickupInstance();
            if (pickup == null)
                continue;

            pickup.transform.SetParent(ring.transform, false);
            pickup.transform.localPosition = localPos;

            Vector3 upDir = localPos.sqrMagnitude > 0.0001f ? -localPos.normalized : Vector3.up;
            pickup.transform.localRotation = Quaternion.LookRotation(Vector3.forward, upDir);

            bool spawnPowerup = ShouldSpawnPowerupInChain() && Random.value <= powerupSpawnChance;
            if (spawnPowerup)
            {
                pickup.Configure(PickupType.Powerup, ChooseRandomPowerup());
            }
            else
            {
                pickup.Configure(PickupType.Coin, PowerupType.CoinMultiplier);
            }
        }

        // Advance chain (so we eventually end and get a gap).
        AdvancePickupChain();
    }

    private PickupPatternType ChoosePickupPatternType()
    {
        _patternTypesBuffer.Clear();

        if (allowFullRingPattern) _patternTypesBuffer.Add(PickupPatternType.FullRing);
        if (allowAlternatingPattern) _patternTypesBuffer.Add(PickupPatternType.Alternating);
        if (allowArcPattern) _patternTypesBuffer.Add(PickupPatternType.Arc);
        if (allowClusterPattern) _patternTypesBuffer.Add(PickupPatternType.Cluster);

        if (_patternTypesBuffer.Count == 0)
        {
            _patternTypesBuffer.Add(PickupPatternType.Arc);
        }

        int index = Random.Range(0, _patternTypesBuffer.Count);
        return _patternTypesBuffer[index];
    }

    private void FillPatternSlots(int slotCount, List<int> output)
    {
        if (slotCount <= 0)
            return;

        PickupPatternType pattern = ChoosePickupPatternType();

        switch (pattern)
        {
            case PickupPatternType.FullRing:
                for (int i = 0; i < slotCount; i++)
                    output.Add(i);
                break;

            case PickupPatternType.Alternating:
                {
                    int startIndex = Random.value < 0.5f ? 0 : 1;
                    for (int i = startIndex; i < slotCount; i += 2)
                        output.Add(i);
                    break;
                }

            case PickupPatternType.Arc:
                {
                    int arcSize = Mathf.Clamp(Random.Range(minArcSize, maxArcSize + 1), 1, slotCount);
                    int center = Random.Range(0, slotCount);
                    int half = arcSize / 2;

                    for (int i = 0; i < arcSize; i++)
                    {
                        int index = (center - half + i + slotCount) % slotCount;
                        if (!output.Contains(index))
                            output.Add(index);
                    }
                    break;
                }

            case PickupPatternType.Cluster:
                {
                    int clusterSize = Mathf.Clamp(Random.Range(minClusterSize, maxClusterSize + 1), 1, slotCount);
                    int start = Random.Range(0, slotCount);

                    for (int i = 0; i < clusterSize; i++)
                    {
                        int index = (start + i) % slotCount;
                        if (!output.Contains(index))
                            output.Add(index);
                    }
                    break;
                }
        }
    }

    private void ConfigureObstacleRingPickups(ObstacleRingController ring)
    {
        if (ring == null || pickupPrefab == null)
            return;

        float obstacleChance = Mathf.Clamp01(obstacleRingPickupChance * _pickupSpawnChanceMultiplier);
        if (Random.value > obstacleChance)
            return;

        var spawnPoints = ring.PickupSpawnPoints;
        if (spawnPoints == null || spawnPoints.Count == 0)
            return;

        var usedSlots = new HashSet<int>();

        // For obstacle rings we treat them as single-slot "chains",
        // but still allow the chain rules to influence powerup placement.
        TrySpawnPickupFromSpawnPoints(
            parent: ring.transform,
            spawnPoints: spawnPoints,
            usedSlots: usedSlots,
            allowPowerup: ShouldSpawnPowerupInChain()
        );
    }

    private bool TrySpawnPickupFromSpawnPoints(
        Transform parent,
        IReadOnlyList<Transform> spawnPoints,
        HashSet<int> usedSlots,
        bool allowPowerup)
    {
        if (parent == null || spawnPoints == null || spawnPoints.Count == 0)
            return false;

        int attempts = Mathf.Max(1, pickupPlacementAttempts);
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            int slotIndex = GetNextPickupSpawnIndex(spawnPoints.Count, usedSlots);
            if (slotIndex < 0)
                return false;

            if (!usedSlots.Add(slotIndex))
                continue;

            Transform spawn = spawnPoints[slotIndex];
            if (spawn == null)
                continue;

            Vector3 worldPos = spawn.position;
            if (IsPickupBlocked(worldPos))
                continue;

            Vector3 localPos = parent.InverseTransformPoint(worldPos);

            var pickup = GetPickupInstance();
            if (pickup == null)
                return false;

            pickup.transform.SetParent(parent, false);
            pickup.transform.localPosition = localPos;

            Vector3 upDir = localPos.sqrMagnitude > 0.0001f ? -localPos.normalized : Vector3.up;
            pickup.transform.localRotation = Quaternion.LookRotation(Vector3.forward, upDir);

            bool spawnPowerup = allowPowerup && Random.value <= powerupSpawnChance;
            if (spawnPowerup)
            {
                pickup.Configure(PickupType.Powerup, ChooseRandomPowerup());
            }
            else
            {
                pickup.Configure(PickupType.Coin, PowerupType.CoinMultiplier);
            }

            return true;
        }

        return false;
    }

    private int GetNextPickupSpawnIndex(int slotCount, HashSet<int> usedSlots)
    {
        if (slotCount <= 0 || usedSlots.Count >= slotCount)
            return -1;

        int index = Random.Range(0, slotCount);
        int safety = 0;
        while (usedSlots.Contains(index) && safety < slotCount)
        {
            index = Random.Range(0, slotCount);
            safety++;
        }

        return usedSlots.Contains(index) ? -1 : index;
    }

    private Pickup GetPickupInstance()
    {
        Pickup instance = null;

        while (_pickupObjectPool.Count > 0 && instance == null)
        {
            instance = _pickupObjectPool.Dequeue();
        }

        if (instance == null)
        {
            if (pickupPrefab == null)
            {
                Debug.LogWarning("[ObstacleRingGenerator] Pickup prefab is not assigned.");
                return null;
            }

            instance = Instantiate(pickupPrefab);
        }

        instance.gameObject.SetActive(true);
        return instance;
    }

    private void ReleasePickup(Pickup pickup)
    {
        if (pickup == null)
            return;

        pickup.gameObject.SetActive(false);
        _pickupObjectPool.Enqueue(pickup);
    }

    private void ClearPickupsUnder(Transform ringTransform)
    {
        if (ringTransform == null)
            return;

        var pickups = ringTransform.GetComponentsInChildren<Pickup>(true);
        for (int i = 0; i < pickups.Length; i++)
        {
            ReleasePickup(pickups[i]);
        }
    }

    private bool IsPickupBlocked(Vector3 worldPosition)
    {
        if (pickupClearanceRadius <= 0f)
            return false;

        return Physics.CheckSphere(worldPosition, pickupClearanceRadius, obstacleOverlapMask);
    }

    #endregion

    #region Pickup Chain & Powerups

    private bool EnsurePickupChain()
    {
        // Already in a chain: keep going.
        if (_pickupChainRemaining > 0)
            return true;

        // In a gap between chains.
        if (_pickupChainGapRemaining > 0)
        {
            _pickupChainGapRemaining--;
            return false;
        }

        // Chance to start a new chain.
        if (Random.value > pickupChainStartChance)
            return false;

        _pickupChainLength = Random.Range(pickupChainMinRings, pickupChainMaxRings + 1);
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
            _pickupChainGapRemaining = Random.Range(pickupChainGapMin, pickupChainGapMax + 1);
        }
    }

    /// <summary>
    /// Returns true for rings that should contain a powerup within the current chain:
    /// first, middle, and last.
    /// </summary>
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

        int roll = Random.Range(0, totalWeight);
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

    public void SetPickupSpawnChanceMultiplier(float multiplier)
    {
        _pickupSpawnChanceMultiplier = Mathf.Max(0f, multiplier);
    }

    #endregion

#if UNITY_EDITOR
    private void OnValidate()
    {
        obstacleRingSpacing = Mathf.Max(0.1f, obstacleRingSpacing);
        pickupRingSpacing = Mathf.Max(0.1f, pickupRingSpacing);
        obstacleRingsAhead = Mathf.Max(1, obstacleRingsAhead);
        pickupRingsAhead = Mathf.Max(1, pickupRingsAhead);
        obstacleRecycleDistanceBehind = Mathf.Max(1f, obstacleRecycleDistanceBehind);
        pickupRecycleDistanceBehind = Mathf.Max(1f, pickupRecycleDistanceBehind);

        pickupSlotsToFillMin = Mathf.Max(0, pickupSlotsToFillMin);
        pickupSlotsToFillMax = Mathf.Max(pickupSlotsToFillMin, pickupSlotsToFillMax);
        pickupPlacementAttempts = Mathf.Max(1, pickupPlacementAttempts);

        pickupChainMinRings = Mathf.Max(1, pickupChainMinRings);
        pickupChainMaxRings = Mathf.Max(pickupChainMinRings, pickupChainMaxRings);
        pickupChainGapMin = Mathf.Max(0, pickupChainGapMin);
        pickupChainGapMax = Mathf.Max(pickupChainGapMin, pickupChainGapMax);

        minArcSize = Mathf.Max(1, minArcSize);
        maxArcSize = Mathf.Max(minArcSize, maxArcSize);
        minClusterSize = Mathf.Max(1, minClusterSize);
        maxClusterSize = Mathf.Max(minClusterSize, maxClusterSize);

        minPickupObstacleSeparation = Mathf.Max(0f, minPickupObstacleSeparation);
    }
#endif
}
