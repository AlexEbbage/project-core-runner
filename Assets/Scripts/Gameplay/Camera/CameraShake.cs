using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float defaultStrength = 0.2f;
    [SerializeField] private float defaultDuration = 0.1f;
    [SerializeField] private float defaultFrequency = 25f;
    [SerializeField] private AnimationCurve falloffCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    [SerializeField] private bool useUnscaledTime = true;

    private Vector3 _originalLocalPos;
    private bool _initialized;
    private Coroutine _shakeRoutine;
    private float _shakeStrength;
    private float _shakeDuration;
    private float _shakeFrequency;
    private float _shakeEndTime;
    private float _noiseSeedX;
    private float _noiseSeedY;

    private void Awake()
    {
        Initialize();

        // Register this as the active shake target
        ScreenShakeHelper.RegisterCameraShake(this);
    }

    private void OnEnable()
    {
        Initialize();
    }

    private void OnDestroy()
    {
        if (ScreenShakeHelper.ActiveCameraShake == this)
        {
            ScreenShakeHelper.RegisterCameraShake(null);
        }
    }

    private void OnDisable()
    {
        if (_shakeRoutine != null)
        {
            StopCoroutine(_shakeRoutine);
            _shakeRoutine = null;
        }

        transform.localPosition = _originalLocalPos;
        _shakeStrength = 0f;
        _shakeDuration = 0f;
        _shakeFrequency = 0f;
        _shakeEndTime = 0f;
    }

    public void PlayShake()
    {
        PlayShake(defaultStrength, defaultDuration, defaultFrequency);
    }

    public void PlayShake(float strength, float duration)
    {
        PlayShake(strength, duration, defaultFrequency);
    }

    public void PlayShake(float strength, float duration, float frequency)
    {
        if (!_initialized) return;
        if (strength <= 0f || duration <= 0f) return;

        if (frequency <= 0f)
        {
            frequency = defaultFrequency;
        }

        _shakeStrength = Mathf.Max(_shakeStrength, strength);
        _shakeDuration = Mathf.Max(_shakeDuration, duration);
        _shakeFrequency = Mathf.Max(_shakeFrequency, frequency);
        _shakeEndTime = Mathf.Max(_shakeEndTime, CurrentTime + duration);
        _noiseSeedX = Random.value * 100f;
        _noiseSeedY = Random.value * 100f;

        if (_shakeRoutine == null)
        {
            _shakeRoutine = StartCoroutine(ShakeRoutine());
        }
    }

    private System.Collections.IEnumerator ShakeRoutine()
    {
        while (true)
        {
            float remaining = Mathf.Max(0f, _shakeEndTime - CurrentTime);
            float normalizedTime = 1f - Mathf.Clamp01(remaining / _shakeDuration);
            float damper = falloffCurve.Evaluate(normalizedTime);
            float noiseTime = CurrentTime * _shakeFrequency;

            float x = (Mathf.PerlinNoise(_noiseSeedX, noiseTime) - 0.5f) * 2f;
            float y = (Mathf.PerlinNoise(_noiseSeedY, noiseTime) - 0.5f) * 2f;

            transform.localPosition = _originalLocalPos + new Vector3(x, y, 0f) * _shakeStrength * damper;

            if (remaining <= 0f)
            {
                break;
            }
            yield return null;
        }

        transform.localPosition = _originalLocalPos;
        _shakeRoutine = null;
        _shakeStrength = 0f;
        _shakeDuration = 0f;
        _shakeFrequency = 0f;
        _shakeEndTime = 0f;
    }

    private void Initialize()
    {
        _originalLocalPos = transform.localPosition;
        _initialized = true;
    }

    private float CurrentTime => useUnscaledTime ? Time.unscaledTime : Time.time;
}
