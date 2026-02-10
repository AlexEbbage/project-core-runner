using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Super Hexagon-style controller:
/// - Player moves forward along +Z at a given speed.
/// - Left/right input rotates the player around the tube axis (Z),
///   staying on a circle of radius tubeRadius.
/// - "Up" always points toward the tube center (no extra tilt).
///
/// Mobile touch:
/// - Uses a Virtual Joystick (position-based, stable).
/// - Touch begins inside a configurable control zone.
/// - Displacement from touch origin drives left/right input.
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

    [Header("Handling Upgrade")]
    [SerializeField] private PlayerProfile profile;
    [Tooltip("Extra degrees per second per handling upgrade level.")]
    [SerializeField] private float handlingAngularSpeedBonusPerLevel = 10f;
    [Tooltip("Reduce input smoothing time per handling upgrade level.")]
    [SerializeField] private float handlingInputSmoothingReductionPerLevel = 0f;
    [Tooltip("Minimum input smoothing time when applying handling upgrades.")]
    [SerializeField] private float minInputSmoothingTime = 0.02f;

    [Header("Input")]
    [Tooltip("If true AND legacy input is enabled, Horizontal axis (A/D, arrows) is used in editor.")]
    [SerializeField] private bool allowKeyboardInputInEditor = true;

    [Tooltip("Ignore tiny input noise below this threshold.")]
    [SerializeField, Range(0f, 0.2f)] private float inputDeadZone = 0.02f;

    [Tooltip("Seconds to smooth input changes to avoid jitter.")]
    [SerializeField, Range(0.01f, 0.5f)] private float inputSmoothingTime = 0.06f;

    [Tooltip("If true, touch uses left/right screen halves instead of drag input.")]
    [SerializeField] private bool forceTouchButtonsMode;

    [Header("Virtual Joystick (Touch)")]
    [Tooltip("Enable virtual joystick touch steering (recommended for mobile).")]
    [SerializeField] private bool useVirtualJoystickOnTouch = true;

    [Tooltip("Control zone in normalized screen fractions (x,y,w,h). Example: (0,0,0.6,0.75) = left 60% + bottom 75%.")]
    [SerializeField] private Rect controlZoneNormalized = new Rect(0f, 0f, 0.6f, 0.75f);

    [Tooltip("Max horizontal drag (in pixels) to reach full input.")]
    [SerializeField] private float joystickRadiusPx = 220f;

    [Tooltip("Dead zone in pixels to ignore micro movement.")]
    [SerializeField] private float joystickDeadZonePx = 14f;

    [Tooltip("Optional curve: <1 softer around centre, >1 more aggressive.")]
    [SerializeField, Range(0.25f, 3f)] private float joystickResponseExponent = 1.35f;

    [Tooltip("If > 0, the joystick origin slowly follows the finger (prevents hitting edge). 0 = fixed origin.")]
    [SerializeField, Range(0f, 1f)] private float joystickRecenter = 0.15f;

    [Tooltip("Ignore touches that start over UI (recommended).")]
    [SerializeField] private bool ignoreTouchesOverUI = true;

    [Header("Run Control")]
    [Tooltip("If true, movement is enabled immediately at Start. Otherwise, call StartRun() from GameManager.")]
    [SerializeField] private bool startMovingOnStart = true;

    private float _currentForwardSpeed;
    private float _moveInputTarget;
    private float _smoothedMoveInput;
    private float _moveInputVelocity;

    private bool _isRunning;
    private bool _autoPilotActive;
    private float _autoPilotInput;

    private float _baseAngularSpeedDegrees;
    private float _handlingAdjustedAngularSpeedDegrees;

    // Angular position around the tube, in degrees.
    private float _angleDegrees;
    private float _zPosition;

    private Rigidbody _rigidbody;
    private float _baseInputSmoothingTime;

    // Virtual joystick touch tracking
    private bool _joystickActive;
    private int _joystickTouchId = -1;
    private Vector2 _joystickOrigin;
    private Vector2 _joystickCurrent;

    private void Awake()
    {
        SettingsData.Initialize();

        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true; // move via transform
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

        _currentForwardSpeed = defaultForwardSpeed;
        _baseAngularSpeedDegrees = angularSpeedDegrees;
        _handlingAdjustedAngularSpeedDegrees = _baseAngularSpeedDegrees;

        _baseInputSmoothingTime = inputSmoothingTime;

        ApplySensitivitySettings(SettingsData.TouchSensitivity);

        Vector3 pos = transform.position;
        _zPosition = pos.z;

        if (Mathf.Abs(pos.x) > 0.001f || Mathf.Abs(pos.y) > 0.001f)
            _angleDegrees = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;
        else
            _angleDegrees = 0f;
    }

    private void OnEnable()
    {
        if (profile != null)
        {
            profile.UpgradeLevelChanged += HandleUpgradeLevelChanged;
        }

        SettingsData.TouchSensitivityChanged += HandleSensitivityChanged;
        SettingsData.RunSensitivityChanged += HandleRunSensitivityChanged;

        ApplyHandlingUpgrade();
    }

    private void OnDisable()
    {
        if (profile != null)
        {
            profile.UpgradeLevelChanged -= HandleUpgradeLevelChanged;
        }

        SettingsData.TouchSensitivityChanged -= HandleSensitivityChanged;
        SettingsData.RunSensitivityChanged -= HandleRunSensitivityChanged;
    }

    private void Start()
    {
        if (startMovingOnStart)
            StartRun();
        else
            StopRun();
    }

    /// <summary>
    /// New Input System callback. We now ignore Touchscreen deltas here (they cause jitter).
    /// Touch is handled explicitly via virtual joystick or button mode in Update().
    /// </summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.control?.device is Touchscreen)
            return;

        Vector2 input = context.ReadValue<Vector2>();
        _moveInputTarget = Mathf.Clamp(input.x, -1f, 1f);
    }

    private void Update()
    {
        if (!_isRunning)
            return;

        float dt = Time.deltaTime;

#if UNITY_ANDROID || UNITY_IOS
        if (Touchscreen.current == null)
        {
            Debug.LogWarning("Touchscreen.current is NULL");
        }
#endif

        // 1. AutoPilot overrides everything
        if (_autoPilotActive)
        {
            UpdateMovementAndRotation(_autoPilotInput, dt);
            return;
        }

        bool inputDrivenThisFrame = false;

        // 2. Touch input (buttons or virtual joystick)
        //if (UseTouchButtonsMode())
        //{
        //    if (TryGetTouchButtonsInput(out float buttons))
        //    {
        //        _moveInputTarget = buttons;
        //        inputDrivenThisFrame = true;
        //    }
        //}
        //else if (useVirtualJoystickOnTouch)
        //{
        if (TryGetVirtualJoystickInput(out float joystick))
        {
            _moveInputTarget = joystick;
            inputDrivenThisFrame = true;
        }
        //}

        // 3. Keyboard fallback (Editor / Standalone only)
#if ENABLE_LEGACY_INPUT_MANAGER && (UNITY_EDITOR || UNITY_STANDALONE)
    if (!inputDrivenThisFrame && allowKeyboardInputInEditor)
    {
        float keyboard = Input.GetAxisRaw("Horizontal"); // A/D, arrows
        if (Mathf.Abs(keyboard) > 0.01f)
        {
            _moveInputTarget = Mathf.Clamp(keyboard, -1f, 1f);
            inputDrivenThisFrame = true;
        }
    }
#endif

        // 4. Smooth and apply movement
        float input = GetSmoothedInput(dt);
        UpdateMovementAndRotation(input, dt);
    }


    // ---- Public API ----

    public void StartRun()
    {
        _isRunning = true;
        ApplySensitivitySettings();
        ResetSmoothedInput(_moveInputTarget);
    }

    public void StopRun()
    {
        _isRunning = false;
        ResetSmoothedInput(0f);
        ResetJoystickState();
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

    public void ApplySensitivitySettings()
    {
        ApplySensitivitySettings(SettingsData.TouchSensitivity);
    }

    private void HandleSensitivityChanged(float sensitivity)
    {
        ApplySensitivitySettings(sensitivity);
    }

    private void HandleRunSensitivityChanged(float _)
    {
        ApplySensitivitySettings(SettingsData.TouchSensitivity);
    }

    private void ApplySensitivitySettings(float sensitivity)
    {
        float runSensitivity = SettingsData.RunSensitivity;
        angularSpeedDegrees = _handlingAdjustedAngularSpeedDegrees * sensitivity * runSensitivity;
    }

    public void RefreshHandlingFromProfile()
    {
        ApplyHandlingUpgrade();
    }

    // ---- Touch modes ----

    private bool UseTouchButtonsMode()
    {
        if (forceTouchButtonsMode)
            return true;

        return SettingsData.CurrentTouchInputMode == SettingsData.TouchInputMode.Buttons;
    }

    private bool TryGetTouchButtonsInput(out float input)
    {
        if (Touchscreen.current == null)
        {
            input = 0f;
            return false;
        }

        bool leftPressed = false;
        bool rightPressed = false;
        bool anyPressed = false;
        float halfWidth = Screen.width * 0.5f;

        foreach (var touch in Touchscreen.current.touches)
        {
            if (!touch.press.isPressed)
                continue;

            // optional: ignore UI touches
            if (ignoreTouchesOverUI && IsTouchOverUI(touch.touchId.ReadValue()))
                continue;

            // optional: enforce control zone
            Vector2 pos = touch.position.ReadValue();
            if (!IsInControlZone(pos))
                continue;

            anyPressed = true;

            float x = pos.x;
            if (x < halfWidth)
                leftPressed = true;
            else
                rightPressed = true;
        }

        if (!anyPressed || leftPressed == rightPressed)
        {
            input = 0f;
            return anyPressed;
        }

        input = leftPressed ? -1f : 1f;
        return true;
    }

    private bool TryGetVirtualJoystickInput(out float input)
    {
        input = 0f;

        var ts = Touchscreen.current;
        if (ts == null)
        {
            ResetJoystickState();
            return false;
        }

        // Acquire a joystick touch if we don't have one
        if (!_joystickActive)
        {
            foreach (var t in ts.touches)
            {
                if (!t.press.isPressed) continue;

                int touchId = t.touchId.ReadValue();
                Vector2 pos = t.position.ReadValue();

                // Must start inside control zone
                if (!IsInControlZone(pos))
                    continue;

                // Optional: ignore UI touches
                if (ignoreTouchesOverUI && IsTouchOverUI(touchId))
                    continue;

                _joystickActive = true;
                _joystickTouchId = touchId;
                _joystickOrigin = pos;
                _joystickCurrent = pos;
                break;
            }

            if (!_joystickActive)
                return false;
        }

        // Find the active touch by id
        TouchControl activeTouch = null;
        foreach (var t in ts.touches)
        {
            if (!t.press.isPressed) continue;
            if (t.touchId.ReadValue() == _joystickTouchId)
            {
                activeTouch = t;
                break;
            }
        }

        // Touch ended
        if (activeTouch == null)
        {
            ResetJoystickState();
            input = 0f;
            return true;
        }

        _joystickCurrent = activeTouch.position.ReadValue();

        // (Optional) keep origin drifting toward finger so you don't hit the edge
        if (joystickRecenter > 0f)
            _joystickOrigin = Vector2.Lerp(_joystickOrigin, _joystickCurrent, joystickRecenter * Time.deltaTime);

        float dx = _joystickCurrent.x - _joystickOrigin.x;

        // deadzone in pixels
        if (Mathf.Abs(dx) < joystickDeadZonePx)
        {
            input = 0f;
            return true;
        }

        float raw = Mathf.Clamp(dx / Mathf.Max(joystickRadiusPx, 1f), -1f, 1f);

        // response curve
        float sign = Mathf.Sign(raw);
        float mag = Mathf.Pow(Mathf.Abs(raw), joystickResponseExponent);
        input = sign * mag;

        return true;
    }

    private void ResetJoystickState()
    {
        _joystickActive = false;
        _joystickTouchId = -1;
        _joystickOrigin = Vector2.zero;
        _joystickCurrent = Vector2.zero;
    }

    private bool IsInControlZone(Vector2 screenPos)
    {
        // controlZoneNormalized is in screen fractions (0..1)
        float xMin = controlZoneNormalized.xMin * Screen.width;
        float xMax = controlZoneNormalized.xMax * Screen.width;
        float yMin = controlZoneNormalized.yMin * Screen.height;
        float yMax = controlZoneNormalized.yMax * Screen.height;

        return screenPos.x >= xMin && screenPos.x <= xMax &&
               screenPos.y >= yMin && screenPos.y <= yMax;
    }

    private bool IsTouchOverUI(int touchId)
    {
        // Requires EventSystem in scene to work. If none, return false.
        if (EventSystem.current == null)
            return false;

        return EventSystem.current.IsPointerOverGameObject(touchId);
    }

    // ---- Core movement ----

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

    private float ApplyDeadZone(float value)
    {
        return Mathf.Abs(value) < inputDeadZone ? 0f : value;
    }

    private float GetSmoothedInput(float dt)
    {
        if (inputSmoothingTime <= 0f)
            return ApplyDeadZone(_moveInputTarget);

        float target = ApplyDeadZone(_moveInputTarget);
        _smoothedMoveInput = Mathf.SmoothDamp(_smoothedMoveInput, target, ref _moveInputVelocity, inputSmoothingTime, Mathf.Infinity, dt);
        return _smoothedMoveInput;
    }

    private void ResetSmoothedInput(float value)
    {
        _moveInputVelocity = 0f;
        _smoothedMoveInput = value;
    }

    private void ApplyHandlingUpgrade(int? upgradeOverride = null)
    {
        int level = upgradeOverride ?? (profile != null ? profile.GetUpgradeLevel(UpgradeType.Handling) : 0);
        _handlingAdjustedAngularSpeedDegrees = _baseAngularSpeedDegrees + handlingAngularSpeedBonusPerLevel * Mathf.Max(0, level);

        float smoothingOffset = handlingInputSmoothingReductionPerLevel * Mathf.Max(0, level);
        inputSmoothingTime = Mathf.Max(minInputSmoothingTime, _baseInputSmoothingTime - smoothingOffset);

        ApplySensitivitySettings(SettingsData.TouchSensitivity);
    }

    private void HandleUpgradeLevelChanged(UpgradeType upgradeType, int level)
    {
        if (upgradeType != UpgradeType.Handling)
            return;

        ApplyHandlingUpgrade(level);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Vector3.zero, tubeRadius);

        // Draw control zone overlay (approx) in Scene view only (not game view accurate)
        // Kept minimal—control zone is screen-space, so this is just a reminder.
    }
#endif
}
