using UnityEngine;

namespace CoreRacer.Gameplay.Obstacles
{
    public sealed class DoorObstacle : MonoBehaviour
    {
        [SerializeField] private Transform doorLeft;
        [SerializeField] private Transform doorRight;
        [SerializeField] private float openDistance = 0.9f;
        [SerializeField] private float openSpeed = 4f;
        [SerializeField] private bool startsOpen;
        [SerializeField] private bool cycleAutomatically = true;
        [SerializeField] private float cycleSeconds = 3f;
        private float _target;
        private float _cycleElapsed;
        private Vector3 _leftClosed;
        private Vector3 _rightClosed;

        private void Awake()
        {
            if (doorLeft != null) _leftClosed = doorLeft.localPosition;
            if (doorRight != null) _rightClosed = doorRight.localPosition;
            _target = startsOpen ? 1f : 0f;
        }

        private void OnEnable()
        {
            _cycleElapsed = startsOpen ? Mathf.Max(0.1f, cycleSeconds) * 0.5f : 0f;
            _target = startsOpen ? 1f : 0f;
            ApplyAmount(_target);
        }

        public void SetOpen(bool open)
        {
            _target = open ? 1f : 0f;
        }

        private void Update()
        {
            if (cycleAutomatically)
            {
                _cycleElapsed += UnityEngine.Time.deltaTime;
                var duration = Mathf.Max(0.5f, cycleSeconds);
                _target = Mathf.PingPong(_cycleElapsed / duration, 1f) >= 0.5f ? 1f : 0f;
            }

            var current = doorLeft != null ? Mathf.InverseLerp(_leftClosed.x, _leftClosed.x - openDistance, doorLeft.localPosition.x) : _target;
            current = Mathf.MoveTowards(current, _target, openSpeed * UnityEngine.Time.deltaTime);
            ApplyAmount(current);
        }

        private void ApplyAmount(float amount)
        {
            if (doorLeft != null) doorLeft.localPosition = _leftClosed + Vector3.left * (openDistance * amount);
            if (doorRight != null) doorRight.localPosition = _rightClosed + Vector3.right * (openDistance * amount);
        }
    }
}
