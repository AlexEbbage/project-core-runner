using UnityEngine;

namespace CoreRacer.Gameplay.Player
{
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private bool enableKeyboard = true;
        [SerializeField] private bool enableTouch = true;
        [SerializeField] private float touchDeadZone = 20f;

        private Vector2 _touchStart;

        public PlayerInputState Read()
        {
            var state = new PlayerInputState();
            if (enableKeyboard)
            {
#if ENABLE_INPUT_SYSTEM
                state.Horizontal += ReadInputSystemKeyboard();
                state.IsPressing |= Mathf.Abs(state.Horizontal) > 0.01f;
#elif ENABLE_LEGACY_INPUT_MANAGER
                state.Horizontal += Input.GetAxisRaw("Horizontal");
                state.IsPressing |= Mathf.Abs(state.Horizontal) > 0.01f;
#endif
            }

#if ENABLE_LEGACY_INPUT_MANAGER
            if (enableTouch && Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began) _touchStart = touch.position;
                var delta = touch.position - _touchStart;
                if (Mathf.Abs(delta.x) > touchDeadZone)
                {
                    state.Horizontal += Mathf.Sign(delta.x);
                    state.IsPressing = true;
                }
            }
#endif

            state.Horizontal = Mathf.Clamp(state.Horizontal, -1f, 1f);
            return state;
        }

#if ENABLE_INPUT_SYSTEM
        private static float ReadInputSystemKeyboard()
        {
            var keyboardType = System.Type.GetType("UnityEngine.InputSystem.Keyboard, Unity.InputSystem");
            if (keyboardType == null)
                return 0f;

            var currentProperty = keyboardType.GetProperty("current", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var keyboard = currentProperty != null ? currentProperty.GetValue(null, null) : null;
            if (keyboard == null)
                return 0f;

            var horizontal = 0f;
            if (IsPressed(keyboard, "leftArrowKey") || IsPressed(keyboard, "aKey")) horizontal -= 1f;
            if (IsPressed(keyboard, "rightArrowKey") || IsPressed(keyboard, "dKey")) horizontal += 1f;
            return horizontal;
        }

        private static bool IsPressed(object device, string propertyName)
        {
            var property = device.GetType().GetProperty(propertyName);
            var control = property != null ? property.GetValue(device, null) : null;
            if (control == null)
                return false;

            var isPressedProperty = control.GetType().GetProperty("isPressed");
            return isPressedProperty != null && (bool)isPressedProperty.GetValue(control, null);
        }
#endif
    }
}
