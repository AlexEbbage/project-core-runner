using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ObstacleRingVisuals : MonoBehaviour
{
    [Header("Dissolve Property")]
    [Tooltip("Name of the float property in the dissolve shader (e.g. _Dissolve, _DissolveAmount).")]
    [SerializeField] private string dissolvePropertyName = "_Dissolve";

    [Tooltip("Value when fully visible.")]
    [SerializeField] private float visibleValue = 0f;

    [Tooltip("Value when fully dissolved/hidden.")]
    [SerializeField] private float hiddenValue = 1f;

    [Header("Targets")]
    [Tooltip("Renderers that use the dissolve shader. If empty, all child renderers are used.")]
    [SerializeField] private List<Renderer> targetRenderers = new List<Renderer>();

    [Tooltip("Colliders to enable/disable as we fade. If empty, all child colliders are used.")]
    [SerializeField] private List<Collider> targetColliders = new List<Collider>();

    [Tooltip("If true, colliders are disabled during fade-out and enabled at the end of fade-in.")]
    [SerializeField] private bool controlColliders = true;

    private MaterialPropertyBlock _propertyBlock;
    private Coroutine _activeRoutine;

    private void Awake()
    {
        CacheTargets();
        _propertyBlock = new MaterialPropertyBlock();
    }

    private void CacheTargets()
    {
        if (targetRenderers == null || targetRenderers.Count == 0)
        {
            targetRenderers = new List<Renderer>(GetComponentsInChildren<Renderer>(true));
        }

        if (targetColliders == null || targetColliders.Count == 0)
        {
            targetColliders = new List<Collider>(GetComponentsInChildren<Collider>(true));
        }
    }

    private void SetCollidersEnabled(bool enabled)
    {
        if (!controlColliders || targetColliders == null)
            return;

        for (int i = 0; i < targetColliders.Count; i++)
        {
            if (targetColliders[i] == null) continue;
            targetColliders[i].enabled = enabled;
        }
    }

    private void SetDissolveValue(float value)
    {
        if (string.IsNullOrEmpty(dissolvePropertyName) || targetRenderers == null)
            return;

        for (int i = 0; i < targetRenderers.Count; i++)
        {
            var r = targetRenderers[i];
            if (r == null) continue;

            r.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(dissolvePropertyName, value);
            r.SetPropertyBlock(_propertyBlock);
        }
    }

    public void SetHiddenImmediate()
    {
        if (_activeRoutine != null)
        {
            StopCoroutine(_activeRoutine);
            _activeRoutine = null;
        }

        SetDissolveValue(hiddenValue);
        SetCollidersEnabled(false);
    }

    public void SetVisibleImmediate()
    {
        if (_activeRoutine != null)
        {
            StopCoroutine(_activeRoutine);
            _activeRoutine = null;
        }

        SetDissolveValue(visibleValue);
        SetCollidersEnabled(true);
    }

    /// <summary>
    /// Fade from hidden -> visible.
    /// Colliders are enabled at the end.
    /// </summary>
    public void PlayFadeIn(float duration)
    {
        if (_activeRoutine != null)
            StopCoroutine(_activeRoutine);

        _activeRoutine = StartCoroutine(FadeRoutine(hiddenValue, visibleValue, duration, enableCollidersAtEnd: true, disableObjectAtEnd: false));
    }

    /// <summary>
    /// Fade from visible -> hidden.
    /// Colliders are disabled at the start; optionally disable GameObject at the end.
    /// </summary>
    public void PlayFadeOut(float duration, bool disableObjectAtEnd)
    {
        if (_activeRoutine != null)
            StopCoroutine(_activeRoutine);

        _activeRoutine = StartCoroutine(FadeRoutine(visibleValue, hiddenValue, duration, enableCollidersAtEnd: false, disableObjectAtEnd: disableObjectAtEnd));
    }

    private IEnumerator FadeRoutine(float from, float to, float duration, bool enableCollidersAtEnd, bool disableObjectAtEnd)
    {
        // For fade-out, make them non-lethal immediately.
        if (!enableCollidersAtEnd)
        {
            SetCollidersEnabled(false);
        }

        if (duration <= 0f)
        {
            SetDissolveValue(to);

            if (enableCollidersAtEnd)
                SetCollidersEnabled(true);

            if (disableObjectAtEnd)
                gameObject.SetActive(false);

            _activeRoutine = null;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / duration);
            float value = Mathf.Lerp(from, to, normalized);
            SetDissolveValue(value);
            yield return null;
        }

        SetDissolveValue(to);

        if (enableCollidersAtEnd)
            SetCollidersEnabled(true);

        if (disableObjectAtEnd)
            gameObject.SetActive(false);

        _activeRoutine = null;
    }
}
