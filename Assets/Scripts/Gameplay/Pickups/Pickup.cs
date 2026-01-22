using UnityEngine;

/// <summary>
/// Collectible pickup that grants currency or powerups.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Pickup : MonoBehaviour
{
    [SerializeField] private PickupType pickupType = PickupType.Coin;
    [SerializeField] private PowerupType powerupType = PowerupType.CoinMultiplier;

    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;

    [Header("Visual Motion")]
    [SerializeField] private float spinDegreesPerSecond = 90f;
    [SerializeField] private float bobAmplitude = 0.15f;
    [SerializeField] private float bobFrequency = 1f;
    [SerializeField] private float bobZPhaseScale = 0.1f;
    [SerializeField] private float zRotationAmplitude = 25f;
    [SerializeField] private float zRotationFrequency = 0.1f;
    [SerializeField] private float yRotationAmplitude = 25f;
    [SerializeField] private float yRotationFrequency = 0.1f;

    private Vector3 _baseLocalPosition;
    private Quaternion _baseLocalRotation;
    private float _bobPhaseOffset;
    private Collider _pickupCollider;
    private Transform _playerTransform;
    private float _baseMagnetRadius = 0.5f;
    private bool _isCollected;

    public static float MagnetRadiusMultiplier { get; private set; } = 1f;

    private void Awake()
    {
        _pickupCollider = GetComponent<Collider>();
        _pickupCollider.isTrigger = true;
        _baseMagnetRadius = GetColliderRadius(_pickupCollider);

        var playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            _playerTransform = playerController.transform;
        }

        if (audioManager == null)
            audioManager = FindFirstObjectByType<AudioManager>();
    }

    private void Start()
    {
        _baseLocalPosition = transform.localPosition;
        _baseLocalRotation = transform.localRotation;

        float zPhase = transform.position.z * zRotationFrequency;
        float zRotationOffset = Mathf.Sin(zPhase) * zRotationAmplitude;

        float yPhase = transform.position.y * yRotationFrequency;
        float yRotationOffset = Mathf.Sin(yPhase) * yRotationAmplitude;

        _baseLocalRotation = Quaternion.AngleAxis(zRotationOffset, Vector3.forward) * _baseLocalRotation;
        _baseLocalRotation = Quaternion.AngleAxis(yRotationOffset, Vector3.up) * _baseLocalRotation;

        transform.localRotation = _baseLocalRotation;

        _bobPhaseOffset = transform.position.z * bobZPhaseScale;
    }

    private void Update()
    {
        float bobOffset = Mathf.Sin(Time.time * bobFrequency + _bobPhaseOffset) * bobAmplitude;
        transform.localPosition = _baseLocalPosition + Vector3.up * bobOffset;

        if (Mathf.Abs(spinDegreesPerSecond) > 0.01f)
        {
            transform.Rotate(Vector3.up, spinDegreesPerSecond * Time.deltaTime, Space.Self);
        }

        if (!_isCollected && MagnetRadiusMultiplier > 1f)
        {
            TryMagnetCollect();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Collect(other.gameObject);
    }

    public void Configure(PickupType newType, PowerupType newPowerupType)
    {
        pickupType = newType;
        powerupType = newPowerupType;
    }

    public static void SetMagnetRadiusMultiplier(float multiplier)
    {
        MagnetRadiusMultiplier = Mathf.Max(1f, multiplier);
    }

    private void TryMagnetCollect()
    {
        if (_playerTransform == null)
        {
            var playerController = FindFirstObjectByType<PlayerController>();
            if (playerController != null)
                _playerTransform = playerController.transform;
        }

        if (_playerTransform == null)
            return;

        float magnetRadius = _baseMagnetRadius * MagnetRadiusMultiplier;
        float sqrDistance = (transform.position - _playerTransform.position).sqrMagnitude;
        if (sqrDistance > magnetRadius * magnetRadius)
            return;

        Collect(_playerTransform.gameObject);
    }

    private void Collect(GameObject playerObject)
    {
        if (_isCollected)
            return;

        _isCollected = true;

        var scoreManager = FindFirstObjectByType<RunScoreManager>();
        var currencyManager = FindFirstObjectByType<RunCurrencyManager>();
        var powerupController = playerObject != null ? playerObject.GetComponent<PlayerPowerupController>() : null;

        if (pickupType == PickupType.Coin)
        {
            scoreManager?.OnPickupCollected();
            if (currencyManager != null)
            {
                float multiplier = scoreManager != null ? scoreManager.CurrentMultiplier : 1f;
                int baseValue = currencyManager.GetCoinValue();
                int bonusValue = Mathf.Max(1, Mathf.RoundToInt(baseValue * multiplier));
                currencyManager.AddCoins(bonusValue);
            }
        }
        else if (pickupType == PickupType.Powerup)
        {
            powerupController?.ActivatePowerup(powerupType);
        }

        audioManager?.PlayPickup();
        Destroy(gameObject);
    }

    private static float GetColliderRadius(Collider collider)
    {
        if (collider == null)
            return 0.5f;

        Vector3 extents = collider.bounds.extents;
        float radius = Mathf.Max(extents.x, extents.y, extents.z);
        return Mathf.Max(radius, 0.1f);
    }
}
