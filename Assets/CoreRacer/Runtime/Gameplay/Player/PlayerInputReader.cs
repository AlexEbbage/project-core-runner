using System;
using System.Reflection;
using CoreRacer.Bootstrap;
using CoreRacer.Common;
using CoreRacer.Services.Accessibility;
using UnityEngine;

namespace CoreRacer.Gameplay.Player
{
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [Header("Sources")]
        [SerializeField] private bool enableKeyboard = true;
        [SerializeField] private bool enableTouch = true;

        [Header("Touch")]
        [SerializeField, Min(1f)] private float touchDeadZonePixels = 20f;
        [SerializeField, Range(0.01f, 0.25f)] private float touchDeadZoneScreenFraction = 0.03f;

        private AccessibilitySettingsService _accessibility;
        private Vector2 _touchStart;
        private bool _touchActive;

#if ENABLE_INPUT_SYSTEM
        private static readonly BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
        private static readonly BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;
        private static Type _keyboardType;
        private static Type _touchscreenType;
        private static PropertyInfo _keyboardCurrent;
        private static PropertyInfo _touchscreenCurrent;
        private static bool _inputSystemReflectionCached;
#endif

        private void OnEnable()
        {
            ResolveServices();
            GameServices.RegistryChanged += HandleRegistryChanged;
        }

        private void OnDisable()
        {
            GameServices.RegistryChanged -= HandleRegistryChanged;
            _touchActive = false;
        }

        public PlayerInputState Read()
        {
            if (_accessibility == null)
                ResolveServices();

            var state = new PlayerInputState();
            if (enableKeyboard)
            {
#if ENABLE_INPUT_SYSTEM
                state.Horizontal += ReadInputSystemKeyboard();
#elif ENABLE_LEGACY_INPUT_MANAGER
                state.Horizontal += Input.GetAxisRaw("Horizontal");
#endif
            }

            if (enableTouch)
            {
#if ENABLE_INPUT_SYSTEM
                ReadInputSystemTouch(ref state);
#elif ENABLE_LEGACY_INPUT_MANAGER
                ReadLegacyTouch(ref state);
#endif
            }

            var settings = _accessibility != null ? _accessibility.State : null;
            var sensitivity = settings != null ? Mathf.Clamp(settings.InputSensitivity, 0.1f, 3f) : 1f;
            state.Horizontal = Mathf.Clamp(state.Horizontal * sensitivity, -1f, 1f);
            state.IsPressing |= Mathf.Abs(state.Horizontal) > 0.01f;
            return state;
        }

        private void ResolveServices()
        {
            GameServices.TryGet(out _accessibility);
        }

        private void HandleRegistryChanged(ServiceRegistry _)
        {
            ResolveServices();
        }

        private float EffectiveDeadZone()
        {
            return Mathf.Max(touchDeadZonePixels, Mathf.Max(1f, Screen.width) * touchDeadZoneScreenFraction);
        }

        private void ApplyTouchPosition(Vector2 position, bool pressed, bool began, ref PlayerInputState state)
        {
            if (!pressed)
            {
                _touchActive = false;
                return;
            }

            var deadZone = EffectiveDeadZone();
            var dragControls = _accessibility != null && _accessibility.State != null && _accessibility.State.DragControlsEnabled;
            if (!dragControls)
            {
                state.Horizontal += TouchSteeringInterpreter.EvaluateScreenSide(position.x, Screen.width, deadZone);
                state.IsPressing |= Mathf.Abs(state.Horizontal) > 0.01f;
                _touchStart = position;
                _touchActive = true;
                return;
            }

            if (began || !_touchActive)
            {
                _touchStart = position;
                _touchActive = true;
            }

            state.Horizontal += TouchSteeringInterpreter.EvaluateDrag(position.x, _touchStart.x, deadZone);
            state.IsPressing |= Mathf.Abs(state.Horizontal) > 0.01f;
        }

#if ENABLE_LEGACY_INPUT_MANAGER
        private void ReadLegacyTouch(ref PlayerInputState state)
        {
            if (Input.touchCount <= 0)
            {
                _touchActive = false;
                return;
            }

            var touch = Input.GetTouch(0);
            var pressed = touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled;
            ApplyTouchPosition(touch.position, pressed, touch.phase == TouchPhase.Began, ref state);
        }
#endif

#if ENABLE_INPUT_SYSTEM
        private static void CacheInputSystemReflection()
        {
            if (_inputSystemReflectionCached)
                return;

            _inputSystemReflectionCached = true;
            _keyboardType = Type.GetType("UnityEngine.InputSystem.Keyboard, Unity.InputSystem");
            _touchscreenType = Type.GetType("UnityEngine.InputSystem.Touchscreen, Unity.InputSystem");
            _keyboardCurrent = _keyboardType?.GetProperty("current", PublicStatic);
            _touchscreenCurrent = _touchscreenType?.GetProperty("current", PublicStatic);
        }

        private static float ReadInputSystemKeyboard()
        {
            CacheInputSystemReflection();
            var keyboard = _keyboardCurrent?.GetValue(null, null);
            if (keyboard == null)
                return 0f;

            var horizontal = 0f;
            if (IsPressed(GetPropertyValue(keyboard, "leftArrowKey")) || IsPressed(GetPropertyValue(keyboard, "aKey"))) horizontal -= 1f;
            if (IsPressed(GetPropertyValue(keyboard, "rightArrowKey")) || IsPressed(GetPropertyValue(keyboard, "dKey"))) horizontal += 1f;
            return horizontal;
        }

        private void ReadInputSystemTouch(ref PlayerInputState state)
        {
            CacheInputSystemReflection();
            var touchscreen = _touchscreenCurrent?.GetValue(null, null);
            if (touchscreen == null)
            {
                _touchActive = false;
                return;
            }

            var primaryTouch = GetPropertyValue(touchscreen, "primaryTouch");
            if (primaryTouch == null)
                return;

            var pressControl = GetPropertyValue(primaryTouch, "press");
            var positionControl = GetPropertyValue(primaryTouch, "position");
            var pressed = IsPressed(pressControl);
            var position = ReadVector2(positionControl);
            ApplyTouchPosition(position, pressed, pressed && !_touchActive, ref state);
        }

        private static object GetPropertyValue(object target, string propertyName)
        {
            return target?.GetType().GetProperty(propertyName, PublicInstance)?.GetValue(target, null);
        }

        private static bool IsPressed(object control)
        {
            var property = control?.GetType().GetProperty("isPressed", PublicInstance);
            return property != null && property.GetValue(control, null) is bool value && value;
        }

        private static Vector2 ReadVector2(object control)
        {
            if (control == null)
                return Vector2.zero;

            var method = control.GetType().GetMethod("ReadValue", PublicInstance, null, Type.EmptyTypes, null);
            if (method == null)
                return Vector2.zero;

            var value = method.Invoke(control, null);
            return value is Vector2 vector ? vector : Vector2.zero;
        }
#endif
    }
}
