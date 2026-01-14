using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks run stats such as distance, max speed, combos, coins, powerups, and hit breakdowns.
/// Updates lifetime totals and bests using PlayerPrefs when a run ends.
/// </summary>
public class RunStatsTracker : MonoBehaviour
{
    private const string TotalDistanceKey = "Stats_TotalDistance";
    private const string LongestRunDistanceKey = "Stats_LongestRunDistance";
    private const string MaxSpeedKey = "Stats_MaxSpeed";
    private const string HighestComboKey = "Stats_HighestCombo";
    private const string TotalCoinsKey = "Stats_TotalCoins";
    private const string TotalPowerupsKey = "Stats_TotalPowerups";
    private const string TotalDeathsKey = "Stats_TotalDeaths";
    private const string TotalGlancingHitsKey = "Stats_TotalGlancingHits";
    private const string TotalObstaclesDodgedKey = "Stats_TotalObstaclesDodged";
    private const string DeathsBySourceKey = "Stats_DeathsBySource";
    private const string GlancingBySourceKey = "Stats_GlancingBySource";
    private const string PowerupsByTypeKey = "Stats_PowerupsByType";

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

    [System.Serializable]
    private struct StringIntEntry
    {
        public string key;
        public int value;
    }

    [System.Serializable]
    private class StringIntList
    {
        public List<StringIntEntry> entries = new List<StringIntEntry>();
    }

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

        if (obstacleRingGenerator != null)
        {
            obstacleRingGenerator.OnObstacleRingPassed += HandleObstaclePassed;
        }
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

        if (obstacleRingGenerator != null)
        {
            obstacleRingGenerator.OnObstacleRingPassed -= HandleObstaclePassed;
        }
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

        float totalDistance = PlayerPrefs.GetFloat(TotalDistanceKey, 0f) + _totalDistance;
        PlayerPrefs.SetFloat(TotalDistanceKey, totalDistance);

        int totalCoins = PlayerPrefs.GetInt(TotalCoinsKey, 0) + _coinsCollected;
        PlayerPrefs.SetInt(TotalCoinsKey, totalCoins);

        int totalPowerups = PlayerPrefs.GetInt(TotalPowerupsKey, 0) + _powerupsCollected;
        PlayerPrefs.SetInt(TotalPowerupsKey, totalPowerups);

        int totalDeaths = PlayerPrefs.GetInt(TotalDeathsKey, 0) + _deaths;
        PlayerPrefs.SetInt(TotalDeathsKey, totalDeaths);

        int totalGlancingHits = PlayerPrefs.GetInt(TotalGlancingHitsKey, 0) + _glancingHits;
        PlayerPrefs.SetInt(TotalGlancingHitsKey, totalGlancingHits);

        int totalObstaclesDodged = PlayerPrefs.GetInt(TotalObstaclesDodgedKey, 0) + _obstaclesDodged;
        PlayerPrefs.SetInt(TotalObstaclesDodgedKey, totalObstaclesDodged);

        float longestRun = PlayerPrefs.GetFloat(LongestRunDistanceKey, 0f);
        if (_totalDistance > longestRun)
        {
            PlayerPrefs.SetFloat(LongestRunDistanceKey, _totalDistance);
        }

        float maxSpeed = PlayerPrefs.GetFloat(MaxSpeedKey, 0f);
        if (_maxSpeed > maxSpeed)
        {
            PlayerPrefs.SetFloat(MaxSpeedKey, _maxSpeed);
        }

        float highestCombo = PlayerPrefs.GetFloat(HighestComboKey, 0f);
        if (_highestCombo > highestCombo)
        {
            PlayerPrefs.SetFloat(HighestComboKey, _highestCombo);
        }

        MergeStringIntMap(DeathsBySourceKey, _deathsBySource);
        MergeStringIntMap(GlancingBySourceKey, _glancingBySource);
        MergePowerupMap(PowerupsByTypeKey, _powerupsByType);

        PlayerPrefs.Save();
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

    private void HandleObstaclePassed(ObstacleRingGenerator.ObstacleRingType type)
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

    private void MergeStringIntMap(string prefsKey, Dictionary<string, int> delta)
    {
        if (delta == null || delta.Count == 0)
            return;

        var current = LoadStringIntMap(prefsKey);
        foreach (var kvp in delta)
        {
            if (current.ContainsKey(kvp.Key))
            {
                current[kvp.Key] += kvp.Value;
            }
            else
            {
                current[kvp.Key] = kvp.Value;
            }
        }

        SaveStringIntMap(prefsKey, current);
    }

    private void MergePowerupMap(string prefsKey, Dictionary<PowerupType, int> delta)
    {
        if (delta == null || delta.Count == 0)
            return;

        var current = LoadStringIntMap(prefsKey);
        foreach (var kvp in delta)
        {
            string key = kvp.Key.ToString();
            if (current.ContainsKey(key))
            {
                current[key] += kvp.Value;
            }
            else
            {
                current[key] = kvp.Value;
            }
        }

        SaveStringIntMap(prefsKey, current);
    }

    private Dictionary<string, int> LoadStringIntMap(string prefsKey)
    {
        string json = PlayerPrefs.GetString(prefsKey, string.Empty);
        if (string.IsNullOrEmpty(json))
            return new Dictionary<string, int>();

        var list = JsonUtility.FromJson<StringIntList>(json);
        var result = new Dictionary<string, int>();
        if (list == null || list.entries == null)
            return result;

        for (int i = 0; i < list.entries.Count; i++)
        {
            var entry = list.entries[i];
            if (string.IsNullOrEmpty(entry.key))
                continue;

            result[entry.key] = entry.value;
        }

        return result;
    }

    private void SaveStringIntMap(string prefsKey, Dictionary<string, int> map)
    {
        var list = new StringIntList();
        foreach (var kvp in map)
        {
            list.entries.Add(new StringIntEntry
            {
                key = kvp.Key,
                value = kvp.Value
            });
        }

        string json = JsonUtility.ToJson(list);
        PlayerPrefs.SetString(prefsKey, json);
    }
}
