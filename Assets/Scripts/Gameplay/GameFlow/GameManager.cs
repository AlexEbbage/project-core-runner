using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const string RunEventName = "run_end";

    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }

    [Header("Config (optional)")]
    [SerializeField] private GameBalanceConfig balanceConfig;

    [Header("References - Core")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private RunScoreManager scoreManager;
    [SerializeField] private RunSpeedController speedController;
    [SerializeField] private RunCurrencyManager currencyManager;
    [SerializeField] private RunZoneManager runZoneManager;
    [SerializeField] private ObstacleRingGenerator obstacleRingGenerator;
    [SerializeField] private RunStatsTracker statsTracker;
    [SerializeField] private PlayerPowerupController powerupController;
    [SerializeField] private PlayerProfile playerProfile;

    [Header("Player Visuals (optional)")]
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private Collider playerCollider;

    [Header("References - UI (GameManager-driven)")]
    // Run UI controller stack removed; use HUD/GameOver/Pause components wired here.
    [SerializeField] private MainMenuUI mainMenuUI;
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private HudController hudController;
    [SerializeField] private PauseMenuUI pauseMenuUI;
    [SerializeField] private CountdownUIController countdownUIController;
    [SerializeField] private LoadingScreenManager loadingScreenManager;
    [SerializeField] private RewardedRunPromptUI rewardedRunPromptUI;

    [Header("Rewarded Run Prompt")]
    [SerializeField] private bool rewardedRunPromptEnabled = true;
    [SerializeField] private float rewardedRunPromptDelaySeconds = 45f;
    [SerializeField] private bool rewardedRunPromptPausesGameplay = false;
    [SerializeField] private float rewardedRunPromptAutoDismissSeconds = 8f;

    [Header("Run Rewards")]
    [SerializeField] private float xpPerScorePoint = 1f;
    [SerializeField] private int gemsPerCoins = 100;
    [SerializeField] private float runRewardGrantCooldownSeconds = 2f;

    [Header("References - Services")]
    [SerializeField] private MonoBehaviour rewardedAdServiceBehaviour;
    [SerializeField] private MonoBehaviour interstitialAdServiceBehaviour;
    [SerializeField] private MonoBehaviour analyticsServiceBehaviour;
    [SerializeField] private MonoBehaviour pushNotificationServiceBehaviour;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private VfxManager vfxManager;

    [Header("Continues")]
    [SerializeField] private int maxContinuesPerRun = 3;
    [SerializeField] private int continuesUsed;

    [Header("Continue Respawn Settings")]
    [SerializeField] private float continueRespawnBackDistance = 8f;
    [SerializeField] private float continueRespawnHeightOffset = 0.5f;

    [Header("Continue VFX")]
    [SerializeField] private GameObject continueRespawnVfxPrefab;
    [SerializeField] private int startClearRings = 5;
    [SerializeField] private float dissolveDuration = 0.4f;

    [Header("Debug")]
    [SerializeField] private bool logStateChanges = false;

    private IRewardedAdService _rewardedAdService;
    private IInterstitialAdService _interstitialAdService;
    private IAnalyticsService _analytics;
    private IPushNotificationService _pushNotifications;
    private bool _adInProgress;
    private bool _interstitialInProgress;
    private bool _interstitialShownThisRun;
    private float _elapsedTime;
    private bool _gameTimerEnabled;
    private bool _rewardedRunPromptShownThisRun;
    private bool _rewardedRunPromptActive;
    private float _rewardedRunPromptPrevTimeScale;
    private bool _rewardedRunPromptPausedRun;
    private GameStateMachine _stateMachine;
    private GameServicesFacade _services;
    private bool _runRewardsGranted;
    private bool _runRewardsDoubled;
    private bool _doubleRunRewardsQueued;
    private float _lastRunRewardGrantTime = float.NegativeInfinity;
    private RunRewardBundle _lastRunRewards;

    private Vector3 _lastDeathPosition;
    private Vector3 _lastDeathForward;

    public GameState CurrentState => _stateMachine != null ? _stateMachine.CurrentState : GameState.Menu;

    public int ContinuesUsed => continuesUsed;

    public float GetElapsedGameTime => _elapsedTime;

    public int MaxContinuesPerRun => maxContinuesPerRun;

    public bool GameTimerEnabled => _gameTimerEnabled;

    public bool CanDoubleRunRewards => _runRewardsGranted && !_runRewardsDoubled && !_adInProgress;

    public bool IsDoubleRewardsAdReady()
    {
        if (!CanDoubleRunRewards)
            return false;

        if (_services?.RewardedAds == null)
            return false;

        return _services.RewardedAds.IsRewardedAdReady();
    }

    public bool IsContinueAdReady()
    {
        if (_adInProgress)
            return false;

        if (_services?.RewardedAds == null)
            return false;

        return _services.RewardedAds.IsRewardedAdReady();
    }

    private void Awake()
    {
        _stateMachine = new GameStateMachine(logStateChanges);

        if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
        if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (scoreManager == null) scoreManager = FindFirstObjectByType<RunScoreManager>();
        if (speedController == null) speedController = FindFirstObjectByType<RunSpeedController>();
        if (currencyManager == null) currencyManager = FindFirstObjectByType<RunCurrencyManager>();
        if (runZoneManager == null) runZoneManager = FindFirstObjectByType<RunZoneManager>();
        if (statsTracker == null) statsTracker = FindFirstObjectByType<RunStatsTracker>();
        if (obstacleRingGenerator == null) obstacleRingGenerator = FindFirstObjectByType<ObstacleRingGenerator>();
        if (powerupController == null) powerupController = FindFirstObjectByType<PlayerPowerupController>();
        if (playerProfile == null)
        {
            var profiles = Resources.FindObjectsOfTypeAll<PlayerProfile>();
            if (profiles != null && profiles.Length > 0)
            {
                playerProfile = profiles[0];
            }
        }

        if (balanceConfig != null)
        {
            maxContinuesPerRun = balanceConfig.maxContinuesPerRun;
            continueRespawnBackDistance = balanceConfig.continueRespawnBackDistance;
            continueRespawnHeightOffset = balanceConfig.continueRespawnHeightOffset;
            if (obstacleRingGenerator != null)
            {
                obstacleRingGenerator.SetPickupFloatHeight(balanceConfig.pickupFloatHeight);
                obstacleRingGenerator.SetPickupSurfaceOffset(balanceConfig.pickupSurfaceOffset);
            }
        }


        if (playerVisual == null && playerController != null)
        {
            playerVisual = playerController.GetComponentInChildren<PlayerVisual>();
        }

        if (playerCollider == null && playerController != null)
        {
            playerCollider = playerController.GetComponent<Collider>();
        }

        if (mainMenuUI == null) mainMenuUI = FindFirstObjectByType<MainMenuUI>();
        if (gameOverUI == null) gameOverUI = FindFirstObjectByType<GameOverUI>();
        if (hudController == null) hudController = FindFirstObjectByType<HudController>();
        if (pauseMenuUI == null) pauseMenuUI = FindFirstObjectByType<PauseMenuUI>();
        if (loadingScreenManager == null) loadingScreenManager = FindFirstObjectByType<LoadingScreenManager>();
        if (rewardedRunPromptUI == null) rewardedRunPromptUI = FindFirstObjectByType<RewardedRunPromptUI>();

        if (audioManager == null) audioManager = FindFirstObjectByType<AudioManager>();
        if (vfxManager == null) vfxManager = VfxManager.Instance;

        if (rewardedAdServiceBehaviour != null)
        {
            _rewardedAdService = rewardedAdServiceBehaviour as IRewardedAdService;
            if (_rewardedAdService == null)
            {
                Debug.LogWarning("GameManager: rewardedAdServiceBehaviour does not implement IRewardedAdService.");
            }
        }

        if (interstitialAdServiceBehaviour != null)
        {
            _interstitialAdService = interstitialAdServiceBehaviour as IInterstitialAdService;
            if (_interstitialAdService == null)
            {
                Debug.LogWarning("GameManager: interstitialAdServiceBehaviour does not implement IInterstitialAdService.");
            }
        }

        if (analyticsServiceBehaviour != null)
        {
            _analytics = analyticsServiceBehaviour as IAnalyticsService;
            if (_analytics == null)
            {
                Debug.LogWarning("GameManager: analyticsServiceBehaviour does not implement IAnalyticsService.");
            }
        }

        if (pushNotificationServiceBehaviour != null)
        {
            _pushNotifications = pushNotificationServiceBehaviour as IPushNotificationService;
            if (_pushNotifications == null)
            {
                Debug.LogWarning("GameManager: pushNotificationServiceBehaviour does not implement IPushNotificationService.");
            }
        }

        _services = new GameServicesFacade(audioManager, vfxManager, _rewardedAdService, _interstitialAdService, _analytics);
        EnsureRewardedRunPrompt();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath += HandlePlayerDeath;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= HandlePlayerDeath;
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;
        _pushNotifications?.Initialize();
        _services?.Audio?.PlayMenuMusic();
        GoToMenu();
    }

    private void Update()
    {
        if (_stateMachine.CurrentState == GameState.Playing && _gameTimerEnabled)
        {
            _elapsedTime += Time.deltaTime;
        }

        if (ShouldTriggerRewardedRunPrompt())
        {
            ShowRewardedRunPrompt();
        }
    }

    private void TransitionToState(GameState newState, float timeScale)
    {
        if (_stateMachine.SetState(newState))
        {
            Time.timeScale = timeScale;
        }
    }

    // --- UI hooks ---

    public void OnPlayButtonPressed()
    {
        if (_stateMachine.CurrentState != GameState.Menu)
            return;

        _services?.Audio?.PlayButtonClick();
        StartNewRunWithFade(() =>
        {
            _services?.Audio?.PlayGameplayMusic();
            StartNewRunFromMenu();
        });
    }

    public void OnRestartButtonPressed()
    {
        if (_stateMachine.CurrentState != GameState.GameOver)
            return;

        _services?.Audio?.PlayButtonClick();
        StartNewRunWithFade(StartNewRunFromGameOver);
    }

    public void OnMenuButtonPressedFromGameOver()
    {
        if (_stateMachine.CurrentState != GameState.GameOver)
            return;

        _services?.Audio?.PlayButtonClick();
        TryShowInterstitial("menu_return", () => ReturnToMenuWithFade(() =>
        {
            _services?.Audio?.PlayMenuMusic();
            runZoneManager?.OnRunEnded();
            GoToMenu();
        }));
    }

    public void OnPauseButtonPressed()
    {
        if (_stateMachine.CurrentState != GameState.Playing)
            return;

        _services?.Audio?.PlayButtonClick();
        PauseGame();
    }

    public void OnResumeButtonPressedFromPause()
    {
        if (_stateMachine.CurrentState != GameState.Paused)
            return;

        _services?.Audio?.PlayButtonClick();
        ResumeGame();
    }

    public void OnMenuButtonPressedFromPause()
    {
        if (_stateMachine.CurrentState != GameState.Paused)
            return;

        _services?.Audio?.PlayButtonClick();
        TryShowInterstitial("menu_return", () => ReturnToMenuWithFade(() =>
        {
            _services?.Audio?.PlayMenuMusic();
            runZoneManager?.OnRunEnded();
            statsTracker?.EndRun();
            GoToMenu();
        }));
    }

    public void OnContinueButtonPressed()
    {
        if (_stateMachine.CurrentState != GameState.GameOver)
            return;

        _services?.Audio?.PlayButtonClick();

        if (continuesUsed >= maxContinuesPerRun)
        {
            if (logStateChanges)
                Debug.Log("GameManager: Continue pressed but no continues remain.");
            return;
        }

        if (_adInProgress)
        {
            if (logStateChanges)
                Debug.Log("GameManager: Continue pressed but ad is already in progress.");
            return;
        }

        if (AdsConfig.RemoveAds)
        {
            if (logStateChanges)
            {
                Debug.Log("GameManager: Remove Ads active, skipping continue ad.");
            }

            LogAnalyticsEvent("ad_bypassed", new Dictionary<string, object>
            {
                { "source", "continue" },
                { "reason", "remove_ads" }
            });

            Time.timeScale = 1f;
            HandleContinueAdResult(RewardedAdResult.Rewarded);
            return;
        }

        if (_services?.RewardedAds == null)
        {
            if (logStateChanges)
            {
                Debug.LogWarning("GameManager: No IRewardedAdService assigned. Cannot show rewarded ad.");
            }

            LogAnalyticsEvent("ad_not_ready", new Dictionary<string, object>
            {
                { "source", "continue" },
                { "reason", "service_missing" }
            });

            return;
        }

        if (!_services.RewardedAds.IsRewardedAdReady())
        {
            if (logStateChanges)
            {
                Debug.Log("GameManager: Rewarded ad not ready.");
            }

            LogAnalyticsEvent("ad_not_ready", new Dictionary<string, object>
            {
                { "source", "continue" }
            });

            return;
        }

        _adInProgress = true;
        Time.timeScale = 1f;

        if (logStateChanges)
        {
            Debug.Log("GameManager: Showing rewarded ad for continue.");
        }

        LogAnalyticsEvent("ad_shown", new Dictionary<string, object>
        {
            { "source", "continue" },
            { "continue_index", continuesUsed + 1 }
        });

        _services.RewardedAds.ShowRewardedAd(HandleContinueAdResult);
    }

    public void OnDoubleRewardsButtonPressed()
    {
        if (_stateMachine.CurrentState != GameState.GameOver)
            return;

        if (!CanDoubleRunRewards)
            return;

        if (AdsConfig.RemoveAds)
        {
            if (logStateChanges)
            {
                Debug.Log("GameManager: Remove Ads active, skipping double rewards ad.");
            }

            LogAnalyticsEvent("ad_bypassed", new Dictionary<string, object>
            {
                { "source", "double_rewards" },
                { "reason", "remove_ads" }
            });

            return;
        }

        if (_services?.RewardedAds == null)
        {
            if (logStateChanges)
            {
                Debug.LogWarning("GameManager: No IRewardedAdService assigned. Cannot show double rewards ad.");
            }
            return;
        }

        if (!_services.RewardedAds.IsRewardedAdReady())
        {
            if (logStateChanges)
            {
                Debug.Log("GameManager: Double rewards ad not ready.");
            }
            return;
        }

        _adInProgress = true;
        Time.timeScale = 1f;

        LogAnalyticsEvent("ad_shown", new Dictionary<string, object>
        {
            { "source", "double_rewards" }
        });

        _services.RewardedAds.ShowRewardedAd(result =>
        {
            _adInProgress = false;

            if (result == RewardedAdResult.Rewarded)
            {
                LogAnalyticsEvent("ad_completed", new Dictionary<string, object>
                {
                    { "source", "double_rewards" }
                });

                GrantDoubleRunRewards();
            }
            else
            {
                LogAnalyticsEvent("ad_skipped", new Dictionary<string, object>
                {
                    { "source", "double_rewards" },
                    { "result", result.ToString() }
                });
            }
        });
    }

    // --- flow ---

    private void GoToMenu()
    {
        TransitionToState(GameState.Menu, 1f);

        continuesUsed = 0;
        _adInProgress = false;
        _interstitialInProgress = false;
        _interstitialShownThisRun = false;
        _gameTimerEnabled = false;
        _rewardedRunPromptShownThisRun = false;
        _rewardedRunPromptActive = false;

        // We WANT the background to fly, so keep movement running
        playerController?.StartRun();
        speedController?.ResetForNewRun();
        speedController?.StartRun();

        // But hide and make non-collidable
        SetPlayerVisible(false);
        SetPlayerCollidable(false);

        mainMenuUI?.Show();
        gameOverUI?.Hide();
        hudController?.Hide();
        pauseMenuUI?.Hide();
        rewardedRunPromptUI?.Hide();
    }

    private void StartNewRunFromMenu()
    {
        continuesUsed = 0;
        StartNewRun();
    }

    private void StartNewRunFromGameOver()
    {
        continuesUsed = 0;
        StartNewRun();
    }

    private void StartNewRunWithFade(System.Action startAction)
    {
        if (loadingScreenManager != null)
        {
            loadingScreenManager.PlayBlackFadeTransition(startAction);
            return;
        }

        startAction?.Invoke();
    }

    private void ReturnToMenuWithFade(System.Action menuAction)
    {
        if (loadingScreenManager != null)
        {
            loadingScreenManager.PlayBlackFadeTransition(menuAction);
            return;
        }

        menuAction?.Invoke();
    }

    private void ApplyRunUpgrades()
    {
        if (playerProfile == null)
            return;

        int comboLevel = playerProfile.GetUpgradeLevel(UpgradeType.ComboMultiplier);
        int pickupLevel = playerProfile.GetUpgradeLevel(UpgradeType.PickupRadius);
        int shieldLevel = playerProfile.GetUpgradeLevel(UpgradeType.ShieldRecharge);

        float comboBase = balanceConfig != null
            ? balanceConfig.comboToMultiplierFactor
            : (scoreManager != null ? scoreManager.ComboToMultiplierFactor : 0f);
        float comboIncrement = balanceConfig != null ? balanceConfig.comboMultiplierFactorPerLevel : 0f;
        float comboFactor = Mathf.Max(0f, comboBase + comboIncrement * comboLevel);
        scoreManager?.SetComboMultiplierFactor(comboFactor);

        float pickupIncrement = balanceConfig != null ? balanceConfig.pickupRadiusMultiplierPerLevel : 0f;
        float pickupMultiplier = Mathf.Max(0f, 1f + pickupIncrement * pickupLevel);
        obstacleRingGenerator?.SetPickupRadiusMultiplier(pickupMultiplier);

        float shieldBase = balanceConfig != null
            ? balanceConfig.shieldRechargeSeconds
            : (powerupController != null ? powerupController.ShieldRechargeSeconds : 0f);
        float shieldIncrement = balanceConfig != null ? balanceConfig.shieldRechargeSecondsPerLevel : 0f;
        float shieldRecharge = Mathf.Max(0f, shieldBase + shieldIncrement * shieldLevel);
        powerupController?.SetShieldRechargeSeconds(shieldRecharge);
        powerupController?.ResetShieldRechargeCooldown();
    }

    private void StartNewRun()
    {
        TransitionToState(GameState.Playing, 1f);

        _elapsedTime = 0;
        _gameTimerEnabled = false;
        _runRewardsGranted = false;
        _runRewardsDoubled = false;
        _doubleRunRewardsQueued = false;
        _lastRunRewards = default;
        _interstitialShownThisRun = false;
        _rewardedRunPromptShownThisRun = false;
        _rewardedRunPromptActive = false;
        _rewardedRunPromptPausedRun = false;

        // Show and enable the player now we're playing
        SetPlayerVisible(true);
        SetPlayerCollidable(true);

        mainMenuUI?.Hide();
        gameOverUI?.Hide();
        pauseMenuUI?.Hide();
        hudController?.Show();
        rewardedRunPromptUI?.Hide();

        playerHealth?.ResetHealth();
        playerVisual.SetVisible(true);
        scoreManager?.ResetRun();
        currencyManager?.ResetRun();
        speedController?.ResetForNewRun();
        statsTracker?.ResetRunStats();
        runZoneManager?.OnResetRun();
        ApplyRunUpgrades();

        LogAnalyticsEvent(RunEventName, new Dictionary<string, object>
        {
            { "phase", "start" }
        });

        obstacleRingGenerator.DissolveNextRings(startClearRings, dissolveDuration);
        playerController?.RefreshHandlingFromProfile();
        playerController?.StartRun();

        countdownUIController.BeginCountdown(3, OnStartCountdownComplete);
    }

    private void OnStartCountdownComplete()
    {
        _gameTimerEnabled = true;
        scoreManager?.StartRun();
        speedController?.StartRun();
        statsTracker?.StartRun();
        runZoneManager?.StartRun();
    }

    public void ContinueRun()
    {
        TransitionToState(GameState.Playing, 1f);
        _gameTimerEnabled = false;

        obstacleRingGenerator.DissolveNextRings(startClearRings, dissolveDuration);
        playerController?.RefreshHandlingFromProfile();
        playerController?.StartRun();
        playerVisual.SetVisible(true);

        countdownUIController.BeginCountdown(3, OnContinueCountdownComplete);
    }

    private void OnContinueCountdownComplete()
    {
        _gameTimerEnabled = true;
        scoreManager?.ResumeAfterContinue();
        speedController?.ResumeAfterContinue();
        statsTracker?.ResumeRun();
        runZoneManager?.StartRun();
    }

    private void PauseGame()
    {
        TransitionToState(GameState.Paused, 0f);

        _gameTimerEnabled = false;

        pauseMenuUI?.Show();
        hudController?.Show();

        scoreManager?.StopRun();
        speedController?.StopRun();
        playerController?.StopRun();
        statsTracker?.PauseRun();
    }

    private void ResumeGame()
    {
        TransitionToState(GameState.Playing, 1f);

        pauseMenuUI?.Hide();
        hudController?.Show();

        ResumePausedRun();
    }

    private void ResumePausedRun()
    {
        playerController?.StartRun();
        scoreManager?.StartRun();
        speedController?.StartRun();
        runZoneManager?.StartRun();

        _gameTimerEnabled = true;
    }

    private void HandlePlayerDeath()
    {
        // Ignore deaths unless we're actually playing
        if (_stateMachine.CurrentState != GameState.Playing)
            return;

        if (_rewardedRunPromptActive)
        {
            HideRewardedRunPrompt();
        }

        if (playerController != null)
        {
            Transform t = playerController.transform;
            _lastDeathPosition = t.position;
            _lastDeathForward = t.forward;
        }

        _gameTimerEnabled = false;
        TransitionToState(GameState.GameOver, 0.2f);
        playerController?.StopRun();
        playerVisual.SetVisible(false);

        LogAnalyticsEvent(RunEventName, new Dictionary<string, object>
        {
            { "phase", "end" },
            { "score", scoreManager != null ? scoreManager.CurrentScore : 0 },
            { "time", _elapsedTime },
            { "continues_used", continuesUsed }
        });
        statsTracker?.EndRun();

        ShowGameOverUI();
        if (continuesUsed >= maxContinuesPerRun)
        {
            TryShowInterstitial("run_end");
        }
    }

    private void EnsureRewardedRunPrompt()
    {
        if (rewardedRunPromptUI != null)
            return;

        RewardedRunPromptUI prefab = Resources.Load<RewardedRunPromptUI>("UI/RewardedRunPrompt");
        if (prefab == null)
        {
            Debug.LogWarning("GameManager: RewardedRunPrompt prefab not found at Resources/UI/RewardedRunPrompt.");
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("GameManager: No Canvas found for rewarded run prompt.");
            return;
        }

        rewardedRunPromptUI = Instantiate(prefab, canvas.transform);
        rewardedRunPromptUI.Hide();
    }

    private bool ShouldTriggerRewardedRunPrompt()
    {
        if (!rewardedRunPromptEnabled)
            return false;

        if (_rewardedRunPromptShownThisRun || _rewardedRunPromptActive)
            return false;

        if (_stateMachine.CurrentState != GameState.Playing)
            return false;

        if (!_gameTimerEnabled || rewardedRunPromptDelaySeconds <= 0f)
            return false;

        if (_elapsedTime < rewardedRunPromptDelaySeconds)
            return false;

        if (_adInProgress || _interstitialInProgress)
            return false;

        return rewardedRunPromptUI != null;
    }

    private void ShowRewardedRunPrompt()
    {
        _rewardedRunPromptShownThisRun = true;
        _rewardedRunPromptActive = true;

        if (rewardedRunPromptPausesGameplay)
            PauseRunForRewardedPrompt();

        bool adReady = _services?.RewardedAds != null && _services.RewardedAds.IsRewardedAdReady();
        string title = LocalizationService.Get("ui.rewarded_run_title", "Boost your run!");
        string body = LocalizationService.Get("ui.rewarded_run_body", "Watch a rewarded ad to claim a bonus.");
        string reward = GetRewardedRunRewardLabel();
        string acceptLabel = LocalizationService.Get("ui.rewarded_run_accept", "Watch Ad");
        string declineLabel = LocalizationService.Get("ui.rewarded_run_decline", "No Thanks");

        rewardedRunPromptUI.Show(
            title,
            body,
            reward,
            acceptLabel,
            declineLabel,
            HandleRewardedRunAccept,
            HandleRewardedRunDecline,
            adReady,
            rewardedRunPromptAutoDismissSeconds,
            HandleRewardedRunTimeout);
    }

    private void HandleRewardedRunAccept()
    {
        if (_services?.RewardedAds == null)
        {
            LogAnalyticsEvent("ad_not_ready", new Dictionary<string, object>
            {
                { "source", "rewarded_run" },
                { "reason", "service_missing" }
            });

            HideRewardedRunPrompt();
            ResumeRunAfterRewardedPrompt();
            return;
        }

        if (!_services.RewardedAds.IsRewardedAdReady())
        {
            LogAnalyticsEvent("ad_not_ready", new Dictionary<string, object>
            {
                { "source", "rewarded_run" }
            });

            HideRewardedRunPrompt();
            ResumeRunAfterRewardedPrompt();
            return;
        }

        _adInProgress = true;
        if (_rewardedRunPromptPausedRun)
            Time.timeScale = 1f;

        LogAnalyticsEvent("ad_shown", new Dictionary<string, object>
        {
            { "source", "rewarded_run" }
        });

        _services.RewardedAds.ShowRewardedAd(result =>
        {
            _adInProgress = false;

            if (result == RewardedAdResult.Rewarded)
            {
                LogAnalyticsEvent("ad_completed", new Dictionary<string, object>
                {
                    { "source", "rewarded_run" }
                });

                GrantRewardedRunReward();
            }
            else
            {
                LogAnalyticsEvent("ad_skipped", new Dictionary<string, object>
                {
                    { "source", "rewarded_run" },
                    { "result", result.ToString() }
                });
            }

            HideRewardedRunPrompt();
            ResumeRunAfterRewardedPrompt();
        });
    }

    private void HandleRewardedRunDecline()
    {
        LogAnalyticsEvent("ad_skipped", new Dictionary<string, object>
        {
            { "source", "rewarded_run" },
            { "result", "declined" }
        });

        HideRewardedRunPrompt();
        ResumeRunAfterRewardedPrompt();
    }

    private void HandleRewardedRunTimeout()
    {
        LogAnalyticsEvent("ad_skipped", new Dictionary<string, object>
        {
            { "source", "rewarded_run" },
            { "result", "timeout" }
        });

        HideRewardedRunPrompt();
        ResumeRunAfterRewardedPrompt();
    }

    private void GrantRewardedRunReward()
    {
        GrantDoubleRunRewards();
    }

    private string GetRewardedRunRewardLabel()
    {
        return LocalizationService.Get(
            "ui.rewarded_run_reward_double",
            "Reward: Double run rewards");
    }

    private void PauseRunForRewardedPrompt()
    {
        _rewardedRunPromptPausedRun = true;
        _rewardedRunPromptPrevTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        _gameTimerEnabled = false;

        scoreManager?.StopRun();
        speedController?.StopRun();
        playerController?.StopRun();
        statsTracker?.PauseRun();
    }

    private void ResumeRunAfterRewardedPrompt()
    {
        if (!_rewardedRunPromptPausedRun)
            return;

        Time.timeScale = _rewardedRunPromptPrevTimeScale <= 0f ? 1f : _rewardedRunPromptPrevTimeScale;
        _gameTimerEnabled = true;

        scoreManager?.StartRun();
        speedController?.StartRun();
        playerController?.StartRun();
        runZoneManager?.StartRun();
        statsTracker?.ResumeRun();

        _rewardedRunPromptPausedRun = false;
    }

    private void HideRewardedRunPrompt()
    {
        _rewardedRunPromptActive = false;
        rewardedRunPromptUI?.Hide();
    }

    private void ShowGameOverUI()
    {
        if (gameOverUI == null || scoreManager == null)
            return;

        AwardRunRewardsOnce();

        float finalScore = scoreManager.CurrentScore;
        float bestScore = scoreManager.BestScore;
        int remainingContinues = Mathf.Max(0, maxContinuesPerRun - continuesUsed);

        gameOverUI.Show(finalScore, bestScore, _elapsedTime, continuesUsed, remainingContinues, maxContinuesPerRun);

        mainMenuUI?.Hide();
        hudController?.Hide();
        pauseMenuUI?.Hide();
    }

    private void HandleContinueAdResult(RewardedAdResult result)
    {
        _adInProgress = false;

        if (result != RewardedAdResult.Rewarded)
        {
            if (logStateChanges)
                Debug.Log($"GameManager: Rewarded ad unavailable or skipped ({result}). No continue.");

            LogAnalyticsEvent("ad_skipped", new Dictionary<string, object>
            {
                { "source", "continue" },
                { "continue_index", continuesUsed + 1 }
            });

            return;
        }

        LogAnalyticsEvent("ad_completed", new Dictionary<string, object>
        {
            { "source", "continue" },
            { "continue_index", continuesUsed + 1 }
        });

        continuesUsed++;
        if (continuesUsed > maxContinuesPerRun)
        {
            continuesUsed = maxContinuesPerRun;
        }

        LogAnalyticsEvent("continue_used", new Dictionary<string, object>
        {
            { "index", continuesUsed }
        });

        if (logStateChanges)
        {
            Debug.Log($"GameManager: Continue granted. ContinuesUsed={continuesUsed}/{maxContinuesPerRun}");
        }

        PerformContinueRespawn();
    }

    private void PerformContinueRespawn()
    {
        if (playerController != null)
        {
            Transform pt = playerController.transform;

            Vector3 backOffset = _lastDeathForward.normalized * (-continueRespawnBackDistance);
            Vector3 newPos = _lastDeathPosition + backOffset;
            newPos.y += continueRespawnHeightOffset;

            pt.position = newPos;
            pt.rotation = Quaternion.LookRotation(_lastDeathForward, Vector3.up);

            if (continueRespawnVfxPrefab != null)
            {
                var vfx = _services?.Vfx;
                if (vfx != null)
                {
                    vfx.Spawn(continueRespawnVfxPrefab, pt.position, Quaternion.identity);
                }
                else
                {
                    Object.Instantiate(continueRespawnVfxPrefab, pt.position, Quaternion.identity);
                }
            }
        }

        playerHealth?.ResetHealth();

        gameOverUI?.Hide();
        pauseMenuUI?.Hide();
        hudController?.Show();
        mainMenuUI?.Hide();

        ContinueRun();
    }

    private void SetPlayerVisible(bool visible)
    {
        if (playerVisual != null)
        {
            playerVisual.SetVisible(visible);
        }
    }

    private void SetPlayerCollidable(bool enabled)
    {
        if (playerCollider != null)
        {
            playerCollider.enabled = enabled;
        }
    }

    private void AwardRunRewardsOnce()
    {
        if (_runRewardsGranted)
            return;

        _runRewardsGranted = true;
        _lastRunRewards = CalculateRunRewards();
        ApplyRunRewards(_lastRunRewards, 1, false);

        if (_doubleRunRewardsQueued)
        {
            GrantDoubleRunRewards();
        }
    }

    private void ApplyRunRewards(RunRewardBundle rewards, int multiplier, bool bypassCooldown)
    {
        if (multiplier <= 0)
            return;

        if (!bypassCooldown
            && runRewardGrantCooldownSeconds > 0f
            && Time.unscaledTime - _lastRunRewardGrantTime < runRewardGrantCooldownSeconds)
            return;

        _lastRunRewardGrantTime = Time.unscaledTime;

        if (playerProfile == null)
            return;

        int coins = rewards.coins * multiplier;
        int gems = rewards.gems * multiplier;
        int xp = rewards.xp * multiplier;

        if (coins > 0)
            playerProfile.AddCurrencyAndSave(ShopCurrencyType.Soft, coins);

        if (gems > 0)
            playerProfile.AddCurrencyAndSave(ShopCurrencyType.Premium, gems);

        if (xp > 0)
            playerProfile.AddXp(xp);
    }

    private RunRewardBundle CalculateRunRewards()
    {
        int coins = currencyManager != null ? currencyManager.CurrentCoins : 0;
        int gems = gemsPerCoins > 0 ? coins / gemsPerCoins : 0;
        int xp = scoreManager != null ? Mathf.RoundToInt(scoreManager.CurrentScore * xpPerScorePoint) : 0;

        return new RunRewardBundle(coins, gems, xp);
    }

    private void GrantDoubleRunRewards()
    {
        if (_runRewardsDoubled)
            return;

        if (!_runRewardsGranted)
        {
            _doubleRunRewardsQueued = true;
            return;
        }

        _runRewardsDoubled = true;
        _doubleRunRewardsQueued = false;
        ApplyRunRewards(_lastRunRewards, 1, true);
    }

    private readonly struct RunRewardBundle
    {
        public RunRewardBundle(int coins, int gems, int xp)
        {
            this.coins = coins;
            this.gems = gems;
            this.xp = xp;
        }

        public int coins { get; }
        public int gems { get; }
        public int xp { get; }
    }

    private sealed class GameStateMachine
    {
        public GameState CurrentState { get; private set; }
        private readonly bool _logStateChanges;

        public GameStateMachine(bool logStateChanges)
        {
            _logStateChanges = logStateChanges;
        }

        public bool SetState(GameState newState)
        {
            if (!IsValidTransition(CurrentState, newState))
            {
                if (_logStateChanges)
                {
                    Debug.LogWarning($"GameManager: Invalid state transition {CurrentState} -> {newState}");
                }

                return false;
            }

            CurrentState = newState;

            if (_logStateChanges)
            {
                Debug.Log($"GameManager: State -> {CurrentState}");
            }

            return true;
        }

        private bool IsValidTransition(GameState from, GameState to)
        {
            if (from == to)
                return true;

            switch (from)
            {
                case GameState.Menu:
                    return to == GameState.Playing;
                case GameState.Playing:
                    return to == GameState.Paused || to == GameState.GameOver || to == GameState.Menu;
                case GameState.Paused:
                    return to == GameState.Playing || to == GameState.Menu;
                case GameState.GameOver:
                    return to == GameState.Playing || to == GameState.Menu;
                default:
                    return false;
            }
        }
    }

    private sealed class GameServicesFacade
    {
        public AudioManager Audio { get; }
        public VfxManager Vfx { get; }
        public IRewardedAdService RewardedAds { get; }
        public IInterstitialAdService InterstitialAds { get; }
        public IAnalyticsService Analytics { get; }

        public GameServicesFacade(AudioManager audio, VfxManager vfx, IRewardedAdService rewardedAds, IInterstitialAdService interstitialAds, IAnalyticsService analytics)
        {
            Audio = audio;
            Vfx = vfx;
            RewardedAds = rewardedAds;
            InterstitialAds = interstitialAds;
            Analytics = analytics;
        }
    }

    private void TryShowInterstitial(string source, System.Action onCompleted = null)
    {
        if (_interstitialInProgress)
        {
            onCompleted?.Invoke();
            return;
        }

        if (_adInProgress)
        {
            onCompleted?.Invoke();
            return;
        }

        if (_interstitialShownThisRun)
        {
            onCompleted?.Invoke();
            return;
        }

        if (AdsConfig.RemoveAds)
        {
            if (logStateChanges)
            {
                Debug.Log("GameManager: Remove Ads active, skipping interstitial.");
            }

            LogAnalyticsEvent("ad_bypassed", new Dictionary<string, object>
            {
                { "source", source },
                { "reason", "remove_ads" },
                { "ad_type", "interstitial" }
            });

            onCompleted?.Invoke();
            return;
        }

        if (!AdsConfig.InterstitialsEnabled)
        {
            if (logStateChanges)
            {
                Debug.Log("GameManager: Interstitials disabled, skipping interstitial.");
            }

            LogAnalyticsEvent("ad_bypassed", new Dictionary<string, object>
            {
                { "source", source },
                { "reason", "interstitials_disabled" },
                { "ad_type", "interstitial" }
            });

            onCompleted?.Invoke();
            return;
        }

        if (_services?.InterstitialAds == null)
        {
            if (logStateChanges)
            {
                Debug.LogWarning("GameManager: No IInterstitialAdService assigned. Cannot show interstitial.");
            }

            LogAnalyticsEvent("ad_not_ready", new Dictionary<string, object>
            {
                { "source", source },
                { "reason", "service_missing" },
                { "ad_type", "interstitial" }
            });

            onCompleted?.Invoke();
            return;
        }

        if (!_services.InterstitialAds.IsInterstitialAdReady())
        {
            if (logStateChanges)
            {
                Debug.Log("GameManager: Interstitial ad not ready.");
            }

            LogAnalyticsEvent("ad_not_ready", new Dictionary<string, object>
            {
                { "source", source },
                { "ad_type", "interstitial" }
            });

            onCompleted?.Invoke();
            return;
        }

        _interstitialInProgress = true;
        _interstitialShownThisRun = true;

        if (logStateChanges)
        {
            Debug.Log("GameManager: Showing interstitial ad.");
        }

        LogAnalyticsEvent("ad_shown", new Dictionary<string, object>
        {
            { "source", source },
            { "ad_type", "interstitial" }
        });

        _services.InterstitialAds.ShowInterstitialAd(result =>
        {
            _interstitialInProgress = false;

            if (result == InterstitialAdResult.Completed)
            {
                LogAnalyticsEvent("ad_completed", new Dictionary<string, object>
                {
                    { "source", source },
                    { "ad_type", "interstitial" }
                });
            }
            else
            {
                LogAnalyticsEvent("ad_skipped", new Dictionary<string, object>
                {
                    { "source", source },
                    { "ad_type", "interstitial" },
                    { "result", result.ToString() }
                });
            }

            onCompleted?.Invoke();
        });
    }

    // --- Analytics ---

    public void LogAnalyticsEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        if (_services?.Analytics == null)
            return;

        if (parameters == null)
        {
            _services.Analytics.LogEvent(eventName);
        }
        else
        {
            _services.Analytics.LogEvent(eventName, parameters);
        }
    }
}
