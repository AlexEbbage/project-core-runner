using UnityEngine;

/// <summary>
/// Computes and applies the player's forward speed based on:
/// - Time-based difficulty (speed ramps as run time increases)
/// - Combo-based bonus (higher combo = faster)
/// Uses SpeedScalingConfig for tuning.
/// </summary>
public class RunSpeedController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private RunScoreManager scoreManager;
    [SerializeField] private SpeedScalingConfig speedConfig;

    [Header("Camera")]
    [SerializeField] private CameraFovController cameraFovController;

    [Header("HUD")]
    [SerializeField] private HudController hudController;

    [Header("Speed-Up FX")]
    [SerializeField] private SpeedUpFlash speedUpFlash;
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private SpeedParticlesController speedParticles;
    [SerializeField] private bool enableSpeedMilestoneShake = true;
    [Tooltip("Optional SFX for speed-up step.")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private AudioClip speedUpSfx;

    [Tooltip("Every time speed increases by this much, we trigger FX.")]
    [SerializeField] private float speedMilestoneStep = 5f;

    [Header("Debug")]
    [SerializeField] private bool logSpeedChanges = false;

    private float _elapsedRunTime;
    private bool _isRunActive;
    private float _nextSpeedMilestone;

    private float _lastAppliedSpeed;

    public float CurrentSpeed => _lastAppliedSpeed;

    private void Awake()
    {
        if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
        if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (scoreManager == null) scoreManager = FindFirstObjectByType<RunScoreManager>();
        if (cameraFovController == null) cameraFovController = FindFirstObjectByType<CameraFovController>();
        if (speedParticles == null) speedParticles = FindFirstObjectByType<SpeedParticlesController>();

        if (speedConfig == null)
        {
            Debug.LogError("RunSpeedController: SpeedScalingConfig is not assigned.", this);
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath += HandlePlayerDeath;
        }

        float startingSpeed = playerController != null
            ? playerController.GetCurrentForwardSpeed()
            : (speedConfig != null ? speedConfig.baseForwardSpeed : 10f);

        _nextSpeedMilestone = startingSpeed + speedMilestoneStep;

        ResetRunSpeed();
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
        if (!_isRunActive || playerController == null || speedConfig == null)
            return;

        _elapsedRunTime += Time.deltaTime;

        float newSpeed = CalculateCurrentSpeed();
        if (!Mathf.Approximately(newSpeed, _lastAppliedSpeed))
        {
            _lastAppliedSpeed = newSpeed;
            playerController.SetForwardSpeed(newSpeed);
            cameraFovController?.SetSpeed(newSpeed);
            hudController?.SetSpeed(newSpeed);
            CheckSpeedMilestone(newSpeed);
            speedParticles?.SetRunSpeed(newSpeed);

            if (logSpeedChanges)
            {
                Debug.Log($"RunSpeedController: Speed={newSpeed:F2}, Time={_elapsedRunTime:F1}, Combo={GetComboValue():F2}");
            }
        }
    }

    private void ResetRunSpeed()
    {
        _elapsedRunTime = 0f;

        float startSpeed = Mathf.Clamp(
            speedConfig.baseForwardSpeed,
            0f,
            speedConfig.maxForwardSpeed
        );

        _lastAppliedSpeed = startSpeed;
        if (playerController != null)
        {
            playerController.SetForwardSpeed(startSpeed);
            cameraFovController?.SetSpeed(startSpeed);
            hudController?.SetSpeed(startSpeed);
            //CheckSpeedMilestone(startSpeed);
        }
    }

    public void StartRun()
    {
        _isRunActive = true;
    }

    public void StopRun()
    {
        _isRunActive = false;
    }

    private void HandlePlayerDeath()
    {
        _isRunActive = false;
    }

    private float CalculateCurrentSpeed()
    {
        float baseSpeed = speedConfig.baseForwardSpeed;

        float timeScale = speedConfig.EvaluateTimeScale(_elapsedRunTime);
        float timeBasedIncrease = speedConfig.speedIncreasePerSecond * _elapsedRunTime * timeScale;

        float comboValue = GetComboValue();
        float comboBonus = comboValue * speedConfig.comboSpeedFactor;
        comboBonus = Mathf.Clamp(comboBonus, 0f, speedConfig.comboMaxSpeedBonus);

        float speed = baseSpeed + timeBasedIncrease + comboBonus;
        speed = Mathf.Clamp(speed, 0f, speedConfig.maxForwardSpeed);

        return speed;
    }

    private float GetComboValue()
    {
        if (scoreManager == null)
            return 0f;

        return scoreManager.ComboValue;
    }

    public void ResetForNewRun()
    {
        ResetRunSpeed();
    }

    public void ResumeAfterContinue()
    {
        _isRunActive = true;

        if (playerController != null)
        {
            playerController.SetForwardSpeed(_lastAppliedSpeed);
            cameraFovController?.SetSpeed(_lastAppliedSpeed);
            hudController?.SetSpeed(_lastAppliedSpeed);
            //CheckSpeedMilestone(_lastAppliedSpeed);
        }

        if (logSpeedChanges)
        {
            Debug.Log($"RunSpeedController: ResumeAfterContinue. Speed={_lastAppliedSpeed:F2}, Time={_elapsedRunTime:F1}");
        }
    }

    public void SetSpeedConfig(SpeedScalingConfig newConfig)
    {
        if (newConfig == null) return;
        speedConfig = newConfig;
    }

    private void CheckSpeedMilestone(float newSpeed)
    {
        if (speedMilestoneStep <= 0f)
            return;

        if (newSpeed < _nextSpeedMilestone)
            return;

        // Advance milestone so we don't spam FX every frame
        _nextSpeedMilestone += speedMilestoneStep;

        // UI flash
        if (speedUpFlash != null)
        {
            speedUpFlash.PlayFlash();
        }

        // Camera shake
        if (enableSpeedMilestoneShake && cameraShake != null)
        {
            cameraShake.PlayShake();
        }

        // Optional SFX
        if (audioManager != null && speedUpSfx != null)
        {
            audioManager.PlaySfx(speedUpSfx);
        }
    }

}
