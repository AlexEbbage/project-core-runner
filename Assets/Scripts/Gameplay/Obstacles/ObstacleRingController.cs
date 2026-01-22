using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ObstacleRingController : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string ringName;
    [SerializeField] private ObstacleType obstacleType;

    [Header("Pickup Spawn Points")]
    [Tooltip("Transforms around this ring where pickups can spawn.")]
    [SerializeField] private List<Transform> pickupSpawnPoints = new List<Transform>();

    [Header("Movement (generic)")]
    [Tooltip("Base movement speed for this obstacle. Ignored for walls.")]
    [SerializeField] private float speed = 0f;

    [Tooltip("Direction multiplier, usually -1 or +1.")]
    [SerializeField] private float direction = 1f;

    [Header("Fan Setup")]
    [Tooltip("Rotating parts of the fan obstacle.")]
    [SerializeField] private List<Transform> fanRotors = new List<Transform>();

    [Tooltip("Axis to rotate the fan around, in local space.")]
    [SerializeField] private Vector3 fanRotationAxis = Vector3.forward;

    [Header("Door Setup")]
    [Tooltip("Door pieces that will move from open to closed.")]
    [SerializeField] private List<Transform> doorTransforms = new List<Transform>();

    [Tooltip("Local offset from the open position toward the closed position.")]
    [SerializeField] private Vector3 doorCloseOffset = new Vector3(0f, -1f, 0f);

    private Vector3[] _doorOpenLocalPositions;
    private bool _initialized;

    // Optional link back to config for pooling.
    public ObstacleRingConfig SourceConfig { get; private set; }

    public string RingName => ringName;
    public ObstacleType Type => obstacleType;
    public IReadOnlyList<Transform> PickupSpawnPoints => pickupSpawnPoints;
    public float CurrentSpeed => speed;
    public float CurrentDirection => direction;

    private void Awake()
    {
        CacheDoorOpenPositions();
    }

    private void OnEnable()
    {
        ResetDoorsToOpen();
        _initialized = true;
    }

    private void Update()
    {
        if (!_initialized)
            return;

        switch (obstacleType)
        {
            case ObstacleType.Walls:
                // Static, no behaviour.
                break;

            case ObstacleType.Fan:
                UpdateFan();
                break;

            case ObstacleType.Doors:
                UpdateDoors();
                break;

            case ObstacleType.Laser:
                UpdateLaser();
                break;
        }
    }

    public void InitializeFromConfig(ObstacleRingConfig config)
    {
        SourceConfig = config;
        if (config != null)
        {
            obstacleType = config.ObstacleType;
        }
    }

    /// <summary>
    /// Called by the generator when this ring is spawned/re-used to apply
    /// difficulty-controlled parameters.
    /// </summary>
    public void SetupForPattern(float patternSpeed, float patternDirection)
    {
        speed = Mathf.Max(0f, patternSpeed);

        // If direction is 0, default to +1.
        direction = Mathf.Approximately(patternDirection, 0f) ? 1f : patternDirection;
    }

    private void UpdateFan()
    {
        if (fanRotors == null || fanRotors.Count == 0 || Mathf.Approximately(speed, 0f))
            return;

        float deltaAngle = speed * direction * Time.deltaTime;
        Quaternion rotation = Quaternion.AngleAxis(deltaAngle, fanRotationAxis.normalized);

        foreach (var rotor in fanRotors)
        {
            if (rotor == null) continue;
            rotor.localRotation = rotation * rotor.localRotation;
        }
    }

    private void CacheDoorOpenPositions()
    {
        if (doorTransforms == null || doorTransforms.Count == 0)
            return;

        _doorOpenLocalPositions = new Vector3[doorTransforms.Count];
        for (int i = 0; i < doorTransforms.Count; i++)
        {
            Transform t = doorTransforms[i];
            _doorOpenLocalPositions[i] = t != null ? t.localPosition : Vector3.zero;
        }
    }

    private void ResetDoorsToOpen()
    {
        if (_doorOpenLocalPositions == null || doorTransforms == null)
            return;

        int count = Mathf.Min(_doorOpenLocalPositions.Length, doorTransforms.Count);
        for (int i = 0; i < count; i++)
        {
            Transform t = doorTransforms[i];
            if (t == null) continue;
            t.localPosition = _doorOpenLocalPositions[i];
        }
    }

    /// <summary>
    /// Moves each door along doorCloseOffset * direction at the configured speed.
    /// </summary>
    private void UpdateDoors()
    {
        if (doorTransforms == null || doorTransforms.Count == 0 || Mathf.Approximately(speed, 0f))
            return;

        if (_doorOpenLocalPositions == null || _doorOpenLocalPositions.Length != doorTransforms.Count)
        {
            CacheDoorOpenPositions();
        }

        float step = speed * Time.deltaTime;
        int count = doorTransforms.Count;

        for (int i = 0; i < count; i++)
        {
            Transform t = doorTransforms[i];
            if (t == null) continue;

            Vector3 openPos = _doorOpenLocalPositions[i];
            Vector3 targetClosedPos = openPos + doorCloseOffset * direction;

            t.localPosition = Vector3.MoveTowards(t.localPosition, targetClosedPos, step);
        }
    }

    private void UpdateLaser()
    {
        // Stub for future laser behaviour (on/off pulsing, rotation, etc.)
        // Safe to extend later.
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (pickupSpawnPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var t in pickupSpawnPoints)
            {
                if (t == null) continue;
                Gizmos.DrawWireSphere(t.position, 0.1f);
            }
        }

        if (doorTransforms != null)
        {
            Gizmos.color = Color.cyan;
            foreach (var t in doorTransforms)
            {
                if (t == null) continue;
                Gizmos.DrawWireCube(t.position, Vector3.one * 0.2f);
            }
        }
    }
#endif
}
