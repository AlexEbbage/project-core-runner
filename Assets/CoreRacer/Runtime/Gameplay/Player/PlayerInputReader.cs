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
                state.Horizontal += Input.GetAxisRaw("Horizontal");
                state.IsPressing |= Mathf.Abs(state.Horizontal) > 0.01f;
            }

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

            state.Horizontal = Mathf.Clamp(state.Horizontal, -1f, 1f);
            return state;
        }
    }
}
