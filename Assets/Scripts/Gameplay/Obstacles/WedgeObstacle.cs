using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds wedge obstacles around the ring using 4 wedge prefabs (25/50/75/100%),
/// snapping to whole side segments of the tunnel polygon.
/// 
/// Wedge sizes are expressed as fractions of the full ring, but converted to
/// a side span in [1 .. sideCount-1]:
///   W25  ~ 25% of sides (rounded, min 1)
///   W50  ~ 50% of sides
///   W75  ~ 75% of sides
///   W100 ~ 100% of sides, clamped to sideCount-1 so there is always a gap.
/// 
/// Patterns:
/// - AllXWithGapY:
///     Repeats base wedge size around ring, leaving one continuous gap of size Y.
/// - Pattern25_50_25 / Pattern50_75_50 / Pattern75_100_75:
///     Simple fixed sequences scaled to fill the ring.
/// - AllRandomWithOneGap:
///     Random wedge sizes (from the 4 prefabs) adjacent around ring,
///     leaving one continuous gap of size gapSize.
/// </summary>
public class WedgeObstacle : MonoBehaviour
{
    public enum WedgeSize
    {
        W25,
        W50,
        W75,
        W100
    }

    public enum WedgePatternMode
    {
        AllXWithGapY,
        Pattern25_50_25,
        Pattern50_75_50,
        Pattern75_100_75,
        AllRandomWithOneGap
    }

    [Header("Prefabs")]
    [SerializeField] private GameObject wedge25Prefab;
    [SerializeField] private GameObject wedge50Prefab;
    [SerializeField] private GameObject wedge75Prefab;
    [SerializeField] private GameObject wedge100Prefab;

    [Header("Pattern")]
    [SerializeField] private WedgePatternMode patternMode = WedgePatternMode.AllXWithGapY;
    [SerializeField] private WedgeSize baseWedgeSize = WedgeSize.W50;
    [SerializeField] private WedgeSize gapSize = WedgeSize.W25;

    [Header("Shape")]
    [Tooltip("Number of sides for the tunnel polygon. All wedges snap to whole sides.")]
    [SerializeField] private int sideCount = 6;

    private readonly List<GameObject> _spawnedWedges = new List<GameObject>();

    private void Start()
    {
        // Only auto-generate at runtime.
        RegeneratePattern();
    }

    private void OnValidate()
    {
        sideCount = Mathf.Max(3, sideCount);
        // No auto-regeneration here to avoid spawning on every tweak,
        // especially on prefab assets.
    }

    public void SetSideCount(int sides)
    {
        sideCount = Mathf.Max(3, sides);
    }

    public void SetBaseWedgeSize(WedgeSize size)
    {
        baseWedgeSize = size;
    }

    public void SetPatternMode(WedgePatternMode mode)
    {
        patternMode = mode;
    }

    [ContextMenu("Regenerate Pattern")]
    public void RegeneratePattern()
    {
        if (!gameObject.scene.IsValid())
        {
            // Prevent instantiating children when editing the prefab asset itself.
            Debug.LogWarning("WedgeObstacle: Cannot regenerate on prefab asset. Place in a scene first.", this);
            return;
        }

        ClearWedges();

        switch (patternMode)
        {
            case WedgePatternMode.AllXWithGapY:
                Build_AllXWithGapY();
                break;

            case WedgePatternMode.Pattern25_50_25:
                Build_FixedPattern(new[] { WedgeSize.W25, WedgeSize.W50, WedgeSize.W25 });
                break;

            case WedgePatternMode.Pattern50_75_50:
                Build_FixedPattern(new[] { WedgeSize.W50, WedgeSize.W75, WedgeSize.W50 });
                break;

            case WedgePatternMode.Pattern75_100_75:
                Build_FixedPattern(new[] { WedgeSize.W75, WedgeSize.W100, WedgeSize.W75 });
                break;

            case WedgePatternMode.AllRandomWithOneGap:
                Build_AllRandomWithOneGap();
                break;
        }
    }

    #region Pattern Builders

    private void Build_AllXWithGapY()
    {
        int baseSides = GetSideSpan(baseWedgeSize);
        int gapSides = GetSideSpan(gapSize);

        // Ensure at least 1 gap
        int usableSides = sideCount - gapSides;
        if (baseSides <= 0 || usableSides <= 0)
            return;

        int wedgeCount = Mathf.Max(1, usableSides / baseSides);
        int usedSides = wedgeCount * baseSides;

        // If we overshoot, trim the last wedge
        if (usedSides > usableSides)
        {
            int overflow = usedSides - usableSides;
            // Just reduce effective count by one if needed
            if (wedgeCount > 1)
            {
                wedgeCount--;
                usedSides = wedgeCount * baseSides;
            }
        }

        // Random starting side so gap position moves around the ring
        int startSide = Random.Range(0, sideCount);
        int sideIndex = startSide;

        for (int i = 0; i < wedgeCount; i++)
        {
            SpawnWedge(baseWedgeSize, sideIndex);
            sideIndex = (sideIndex + baseSides) % sideCount;
        }
        // Remaining sides (gapSides + leftover) form one continuous gap.
    }

    private void Build_FixedPattern(WedgeSize[] patternSizes)
    {
        if (patternSizes == null || patternSizes.Length == 0)
            return;

        int totalBase = 0;
        foreach (var s in patternSizes)
            totalBase += GetSideSpan(s);

        if (totalBase <= 0)
            return;

        // Scale to fill exactly sideCount sides.
        float scale = (float)sideCount / totalBase;
        int[] spans = new int[patternSizes.Length];
        int sum = 0;
        for (int i = 0; i < patternSizes.Length; i++)
        {
            int baseSpan = GetSideSpan(patternSizes[i]);
            int span = Mathf.Max(1, Mathf.RoundToInt(baseSpan * scale));
            spans[i] = span;
            sum += span;
        }

        int diff = sum - sideCount;
        if (diff != 0)
        {
            int last = spans.Length - 1;
            spans[last] = Mathf.Clamp(spans[last] - diff, 1, sideCount);
        }

        int startSide = Random.Range(0, sideCount);
        int sideIndex = startSide;

        for (int i = 0; i < patternSizes.Length; i++)
        {
            SpawnWedge(patternSizes[i], sideIndex);
            sideIndex = (sideIndex + spans[i]) % sideCount;
        }
    }

    private void Build_AllRandomWithOneGap()
    {
        // Gap consumes gapSides; the rest is wedges, all adjacent.
        int gapSides = GetSideSpan(gapSize);
        int usableSides = sideCount - gapSides;
        if (usableSides <= 0)
            return;

        var sizeOptions = new[] { WedgeSize.W25, WedgeSize.W50, WedgeSize.W75, WedgeSize.W100 };

        List<(WedgeSize size, int span)> wedges = new List<(WedgeSize, int)>();
        int used = 0;
        int safety = 0;

        while (used < usableSides && safety < 50)
        {
            safety++;
            WedgeSize pick = sizeOptions[Random.Range(0, sizeOptions.Length)];
            int span = GetSideSpan(pick);

            if (span <= 0) continue;

            if (used + span > usableSides)
            {
                // Try a smaller size instead of giving up immediately
                bool placed = false;
                for (int i = 0; i < sizeOptions.Length; i++)
                {
                    int altSpan = GetSideSpan(sizeOptions[i]);
                    if (altSpan > 0 && used + altSpan <= usableSides)
                    {
                        wedges.Add((sizeOptions[i], altSpan));
                        used += altSpan;
                        placed = true;
                        break;
                    }
                }
                if (!placed) break;
            }
            else
            {
                wedges.Add((pick, span));
                used += span;
            }
        }

        // All wedges go in a contiguous block; the remaining sides are one gap.
        int startSide = Random.Range(0, sideCount);
        int sideIndex = startSide;

        foreach (var w in wedges)
        {
            SpawnWedge(w.size, sideIndex);
            sideIndex = (sideIndex + w.span) % sideCount;
        }
        // The remaining arc is the single gap of size gapSize (plus any leftover).
    }

    #endregion

    #region Helpers

    private int GetSideSpan(WedgeSize size)
    {
        // Convert wedge size fraction to a side span in [1 .. sideCount-1].
        float f = 1f;
        switch (size)
        {
            case WedgeSize.W25: f = 0.25f; break;
            case WedgeSize.W50: f = 0.5f; break;
            case WedgeSize.W75: f = 0.75f; break;
            case WedgeSize.W100: f = 1f; break;
        }

        // sideCount-1 max to always leave at least 1 side gap possible.
        int span = Mathf.RoundToInt(sideCount * f);
        span = Mathf.Clamp(span, 1, sideCount - 1);
        return span;
    }

    private GameObject GetPrefabForSize(WedgeSize size)
    {
        switch (size)
        {
            case WedgeSize.W25: return wedge25Prefab;
            case WedgeSize.W50: return wedge50Prefab;
            case WedgeSize.W75: return wedge75Prefab;
            case WedgeSize.W100: return wedge100Prefab;
        }
        return null;
    }

    private void SpawnWedge(WedgeSize size, int sideIndex)
    {
        GameObject prefab = GetPrefabForSize(size);
        if (prefab == null) return;

        float sideAngle = 360f / Mathf.Max(3, sideCount);
        float angleDeg = sideIndex * sideAngle;

        GameObject go = Instantiate(prefab, transform);
        go.name = $"Wedge_{size}_side{sideIndex}";
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.Euler(0f, 0f, angleDeg);

        _spawnedWedges.Add(go);
    }

    private void ClearWedges()
    {
        foreach (var go in _spawnedWedges)
        {
            if (go != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(go);
                else
                    Destroy(go);
#else
                Destroy(go);
#endif
            }
        }
        _spawnedWedges.Clear();
    }

    #endregion
}
