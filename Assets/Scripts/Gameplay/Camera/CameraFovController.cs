using UnityEngine;

/// <summary>
/// Smoothly adjusts camera FOV based on the current forward speed.
/// You call SetSpeed(newSpeed) from RunSpeedController.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFovController : MonoBehaviour
{
    [Header("Speed Range")]
    [SerializeField] private float minSpeed = 32f;
    [SerializeField] private float maxSpeed = 80f;

    [Header("FOV")]
    [SerializeField] private float minFov = 60f;
    [SerializeField] private float maxFov = 78f;
    [Tooltip("How many FOV degrees we add per speed unit above minSpeed.")]
    [SerializeField] private float fovIncreasePerSpeed = 0.35f;

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
        _currentSpeed = Mathf.Max(0f, speed);
    }

    private void LateUpdate()
    {
        if (_cam == null)
            return;

        float safeMaxFov = Mathf.Max(minFov, maxFov);
        float safeMaxSpeed = Mathf.Max(minSpeed, maxSpeed);

        float speedAboveMin = Mathf.Max(0f, _currentSpeed - minSpeed);
        float speedBasedFov = minFov + (speedAboveMin * fovIncreasePerSpeed);

        float normalizedSpeedT = Mathf.InverseLerp(minSpeed, safeMaxSpeed, _currentSpeed);
        float rangeBasedCapFov = Mathf.Lerp(minFov, safeMaxFov, normalizedSpeedT);

        float targetFov = Mathf.Clamp(speedBasedFov, minFov, Mathf.Min(safeMaxFov, Mathf.Max(minFov, rangeBasedCapFov)));
        _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, targetFov, Mathf.Max(0f, lerpSpeed) * Time.deltaTime);
    }
}
