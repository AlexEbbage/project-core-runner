using System.Collections;
using UnityEngine;

namespace CoreRacer.Gameplay.Camera
{
    public sealed class CameraShakeController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float defaultDuration = 0.18f;
        [SerializeField] private float defaultMagnitude = 0.16f;
        private Coroutine _shakeRoutine;
        private Vector3 _baseLocalPosition;

        private void Awake()
        {
            if (target == null) target = transform;
            _baseLocalPosition = target.localPosition;
        }

        public void Shake() => Shake(defaultDuration, defaultMagnitude);

        public void Shake(float duration, float magnitude)
        {
            if (!isActiveAndEnabled || target == null) return;
            if (_shakeRoutine != null) StopCoroutine(_shakeRoutine);
            _shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
        }

        private IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                var t = 1f - elapsed / duration;
                target.localPosition = _baseLocalPosition + Random.insideUnitSphere * (magnitude * t);
                elapsed += UnityEngine.Time.deltaTime;
                yield return null;
            }
            target.localPosition = _baseLocalPosition;
            _shakeRoutine = null;
        }
    }
}
