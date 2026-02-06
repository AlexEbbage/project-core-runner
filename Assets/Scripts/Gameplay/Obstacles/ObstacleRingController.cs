using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ObstacleRingController : MonoBehaviour
{
    [System.Serializable]
    private struct DoorPanel
    {
        public Transform panel;
        public Vector3 openLocalOffset;
        public Vector3 openLocalEulerOffset;
    }

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

    [Header("Fan Timing")]
    [Tooltip("If true, fan speed is derived from timePerRotationSeconds instead of using 'speed' as degrees/second.")]
    [SerializeField] private bool useTimePerRotation = false;

    [Tooltip("Base time (in seconds) for the fan to complete one full 360° rotation when speed = 1.")]
    [SerializeField] private float timePerRotationSeconds = 1f;

    [Tooltip("Axis to rotate the fan around, in local space.")]
    [SerializeField] private Vector3 fanRotationAxis = Vector3.forward;

    [Header("Door Setup")]
    [Tooltip("Door pieces that will move from open to closed.")]
    [SerializeField] private DoorPanel[] doorPanels;

    [Tooltip("Total duration of a full open/close cycle (open hold + close + closed hold + open). Used when useWeightedDurations is true.")]
    [SerializeField] private float cycleDuration = 3f;

    [SerializeField] private float openHoldDuration = 1.2f;
    [SerializeField] private float closedHoldDuration = 0.8f;
    [SerializeField] private float transitionDuration = 0.4f;

    [SerializeField] private bool useWeightedDurations = false;

    [Tooltip("Relative weight for how long doors stay open in a full cycle.")]
    [SerializeField] private float openHoldWeight = 1f;

    [Tooltip("Relative weight for how long doors stay closed in a full cycle.")]
    [SerializeField] private float closedHoldWeight = 1f;

    [Tooltip("Relative weight for a single transition (open->closed or closed->open). There are two transitions per cycle.")]
    [SerializeField] private float transitionWeight = 1f;

    [SerializeField] private bool startOpen = true;
    [SerializeField] private bool openOrCloseOnce = true;
    [SerializeField] private bool randomizePhaseOnEnable = false;
    [SerializeField] private AnimationCurve doorMotionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // Cache for doors
    private Vector3[] _closedPositions;
    private Quaternion[] _closedRotations;
    private float _cycleTimer;
    private float _cycleDuration;
    private bool _initialized;

    // Optional link back to config for pooling.
    public ObstacleRingConfig SourceConfig { get; private set; }

    // Colliders for activation/fade-in control
    private Collider[] _colliders;
    private bool _collidersEnabled = true;

    public string RingName => ringName;
    public ObstacleType Type => obstacleType;
    public IReadOnlyList<Transform> PickupSpawnPoints => pickupSpawnPoints;
    public float CurrentSpeed => speed;
    public float CurrentDirection => direction;
    public bool CollidersEnabled => _collidersEnabled;

    private void Awake()
    {
        CacheClosedTransforms();
        UpdateCycleDuration();
        CacheCollidersIfNeeded();
    }

    private void OnEnable()
    {
        ResetCycle();
        _initialized = true;
    }

    private void OnValidate()
    {
        timePerRotationSeconds = Mathf.Max(0.01f, timePerRotationSeconds);
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
                if (_cycleDuration <= 0f)
                    return;

                _cycleTimer += Time.deltaTime;
                float rawAmount = GetOpenAmount(_cycleTimer);
                ApplyDoorAmount(rawAmount);
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

    #region Collider Activation

    private void CacheCollidersIfNeeded()
    {
        if (_colliders == null)
        {
            _colliders = GetComponentsInChildren<Collider>(true);
        }
    }

    /// <summary>
    /// Enables or disables all colliders under this ring.
    /// Used by the generator to keep far-away rings non-lethal until near the player.
    /// </summary>
    public void SetCollidersEnabled(bool enabled)
    {
        CacheCollidersIfNeeded();
        _collidersEnabled = enabled;

        if (_colliders == null)
            return;

        for (int i = 0; i < _colliders.Length; i++)
        {
            if (_colliders[i] == null) continue;
            _colliders[i].enabled = enabled;
        }
    }

    #endregion

    #region Fans

    private void UpdateFan()
    {
        if (fanRotors == null || fanRotors.Count == 0)
            return;

        // If no movement requested, bail early.
        if (Mathf.Approximately(speed, 0f) && !useTimePerRotation)
            return;

        float degreesPerSecond;

        if (useTimePerRotation)
        {
            // Base angular speed from desired rotation time.
            // timePerRotationSeconds = time for one full rotation *when speed = 1*.
            float baseDegPerSec = 360f / Mathf.Max(0.01f, timePerRotationSeconds);

            // 'speed' from SetupForPattern is treated as a multiplier (0 = stop, 1 = normal, 2 = twice as fast, etc.)
            float speedMultiplier = Mathf.Max(0f, speed);

            degreesPerSecond = baseDegPerSec * speedMultiplier;
        }
        else
        {
            // Legacy behaviour: 'speed' is directly degrees/second.
            degreesPerSecond = speed;
        }

        if (Mathf.Approximately(degreesPerSecond, 0f))
            return;

        float deltaAngle = degreesPerSecond * direction * Time.deltaTime;
        Quaternion rotation = Quaternion.AngleAxis(deltaAngle, fanRotationAxis.normalized);

        foreach (var rotor in fanRotors)
        {
            if (rotor == null) continue;
            rotor.localRotation = rotation * rotor.localRotation;
        }
    }

    #endregion

    #region Doors

    /// <summary>
    /// Allows external systems (e.g. ring generator) to set the starting time
    /// for the door cycle. Negative values mean the door will take longer
    /// before reaching its closing phase.
    /// </summary>
    public void SetInitialDoorTime(float initialTime)
    {
        _cycleTimer = initialTime;

        // Immediately apply so visual state matches the time.
        float rawAmount = GetOpenAmount(_cycleTimer);
        ApplyDoorAmount(rawAmount);
    }

    public void ResetCycle()
    {
        CacheClosedTransforms();
        UpdateCycleDuration();

        if (_cycleDuration <= 0f)
        {
            ApplyDoorAmount(startOpen ? 1f : 0f);
            return;
        }

        if (randomizePhaseOnEnable)
        {
            _cycleTimer = Random.Range(0f, _cycleDuration);
            float rawAmount = GetOpenAmount(_cycleTimer);
            ApplyDoorAmount(rawAmount);
        }
        else
        {
            // Leave _cycleTimer as-is; it may be set externally via SetInitialDoorTime.
            float rawAmount = GetOpenAmount(_cycleTimer);
            ApplyDoorAmount(rawAmount);
        }
    }

    private void CacheClosedTransforms()
    {
        if (doorPanels == null)
        {
            _closedPositions = null;
            _closedRotations = null;
            return;
        }

        int count = doorPanels.Length;
        if (_closedPositions == null || _closedPositions.Length != count)
        {
            _closedPositions = new Vector3[count];
            _closedRotations = new Quaternion[count];
        }

        for (int i = 0; i < count; i++)
        {
            var panel = doorPanels[i].panel;
            if (panel == null)
                continue;

            _closedPositions[i] = panel.localPosition;
            _closedRotations[i] = panel.localRotation;
        }
    }

    private void UpdateCycleDuration()
    {
        if (useWeightedDurations)
        {
            // Use weights to derive individual durations from a single total.
            float totalWeight = openHoldWeight + closedHoldWeight + transitionWeight * 2f;
            if (totalWeight <= 0f)
            {
                // Fallback to current explicit durations
                _cycleDuration = openHoldDuration + closedHoldDuration + (transitionDuration * 2f);
                return;
            }

            // Compute each segment as a fraction of the full cycleDuration
            float normalized = Mathf.Max(0.0001f, cycleDuration); // avoid zero

            openHoldDuration = normalized * (openHoldWeight / totalWeight);
            closedHoldDuration = normalized * (closedHoldWeight / totalWeight);
            transitionDuration = normalized * (transitionWeight / totalWeight);

            _cycleDuration = openHoldDuration + closedHoldDuration + (transitionDuration * 2f);
        }
        else
        {
            // Original explicit-duration behaviour
            _cycleDuration = openHoldDuration + closedHoldDuration + (transitionDuration * 2f);
        }
    }

    private float GetOpenAmount(float time)
    {
        // ONE-SHOT BEHAVIOUR (no looping)
        if (openOrCloseOnce)
        {
            if (startOpen)
            {
                // Start open -> hold -> close -> stay closed
                float oneShotDuration = openHoldDuration + transitionDuration;
                if (oneShotDuration <= 0f)
                {
                    // Immediately closed
                    return 0f;
                }

                float t = Mathf.Clamp(time, 0f, oneShotDuration);

                // Hold open
                if (t < openHoldDuration)
                    return 1f;

                // Transition from open (1) to closed (0)
                t -= openHoldDuration;
                if (t < transitionDuration)
                    return EvaluateTransition(t, transitionDuration, 1f, 0f);

                // Finished: stay closed
                return 0f;
            }
            else
            {
                // Start closed -> hold -> open -> stay open
                float oneShotDuration = closedHoldDuration + transitionDuration;
                if (oneShotDuration <= 0f)
                {
                    // Immediately open
                    return 1f;
                }

                float t = Mathf.Clamp(time, 0f, oneShotDuration);

                // Hold closed
                if (t < closedHoldDuration)
                    return 0f;

                // Transition from closed (0) to open (1)
                t -= closedHoldDuration;
                if (t < transitionDuration)
                    return EvaluateTransition(t, transitionDuration, 0f, 1f);

                // Finished: stay open
                return 1f;
            }
        }

        // LOOPING BEHAVIOUR
        if (_cycleDuration <= 0f)
        {
            return startOpen ? 1f : 0f;
        }

        float loopT = time % _cycleDuration;

        if (startOpen)
        {
            // OPEN HOLD
            if (loopT < openHoldDuration)
                return 1f;
            loopT -= openHoldDuration;

            // OPEN -> CLOSED
            if (loopT < transitionDuration)
                return EvaluateTransition(loopT, transitionDuration, 1f, 0f);
            loopT -= transitionDuration;

            // CLOSED HOLD
            if (loopT < closedHoldDuration)
                return 0f;
            loopT -= closedHoldDuration;

            // CLOSED -> OPEN
            if (loopT < transitionDuration)
                return EvaluateTransition(loopT, transitionDuration, 0f, 1f);

            return 1f;
        }
        else
        {
            // CLOSED HOLD
            if (loopT < closedHoldDuration)
                return 0f;
            loopT -= closedHoldDuration;

            // CLOSED -> OPEN
            if (loopT < transitionDuration)
                return EvaluateTransition(loopT, transitionDuration, 0f, 1f);
            loopT -= transitionDuration;

            // OPEN HOLD
            if (loopT < openHoldDuration)
                return 1f;
            loopT -= openHoldDuration;

            // OPEN -> CLOSED
            if (loopT < transitionDuration)
                return EvaluateTransition(loopT, transitionDuration, 1f, 0f);

            return 1f;
        }
    }

    private float EvaluateTransition(float t, float duration, float from, float to)
    {
        if (duration <= 0f)
            return to;

        float normalized = Mathf.Clamp01(t / duration);
        return Mathf.Lerp(from, to, normalized);
    }

    private void ApplyDoorAmount(float rawAmount)
    {
        float amount = doorMotionCurve != null ? doorMotionCurve.Evaluate(rawAmount) : rawAmount;

        if (doorPanels == null || _closedPositions == null)
            return;

        for (int i = 0; i < doorPanels.Length; i++)
        {
            var panel = doorPanels[i].panel;
            if (panel == null)
                continue;

            Vector3 closedPosition = _closedPositions[i];
            Quaternion closedRotation = _closedRotations[i];
            Vector3 offset = doorPanels[i].openLocalOffset;
            Vector3 eulerOffset = doorPanels[i].openLocalEulerOffset;

            panel.localPosition = closedPosition + offset * amount;
            panel.localRotation = closedRotation * Quaternion.Euler(eulerOffset * amount);
        }
    }

    #endregion

    #region Lasers

    private void UpdateLaser()
    {
        // Stub for future laser behaviour (on/off pulsing, rotation, etc.)
        // Safe to extend later.
    }

    #endregion

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

        if (doorPanels != null && doorPanels.Length > 0)
        {
            foreach (var t in doorPanels)
            {
                if (t.panel == null) continue;
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(t.panel.position, Vector3.one * 0.2f);
            }
        }
    }
#endif
}
