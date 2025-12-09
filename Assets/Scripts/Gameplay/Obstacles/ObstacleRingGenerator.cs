using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns and recycles obstacle rings in an endless tube.
/// Each ring hosts a single obstacle prefab (fan, laser, wedge pattern, etc.)
/// centered at the tube origin and rotated around Z.
/// 
/// Features:
/// - Obstacle types: Laser, Fan, WedgeFull, Wedge75, Wedge50, Wedge25
/// - Pattern modes:
///     * Type X for Y rings, random orientations
///     * Type X for Y rings, shifting orientation left/right by 1-2 steps
/// - Color: gradient with alternating light/dark per ring.
/// 
/// Obstacle prefabs are expected to have their own controller scripts, e.g.
/// FanObstacle, LaserObstacle, WedgeObstacle, etc.
/// </summary>
public class ObstacleRingGenerator : MonoBehaviour
{
    public enum ObstacleRingType
    {
        Laser,
        Fan,
        WedgeFull,
        Wedge75,
        Wedge50,
        Wedge25
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

    [Header("Pattern Settings")]
    [Tooltip("Minimum rings in a pattern run.")]
    [SerializeField] private int minPatternRunLength = 3;

    [Tooltip("Maximum rings in a pattern run.")]
    [SerializeField] private int maxPatternRunLength = 7;

    [Tooltip("Chance to use shifted orientation vs random orientation for a new pattern run.")]
    [Range(0f, 1f)]
    [SerializeField] private float shiftedPatternChance = 0.5f;

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

    // Pattern state
    private ObstacleRingType _currentPatternType;
    private PatternMode _currentPatternMode;
    private int _patternRingsRemaining;
    private int _currentRotationStep;
    private int _rotationStepDelta; // -2..+2

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
        _nextSpawnZ = player.position.z + ringSpacing;

        for (int i = 0; i < ringBufferCount; i++)
        {
            SpawnRing();
        }
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
                // Move ring onward
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

        ObstacleRingType type = _currentPatternType;
        GameObject prefab = GetPrefabForType(type);

        // If type changed or no instance, respawn
        if (ring.obstacleInstance != null &&
            ring.type != type)
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

            // Configure by type
            ConfigureObstacleInstance(ring.obstacleInstance, type);
        }

        // Decrement pattern run
        _patternRingsRemaining--;
    }

    private void EnsurePatternState()
    {
        if (_patternRingsRemaining > 0)
            return;

        // Choose new obstacle type
        _currentPatternType = ChooseRandomObstacleType();

        // Choose pattern mode
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
        // simple uniform pick among configured types
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
            return ObstacleRingType.WedgeFull; // default fallback

        int idx = RandomRange(0, available.Count);
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

    private void ConfigureObstacleInstance(GameObject instance, ObstacleRingType type)
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

        // Wedge patterns
        var wedge = instance.GetComponent<WedgeObstacle>();
        if (wedge != null)
        {
            wedge.SetSideCount(sideCount);

            switch (type)
            {
                case ObstacleRingType.WedgeFull:
                    wedge.SetBaseWedgeSize(WedgeObstacle.WedgeSize.W100);
                    break;
                case ObstacleRingType.Wedge75:
                    wedge.SetBaseWedgeSize(WedgeObstacle.WedgeSize.W75);
                    break;
                case ObstacleRingType.Wedge50:
                    wedge.SetBaseWedgeSize(WedgeObstacle.WedgeSize.W50);
                    break;
                case ObstacleRingType.Wedge25:
                    wedge.SetBaseWedgeSize(WedgeObstacle.WedgeSize.W25);
                    break;
            }

            int modePick = RandomRange(0, 5);
            var modes = (WedgeObstacle.WedgePatternMode[])System.Enum.GetValues(typeof(WedgeObstacle.WedgePatternMode));
            wedge.SetPatternMode(modes[modePick % modes.Length]);

            wedge.RegeneratePattern();
        }
    }

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
}
