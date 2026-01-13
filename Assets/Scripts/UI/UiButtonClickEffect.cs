using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonClickEffect : MonoBehaviour, IPointerClickHandler
{
    [Header("Scale")]
    [SerializeField] private RectTransform target;
    [SerializeField] private float scaleUp = 1.1f;
    [SerializeField] private float duration = 0.08f;

    [Header("Color")]
    [SerializeField] private Graphic targetGraphic;
    [SerializeField] private bool usePressedColor = true;
    [SerializeField] private Color pressedColor = new(0.9f, 0.9f, 0.9f, 1f);
    [SerializeField] private float colorReturnDuration = 0.08f;

    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private AudioClip clickSfxOverride;

    [Header("VFX")]
    [SerializeField] private GameObject clickVfxPrefab;
    [SerializeField] private Transform vfxParent;
    [SerializeField] private Vector3 vfxOffset;
    [SerializeField] private float vfxDestroyDelay = 2f;
    [SerializeField] private bool spawnVfxInWorldSpace;

    private Vector3 _originalScale;
    private Color _originalColor;
    private bool _initialized;
    private Coroutine _clickRoutine;
    private Coroutine _colorReturnRoutine;

    private void Awake()
    {
        if (target == null)
            target = GetComponent<RectTransform>();

        if (targetGraphic == null)
        {
            var button = GetComponent<Button>();
            targetGraphic = button != null ? button.targetGraphic : GetComponent<Graphic>();
        }

        if (audioManager == null)
            audioManager = FindFirstObjectByType<AudioManager>();

        if (target != null)
        {
            _originalScale = target.localScale;
            _initialized = true;
        }

        if (targetGraphic != null)
            _originalColor = targetGraphic.color;
    }

    private void OnDisable()
    {
        if (target != null)
            target.localScale = _originalScale;
        if (targetGraphic != null)
            targetGraphic.color = _originalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_initialized || target == null) return;
        if (_clickRoutine != null)
            StopCoroutine(_clickRoutine);
        _clickRoutine = StartCoroutine(ClickRoutine());
        PlayClickSfx();
        SpawnClickVfx();
    }

    private System.Collections.IEnumerator ClickRoutine()
    {
        float totalDuration = Mathf.Max(0.01f, duration);
        float half = totalDuration * 0.5f;
        float t = 0f;

        // Scale up
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / half;
            target.localScale = Vector3.Lerp(_originalScale, _originalScale * scaleUp, lerp);
            if (usePressedColor)
                LerpColor(lerp);
            yield return null;
        }

        // Scale back down
        t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / half;
            target.localScale = Vector3.Lerp(_originalScale * scaleUp, _originalScale, lerp);
            if (usePressedColor)
                LerpColor(1f - lerp);
            yield return null;
        }

        target.localScale = _originalScale;
        ResetColor();
        _clickRoutine = null;
    }

    private void LerpColor(float t)
    {
        if (targetGraphic == null) return;
        targetGraphic.color = Color.Lerp(_originalColor, pressedColor, t);
    }

    private void ResetColor()
    {
        if (targetGraphic == null) return;
        if (colorReturnDuration <= 0f)
        {
            targetGraphic.color = _originalColor;
            return;
        }

        if (_colorReturnRoutine != null)
            StopCoroutine(_colorReturnRoutine);
        _colorReturnRoutine = StartCoroutine(ColorReturnRoutine());
    }

    private System.Collections.IEnumerator ColorReturnRoutine()
    {
        float t = 0f;
        Color start = targetGraphic.color;
        float returnDuration = Mathf.Max(0.01f, colorReturnDuration);
        while (t < returnDuration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / returnDuration;
            targetGraphic.color = Color.Lerp(start, _originalColor, lerp);
            yield return null;
        }

        targetGraphic.color = _originalColor;
        _colorReturnRoutine = null;
    }

    private void PlayClickSfx()
    {
        if (audioManager == null) return;
        if (clickSfxOverride != null)
        {
            audioManager.PlaySfx(clickSfxOverride);
            return;
        }

        audioManager.PlayButtonClick();
    }

    private void SpawnClickVfx()
    {
        if (clickVfxPrefab == null) return;

        Vector3 spawnPosition = (target != null ? target.position : transform.position) + vfxOffset;
        if (spawnVfxInWorldSpace)
        {
            var instance = Instantiate(clickVfxPrefab, spawnPosition, Quaternion.identity);
            Destroy(instance, vfxDestroyDelay);
            return;
        }

        Transform parent = vfxParent != null ? vfxParent : (target != null ? target : transform);
        var vfxInstance = Instantiate(clickVfxPrefab, parent);
        vfxInstance.transform.position = spawnPosition;
        if (vfxInstance.TryGetComponent<RectTransform>(out var rectTransform))
        {
            rectTransform.localScale = Vector3.one;
        }

        Destroy(vfxInstance, vfxDestroyDelay);
    }
}
