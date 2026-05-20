using UnityEngine;
using UnityEngine.EventSystems;

namespace CoreRacer.UI
{
    public sealed class UiButtonClickEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private float pressedScale = 0.94f;
        [SerializeField] private float returnSharpness = 20f;
        private Vector3 _baseScale = Vector3.one;
        private bool _pressed;

        private void Awake()
        {
            if (target == null) target = transform as RectTransform;
            if (target != null) _baseScale = target.localScale;
        }

        private void Update()
        {
            if (target == null) return;
            var desired = _pressed ? _baseScale * pressedScale : _baseScale;
            var t = 1f - Mathf.Exp(-returnSharpness * UnityEngine.Time.unscaledDeltaTime);
            target.localScale = Vector3.Lerp(target.localScale, desired, t);
        }

        public void OnPointerDown(PointerEventData eventData) => _pressed = true;
        public void OnPointerUp(PointerEventData eventData) => _pressed = false;
        public void OnPointerExit(PointerEventData eventData) => _pressed = false;
    }
}
