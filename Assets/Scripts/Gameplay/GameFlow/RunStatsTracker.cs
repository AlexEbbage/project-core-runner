using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks run stats such as distance, max speed, combos, coins, powerups, and hit breakdowns.
/// Updates lifetime totals and bests using RunStatsStore when a run ends.
/// </summary>
public class RunStatsTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private RunScoreManager scoreManager;
    [SerializeField] private RunSpeedController speedController;
    [SerializeField] private RunCurrencyManager currencyManager;
    [SerializeField] private PlayerPowerupController powerupController;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private ObstacleRingGenerator obstacleRingGenerator;

    private readonly Dictionary<string, int> _deathsBySource = new Dictionary<string, int>();
    private readonly Dictionary<string, int> _glancingBySource = new Dictionary<string, int>();
    private readonly Dictionary<PowerupType, int> _powerupsByType = new Dictionary<PowerupType, int>();

    private bool _isTracking;
    private bool _runFinalized;
    private float _runTime;
    private float _totalDistance;
    private float _maxSpeed;
    private float _highestCombo;
    private int _coinsCollected;
    private int _powerupsCollected;
    private int _obstaclesDodged;
    private int _deaths;
    private int _glancingHits;
    private float _lastTrackedZ;

    public float CurrentRunTime => _runTime;
    public float CurrentRunDistance => _totalDistance;
    public float MaxSpeed => _maxSpeed;
    public float HighestCombo => _highestCombo;
    public int CoinsCollected => _coinsCollected;
    public int PowerupsCollected => _powerupsCollected;
    public int ObstaclesDodged => _obstaclesDodged;
    public int Deaths => _deaths;
    public int GlancingHits => _glancingHits;

    public IReadOnlyDictionary<string, int> DeathsBySource => _deathsBySource;
    public IReadOnlyDictionary<string, int> GlancingHitsBySource => _glancingBySource;
    public IReadOnlyDictionary<PowerupType, int> PowerupsByType => _powerupsByType;

    private void Awake()
    {
        if (playerTransform == null)
        {
            var player = FindFirstObjectByType<PlayerController>();
            playerTransform = player != null ? player.transform : null;
        }

        if (scoreManager == null) scoreManager = FindFirstObjectByType<RunScoreManager>();
        if (speedController == null) speedController = FindFirstObjectByType<RunSpeedController>();
        if (currencyManager == null) currencyManager = FindFirstObjectByType<RunCurrencyManager>();
        if (powerupController == null) powerupController = FindFirstObjectByType<PlayerPowerupController>();
        if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (obstacleRingGenerator == null) obstacleRingGenerator = FindFirstObjectByType<ObstacleRingGenerator>();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeathWithSource += HandleDeath;
            playerHealth.OnGlancingHit += HandleGlancingHit;
        }

        if (powerupController != null)
        {
            powerupController.OnPowerupCollected += HandlePowerupCollected;
        }

        if (currencyManager != null)
        {
            currencyManager.OnCoinsAdded += HandleCoinsAdded;
        }

        //if (obstacleRingGenerator != null)
        //{
        //    obstacleRingGenerator.OnObstacleRingPassed += HandleObstaclePassed;
        //}
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeathWithSource -= HandleDeath;
            playerHealth.OnGlancingHit -= HandleGlancingHit;
        }

        if (powerupController != null)
        {
            powerupController.OnPowerupCollected -= HandlePowerupCollected;
        }

        if (currencyManager != null)
        {
            currencyManager.OnCoinsAdded -= HandleCoinsAdded;
        }

        //if (obstacleRingGenerator != null)
        //{
        //    obstacleRingGenerator.OnObstacleRingPassed -= HandleObstaclePassed;
        //}
    }

    private void Update()
    {
        if (!_isTracking || playerTransform == null)
            return;

        _runTime += Time.deltaTime;

        float currentZ = playerTransform.position.z;
        float deltaZ = currentZ - _lastTrackedZ;
        if (deltaZ > 0f)
        {
            _totalDistance += deltaZ;
        }
        _lastTrackedZ = currentZ;

        if (speedController != null)
        {
            _maxSpeed = Mathf.Max(_maxSpeed, speedController.CurrentSpeed);
        }

        if (scoreManager != null)
        {
            _highestCombo = Mathf.Max(_highestCombo, scoreManager.ComboValue);
        }
    }

    public void ResetRunStats()
    {
        _isTracking = false;
        _runFinalized = false;
        _runTime = 0f;
        _totalDistance = 0f;
        _maxSpeed = 0f;
        _highestCombo = 0f;
        _coinsCollected = 0;
        _powerupsCollected = 0;
        _obstaclesDodged = 0;
        _deaths = 0;
        _glancingHits = 0;

        _deathsBySource.Clear();
        _glancingBySource.Clear();
        _powerupsByType.Clear();

        if (playerTransform != null)
            _lastTrackedZ = playerTransform.position.z;
    }

    public void StartRun()
    {
        _isTracking = true;
        if (playerTransform != null)
            _lastTrackedZ = playerTransform.position.z;
    }

    public void PauseRun()
    {
        _isTracking = false;
    }

    public void ResumeRun()
    {
        _isTracking = true;
        if (playerTransform != null)
            _lastTrackedZ = playerTransform.position.z;
    }

    public void EndRun()
    {
        if (_runFinalized)
            return;

        _isTracking = false;
        _runFinalized = true;

        RunStatsStore.AddTotalDistance(_totalDistance);
        RunStatsStore.AddTotalCoins(_coinsCollected);
        RunStatsStore.AddTotalPowerups(_powerupsCollected);
        RunStatsStore.AddTotalDeaths(_deaths);
        RunStatsStore.AddTotalGlancingHits(_glancingHits);
        RunStatsStore.AddTotalObstaclesDodged(_obstaclesDodged);
        RunStatsStore.SetLongestRunDistance(_totalDistance);
        RunStatsStore.SetMaxSpeed(_maxSpeed);
        RunStatsStore.SetHighestCombo(_highestCombo);
        RunStatsStore.MergeDeathsBySource(_deathsBySource);
        RunStatsStore.MergeGlancingHitsBySource(_glancingBySource);
        RunStatsStore.MergePowerupsByType(_powerupsByType);
        RunStatsStore.Save();
    }

    private void HandleCoinsAdded(int amount)
    {
        if (!_isTracking)
            return;

        _coinsCollected += amount;
    }

    private void HandlePowerupCollected(PowerupType type)
    {
        if (!_isTracking)
            return;

        _powerupsCollected++;
        if (_powerupsByType.ContainsKey(type))
        {
            _powerupsByType[type]++;
        }
        else
        {
            _powerupsByType[type] = 1;
        }
    }

    private void HandleObstaclePassed()
    {
        if (!_isTracking)
            return;

        _obstaclesDodged++;
    }

    private void HandleDeath(string source)
    {
        if (!_isTracking)
            return;

        _deaths++;
        string key = string.IsNullOrEmpty(source) ? "Unknown" : source;
        if (_deathsBySource.ContainsKey(key))
        {
            _deathsBySource[key]++;
        }
        else
        {
            _deathsBySource[key] = 1;
        }
    }

    private void HandleGlancingHit(string source)
    {
        if (!_isTracking)
            return;

        _glancingHits++;
        string key = string.IsNullOrEmpty(source) ? "Unknown" : source;
        if (_glancingBySource.ContainsKey(key))
        {
            _glancingBySource[key]++;
        }
        else
        {
            _glancingBySource[key] = 1;
        }
    }

}
