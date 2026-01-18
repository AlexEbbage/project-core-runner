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
    [Header("Rotation")]
    [SerializeField] private bool enableRotation = false;
    [SerializeField] private float rotationSpeed = 45f;

    [Header("Beam Cycling")]
    [SerializeField] private bool enableBeamCycle = false;
    [SerializeField] private float pulseDuration = 2f;
    [Range(0f, 1f)]
    [SerializeField] private float dutyCycle = 0.6f;
    [SerializeField] private bool startBeamsOn = true;
    [SerializeField] private bool randomizeBeamCyclePhase = false;

    private int _sideCount = 6;
    private readonly List<GameObject> _spawnedLasers = new List<GameObject>();
    private float _beamCycleTimer;
    private bool _beamsOn = true;

    private void Start()
    {
        Regenerate();
    }

    private void OnEnable()
    {
        ResetBeamCycle();
    }

    private void OnValidate()
    {
        laserCount = Mathf.Max(1, laserCount);
        rotationSpeed = Mathf.Max(0f, rotationSpeed);
        pulseDuration = Mathf.Max(0.05f, pulseDuration);
        dutyCycle = Mathf.Clamp01(dutyCycle);
        // No auto-regenerate: use context menu or runtime.
    }

    private void Update()
    {
        if (enableRotation && rotationSpeed > 0f)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime, Space.Self);
        }

        if (enableBeamCycle)
        {
            _beamCycleTimer += Time.deltaTime;
            float duration = _beamsOn ? GetBeamOnDuration() : GetBeamOffDuration();
            if (_beamCycleTimer >= duration)
            {
                _beamCycleTimer = 0f;
                _beamsOn = !_beamsOn;
                SetBeamsActive(_beamsOn);
            }
        }
        else if (!_beamsOn)
        {
            _beamsOn = true;
            SetBeamsActive(true);
        }
    }

    public void SetSideCount(int sides)
    {
        _sideCount = Mathf.Max(3, sides);
    }

    public void ConfigureRotation(bool enabled, float speed)
    {
        enableRotation = enabled;
        rotationSpeed = Mathf.Max(0f, speed);
    }

    public void ConfigureBeamCycle(bool enabled, float pulseLength, float duty, bool startOn, bool randomizePhase)
    {
        enableBeamCycle = enabled;
        pulseDuration = Mathf.Max(0.05f, pulseLength);
        dutyCycle = Mathf.Clamp01(duty);
        startBeamsOn = startOn;
        randomizeBeamCyclePhase = randomizePhase;
        if (!enableBeamCycle)
        {
            _beamsOn = true;
            _beamCycleTimer = 0f;
            SetBeamsActive(true);
        }
        else
        {
            ResetBeamCycle();
        }
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

        SetBeamsActive(_beamsOn || !enableBeamCycle);
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

    private void SetBeamsActive(bool isActive)
    {
        foreach (var laser in _spawnedLasers)
        {
            if (laser != null)
                laser.SetActive(isActive);
        }
    }

    private void ResetBeamCycle()
    {
        _beamsOn = startBeamsOn || !enableBeamCycle;
        float duration = _beamsOn ? GetBeamOnDuration() : GetBeamOffDuration();
        _beamCycleTimer = randomizeBeamCyclePhase && duration > 0f
            ? Random.Range(0f, duration)
            : 0f;
        SetBeamsActive(_beamsOn);
    }

    private float GetBeamOnDuration()
    {
        return Mathf.Max(0.05f, pulseDuration * Mathf.Clamp01(dutyCycle));
    }

    private float GetBeamOffDuration()
    {
        return Mathf.Max(0.05f, pulseDuration * (1f - Mathf.Clamp01(dutyCycle)));
    }
}
