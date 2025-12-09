using UnityEngine;

/// <summary>
/// Smoothly adjusts camera FOV based on current forward speed.
/// You call SetSpeed(newSpeed) from RunSpeedController.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFovController : MonoBehaviour
{
    [Header("Speed Range")]
    [SerializeField] private float minSpeed = 32f;
    [SerializeField] private float maxSpeed = 80f;

    [Header("FOV Range")]
    [SerializeField] private float minFov = 60f;
    [SerializeField] private float maxFov = 78f;

    [Header("Smoothing")]
    [SerializeField] private float lerpSpeed = 5f;

    private Camera _cam;
    private float _currentSpeed;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    public void SetSpeed(float speed)
    {
        _currentSpeed = speed;
    }

    private void LateUpdate()
    {
        if (_cam == null) return;

        // Normalise speed 0..1
        float t = Mathf.InverseLerp(minSpeed, maxSpeed, _currentSpeed);
        float targetFov = Mathf.Lerp(minFov, maxFov, t);

        _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, targetFov, lerpSpeed * Time.deltaTime);
    }
}
