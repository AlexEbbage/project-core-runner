using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns a configurable number of laser beams around the ring,
/// aligned to the tunnel sideCount.
/// - The laserSegmentPrefab is rotated around local Z to form each beam.
/// - Colliders should be on the laser prefab.
/// </summary>
public class LaserObstacle : MonoBehaviour
{
    [SerializeField] private GameObject laserSegmentPrefab;
    [SerializeField] private int laserCount = 3;

    private int _sideCount = 6;
    private readonly List<GameObject> _spawnedLasers = new List<GameObject>();

    private void Start()
    {
        Regenerate();
    }

    private void OnValidate()
    {
        laserCount = Mathf.Max(1, laserCount);
        // No auto-regenerate: use context menu or runtime.
    }

    public void SetSideCount(int sides)
    {
        _sideCount = Mathf.Max(3, sides);
    }

    [ContextMenu("Regenerate Lasers")]
    public void Regenerate()
    {
        if (!gameObject.scene.IsValid())
        {
            Debug.LogWarning("LaserObstacle: Cannot regenerate on prefab asset. Place in a scene first.", this);
            return;
        }

        ClearLasers();

        if (laserSegmentPrefab == null)
            return;

        int sides = Mathf.Max(3, _sideCount);
        int count = Mathf.Clamp(laserCount, 1, sides);

        float anglePerSide = 360f / sides;

        // Spread lasers across sides as evenly as possible
        int stepSides = Mathf.Max(1, sides / count);
        int currentSideIndex = 0;

        for (int i = 0; i < count; i++)
        {
            int sideIndex = currentSideIndex;
            float angle = sideIndex * anglePerSide;

            GameObject laser = Instantiate(laserSegmentPrefab, transform);
            laser.name = $"Laser_{i}";
            laser.transform.localPosition = Vector3.zero;
            laser.transform.localRotation = Quaternion.Euler(0f, 0f, angle);

            _spawnedLasers.Add(laser);

            currentSideIndex = (currentSideIndex + stepSides) % sides;
        }
    }

    private void ClearLasers()
    {
        foreach (var go in _spawnedLasers)
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
        _spawnedLasers.Clear();
    }
}
