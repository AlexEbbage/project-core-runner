using UnityEngine;

/// <summary>
/// Manages score, combo, and best score for a single endless run.
/// Uses GameBalanceConfig if provided, otherwise local inspector values.
/// </summary>
public class RunScoreManager : MonoBehaviour
{
    private const string BestScoreKey = "BestScore";

    [Header("Config (optional)")]
    [SerializeField] private GameBalanceConfig balanceConfig;

    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Distance Scoring")]
    [SerializeField] private float distanceScoreMultiplier = 1f;

    [Header("Pickup Scoring & Combo")]
    [SerializeField] private int pickupBaseScore = 10;
    [SerializeField] private float comboIncreasePerPickup = 1f;
    [SerializeField] private float maxComboValue = 10f;
    [SerializeField] private float comboDecayPerSecond = 1f;
    [SerializeField] private float comboToMultiplierFactor = 0.1f;

    [Header("Debug")]
    [SerializeField] private bool logScoreEvents = false;

    private float _currentScore;
    private float _bestScore;

    private float _comboValue;
    private float _lastPlayerZ;

    private bool _isRunActive = true;

    public float CurrentScore => _currentScore;
    public float BestScore => _bestScore;
    public float ComboValue => _comboValue;
    public float CurrentMultiplier => 1f + _comboValue * comboToMultiplierFactor;

    private void Awake()
    {
        if (playerTransform == null)
        {
            Debug.LogError("RunScoreManager: Player Transform is not assigned.", this);
            enabled = false;
            return;
        }

        if (balanceConfig != null)
        {
            distanceScoreMultiplier = balanceConfig.distanceScoreMultiplier;
            pickupBaseScore = balanceConfig.pickupBaseScore;
            comboIncreasePerPickup = balanceConfig.comboIncreasePerPickup;
            maxComboValue = balanceConfig.maxComboValue;
            comboDecayPerSecond = balanceConfig.comboDecayPerSecond;
            comboToMultiplierFactor = balanceConfig.comboToMultiplierFactor;
        }

        _bestScore = PlayerPrefs.GetFloat(BestScoreKey, 0f);
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath += HandlePlayerDeath;
        }

        ResetRunState();
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= HandlePlayerDeath;
        }
    }

    private void Update()
    {
        if (!_isRunActive)
            return;

        UpdateDistanceScore();
        UpdateComboDecay();
    }

    private void ResetRunState()
    {
        _currentScore = 0f;
        _comboValue = 0f;
        _lastPlayerZ = playerTransform.position.z;
    }

    private void UpdateDistanceScore()
    {
        float currentZ = playerTransform.position.z;
        float deltaZ = currentZ - _lastPlayerZ;

        if (deltaZ > 0f)
        {
            float baseScore = deltaZ * distanceScoreMultiplier;
            float added = baseScore * CurrentMultiplier;
            _currentScore += added;

            if (logScoreEvents)
            {
                Debug.Log($"RunScoreManager: +{added:F2} from distance (deltaZ={deltaZ:F2}, mult={CurrentMultiplier:F2}).");
            }
        }

        _lastPlayerZ = currentZ;
    }

    private void UpdateComboDecay()
    {
        if (_comboValue <= 0f)
            return;

        float decay = comboDecayPerSecond * Time.deltaTime;
        _comboValue = Mathf.Max(0f, _comboValue - decay);
    }

    public void OnPickupCollected()
    {
        if (!_isRunActive)
            return;

        _comboValue = Mathf.Min(maxComboValue, _comboValue + comboIncreasePerPickup);
        float added = pickupBaseScore * CurrentMultiplier;
        _currentScore += added;

        if (logScoreEvents)
        {
            Debug.Log($"RunScoreManager: Pickup collected. Combo={_comboValue:F2}, Mult={CurrentMultiplier:F2}, +{added:F1} points.");
        }
    }

    public void StopRun()
    {
        _isRunActive = false;
    }

    private void HandlePlayerDeath()
    {
        if (!_isRunActive)
            return;

        _isRunActive = false;

        if (_currentScore > _bestScore)
        {
            _bestScore = _currentScore;
            PlayerPrefs.SetFloat(BestScoreKey, _bestScore);
            PlayerPrefs.Save();

            if (logScoreEvents)
            {
                Debug.Log($"RunScoreManager: New best score: {_bestScore:F1}");
            }
        }
        else if (logScoreEvents)
        {
            Debug.Log($"RunScoreManager: Run ended. Score={_currentScore:F1}, Best={_bestScore:F1}");
        }
    }

    public void StartRun()
    {
        _isRunActive = true;
    }

    public void ResetRun()
    {
        ResetRunState();
    }

    public void ResumeAfterContinue()
    {
        _isRunActive = true;
        _lastPlayerZ = playerTransform.position.z;

        if (logScoreEvents)
        {
            Debug.Log($"RunScoreManager: ResumeAfterContinue. Score={_currentScore:F1}, Combo={_comboValue:F2}");
        }
    }
}
