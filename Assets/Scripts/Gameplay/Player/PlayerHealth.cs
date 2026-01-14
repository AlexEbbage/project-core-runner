using System;
using UnityEngine;

/// <summary>
/// Handles player health, side scrape damage, and instant death on head-on collisions.
/// Also spawns hit and death VFX and plays audio via AudioManager.
/// Attach this to the same GameObject as PlayerController.
/// </summary>
[DisallowMultipleComponent]
public class PlayerHealth : MonoBehaviour
{
    [Header("Config (optional)")]
    [SerializeField] private GameBalanceConfig balanceConfig;

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float sideScrapeDamage = 25f;
    [SerializeField] private float sideScrapeCooldown = 0.2f;

    [Tooltip("If true, head-on collisions kill instantly regardless of current health.")]
    [SerializeField] private bool headOnIsInstantDeath = true;

    [Header("VFX")]
    [SerializeField] private GameObject sideScrapeVfxPrefab;
    [SerializeField] private GameObject deathImpactVfxPrefab;
    [SerializeField] private GameObject glancingSmokeVfxPrefab;
    [SerializeField] private GameObject glancingSparksVfxPrefab;
    [SerializeField] private float glancingEffectDuration = 3f;
    [SerializeField] private VfxManager vfxManager;

    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;

    [Header("Debug")]
    [SerializeField] private bool logEvents = false;

    public event Action OnDeath;
    public event Action<string> OnDeathWithSource;
    public event Action<string> OnGlancingHit;
    public event Action<float, float> OnHealthChanged;
    public event Action OnShieldBroken;

    private float _currentHealth;
    private bool _isDead;
    private float _lastScrapeTime = -999f;
    private bool _shieldActive;
    private bool _glancingActive;
    private Coroutine _glancingRoutine;
    private GameObject _glancingSmokeInstance;
    private GameObject _glancingSparksInstance;

    private PlayerController _playerController;

    private void Awake()
    {
        if (balanceConfig != null)
        {
            maxHealth = balanceConfig.maxHealth;
            sideScrapeDamage = balanceConfig.sideScrapeDamage;
            sideScrapeCooldown = balanceConfig.sideScrapeCooldown;
        }

        _playerController = GetComponent<PlayerController>();
        _currentHealth = maxHealth;

        if (audioManager == null)
        {
            audioManager = FindFirstObjectByType<AudioManager>();
        }

        if (vfxManager == null)
        {
            vfxManager = VfxManager.Instance;
        }

        RaiseHealthChanged();
    }

    public bool IsDead => _isDead;
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => maxHealth;

    public void HandleSideScrapeHit(Vector3 hitPoint, Vector3 hitNormal, string obstacleType = null)
    {
        if (_isDead)
            return;

        if (_shieldActive)
        {
            _shieldActive = false;
            OnShieldBroken?.Invoke();
            return;
        }

        if (_glancingActive)
        {
            Kill(obstacleType);
            return;
        }

        if (Time.time < _lastScrapeTime + sideScrapeCooldown)
            return;

        _lastScrapeTime = Time.time;

        if (logEvents)
        {
            Debug.Log($"PlayerHealth: Side scrape hit, applying {sideScrapeDamage} damage.");
        }

        SpawnVfx(sideScrapeVfxPrefab, hitPoint, hitNormal);
        audioManager?.PlayHit();

        StartGlancingState();
        OnGlancingHit?.Invoke(obstacleType);
        ApplyDamage(sideScrapeDamage);
    }

    public void HandleHeadOnHit(Vector3 hitPoint, Vector3 hitNormal, string obstacleType = null)
    {
        if (_isDead)
            return;

        if (_shieldActive)
        {
            _shieldActive = false;
            OnShieldBroken?.Invoke();
            return;
        }

        if (logEvents)
        {
            Debug.Log("PlayerHealth: Head-on collision.");
        }

        SpawnVfx(deathImpactVfxPrefab, hitPoint, hitNormal);
        audioManager?.PlayHit();

        if (headOnIsInstantDeath)
        {
            Kill(obstacleType);
        }
        else
        {
            ApplyDamage(maxHealth);
        }
    }

    private void ApplyDamage(float amount)
    {
        if (_isDead)
            return;

        _currentHealth = Mathf.Max(0f, _currentHealth - amount);
        RaiseHealthChanged();

        if (_currentHealth <= 0f)
        {
            Kill();
        }
    }

    private void Kill(string obstacleType = null)
    {
        if (_isDead)
            return;

        _isDead = true;

        if (_playerController != null)
        {
            _playerController.StopRun();
        }

        if (logEvents)
        {
            Debug.Log("PlayerHealth: Player died.");
        }

        OnDeath?.Invoke();
        OnDeathWithSource?.Invoke(obstacleType);
    }

    private void SpawnVfx(GameObject prefab, Vector3 position, Vector3 normal)
    {
        if (prefab == null)
            return;

        Quaternion rotation = Quaternion.LookRotation(normal, Vector3.up);
        if (vfxManager != null)
        {
            vfxManager.Spawn(prefab, position, rotation);
        }
        else
        {
            Instantiate(prefab, position, rotation);
        }
    }

    private void RaiseHealthChanged()
    {
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    }

    public void ResetHealth()
    {
        _isDead = false;
        _currentHealth = maxHealth;
        RaiseHealthChanged();
    }

    public void SetShieldActive(bool active)
    {
        _shieldActive = active;
    }

    private void StartGlancingState()
    {
        _glancingActive = true;

        if (_glancingRoutine != null)
            StopCoroutine(_glancingRoutine);

        if (glancingSmokeVfxPrefab != null)
        {
            _glancingSmokeInstance = Instantiate(glancingSmokeVfxPrefab, transform);
        }

        if (glancingSparksVfxPrefab != null)
        {
            _glancingSparksInstance = Instantiate(glancingSparksVfxPrefab, transform);
        }

        _glancingRoutine = StartCoroutine(GlancingCooldownRoutine());
    }

    private System.Collections.IEnumerator GlancingCooldownRoutine()
    {
        float duration = Mathf.Max(0.1f, glancingEffectDuration);
        yield return new WaitForSeconds(duration);

        _glancingActive = false;

        if (_glancingSmokeInstance != null)
            Destroy(_glancingSmokeInstance);
        if (_glancingSparksInstance != null)
            Destroy(_glancingSparksInstance);

        _glancingSmokeInstance = null;
        _glancingSparksInstance = null;
        _glancingRoutine = null;
    }
}
