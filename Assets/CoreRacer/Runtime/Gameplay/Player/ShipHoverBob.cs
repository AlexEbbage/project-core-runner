using UnityEngine;

namespace CoreRacer.Gameplay.Player
{
    public sealed class ShipHoverBob : MonoBehaviour
    {
        [SerializeField] private float amplitude = 0.08f;
        [SerializeField] private float frequency = 2.5f;
        [SerializeField] private float rollAmplitude = 2f;
        private Vector3 _baseLocalPosition;
        private Quaternion _baseLocalRotation;

        private void Awake()
        {
            _baseLocalPosition = transform.localPosition;
            _baseLocalRotation = transform.localRotation;
        }

        private void Update()
        {
            var wave = Mathf.Sin(UnityEngine.Time.time * frequency);
            transform.localPosition = _baseLocalPosition + Vector3.up * (wave * amplitude);
            transform.localRotation = _baseLocalRotation * Quaternion.Euler(0f, 0f, wave * rollAmplitude);
        }
    }
}
