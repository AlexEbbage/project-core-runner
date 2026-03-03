using System.Collections;
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

    [Header("Runtime References")]
    [SerializeField] private RunScoreManager scoreManager;
    [SerializeField] private RunCurrencyManager currencyManager;
    [SerializeField] private HudController hudController;

    [Header("VFX")]
    [SerializeField] private GameObject coinPickupVfxPrefab;
    [SerializeField] private GameObject powerupPickupVfxPrefab;
    [SerializeField] private GameObject defaultPickupVfxPrefab;

    [Header("Visual Motion")]
    [SerializeField] private float spinDegreesPerSecond = 90f;
    [SerializeField] private float bobAmplitude = 0.15f;
    [SerializeField] private float bobFrequency = 1f;
    [SerializeField] private float bobZPhaseScale = 0.1f;
    [SerializeField] private float zRotationAmplitude = 25f;
    [SerializeField] private float zRotationFrequency = 0.1f;
    [SerializeField] private float yRotationAmplitude = 25f;
    [SerializeField] private float yRotationFrequency = 0.1f;

    [Header("Collect Animation")]
    [SerializeField] private float collectAnimationDuration = 0.35f;
    [SerializeField] private Vector3 collectMoveOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private AnimationCurve collectMoveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve collectScaleCurve = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(0.25f, 1.18f),
        new Keyframe(1f, 0f));
    [SerializeField] private AnimationCurve collectFadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private Vector3 _baseLocalPosition;
    private Quaternion _baseLocalRotation;
    private float _bobPhaseOffset;
    private Collider _pickupCollider;
    private Transform _playerTransform;
    private float _baseMagnetRadius = 0.5f;
    private bool _isCollected;
    private Renderer[] _pickupRenderers;
    private MaterialPropertyBlock _propertyBlock;

    public static float MagnetRadiusMultiplier { get; private set; } = 1f;

    private void Awake()
    {
        _pickupCollider = GetComponent<Collider>();
        _pickupCollider.isTrigger = true;
        _baseMagnetRadius = GetColliderRadius(_pickupCollider);
        _pickupRenderers = GetComponentsInChildren<Renderer>();
        _propertyBlock = new MaterialPropertyBlock();

        var playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            _playerTransform = playerController.transform;
        }

        if (audioManager == null)
            audioManager = FindFirstObjectByType<AudioManager>();

        if (scoreManager == null)
            scoreManager = FindFirstObjectByType<RunScoreManager>();

        if (currencyManager == null)
            currencyManager = FindFirstObjectByType<RunCurrencyManager>();

        if (hudController == null)
            hudController = FindFirstObjectByType<HudController>();
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
        if (_isCollected)
            return;

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
        if (_pickupCollider != null)
            _pickupCollider.enabled = false;

        var powerupController = playerObject != null ? playerObject.GetComponent<PlayerPowerupController>() : null;

        if (pickupType == PickupType.Coin)
        {
            float pickupScore = scoreManager != null ? scoreManager.OnPickupCollected() : 0f;

            if (currencyManager != null)
            {
                float multiplier = scoreManager != null ? scoreManager.CurrentMultiplier : 1f;
                int baseValue = currencyManager.GetCoinValue();
                int bonusValue = Mathf.Max(1, Mathf.RoundToInt(baseValue * multiplier));
                currencyManager.AddCoins(bonusValue);
            }

            if (hudController == null)
                hudController = FindFirstObjectByType<HudController>();

            hudController?.ShowPickupScorePopup(pickupScore, transform.position);
        }
        else if (pickupType == PickupType.Powerup)
        {
            powerupController?.ActivatePowerup(powerupType);
        }

        audioManager?.PlayPickup();
        SpawnPickupVfx();
        StartCoroutine(PlayCollectAnimationAndDestroy());
    }

    private IEnumerator PlayCollectAnimationAndDestroy()
    {
        Vector3 startPosition = transform.localPosition;
        Quaternion startRotation = transform.localRotation;
        Vector3 startScale = transform.localScale;

        float duration = Mathf.Max(collectAnimationDuration, 0.01f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / duration);

            float moveT = collectMoveCurve.Evaluate(normalizedTime);
            float scaleT = collectScaleCurve.Evaluate(normalizedTime);
            float alpha = Mathf.Clamp01(collectFadeCurve.Evaluate(normalizedTime));

            transform.localPosition = startPosition + collectMoveOffset * moveT;
            transform.localScale = startScale * Mathf.Max(scaleT, 0f);
            transform.localRotation = startRotation;

            ApplyRendererAlpha(alpha);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void ApplyRendererAlpha(float alpha)
    {
        if (_pickupRenderers == null)
            return;

        for (int i = 0; i < _pickupRenderers.Length; i++)
        {
            Renderer currentRenderer = _pickupRenderers[i];
            if (currentRenderer == null)
                continue;

            currentRenderer.GetPropertyBlock(_propertyBlock);

            if (currentRenderer.sharedMaterial != null && currentRenderer.sharedMaterial.HasProperty("_BaseColor"))
            {
                Color baseColor = currentRenderer.sharedMaterial.GetColor("_BaseColor");
                baseColor.a = alpha;
                _propertyBlock.SetColor("_BaseColor", baseColor);
            }

            if (currentRenderer.sharedMaterial != null && currentRenderer.sharedMaterial.HasProperty("_Color"))
            {
                Color color = currentRenderer.sharedMaterial.GetColor("_Color");
                color.a = alpha;
                _propertyBlock.SetColor("_Color", color);
            }

            currentRenderer.SetPropertyBlock(_propertyBlock);
        }
    }

    private void SpawnPickupVfx()
    {
        GameObject vfxPrefab = null;
        switch (pickupType)
        {
            case PickupType.Coin:
                vfxPrefab = coinPickupVfxPrefab;
                break;
            case PickupType.Powerup:
                vfxPrefab = powerupPickupVfxPrefab;
                break;
        }

        if (vfxPrefab == null)
            vfxPrefab = defaultPickupVfxPrefab;

        if (vfxPrefab == null)
            return;

        Instantiate(vfxPrefab, transform.position, transform.rotation);
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
