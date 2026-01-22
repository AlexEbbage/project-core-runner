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

    [Header("Pickup Ring Placement")]
    [Tooltip("Distance along Z between consecutive pickup rings.")]
    [SerializeField] private float pickupRingSpacing = 7f;

    [Tooltip("Number of pickup rings to maintain ahead of the player.")]
    [SerializeField] private int pickupRingsAhead = 15;

    [Tooltip("When a pickup ring is this far behind the player on Z, recycle it.")]
    [SerializeField] private float pickupRecycleDistanceBehind = 20f;

    [Header("Pickup Ring Difficulty Scaling")]
    [Tooltip("Initial uniform scale for pickup rings at starting difficulty.")]
    [SerializeField] private float startingPickupRingScale = 1.0f;

    [Tooltip("Minimum uniform scale for pickup rings at high difficulty.")]
    [SerializeField] private float minPickupRingScale = 0.5f;

    [Tooltip("Difficulty level at which pickup rings will reach their minimum scale.")]
    [SerializeField] private float difficultyForMinPickupScale = 100f;

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

    // Runtime difficulty
    private float _currentDifficulty;
    private float _startPlayerZ;

    // Next spawn Z positions
    private float _nextObstacleSpawnZ;
    private float _nextPickupSpawnZ;

    // Pools & active lists
    private readonly Dictionary<ObstacleRingConfig, Queue<ObstacleRingController>> _obstaclePools
        = new Dictionary<ObstacleRingConfig, Queue<ObstacleRingController>>();
    private readonly List<ObstacleRingController> _activeObstacleRings
        = new List<ObstacleRingController>();

    private readonly Queue<PickupRing> _pickupRingPool = new Queue<PickupRing>();
    private readonly List<PickupRing> _activePickupRings = new List<PickupRing>();

    // New: pool for individual pickup instances
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

    private void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("[ObstacleRingGenerator] Player transform is not assigned.");
            enabled = false;
            return;
        }

        _startPlayerZ = playerTransform.position.z;
        _currentDifficulty = startingDifficulty;

        _nextObstacleSpawnZ = playerTransform.position.z + obstacleRingSpacing;
        _nextPickupSpawnZ = playerTransform.position.z + pickupRingSpacing;

        // Seed rings ahead of the player at the start of the run.
        for (int i = 0; i < obstacleRingsAhead; i++)
        {
            SpawnNextObstacleRing();
        }

        for (int i = 0; i < pickupRingsAhead; i++)
        {
            SpawnNextPickupRing();
        }
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
        // Assuming tube axis is Z; rotate around Z.
        ring.transform.rotation = Quaternion.AngleAxis(_lastObstacleRingAngle, Vector3.forward);

        float speed = Random.Range(_currentPatternDifficultyConfig.minSpeed, _currentPatternDifficultyConfig.maxSpeed);
        ring.SetupForPattern(speed, directionSign);

        // Optionally spawn pickups on this obstacle ring
        ConfigureObstacleRingPickups(ring);

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

        // First, find all configs that have at least one valid difficulty band.
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
            // Fallback to all bands if none match.
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
            // Random.Range for ints is [min, maxExclusive), so we use maxSteps + 1.
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

        var ring = GetPickupRingInstance();
        if (ring == null)
            return;

        if (ringsParent != null)
            ring.transform.SetParent(ringsParent, false);

        ring.transform.position = new Vector3(0f, 0f, _nextPickupSpawnZ);
        ring.transform.rotation = Quaternion.identity;

        float scale = CalculatePickupRingScale();
        ring.transform.localScale = Vector3.one * scale;

        // Spawn pickups onto this pickup ring using its spawn points
        ConfigurePickupRingPickups(ring);

        _activePickupRings.Add(ring);

        _nextPickupSpawnZ += pickupRingSpacing;
    }

    internal void SetPickupRadiusMultiplier(float pickupMultiplier)
    {
        // TODO: Can't recall what this is for anymore. I think it's when you upgrade the magnet powerup.
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

        // Clear any pickups parented to this ring
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

    #endregion

    #region Pickup Instantiation

    private void ConfigurePickupRingPickups(PickupRing ring)
    {
        if (ring == null || pickupPrefab == null)
            return;

        // Chains: if we're in a "gap", skip this ring entirely
        if (!EnsurePickupChain())
            return;

        var spawnPoints = ring.PickupSpawnPoints;
        if (spawnPoints == null || spawnPoints.Count == 0)
            return;

        int slotsToFill = Random.Range(pickupSlotsToFillMin, pickupSlotsToFillMax + 1);
        slotsToFill = Mathf.Clamp(slotsToFill, 0, spawnPoints.Count);

        var usedSlots = new HashSet<int>();
        for (int i = 0; i < slotsToFill; i++)
        {
            float spawnChance = Mathf.Clamp01(pickupSlotSpawnChance * _pickupSpawnChanceMultiplier);
            if (Random.value > spawnChance)
                continue;

            bool spawned = TrySpawnPickupFromSpawnPoints(
                parent: ring.transform,
                spawnPoints: spawnPoints,
                usedSlots: usedSlots,
                allowPowerup: ShouldSpawnPowerupInChain()
            );

            if (!spawned)
                break;
        }

        // move chain forward (length + gap logic)
        AdvancePickupChain();
    }

    internal void DissolveNextRings(int startClearRings, float dissolveDuration)
    {
        // TODO: Clear next rings. Use dissolve shader on obstacle ring meshes.
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

            // Local position relative to ring
            Vector3 localPos = parent.InverseTransformPoint(worldPos);

            var pickup = GetPickupInstance();
            if (pickup == null)
                return false;

            pickup.transform.SetParent(parent, false);
            pickup.transform.localPosition = localPos;

            // Face roughly inward toward the tube center
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
    }
#endif
}
