using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class UIButtonClickEffect : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RectTransform target;
    [SerializeField] private float scaleUp = 1.1f;
    [SerializeField] private float duration = 0.08f;

    private Vector3 _originalScale;
    private bool _initialized;

    private void Awake()
    {
        if (target == null)
            target = GetComponent<RectTransform>();

        if (target != null)
        {
            _originalScale = target.localScale;
            _initialized = true;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_initialized || target == null) return;
        StopAllCoroutines();
        StartCoroutine(ClickRoutine());
    }

    private System.Collections.IEnumerator ClickRoutine()
    {
        float half = duration * 0.5f;
        float t = 0f;

        // Scale up
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / half;
            target.localScale = Vector3.Lerp(_originalScale, _originalScale * scaleUp, lerp);
            yield return null;
        }

        // Scale back down
        t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / half;
            target.localScale = Vector3.Lerp(_originalScale * scaleUp, _originalScale, lerp);
            yield return null;
        }

        target.localScale = _originalScale;
    }
}
