using System.Collections;
using UnityEngine;

namespace CoreRacer.UI
{
    public sealed class UiMotion : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform target;
        [SerializeField] private float duration = 0.18f;
        [SerializeField] private Vector3 hiddenScale = new Vector3(0.96f, 0.96f, 1f);
        private Coroutine _routine;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (target == null) target = transform as RectTransform;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            Play(1f, Vector3.one);
        }

        public void Hide()
        {
            Play(0f, hiddenScale, true);
        }

        private void Play(float alpha, Vector3 scale, bool deactivateAtEnd = false)
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(Animate(alpha, scale, deactivateAtEnd));
        }

        private IEnumerator Animate(float targetAlpha, Vector3 targetScale, bool deactivateAtEnd)
        {
            var startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
            var startScale = target != null ? target.localScale : transform.localScale;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += UnityEngine.Time.unscaledDeltaTime;
                var t = duration <= 0f ? 1f : Mathf.SmoothStep(0f, 1f, elapsed / duration);
                if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                if (target != null) target.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            if (canvasGroup != null) canvasGroup.alpha = targetAlpha;
            if (target != null) target.localScale = targetScale;
            if (deactivateAtEnd) gameObject.SetActive(false);
            _routine = null;
        }
    }
}
