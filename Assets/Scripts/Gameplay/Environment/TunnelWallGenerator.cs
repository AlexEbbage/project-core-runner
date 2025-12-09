using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds an endless tunnel by stacking a single full-ring prefab along +Z.
/// - Each segment is scaled by segmentZScale on local Z.
/// - Segments are recycled in front of the player.
/// - Colors alternate between a base gradient color and a darker variant
///   to give a better sense of speed.
/// 
/// The wallSegmentPrefab should already be modeled for the current sideCount
/// (e.g., a full 6-sided tunnel slice, 8-sided slice, etc.).
/// </summary>
public class TunnelWallGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject wallSegmentPrefab;
    [SerializeField] private Transform player;

    [Header("Shape")]
    [Tooltip("Number of sides for the tunnel polygon (used for prefab selection / reference).")]
    [SerializeField] private int sideCount = 6;

    [Header("Length")]
    [Tooltip("How many tunnel slices to keep around the player.")]
    [SerializeField] private int segmentCount = 40;

    [Tooltip("Distance between segment centers along Z, in world units.")]
    [SerializeField] private float segmentLength = 10f;

    [Tooltip("How far behind the player a segment can be before we recycle it.")]
    [SerializeField] private float recycleBehindDistance = 30f;

    [Tooltip("Local Z scale applied to each segment (e.g. 10 for a 10x stretch).")]
    [SerializeField] private float segmentZScale = 10f;

    [Header("Colors")]
    [SerializeField] private Gradient tunnelColorGradient;
    [SerializeField] private Color colorA;
    [SerializeField] private Color colorB;
    [SerializeField] private float colorCycleSpeed = 0.2f;

    [Tooltip("How much darker the alternating slices should be (0 = no darkening, 1 = black).")]
    [Range(0f, 1f)]
    [SerializeField] private float darkenFactor = 0.35f;

    private readonly List<Transform> _segments = new List<Transform>();
    private float _colorTime;
    private float _furthestZ;

    private void Start()
    {
        if (wallSegmentPrefab == null || player == null)
        {
            Debug.LogError("TunnelWallGenerator: missing references.", this);
            enabled = false;
            return;
        }

        BuildInitialSegments();
    }

    private void Update()
    {
        if (player == null)
            return;

        _colorTime += Time.deltaTime * colorCycleSpeed;

        RecycleSegments();
        ApplyColors(_colorTime);
    }

    private void BuildInitialSegments()
    {
        ClearSegments();

        float playerZ = player.position.z;
        _furthestZ = playerZ;

        for (int i = 0; i < segmentCount; i++)
        {
            float z = playerZ + i * segmentLength;
            _furthestZ = z;

            GameObject segGO = Instantiate(wallSegmentPrefab, transform);
            segGO.name = $"TunnelSegment_{i}";
            segGO.transform.position = new Vector3(0f, 0f, z);

            // Apply Z scale
            Vector3 localScale = segGO.transform.localScale;
            localScale.z *= segmentZScale;
            segGO.transform.localScale = localScale;

            _segments.Add(segGO.transform);
        }
    }

    private void ClearSegments()
    {
        foreach (Transform t in _segments)
        {
            if (t != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(t.gameObject);
                else
                    Destroy(t.gameObject);
#else
                Destroy(t.gameObject);
#endif
            }
        }
        _segments.Clear();
    }

    private void RecycleSegments()
    {
        float playerZ = player.position.z;

        for (int i = 0; i < _segments.Count; i++)
        {
            Transform seg = _segments[i];
            if (seg == null) continue;

            float z = seg.position.z;
            if (playerZ - z > recycleBehindDistance)
            {
                _furthestZ += segmentLength;
                seg.position = new Vector3(0f, 0f, _furthestZ);
            }
        }
    }

    private void ApplyColors(float time)
    {
        if (tunnelColorGradient == null)
            return;

        // One global gradient position; slices alternate darker vs. normal.
        //float t = 0.5f + 0.5f * Mathf.Sin(time);
        //Color baseColor = tunnelColorGradient.Evaluate(t);
        //Color darkColor = Color.Lerp(baseColor, Color.black, darkenFactor);

        for (int i = 0; i < _segments.Count; i++)
        {
            Transform seg = _segments[i];
            if (seg == null) continue;

            bool isEven = (i % 2) == 0;
            //Color c = isEven ? baseColor : darkColor;
            Color c = isEven ? colorA : colorB;

            var renderers = seg.GetComponentsInChildren<Renderer>();
            foreach (var rend in renderers)
            {
                if (rend != null)
                    rend.material.color = c;
            }
        }
    }

    /// <summary>
    /// Called by menu when sideCount changes.
    /// Here we just rebuild the stack; make sure you assign the appropriate prefab
    /// for the new sideCount.
    /// </summary>
    public void Rebuild(int sides)
    {
        sideCount = Mathf.Max(3, sides);
        BuildInitialSegments();
    }

    public void SetColorGradient(Gradient gradient)
    {
        if (gradient == null) return;
        tunnelColorGradient = gradient;
    }
}
