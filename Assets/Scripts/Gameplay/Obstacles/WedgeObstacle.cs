using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Segment-based wedge obstacle:
/// - Treats each side of the tunnel as a "slot".
/// - Builds a boolean pattern per ring (occupied vs gap).
/// - Instantiates a wedge segment prefab at each occupied slot.
/// 
/// Supported local patterns:
/// - EveryOther: 101010...
/// - SingleGap: exactly one gap
/// - DoubleGap: exactly two gaps
/// - RandomSpots: random wedges/gaps, but guaranteed at least 1 wedge and 1 gap.
/// 
/// Cross-ring rotation (how the pattern drifts between rings) is handled
/// by the ring generator (ObstacleRingGenerator) via rotating the whole
/// obstacle ring root around Z.
/// </summary>
public class WedgeObstacle : MonoBehaviour
{
    public enum LocalPatternType
    {
        EveryOther,
        SingleGap,
        DoubleGap,
        RandomSpots
    }

    [Header("Shape")]
    [Tooltip("Number of sides for the tunnel polygon. Pattern has this many slots.")]
    [SerializeField] private int sideCount = 6;

    [Header("Prefabs")]
    [Tooltip("One or more wedge segment prefabs; a random one is used per occupied slot.")]
    [SerializeField] private GameObject[] wedgeSegmentPrefabs;

    [Header("Pattern")]
    [SerializeField] private LocalPatternType patternType = LocalPatternType.EveryOther;

    private readonly List<GameObject> _spawnedSegments = new List<GameObject>();

    private void Start()
    {
        // Generate when the instance is placed in the scene at runtime.
        RegeneratePattern();
    }

    private void OnValidate()
    {
        sideCount = Mathf.Max(3, sideCount);
        // No auto-regenerate here: use the context menu or let runtime handle it.
    }

    /// <summary>
    /// Called by the ring generator to set the current side count.
    /// </summary>
    public void SetSideCount(int sides)
    {
        sideCount = Mathf.Max(3, sides);
    }

    /// <summary>
    /// Called by the ring generator to pick the local pattern.
    /// </summary>
    public void SetLocalPattern(LocalPatternType type)
    {
        patternType = type;
    }

    [ContextMenu("Regenerate Pattern")]
    public void RegeneratePattern()
    {
        // Avoid instantiating children on the prefab asset itself.
        if (!gameObject.scene.IsValid())
        {
            Debug.LogWarning("WedgeObstacle: Cannot regenerate on prefab asset; place it in a scene.", this);
            return;
        }

        ClearSegments();

        if (wedgeSegmentPrefabs == null || wedgeSegmentPrefabs.Length == 0)
        {
            Debug.LogWarning("WedgeObstacle: No wedgeSegmentPrefabs assigned.", this);
            return;
        }

        bool[] occupied = BuildPattern(sideCount, patternType);

        float sideAngle = 360f / Mathf.Max(3, sideCount);

        // Pick a random segment prefab so you can have variation in wedge visuals.
        GameObject prefab = patternType switch
        {   
            LocalPatternType.SingleGap => wedgeSegmentPrefabs[0],
            LocalPatternType.DoubleGap => wedgeSegmentPrefabs[Random.Range(0, 1)],
            LocalPatternType.EveryOther => wedgeSegmentPrefabs[Random.Range(1, 3)],
            LocalPatternType.RandomSpots => wedgeSegmentPrefabs[Random.Range(0, 1)],
            _ => wedgeSegmentPrefabs[Random.Range(0, wedgeSegmentPrefabs.Length)]
        };

        if (prefab == null) return;

        for (int i = 0; i < sideCount; i++)
        {
            if (!occupied[i]) continue;

            float angleDeg = i * sideAngle;

            GameObject go = Instantiate(prefab, transform);
            go.name = $"WedgeSlot_{i}";
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.Euler(0f, 0f, angleDeg);
            if (go.GetComponent<ObstacleDissolver>() == null)
            {
                go.AddComponent<ObstacleDissolver>();
            }

            _spawnedSegments.Add(go);
        }
    }

    private bool[] BuildPattern(int sides, LocalPatternType type)
    {
        bool[] occupied = new bool[sides];

        switch (type)
        {
            case LocalPatternType.EveryOther:
                for (int i = 0; i < sides; i++)
                    occupied[i] = (i % 2 == 0); // 1 0 1 0 ...
                break;

            case LocalPatternType.SingleGap:
                {
                    int gapIndex = Random.Range(0, sides);
                    for (int i = 0; i < sides; i++)
                        occupied[i] = (i != gapIndex);
                    break;
                }

            case LocalPatternType.DoubleGap:
                {
                    int gapA = Random.Range(0, sides);
                    int gapB = gapA;
                    int safety = 0;
                    while (gapB == gapA && safety++ < 10)
                        gapB = Random.Range(0, sides);

                    for (int i = 0; i < sides; i++)
                        occupied[i] = (i != gapA && i != gapB);
                    break;
                }

            case LocalPatternType.RandomSpots:
                {
                    // Start random but force at least 1 wedge and 1 gap.
                    int wedgeCount = 0;
                    int gapCount = 0;
                    for (int i = 0; i < sides; i++)
                    {
                        bool occ = Random.value > 0.5f;
                        occupied[i] = occ;
                        if (occ) wedgeCount++;
                        else gapCount++;
                    }

                    if (wedgeCount == 0)
                    {
                        int idx = Random.Range(0, sides);
                        occupied[idx] = true;
                        wedgeCount = 1;
                        gapCount = Mathf.Max(0, gapCount - 1);
                    }

                    if (gapCount == 0)
                    {
                        int idx = Random.Range(0, sides);
                        occupied[idx] = false;
                        gapCount = 1;
                    }

                    break;
                }
        }

        return occupied;
    }

    private void ClearSegments()
    {
        foreach (var go in _spawnedSegments)
        {
            if (go != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Object.DestroyImmediate(go);
                else
                    Object.Destroy(go);
#else
                Object.Destroy(go);
#endif
            }
        }
        _spawnedSegments.Clear();
    }
}
