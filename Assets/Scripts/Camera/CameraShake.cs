using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float defaultStrength = 0.2f;
    [SerializeField] private float defaultDuration = 0.1f;

    private Vector3 _originalLocalPos;
    private bool _initialized;

    private void Awake()
    {
        _originalLocalPos = transform.localPosition;
        _initialized = true;

        // Register this as the active shake target
        ScreenShakeHelper.RegisterCameraShake(this);
    }

    private void OnDestroy()
    {
        if (ScreenShakeHelper.ActiveCameraShake == this)
        {
            ScreenShakeHelper.RegisterCameraShake(null);
        }
    }

    public void PlayShake()
    {
        //Debug.Log("PlayShake()");
        //PlayShake(defaultStrength, defaultDuration);
    }

    public void PlayShake(float strength, float duration)
    {
        if (!_initialized) return;
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(strength, duration));
    }

    private System.Collections.IEnumerator ShakeRoutine(float strength, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float damper = 1f - (t / duration);

            float x = Random.Range(-1f, 1f) * strength * damper;
            float y = Random.Range(-1f, 1f) * strength * damper;

            transform.localPosition = _originalLocalPos + new Vector3(x, y, 0f);
            yield return null;
        }

        transform.localPosition = _originalLocalPos;
    }
}
