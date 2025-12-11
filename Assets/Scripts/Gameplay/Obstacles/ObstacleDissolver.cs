using System.Collections;
using UnityEngine;

/// <summary>
/// Simple "dissolve" effect by scaling down and disabling.
/// Attach to your obstacle segment prefab.
/// </summary>
public class ObstacleDissolver : MonoBehaviour
{
    [SerializeField] private float defaultDuration = 0.4f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private Coroutine _routine;
    private Vector3 _originalScale;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    public void Dissolve(float duration = -1f)
    {
        if (duration <= 0f) duration = defaultDuration;
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(DissolveRoutine(duration));
    }

    private IEnumerator DissolveRoutine(float duration)
    {
        float t = 0f;
        var renderer = GetComponent<Renderer>();
        var coll = GetComponent<Collider>();

        if (coll != null) coll.enabled = false;

        while (t < duration)
        {
            float u = t / duration;
            float s = scaleCurve.Evaluate(u);
            transform.localScale = _originalScale * s;

            t += Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // Reset scale when reused by pooling
        transform.localScale = _originalScale;
        var coll = GetComponent<Collider>();
        if (coll != null) coll.enabled = true;
    }
}