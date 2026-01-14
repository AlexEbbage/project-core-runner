using System.Collections;
using UnityEngine;

/// <summary>
/// Handles temporary powerups for the player.
/// </summary>
public class PlayerPowerupController : MonoBehaviour
{
    [System.Serializable]
    public class PowerupEffects
    {
        public PowerupType powerupType;
        public GameObject startVfxPrefab;
        public GameObject loopVfxPrefab;
        public GameObject endVfxPrefab;
        public AudioClip startSfx;
        public AudioClip endSfx;
    }

    [Header("Config (optional)")]
    [SerializeField] private GameBalanceConfig balanceConfig;

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private RunScoreManager runScoreManager;
    [SerializeField] private ObstacleRingGenerator obstacleRingGenerator;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private VfxManager vfxManager;

    [Header("Powerup Values")]
    [SerializeField] private float autoPilotDuration = 4f;
    [SerializeField] private float coinMultiplierValue = 2f;
    [SerializeField] private float coinMultiplierDuration = 6f;
    [SerializeField] private float scoreMultiplierValue = 2f;
    [SerializeField] private float scoreMultiplierDuration = 6f;
    [SerializeField] private float shieldDuration = 5f;
    [SerializeField] private float coinBonanzaSpawnMultiplier = 2.5f;
    [SerializeField] private float coinBonanzaDuration = 6f;

    [Header("Powerup VFX/SFX")]
    [SerializeField] private PowerupEffects[] powerupEffects;

    [Header("Auto Pilot")]
    [SerializeField] private float autoPilotLookAhead = 35f;
    [SerializeField] private float autoPilotProbeRadius = 0.4f;
    [SerializeField] private int autoPilotSampleCount = 12;
    [SerializeField] private LayerMask obstacleMask = ~0;
    [SerializeField] private LayerMask pickupMask = ~0;

    private Coroutine _autoPilotRoutine;
    private Coroutine _coinMultiplierRoutine;
    private Coroutine _scoreMultiplierRoutine;
    private Coroutine _shieldRoutine;
    private Coroutine _coinBonanzaRoutine;

    private readonly System.Collections.Generic.Dictionary<PowerupType, float> _powerupEndTimes =
        new System.Collections.Generic.Dictionary<PowerupType, float>();
    private readonly System.Collections.Generic.Dictionary<PowerupType, GameObject> _loopVfxInstances =
        new System.Collections.Generic.Dictionary<PowerupType, GameObject>();

    public event System.Action<PowerupType> OnPowerupCollected;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        if (audioManager == null)
            audioManager = FindFirstObjectByType<AudioManager>();

        if (vfxManager == null)
            vfxManager = VfxManager.Instance;

        if (balanceConfig != null)
        {
            autoPilotDuration = balanceConfig.autoPilotDuration;
            coinMultiplierValue = balanceConfig.coinMultiplierValue;
            coinMultiplierDuration = balanceConfig.coinMultiplierDuration;
            scoreMultiplierValue = balanceConfig.scoreMultiplierValue;
            scoreMultiplierDuration = balanceConfig.scoreMultiplierDuration;
            shieldDuration = balanceConfig.shieldDuration;
            coinBonanzaSpawnMultiplier = balanceConfig.coinBonanzaSpawnMultiplier;
            coinBonanzaDuration = balanceConfig.coinBonanzaDuration;
        }
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnShieldBroken += HandleShieldBroken;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnShieldBroken -= HandleShieldBroken;
        }
    }

    public void ActivatePowerup(PowerupType powerupType)
    {
        OnPowerupCollected?.Invoke(powerupType);

        switch (powerupType)
        {
            case PowerupType.AutoPilot:
                RestartRoutine(ref _autoPilotRoutine, AutoPilotRoutine());
                break;
            case PowerupType.CoinMultiplier:
                RestartRoutine(ref _coinMultiplierRoutine, CoinMultiplierRoutine());
                break;
            case PowerupType.ScoreMultiplier:
                RestartRoutine(ref _scoreMultiplierRoutine, ScoreMultiplierRoutine());
                break;
            case PowerupType.Shield:
                RestartRoutine(ref _shieldRoutine, ShieldRoutine());
                break;
            case PowerupType.CoinBonanza:
                RestartRoutine(ref _coinBonanzaRoutine, CoinBonanzaRoutine());
                break;
        }
    }

    public PowerupStatus[] GetActivePowerups()
    {
        var list = new System.Collections.Generic.List<PowerupStatus>();
        foreach (var kvp in _powerupEndTimes)
        {
            float remaining = Mathf.Max(0f, kvp.Value - Time.time);
            list.Add(new PowerupStatus(kvp.Key, remaining, true));
        }

        return list.ToArray();
    }

    private void RestartRoutine(ref Coroutine routine, IEnumerator newRoutine)
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(newRoutine);
    }

    private IEnumerator AutoPilotRoutine()
    {
        BeginPowerup(PowerupType.AutoPilot, autoPilotDuration);

        if (playerController != null)
            playerController.SetAutoPilotActive(true, 0f);

        float endTime = Time.time + autoPilotDuration;
        while (Time.time < endTime)
        {
            UpdateAutoPilotInput();
            yield return null;
        }

        if (playerController != null)
            playerController.SetAutoPilotActive(false, 0f);

        EndPowerup(PowerupType.AutoPilot);
    }

    private IEnumerator CoinMultiplierRoutine()
    {
        BeginPowerup(PowerupType.CoinMultiplier, coinMultiplierDuration);

        if (runScoreManager != null)
            runScoreManager.SetPowerupPickupMultiplier(coinMultiplierValue);

        yield return new WaitForSeconds(coinMultiplierDuration);

        if (runScoreManager != null)
            runScoreManager.SetPowerupPickupMultiplier(1f);

        EndPowerup(PowerupType.CoinMultiplier);
    }

    private IEnumerator ScoreMultiplierRoutine()
    {
        BeginPowerup(PowerupType.ScoreMultiplier, scoreMultiplierDuration);

        if (runScoreManager != null)
            runScoreManager.SetPowerupScoreMultiplier(scoreMultiplierValue);

        yield return new WaitForSeconds(scoreMultiplierDuration);

        if (runScoreManager != null)
            runScoreManager.SetPowerupScoreMultiplier(1f);

        EndPowerup(PowerupType.ScoreMultiplier);
    }

    private IEnumerator ShieldRoutine()
    {
        BeginPowerup(PowerupType.Shield, shieldDuration);

        if (playerHealth != null)
            playerHealth.SetShieldActive(true);

        yield return new WaitForSeconds(shieldDuration);

        if (playerHealth != null)
            playerHealth.SetShieldActive(false);

        EndPowerup(PowerupType.Shield);
    }

    private IEnumerator CoinBonanzaRoutine()
    {
        BeginPowerup(PowerupType.CoinBonanza, coinBonanzaDuration);

        if (obstacleRingGenerator != null)
            obstacleRingGenerator.SetPickupSpawnChanceMultiplier(coinBonanzaSpawnMultiplier);

        yield return new WaitForSeconds(coinBonanzaDuration);

        if (obstacleRingGenerator != null)
            obstacleRingGenerator.SetPickupSpawnChanceMultiplier(1f);

        EndPowerup(PowerupType.CoinBonanza);
    }

    private void HandleShieldBroken()
    {
        if (_shieldRoutine != null)
        {
            StopCoroutine(_shieldRoutine);
            _shieldRoutine = null;
        }

        EndPowerup(PowerupType.Shield);
    }

    private void BeginPowerup(PowerupType type, float duration)
    {
        StopLoopEffect(type);
        _powerupEndTimes[type] = Time.time + duration;
        PlayPowerupStartEffects(type);
    }

    private void EndPowerup(PowerupType type)
    {
        _powerupEndTimes.Remove(type);
        StopLoopEffect(type);
        PlayPowerupEndEffects(type);
    }

    private void PlayPowerupStartEffects(PowerupType type)
    {
        var effects = GetEffects(type);
        if (effects == null)
            return;

        if (effects.startVfxPrefab != null)
        {
            SpawnOneShotVfx(effects.startVfxPrefab, transform.position, transform.rotation, transform);
        }

        if (effects.loopVfxPrefab != null)
        {
            var instance = Instantiate(effects.loopVfxPrefab, transform.position, transform.rotation, transform);
            _loopVfxInstances[type] = instance;
        }

        if (effects.startSfx != null)
            audioManager?.PlaySfx(effects.startSfx);
    }

    private void PlayPowerupEndEffects(PowerupType type)
    {
        var effects = GetEffects(type);
        if (effects == null)
            return;

        if (effects.endVfxPrefab != null)
        {
            SpawnOneShotVfx(effects.endVfxPrefab, transform.position, transform.rotation, transform);
        }

        if (effects.endSfx != null)
            audioManager?.PlaySfx(effects.endSfx);
    }

    private void StopLoopEffect(PowerupType type)
    {
        if (_loopVfxInstances.TryGetValue(type, out var instance) && instance != null)
        {
            Destroy(instance);
        }
        _loopVfxInstances.Remove(type);
    }

    private PowerupEffects GetEffects(PowerupType type)
    {
        if (powerupEffects == null)
            return null;

        foreach (var effects in powerupEffects)
        {
            if (effects != null && effects.powerupType == type)
                return effects;
        }

        return null;
    }

    private void SpawnOneShotVfx(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (prefab == null)
            return;

        if (vfxManager != null)
        {
            var spawnedInstance = vfxManager.Spawn(prefab, position, rotation);
            if (spawnedInstance != null && parent != null)
            {
                spawnedInstance.transform.SetParent(parent, true);
            }
            return;
        }

        var instance = parent != null
            ? Instantiate(prefab, parent)
            : Instantiate(prefab, position, rotation);
        instance.transform.SetPositionAndRotation(position, rotation);
        float lifetime = GetInstanceLifetime(instance);
        Destroy(instance, lifetime);
    }

    private float GetInstanceLifetime(GameObject instance)
    {
        if (instance == null)
            return 2f;

        ParticleSystem[] systems = instance.GetComponentsInChildren<ParticleSystem>(true);
        float maxLifetime = 0f;

        foreach (ParticleSystem system in systems)
        {
            var main = system.main;
            float startLifetime = GetMaxCurveValue(main.startLifetime);
            maxLifetime = Mathf.Max(maxLifetime, main.duration + startLifetime);
        }

        if (maxLifetime <= 0f)
        {
            maxLifetime = 2f;
        }

        return maxLifetime;
    }

    private static float GetMaxCurveValue(ParticleSystem.MinMaxCurve curve)
    {
        switch (curve.mode)
        {
            case ParticleSystemCurveMode.TwoConstants:
                return curve.constantMax;
            case ParticleSystemCurveMode.TwoCurves:
                return curve.curveMultiplier * curve.curveMax.Evaluate(1f);
            case ParticleSystemCurveMode.Curve:
                return curve.curveMultiplier * curve.curve.Evaluate(1f);
            case ParticleSystemCurveMode.Constant:
            default:
                return curve.constant;
        }
    }

    private void UpdateAutoPilotInput()
    {
        if (playerController == null)
            return;

        if (TrySteerTowardPickup())
            return;

        float currentAngle = playerController.CurrentAngleDegrees;
        int samples = Mathf.Max(3, autoPilotSampleCount);
        float step = 360f / samples;
        float radius = playerController.TubeRadius;
        Vector3 origin = transform.position;

        float bestScore = -1f;
        float bestAngle = currentAngle;

        for (int i = 0; i < samples; i++)
        {
            float candidateAngle = i * step;
            float rad = candidateAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0f);
            Vector3 start = new Vector3(offset.x, offset.y, origin.z);
            Vector3 worldStart = transform.parent != null ? transform.parent.TransformPoint(start) : start;

            float score = autoPilotLookAhead;
            if (Physics.SphereCast(worldStart, autoPilotProbeRadius, Vector3.forward, out var hit, autoPilotLookAhead, obstacleMask))
            {
                if (hit.collider != null && hit.collider.CompareTag("Obstacle"))
                {
                    score = hit.distance;
                }
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestAngle = candidateAngle;
            }
        }

        float delta = Mathf.DeltaAngle(currentAngle, bestAngle);
        float angularSpeed = Mathf.Max(1f, playerController.AngularSpeedDegrees);
        float input = Mathf.Clamp(delta / (angularSpeed * Time.deltaTime), -1f, 1f);
        playerController.SetAutoPilotInput(input);
    }

    private bool TrySteerTowardPickup()
    {
        if (playerController == null)
            return false;

        Vector3 origin = transform.position;
        float lookAhead = Mathf.Max(1f, autoPilotLookAhead);
        Collider[] hits = Physics.OverlapSphere(origin + Vector3.forward * lookAhead * 0.5f, lookAhead * 0.75f, pickupMask);
        if (hits == null || hits.Length == 0)
            return false;

        float bestZ = float.MaxValue;
        Vector3 bestPos = Vector3.zero;
        bool found = false;

        for (int i = 0; i < hits.Length; i++)
        {
            var pickup = hits[i].GetComponentInParent<Pickup>();
            if (pickup == null)
                continue;

            float dz = pickup.transform.position.z - origin.z;
            if (dz < 0f || dz > lookAhead)
                continue;

            if (dz < bestZ)
            {
                bestZ = dz;
                bestPos = pickup.transform.position;
                found = true;
            }
        }

        if (!found)
            return false;

        Vector3 local = transform.parent != null ? transform.parent.InverseTransformPoint(bestPos) : bestPos;
        float targetAngle = Mathf.Atan2(local.y, local.x) * Mathf.Rad2Deg;
        float delta = Mathf.DeltaAngle(playerController.CurrentAngleDegrees, targetAngle);
        float angularSpeed = Mathf.Max(1f, playerController.AngularSpeedDegrees);
        float input = Mathf.Clamp(delta / (angularSpeed * Time.deltaTime), -1f, 1f);
        playerController.SetAutoPilotInput(input);
        return true;
    }

    public readonly struct PowerupStatus
    {
        public PowerupStatus(PowerupType type, float remainingTime, bool isTimed)
        {
            Type = type;
            RemainingTime = remainingTime;
            IsTimed = isTimed;
        }

        public PowerupType Type { get; }
        public float RemainingTime { get; }
        public bool IsTimed { get; }
    }
}
