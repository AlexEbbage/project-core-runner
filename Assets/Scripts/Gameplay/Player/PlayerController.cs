using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Super Hexagon-style controller:
/// - Player moves forward along +Z at a given speed.
/// - Left/right input rotates the player around the tube axis (Z),
///   staying on a circle of radius tubeRadius.
/// - "Up" always points toward the tube center (no extra tilt).
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Forward Movement")]
    [SerializeField] private float defaultForwardSpeed = 24f;

    [Header("Tube Movement")]
    [Tooltip("Radius of the tube (distance from centre).")]
    [SerializeField] private float tubeRadius = 5f;

    [Tooltip("Degrees per second of rotation around the tube at full input.")]
    [SerializeField] private float angularSpeedDegrees = 260f;

    [Header("Input")]
    [Tooltip("If true AND legacy input is enabled, Horizontal axis (A/D, arrows) is used in editor.")]
    [SerializeField] private bool allowKeyboardInputInEditor = true;
    [Tooltip("Scales touch drag input after normalizing by screen width.")]
    [SerializeField] private float touchInputSensitivity = 1f;
    [Tooltip("Fraction of screen width per second needed to reach full touch input.")]
    [SerializeField, Range(0.01f, 0.5f)] private float touchInputFullScaleFraction = 0.02f;

    [Header("Run Control")]
    [Tooltip("If true, movement is enabled immediately at Start. Otherwise, call StartRun() from GameManager.")]
    [SerializeField] private bool startMovingOnStart = true;

    private float _currentForwardSpeed;
    private float _moveInput;
    private bool _isRunning;
    private bool _autoPilotActive;
    private float _autoPilotInput;

    // Angular position around the tube, in degrees.
    private float _angleDegrees;
    private float _zPosition;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;        // move via transform
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

        _currentForwardSpeed = defaultForwardSpeed;

        Vector3 pos = transform.position;
        _zPosition = pos.z;

        if (Mathf.Abs(pos.x) > 0.001f || Mathf.Abs(pos.y) > 0.001f)
            _angleDegrees = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;
        else
            _angleDegrees = 0f;
    }

    private void Start()
    {
        if (startMovingOnStart)
            StartRun();
        else
            StopRun();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        if (context.control?.device is Touchscreen)
        {
            float deltaTime = Mathf.Max(Time.unscaledDeltaTime, 0.001f);
            float normalized = input.x / Mathf.Max(Screen.width * touchInputFullScaleFraction * deltaTime, 1f);
            _moveInput = Mathf.Clamp(normalized * touchInputSensitivity, -1f, 1f);
            return;
        }

        _moveInput = Mathf.Clamp(input.x, -1f, 1f);
    }

    private void Update()
    {
        if (!_isRunning)
            return;

        float dt = Time.deltaTime;
        //float horizontalInput = GetHorizontalInput();
        float input = _autoPilotActive ? _autoPilotInput : _moveInput;
        UpdateMovementAndRotation(input, dt);
    }

    // ---- Public API ----

    public void StartRun()
    {
        _isRunning = true;
    }

    public void StopRun()
    {
        _isRunning = false;
    }

    public void SetForwardSpeed(float newSpeed)
    {
        _currentForwardSpeed = newSpeed > 0f ? newSpeed : defaultForwardSpeed;
    }

    public float GetCurrentForwardSpeed() => _currentForwardSpeed;

    public float CurrentAngleDegrees => _angleDegrees;
    public float TubeRadius => tubeRadius;

    public void SetAutoPilotActive(bool active, float inputValue = 0f)
    {
        _autoPilotActive = active;
        _autoPilotInput = Mathf.Clamp(inputValue, -1f, 1f);
    }

    public void SetAutoPilotInput(float inputValue)
    {
        _autoPilotInput = Mathf.Clamp(inputValue, -1f, 1f);
    }

    public float AngularSpeedDegrees => angularSpeedDegrees;

    // ---- Core movement ----

//    private float GetHorizontalInput()
//    {
//        float input = 0f;

//        // Touch: left half screen = left, right half = right
//        if (Input.touchCount > 0)
//        {
//            Touch t = Input.GetTouch(0);
//            if (t.phase == TouchPhase.Began ||
//                t.phase == TouchPhase.Moved ||
//                t.phase == TouchPhase.Stationary)
//            {
//                float halfWidth = Screen.width * 0.5f;
//                input = (t.position.x > halfWidth) ? 1f : -1f;
//            }
//        }


//#if ENABLE_LEGACY_INPUT_MANAGER && (UNITY_EDITOR || UNITY_STANDALONE)
//        // Optional keyboard for testing in editor
//        if (allowKeyboardInputInEditor)
//        {
//            float keyboard = Input.GetAxisRaw("Horizontal");
//            if (Mathf.Abs(keyboard) > 0.01f)
//            {
//                input = keyboard;
//            }
//        }
//#endif

//        return Mathf.Clamp(input, -1f, 1f);
//    }

    private void UpdateMovementAndRotation(float horizontalInput, float dt)
    {
        // Move forward along Z
        _zPosition += _currentForwardSpeed * dt;

        // Rotate around tube axis
        _angleDegrees += horizontalInput * angularSpeedDegrees * dt;
        float angleRad = _angleDegrees * Mathf.Deg2Rad;

        float x = Mathf.Cos(angleRad) * tubeRadius;
        float y = Mathf.Sin(angleRad) * tubeRadius;

        Vector3 newPos = new Vector3(x, y, _zPosition);

        // Up points toward centre, forward is +Z
        Vector3 radialIn = new Vector3(-x, -y, 0f).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, radialIn);

        transform.SetPositionAndRotation(newPos, targetRotation);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Vector3.zero, tubeRadius);
    }
#endif
}
