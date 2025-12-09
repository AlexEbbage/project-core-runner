using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates spinning fan blades around Z.
/// - Assign bladePrefab (rotated around Z), and optional hubPrefab.
/// - bladeCount blades are spread evenly 360 degrees.
/// - Direction and speed can be randomised on spawn.
/// - Use the context menu "Regenerate Fan" to rebuild in the scene.
/// </summary>
public class FanObstacle : MonoBehaviour
{
    [Header("Geometry")]
    [SerializeField] private GameObject bladePrefab;
    [SerializeField] private GameObject hubPrefab;
    [SerializeField] private int bladeCount = 4;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeedDegreesPerSecond = 60f;
    [SerializeField] private bool clockwise = true;

    [Header("Randomisation")]
    [SerializeField] private bool randomiseDirectionOnSpawn = true;
    [SerializeField] private bool randomiseSpeedOnSpawn = true;
    [SerializeField] private float minRotationSpeed = 20f;
    [SerializeField] private float maxRotationSpeed = 100f;

    private readonly List<GameObject> _spawnedBlades = new List<GameObject>();
    private GameObject _hubInstance;
    private int _sideCount = 6;

    private void Start()
    {
        // Randomize direction & speed at runtime.
        if (randomiseDirectionOnSpawn)
            clockwise = Random.value > 0.5f;

        if (randomiseSpeedOnSpawn)
            rotationSpeedDegreesPerSecond = Random.Range(minRotationSpeed, maxRotationSpeed);

        Regenerate();
    }

    private void OnValidate()
    {
        bladeCount = Mathf.Max(1, bladeCount);
        // No auto regeneration here; use context menu or runtime.
    }

    private void Update()
    {
        if (!Application.isPlaying) return;

        float dir = clockwise ? -1f : 1f; // Z+ rotation with clockwise meaning negative
        float delta = rotationSpeedDegreesPerSecond * dir * Time.deltaTime;
        transform.Rotate(0f, 0f, delta, Space.Self);
    }

    public void SetSideCount(int sides)
    {
        _sideCount = Mathf.Max(3, sides);
    }

    [ContextMenu("Regenerate Fan")]
    public void Regenerate()
    {
        if (!gameObject.scene.IsValid())
        {
            Debug.LogWarning("FanObstacle: Cannot regenerate on prefab asset. Place in a scene first.", this);
            return;
        }

        ClearBladesAndHub();

        if (bladePrefab == null)
            return;

        // Hub
        if (hubPrefab != null)
        {
            _hubInstance = Instantiate(hubPrefab, transform);
            _hubInstance.name = "FanHub";
            _hubInstance.transform.localPosition = Vector3.zero;
            _hubInstance.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        // Blades
        float angleStep = 360f / Mathf.Max(1, bladeCount);
        for (int i = 0; i < bladeCount; i++)
        {
            float angle = i * angleStep;

            GameObject blade = Instantiate(bladePrefab, transform);
            blade.name = $"FanBlade_{i}";
            blade.transform.localPosition = Vector3.zero;
            blade.transform.localRotation = Quaternion.Euler(0f, 0f, angle);

            _spawnedBlades.Add(blade);
        }
    }

    private void ClearBladesAndHub()
    {
        foreach (var b in _spawnedBlades)
        {
            if (b != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(b);
                else
                    Destroy(b);
#else
                Destroy(b);
#endif
            }
        }
        _spawnedBlades.Clear();

        if (_hubInstance != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(_hubInstance);
            else
                Destroy(_hubInstance);
#else
            Destroy(_hubInstance);
#endif
            _hubInstance = null;
        }
    }
}
