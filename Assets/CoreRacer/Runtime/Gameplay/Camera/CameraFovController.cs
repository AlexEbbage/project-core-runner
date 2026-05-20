using UnityEngine;

namespace CoreRacer.Gameplay.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public sealed class CameraFovController : MonoBehaviour
    {
        [SerializeField] private float baseFov = 68f;
        [SerializeField] private float maxBonusFov = 14f;
        [SerializeField] private float smoothing = 8f;
        private UnityEngine.Camera _camera;
        private float _targetIntensity;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            _camera.fieldOfView = baseFov;
        }

        public void SetSpeedIntensity(float normalizedIntensity)
        {
            _targetIntensity = Mathf.Clamp01(normalizedIntensity);
        }

        private void LateUpdate()
        {
            var target = baseFov + maxBonusFov * _targetIntensity;
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, target, 1f - Mathf.Exp(-smoothing * UnityEngine.Time.deltaTime));
        }
    }
}
