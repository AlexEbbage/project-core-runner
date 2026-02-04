using UnityEngine;

/// <summary>
/// Animates a timed open/close door gap by sliding door panels.
/// Configure panels with per-panel open offsets/rotations.
/// </summary>
public class DoorObstacle : MonoBehaviour
{
    [System.Serializable]
    private struct DoorPanel
    {
        public Transform panel;
        public Vector3 openLocalOffset;
        public Vector3 openLocalEulerOffset;
    }

    [Header("Panels")]
    [SerializeField] private DoorPanel[] doorPanels;

    [Header("Timing")]
    [SerializeField] private float openHoldDuration = 1.2f;
    [SerializeField] private float closedHoldDuration = 0.8f;
    [SerializeField] private float transitionDuration = 0.4f;
    [SerializeField] private bool startOpen = true;
    [SerializeField] private bool randomizePhaseOnEnable = true;

    [Header("Distance Trigger")]
    [SerializeField] private bool useDistanceBasedClosing = true;
    [SerializeField] private float closeLeadTimeSeconds = 5f;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private RunSpeedController runSpeedController;

    [Header("Motion")]
    [SerializeField] private AnimationCurve doorMotionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Vector3[] _closedPositions;
    private Quaternion[] _closedRotations;
    private float _cycleTimer;
    private float _cycleDuration;
    private bool _isClosing;
    private bool _isClosed;
    private float _closeStartTime;
    private float _startOpenAmount;

    private void Awake()
    {
        CacheClosedTransforms();
        UpdateCycleDuration();
        ApplyDoorAmount(startOpen ? 1f : 0f);
        _startOpenAmount = startOpen ? 1f : 0f;

        if (useDistanceBasedClosing)
            EnsureDistanceReferences();
    }

    private void OnEnable()
    {
        ResetCycle();
    }

    private void OnValidate()
    {
        openHoldDuration = Mathf.Max(0f, openHoldDuration);
        closedHoldDuration = Mathf.Max(0f, closedHoldDuration);
        transitionDuration = Mathf.Max(0f, transitionDuration);
        closeLeadTimeSeconds = Mathf.Max(0f, closeLeadTimeSeconds);
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        if (useDistanceBasedClosing)
        {
            UpdateDistanceClosing();
            return;
        }

        if (_cycleDuration <= 0f)
            return;

        _cycleTimer += Time.deltaTime;
        float rawAmount = GetOpenAmount(_cycleTimer);
        ApplyDoorAmount(rawAmount);
    }

    public void ResetCycle()
    {
        CacheClosedTransforms();
        UpdateCycleDuration();

        if (useDistanceBasedClosing)
        {
            EnsureDistanceReferences();
            ResetDistanceState();
            return;
        }

        if (_cycleDuration <= 0f)
        {
            ApplyDoorAmount(startOpen ? 1f : 0f);
            return;
        }

        _cycleTimer = randomizePhaseOnEnable
            ? Random.Range(0f, _cycleDuration)
            : 0f;

        float rawAmount = GetOpenAmount(_cycleTimer);
        ApplyDoorAmount(rawAmount);
    }

    private void EnsureDistanceReferences()
    {
        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

        if (runSpeedController == null)
            runSpeedController = FindFirstObjectByType<RunSpeedController>();
    }

    private void ResetDistanceState()
    {
        _isClosing = false;
        _isClosed = !startOpen;
        _closeStartTime = 0f;
        _startOpenAmount = startOpen ? 1f : 0f;
        ApplyDoorAmount(_startOpenAmount);
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
        _cycleDuration = openHoldDuration + closedHoldDuration + (transitionDuration * 2f);
    }

    private float GetOpenAmount(float time)
    {
        if (_cycleDuration <= 0f)
            return startOpen ? 1f : 0f;

        float t = time % _cycleDuration;

        if (startOpen)
        {
            if (t < openHoldDuration)
                return 1f;
            t -= openHoldDuration;

            if (t < transitionDuration)
                return EvaluateTransition(t, transitionDuration, 1f, 0f);
            t -= transitionDuration;

            if (t < closedHoldDuration)
                return 0f;
            t -= closedHoldDuration;

            if (t < transitionDuration)
                return EvaluateTransition(t, transitionDuration, 0f, 1f);

            return 1f;
        }

        if (t < closedHoldDuration)
            return 0f;
        t -= closedHoldDuration;

        if (t < transitionDuration)
            return EvaluateTransition(t, transitionDuration, 0f, 1f);
        t -= transitionDuration;

        if (t < openHoldDuration)
            return 1f;
        t -= openHoldDuration;

        if (t < transitionDuration)
            return EvaluateTransition(t, transitionDuration, 1f, 0f);

        return 0f;
    }

    private float EvaluateTransition(float t, float duration, float from, float to)
    {
        if (duration <= 0f)
            return to;

        float normalized = Mathf.Clamp01(t / duration);
        return Mathf.Lerp(from, to, normalized);
    }

    private void UpdateDistanceClosing()
    {
        if (_isClosed)
            return;

        if (playerController == null)
            return;

        float playerZ = playerController.transform.position.z;
        float doorZ = transform.position.z;
        float distance = doorZ - playerZ;

        if (distance <= 0f)
        {
            ApplyDoorAmount(0f);
            _isClosing = true;
            _isClosed = true;
            return;
        }

        if (!_isClosing)
        {
            float speed = runSpeedController != null
                ? runSpeedController.CurrentSpeed
                : playerController.GetCurrentForwardSpeed();

            if (speed > 0f)
            {
                float triggerDistance = speed * closeLeadTimeSeconds;
                if (distance <= triggerDistance)
                {
                    _isClosing = true;
                    _closeStartTime = Time.time;
                }
                else
                {
                    ApplyDoorAmount(_startOpenAmount);
                    return;
                }
            }
            else
            {
                ApplyDoorAmount(_startOpenAmount);
                return;
            }
        }

        float elapsed = Time.time - _closeStartTime;
        float rawAmount = EvaluateTransition(elapsed, transitionDuration, 1f, 0f);
        ApplyDoorAmount(rawAmount);

        if (elapsed >= transitionDuration)
            _isClosed = true;
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
}
