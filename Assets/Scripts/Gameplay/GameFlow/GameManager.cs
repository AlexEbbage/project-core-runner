using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
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
    [SerializeField] private BoxCollider[] playerColliders;

    [Header("References - UI (GameManager-driven)")]
    // Run UI controller stack removed; use HUD/GameOver/Pause components wired here.
    [SerializeField] private MainMenuUI mainMenuUI;
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private HudController hudController;
    [SerializeField] private PauseMenuUI pauseMenuUI;
    [SerializeField] private CountdownUIController countdownUIController;
    [SerializeField] private LoadingScreenManager loadingScreenManager;
    [SerializeField] private RewardedRunPromptUI rewardedRunPromptUI;
    [SerializeField] private RewardedOfferConfig rewardedOfferConfig;

    [Header("Rewarded Run Prompt")]
    [SerializeField] private bool rewardedRunPromptEnabled = false;
    [SerializeField] private float rewardedRunPromptDelaySeconds = 45f;
    [SerializeField] private float rewardedRunPromptIntervalSeconds = 45f;
    [SerializeField] private float rewardedRunPromptCooldownSeconds = 20f;
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
    [SerializeField] private float preRunCountdownSeconds = 3f;
    [SerializeField] private float preRunClearOffset = 2f;
    [SerializeField] private float dissolveDuration = 0.4f;

    [Header("Game Over")]
    [SerializeField] private float continueUnlockDelaySeconds = 2.5f;

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
    private float _nextRewardedOfferCheckTime;
    private float _rewardedOfferCooldownUntilTime;
    private RewardedOfferRuntimeData _pendingRewardedOffer;
    private GameStateMachine _stateMachine;
    private GameServicesFacade _services;
    private bool _runRewardsGranted;
    private bool _runRewardsDoubled;
    private bool _doubleRunRewardsQueued;
    private float _lastRunRewardGrantTime = float.NegativeInfinity;
    private RunRewardBundle _lastRunRewards;
    private float _gameOverContinueAvailableAt;
    private bool _gameOverContinueDelayLogged;
    private GameOverPresentationData _lastGameOverPresentation;

    private Vector3 _lastDeathPosition;
    private Vector3 _lastDeathForward;

    private sealed class RewardedOfferRuntimeData
    {
        public RewardedOfferRewardKind rewardKind;
        public PowerupType powerupType;
        public ShopCurrencyType currencyType;
        public int amount;
        public string title;
        public string body;
        public string rewardLabel;
    }

    public readonly struct GameOverPresentationData
    {
        public GameOverPresentationData(
            float finalScore,
            float bestScore,
            float elapsedTime,
            float distance,
            int coinsCollected,
            float comboModifier,
            int baseRewardCoins,
            int baseRewardPremiumCurrency,
            int baseRewardXp,
            int continuesUsed,
            int continuesRemaining,
            int maxContinues,
            bool canContinue,
            bool canDoubleRewards,
            float continueUnlockDelaySeconds)
        {
            this.finalScore = finalScore;
            this.bestScore = bestScore;
            this.elapsedTime = elapsedTime;
            this.distance = distance;
            this.coinsCollected = coinsCollected;
            this.comboModifier = comboModifier;
            this.baseRewardCoins = baseRewardCoins;
            this.baseRewardPremiumCurrency = baseRewardPremiumCurrency;
            this.baseRewardXp = baseRewardXp;
            this.continuesUsed = continuesUsed;
            this.continuesRemaining = continuesRemaining;
            this.maxContinues = maxContinues;
            this.canContinue = canContinue;
            this.canDoubleRewards = canDoubleRewards;
            this.continueUnlockDelaySeconds = continueUnlockDelaySeconds;
        }

        public float finalScore { get; }
        public float bestScore { get; }
        public float elapsedTime { get; }
        public float distance { get; }
        public int coinsCollected { get; }
        public float comboModifier { get; }
        public int baseRewardCoins { get; }
        public int baseRewardPremiumCurrency { get; }
        public int baseRewardXp { get; }
        public int continuesUsed { get; }
        public int continuesRemaining { get; }
        public int maxContinues { get; }
        public bool canContinue { get; }
        public bool canDoubleRewards { get; }
        public float continueUnlockDelaySeconds { get; }
    }

    public GameState CurrentState => _stateMachine != null ? _stateMachine.CurrentState : GameState.Menu;

    public int ContinuesUsed => continuesUsed;

    public float GetElapsedGameTime => _elapsedTime;

    public int MaxContinuesPerRun => maxContinuesPerRun;

    public bool GameTimerEnabled => _gameTimerEnabled;

    public bool CanDoubleRunRewards => _runRewardsGranted && !_runRewardsDoubled && !_adInProgress;

    public bool IsContinueUnlocked => Time.unscaledTime >= _gameOverContinueAvailableAt;

    public GameOverPresentationData LastGameOverPresentation => _lastGameOverPresentation;

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

        if (rewardedOfferConfig == null)
        {
            var rewardedOfferConfigs = Resources.FindObjectsOfTypeAll<RewardedOfferConfig>();
            if (rewardedOfferConfigs != null && rewardedOfferConfigs.Length > 0)
            {
                rewardedOfferConfig = rewardedOfferConfigs[0];
            }
        }

        if (powerupController != null && !powerupController.enabled)
        {
            powerupController.enabled = true;
        }

        if (balanceConfig != null)
        {
            maxContinuesPerRun = balanceConfig.maxContinuesPerRun;
            continueRespawnBackDistance = balanceConfig.continueRespawnBackDistance;
            continueRespawnHeightOffset = balanceConfig.continueRespawnHeightOffset;
        }


        if (playerVisual == null && playerController != null)
        {
            playerVisual = playerController.GetComponentInChildren<PlayerVisual>();
        }

        if (playerColliders == null && playerController != null)
        {
            playerColliders = playerController.GetComponents<BoxCollider>();
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
        LogAnalyticsEvent(AnalyticsEventNames.GameOverMenuPressed, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Source, "game_over" }
        });
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

        if (!IsContinueUnlocked)
        {
            if (logStateChanges)
                Debug.Log("GameManager: Continue pressed before delay completed.");
            return;
        }

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

            LogAnalyticsEvent(AnalyticsEventNames.AdBypassed, new Dictionary<string, object>
            {
                { AnalyticsEventNames.Params.Source, "continue" },
                { AnalyticsEventNames.Params.Reason, "remove_ads" },
                { AnalyticsEventNames.Params.AdType, "rewarded" }
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

            LogAnalyticsEvent(AnalyticsEventNames.AdNotReady, new Dictionary<string, object>
            {
                { AnalyticsEventNames.Params.Source, "continue" },
                { AnalyticsEventNames.Params.Reason, "service_missing" },
                { AnalyticsEventNames.Params.AdType, "rewarded" }
            });

            return;
        }

        if (!_services.RewardedAds.IsRewardedAdReady())
        {
            if (logStateChanges)
            {
                Debug.Log("GameManager: Rewarded ad not ready.");
            }

            LogAnalyticsEvent(AnalyticsEventNames.AdNotReady, new Dictionary<string, object>
            {
                { AnalyticsEventNames.Params.Source, "continue" },
                { AnalyticsEventNames.Params.AdType, "rewarded" }
            });

            return;
        }

        LogAnalyticsEvent(AnalyticsEventNames.GameOverContinuePressed, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Source, "game_over" },
            { AnalyticsEventNames.Params.ContinueIndex, continuesUsed + 1 }
        });

        _adInProgress = true;
        Time.timeScale = 1f;

        if (logStateChanges)
        {
            Debug.Log("GameManager: Showing rewarded ad for continue.");
        }

        LogAnalyticsEvent(AnalyticsEventNames.AdShown, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Source, "continue" },
            { AnalyticsEventNames.Params.ContinueIndex, continuesUsed + 1 },
            { AnalyticsEventNames.Params.AdType, "rewarded" }
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

            LogAnalyticsEvent(AnalyticsEventNames.AdBypassed, new Dictionary<string, object>
            {
                { AnalyticsEventNames.Params.Source, "double_rewards" },
                { AnalyticsEventNames.Params.Reason, "remove_ads" },
                { AnalyticsEventNames.Params.AdType, "rewarded" }
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

        LogAnalyticsEvent(AnalyticsEventNames.GameOverDoubleRewardsStarted, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Source, "game_over" }
        });

        LogAnalyticsEvent(AnalyticsEventNames.AdShown, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Source, "double_rewards" },
            { AnalyticsEventNames.Params.AdType, "rewarded" }
        });

        _services.RewardedAds.ShowRewardedAd(result =>
        {
            _adInProgress = false;

            if (result == RewardedAdResult.Rewarded)
            {
                LogAnalyticsEvent(AnalyticsEventNames.GameOverDoubleRewardsRewarded, new Dictionary<string, object>
                {
                    { AnalyticsEventNames.Params.Source, "game_over" }
                });

                LogAnalyticsEvent(AnalyticsEventNames.AdCompleted, new Dictionary<string, object>
                {
                    { AnalyticsEventNames.Params.Source, "double_rewards" },
                    { AnalyticsEventNames.Params.AdType, "rewarded" }
                });

                GrantDoubleRunRewards();
            }
            else
            {
                LogAnalyticsEvent(AnalyticsEventNames.GameOverDoubleRewardsFailed, new Dictionary<string, object>
                {
                    { AnalyticsEventNames.Params.Source, "game_over" },
                    { AnalyticsEventNames.Params.Result, result.ToString() }
                });

                LogAnalyticsEvent(AnalyticsEventNames.AdSkipped, new Dictionary<string, object>
                {
                    { AnalyticsEventNames.Params.Source, "double_rewards" },
                    { AnalyticsEventNames.Params.Result, result.ToString() },
                    { AnalyticsEventNames.Params.AdType, "rewarded" }
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
        _pendingRewardedOffer = null;
        _nextRewardedOfferCheckTime = 0f;
        _rewardedOfferCooldownUntilTime = 0f;
        _gameOverContinueAvailableAt = 0f;
        _gameOverContinueDelayLogged = false;
        _lastGameOverPresentation = default;
        _lastRunRewardGrantTime = float.NegativeInfinity;

        // We WANT the background to fly, so keep movement running
        playerController?.StartRun();
        speedController?.ResetForNewRun();
        speedController?.StartRun();
        powerupController?.ResetAllPowerups();

        // But hide and make non-collidable
        SetPlayerVisible(false);
        SetPlayerCollidable(false);

        mainMenuUI?.Show();
        gameOverUI?.Hide();
        hudController?.Hide();
        pauseMenuUI?.Hide();
        rewardedRunPromptUI?.Hide();
        hudController?.HideRewardedOfferPopout(false);
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
        _pendingRewardedOffer = null;
        _nextRewardedOfferCheckTime = GetRewardedOfferFirstDelaySeconds();
        _rewardedOfferCooldownUntilTime = 0f;
        _gameOverContinueAvailableAt = 0f;
        _gameOverContinueDelayLogged = false;
        _lastGameOverPresentation = default;
        _lastRunRewardGrantTime = float.NegativeInfinity;

        // Show and enable the player now we're playing
        SetPlayerVisible(true);
        SetPlayerCollidable(true);

        mainMenuUI?.Hide();
        gameOverUI?.Hide();
        pauseMenuUI?.Hide();
        hudController?.Show();
        rewardedRunPromptUI?.Hide();
        hudController?.HideRewardedOfferPopout(false);

        playerHealth?.ResetHealth();
        playerVisual?.SetVisible(true);
        scoreManager?.ResetRun();
        currencyManager?.ResetRun();
        speedController?.ResetForNewRun();
        statsTracker?.ResetRunStats();
        runZoneManager?.OnResetRun();
        powerupController?.ResetAllPowerups();
        ApplyRunUpgrades();

        LogAnalyticsEvent(AnalyticsEventNames.RunStarted, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Phase, "start" }
        });

        float preRunClearDistance = GetPreRunClearDistance();
        obstacleRingGenerator.DissolveNextRings(preRunClearDistance, dissolveDuration);
        obstacleRingGenerator.ClearNextPickupRings(preRunClearDistance);
        playerController?.RefreshHandlingFromProfile();
        playerController?.StartRun();

        countdownUIController.BeginCountdown(Mathf.CeilToInt(preRunCountdownSeconds), OnStartCountdownComplete);
    }

    private void OnStartCountdownComplete()
    {
        _gameTimerEnabled = true;
        scoreManager?.StartRun();
        speedController?.StartRun();
        statsTracker?.StartRun();
        runZoneManager?.StartRun();
        obstacleRingGenerator?.StartRun();
    }

    public void ContinueRun()
    {
        TransitionToState(GameState.Playing, 1f);
        _gameTimerEnabled = false;
        powerupController?.ResetAllPowerups();
        ApplyRunUpgrades();
        _pendingRewardedOffer = null;
        _rewardedRunPromptActive = false;
        _rewardedRunPromptPausedRun = false;
        _rewardedOfferCooldownUntilTime = Mathf.Max(_rewardedOfferCooldownUntilTime, _elapsedTime + GetRewardedOfferCooldownSeconds());
        _gameOverContinueAvailableAt = 0f;
        _gameOverContinueDelayLogged = false;
        _lastGameOverPresentation = default;
        _lastRunRewardGrantTime = float.NegativeInfinity;
        rewardedRunPromptUI?.Hide();
        hudController?.HideRewardedOfferPopout(false);

        float preRunClearDistance = GetPreRunClearDistance();
        obstacleRingGenerator.DissolveNextRings(preRunClearDistance, dissolveDuration);
        obstacleRingGenerator.ClearNextPickupRings(preRunClearDistance);
        playerController?.RefreshHandlingFromProfile();
        playerController?.StartRun();
        playerVisual.SetVisible(true);

        countdownUIController.BeginCountdown(Mathf.CeilToInt(preRunCountdownSeconds), OnContinueCountdownComplete);
    }

    private float GetPreRunClearDistance()
    {
        float countdownDuration = Mathf.Max(preRunClearOffset, preRunCountdownSeconds + preRunClearOffset);
        float runSpeed = speedController != null ? Mathf.Max(0f, speedController.CurrentSpeed) : 0f;
        return countdownDuration * runSpeed;
    }

    private void OnContinueCountdownComplete()
    {
        _gameTimerEnabled = true;
        scoreManager?.ResumeAfterContinue();
        speedController?.ResumeAfterContinue();
        statsTracker?.ResumeRun();
        runZoneManager?.StartRun();
        obstacleRingGenerator?.StartRun();
    }

    private void PauseGame()
    {
        if (_rewardedRunPromptActive)
        {
            HideRewardedRunPrompt();
        }

        TransitionToState(GameState.Paused, 0f);

        _gameTimerEnabled = false;

        pauseMenuUI?.Show();
        hudController?.Show();

        scoreManager?.StopRun();
        speedController?.StopRun();
        playerController?.StopRun();
        obstacleRingGenerator?.StopRun();
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
        obstacleRingGenerator?.StartRun();

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

        hudController?.HideRewardedOfferPopout(false);
        _pendingRewardedOffer = null;

        if (playerController != null)
        {
            Transform t = playerController.transform;
            _lastDeathPosition = t.position;
            _lastDeathForward = t.forward;
        }

        _gameTimerEnabled = false;
        TransitionToState(GameState.GameOver, 0.2f);
        scoreManager?.StopRun();
        speedController?.StopRun();
        obstacleRingGenerator?.StopRun();
        playerController?.StopRun();
        playerVisual?.SetVisible(false);
        powerupController?.ResetAllPowerups();

        LogAnalyticsEvent(AnalyticsEventNames.RunEnded, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Phase, "end" },
            { AnalyticsEventNames.Params.Score, scoreManager != null ? scoreManager.CurrentScore : 0 },
            { AnalyticsEventNames.Params.Time, _elapsedTime },
            { AnalyticsEventNames.Params.ContinuesUsed, continuesUsed }
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
        if (!IsRewardedOfferFeatureEnabled())
            return false;

        if (_rewardedRunPromptActive || _pendingRewardedOffer != null)
            return false;

        if (_stateMachine.CurrentState != GameState.Playing)
            return false;

        if (!_gameTimerEnabled)
            return false;

        if (_elapsedTime < _nextRewardedOfferCheckTime)
            return false;

        if (_elapsedTime < _rewardedOfferCooldownUntilTime)
            return false;

        if (_adInProgress || _interstitialInProgress)
            return false;

        if (rewardedRunPromptUI == null || hudController == null)
            return false;

        if (_rewardedRunPromptPausedRun || hudController.IsRewardedOfferPopoutVisible)
            return false;

        return true;
    }

    private void ShowRewardedRunPrompt()
    {
        _pendingRewardedOffer = SelectRewardedOffer();
        if (_pendingRewardedOffer == null)
        {
            _nextRewardedOfferCheckTime = _elapsedTime + GetRewardedOfferIntervalSeconds();
            return;
        }

        _rewardedRunPromptActive = true;
        _rewardedRunPromptShownThisRun = true;
        _nextRewardedOfferCheckTime = _elapsedTime + GetRewardedOfferIntervalSeconds();

        hudController.ShowRewardedOfferPopout(
            _pendingRewardedOffer.title,
            _pendingRewardedOffer.rewardLabel,
            GetRewardedOfferPopoutLifetimeSeconds(),
            HandleRewardedOfferPopoutTapped,
            HandleRewardedRunDecline,
            HandleRewardedRunTimeout);

        LogAnalyticsEvent(AnalyticsEventNames.RewardedOfferShown, BuildRewardedOfferAnalyticsParams(_pendingRewardedOffer));
    }

    private void HandleRewardedOfferPopoutTapped()
    {
        if (_pendingRewardedOffer == null)
            return;

        LogAnalyticsEvent(AnalyticsEventNames.RewardedOfferTapped, BuildRewardedOfferAnalyticsParams(_pendingRewardedOffer));
        PauseRunForRewardedPrompt();
        ShowRewardedOfferModal();
    }

    private void ShowRewardedOfferModal()
    {
        if (_pendingRewardedOffer == null)
            return;

        bool adReady = _services?.RewardedAds != null && _services.RewardedAds.IsRewardedAdReady();
        rewardedRunPromptUI.Show(
            _pendingRewardedOffer.title,
            _pendingRewardedOffer.body,
            _pendingRewardedOffer.rewardLabel,
            LocalizationService.Get("ui.rewarded_run_accept", "Watch Ad"),
            LocalizationService.Get("ui.rewarded_run_decline", "Ignore"),
            HandleRewardedRunAccept,
            HandleRewardedRunDecline,
            adReady,
            0f,
            null);
    }

    private void HandleRewardedRunAccept()
    {
        if (_services?.RewardedAds == null)
        {
            LogAnalyticsEvent(AnalyticsEventNames.AdNotReady, new Dictionary<string, object>
            {
                { AnalyticsEventNames.Params.Source, "rewarded_run" },
                { AnalyticsEventNames.Params.Reason, "service_missing" },
                { AnalyticsEventNames.Params.AdType, "rewarded" }
            });

            HideRewardedRunPrompt();
            ResumeRunAfterRewardedPrompt();
            return;
        }

        if (!_services.RewardedAds.IsRewardedAdReady())
        {
            LogAnalyticsEvent(AnalyticsEventNames.AdNotReady, new Dictionary<string, object>
            {
                { AnalyticsEventNames.Params.Source, "rewarded_run" },
                { AnalyticsEventNames.Params.AdType, "rewarded" }
            });

            HideRewardedRunPrompt();
            ResumeRunAfterRewardedPrompt();
            return;
        }

        _adInProgress = true;
        if (_rewardedRunPromptPausedRun)
            Time.timeScale = 1f;

        LogAnalyticsEvent(AnalyticsEventNames.AdShown, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Source, "rewarded_offer" },
            { AnalyticsEventNames.Params.AdType, "rewarded" }
        });

        _services.RewardedAds.ShowRewardedAd(result =>
        {
            _adInProgress = false;

            if (result == RewardedAdResult.Rewarded)
            {
                LogAnalyticsEvent(AnalyticsEventNames.AdCompleted, new Dictionary<string, object>
                {
                    { AnalyticsEventNames.Params.Source, "rewarded_offer" },
                    { AnalyticsEventNames.Params.AdType, "rewarded" }
                });

                GrantRewardedRunReward();
            }
            else
            {
                LogAnalyticsEvent(AnalyticsEventNames.AdSkipped, new Dictionary<string, object>
                {
                    { AnalyticsEventNames.Params.Source, "rewarded_offer" },
                    { AnalyticsEventNames.Params.Result, result.ToString() },
                    { AnalyticsEventNames.Params.AdType, "rewarded" }
                });
            }

            HideRewardedRunPrompt();
            ResumeRunAfterRewardedPrompt();
        });
    }

    private void HandleRewardedRunDecline()
    {
        if (_pendingRewardedOffer != null)
            LogAnalyticsEvent(AnalyticsEventNames.RewardedOfferIgnored, BuildRewardedOfferAnalyticsParams(_pendingRewardedOffer));

        HideRewardedRunPrompt();
        ResumeRunAfterRewardedPrompt();
    }

    private void HandleRewardedRunTimeout()
    {
        if (_pendingRewardedOffer != null)
            LogAnalyticsEvent(AnalyticsEventNames.RewardedOfferTimedOut, BuildRewardedOfferAnalyticsParams(_pendingRewardedOffer));

        HideRewardedRunPrompt();
        ResumeRunAfterRewardedPrompt();
    }

    private void GrantRewardedRunReward()
    {
        if (_pendingRewardedOffer == null || playerProfile == null)
            return;

        switch (_pendingRewardedOffer.rewardKind)
        {
            case RewardedOfferRewardKind.Powerup:
                powerupController?.ActivatePowerup(_pendingRewardedOffer.powerupType);
                break;
            case RewardedOfferRewardKind.PremiumCurrency:
                playerProfile.AddCurrencyAndSave(ShopCurrencyType.Premium, _pendingRewardedOffer.amount);
                break;
            case RewardedOfferRewardKind.SoftCurrency:
            default:
                playerProfile.AddCurrencyAndSave(ShopCurrencyType.Soft, _pendingRewardedOffer.amount);
                break;
        }

        LogAnalyticsEvent(AnalyticsEventNames.RewardedOfferRewardGranted, BuildRewardedOfferAnalyticsParams(_pendingRewardedOffer));
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
        obstacleRingGenerator?.StopRun();
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
        obstacleRingGenerator?.StartRun();
        statsTracker?.ResumeRun();

        _rewardedRunPromptPausedRun = false;
    }

    private void HideRewardedRunPrompt()
    {
        _rewardedRunPromptActive = false;
        rewardedRunPromptUI?.Hide();
        hudController?.HideRewardedOfferPopout(false);
        _rewardedOfferCooldownUntilTime = Mathf.Max(_rewardedOfferCooldownUntilTime, _elapsedTime + GetRewardedOfferCooldownSeconds());
        _pendingRewardedOffer = null;
    }

    private bool IsRewardedOfferFeatureEnabled()
    {
        if (rewardedOfferConfig != null)
            return rewardedOfferConfig.enabled;

        return rewardedRunPromptEnabled;
    }

    private float GetRewardedOfferFirstDelaySeconds()
    {
        return rewardedOfferConfig != null
            ? Mathf.Max(0f, rewardedOfferConfig.firstOfferDelaySeconds)
            : Mathf.Max(0f, rewardedRunPromptDelaySeconds);
    }

    private float GetRewardedOfferIntervalSeconds()
    {
        return rewardedOfferConfig != null
            ? Mathf.Max(1f, rewardedOfferConfig.repeatIntervalSeconds)
            : Mathf.Max(1f, rewardedRunPromptIntervalSeconds);
    }

    private float GetRewardedOfferPopoutLifetimeSeconds()
    {
        return rewardedOfferConfig != null
            ? Mathf.Max(1f, rewardedOfferConfig.offerPopoutLifetimeSeconds)
            : Mathf.Max(1f, rewardedRunPromptAutoDismissSeconds);
    }

    private float GetRewardedOfferCooldownSeconds()
    {
        return rewardedOfferConfig != null
            ? Mathf.Max(0f, rewardedOfferConfig.offerCooldownSeconds)
            : Mathf.Max(0f, rewardedRunPromptCooldownSeconds);
    }

    private RewardedOfferRuntimeData SelectRewardedOffer()
    {
        RewardedOfferRewardEntry[] rewards = rewardedOfferConfig != null
            ? rewardedOfferConfig.GetResolvedRewards()
            : RewardedOfferConfig.GetDefaultRewards();

        if (rewards == null || rewards.Length == 0)
            return null;

        int totalWeight = 0;
        for (int i = 0; i < rewards.Length; i++)
        {
            RewardedOfferRewardEntry reward = rewards[i];
            if (!IsEligibleRewardEntry(reward))
                continue;

            totalWeight += Mathf.Max(0, reward.weight);
        }

        if (totalWeight <= 0)
            return null;

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;
        for (int i = 0; i < rewards.Length; i++)
        {
            RewardedOfferRewardEntry reward = rewards[i];
            if (!IsEligibleRewardEntry(reward))
                continue;

            cumulative += Mathf.Max(0, reward.weight);
            if (roll < cumulative)
            {
                return CreateRuntimeReward(reward);
            }
        }

        return null;
    }

    private static bool IsEligibleRewardEntry(RewardedOfferRewardEntry reward)
    {
        if (reward == null || reward.weight <= 0)
            return false;

        switch (reward.rewardKind)
        {
            case RewardedOfferRewardKind.Powerup:
                return PowerupUpgradeConfig.IsTargetGameplayPowerup(reward.powerupType);
            case RewardedOfferRewardKind.SoftCurrency:
            case RewardedOfferRewardKind.PremiumCurrency:
                return reward.amount > 0;
            default:
                return false;
        }
    }

    private static RewardedOfferRuntimeData CreateRuntimeReward(RewardedOfferRewardEntry reward)
    {
        if (reward == null)
            return null;

        return new RewardedOfferRuntimeData
        {
            rewardKind = reward.rewardKind,
            powerupType = reward.powerupType,
            currencyType = reward.rewardKind == RewardedOfferRewardKind.PremiumCurrency
                ? ShopCurrencyType.Premium
                : ShopCurrencyType.Soft,
            amount = Mathf.Max(0, reward.amount),
            title = reward.GetResolvedTitle(),
            body = reward.GetResolvedBody(),
            rewardLabel = reward.GetResolvedRewardLabel()
        };
    }

    private Dictionary<string, object> BuildRewardedOfferAnalyticsParams(RewardedOfferRuntimeData reward)
    {
        var parameters = new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Source, "rewarded_offer" }
        };

        if (reward == null)
            return parameters;

        parameters[AnalyticsEventNames.Params.RewardKind] = reward.rewardKind.ToString();

        switch (reward.rewardKind)
        {
            case RewardedOfferRewardKind.Powerup:
                parameters[AnalyticsEventNames.Params.Type] = reward.powerupType.ToString();
                break;
            case RewardedOfferRewardKind.PremiumCurrency:
            case RewardedOfferRewardKind.SoftCurrency:
                parameters[AnalyticsEventNames.Params.Type] = reward.currencyType.ToString();
                parameters[AnalyticsEventNames.Params.Amount] = reward.amount;
                break;
        }

        return parameters;
    }

    private GameOverPresentationData BuildGameOverPresentation()
    {
        float finalScore = scoreManager != null ? scoreManager.CurrentScore : 0f;
        float bestScore = scoreManager != null ? scoreManager.BestScore : 0f;
        float distance = statsTracker != null ? statsTracker.CurrentRunDistance : 0f;
        int coinsCollected = currencyManager != null ? currencyManager.CurrentCoins : 0;
        float comboModifier = scoreManager != null ? scoreManager.CurrentMultiplier : 1f;
        int remainingContinues = Mathf.Max(0, maxContinuesPerRun - continuesUsed);

        return new GameOverPresentationData(
            finalScore,
            bestScore,
            _elapsedTime,
            distance,
            coinsCollected,
            comboModifier,
            _lastRunRewards.coins,
            _lastRunRewards.gems,
            _lastRunRewards.xp,
            continuesUsed,
            remainingContinues,
            maxContinuesPerRun,
            remainingContinues > 0,
            _runRewardsGranted && !_runRewardsDoubled,
            continueUnlockDelaySeconds);
    }

    private Dictionary<string, object> BuildGameOverAnalyticsParams()
    {
        return new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Source, "game_over" },
            { AnalyticsEventNames.Params.Score, _lastGameOverPresentation.finalScore },
            { AnalyticsEventNames.Params.Distance, _lastGameOverPresentation.distance },
            { AnalyticsEventNames.Params.Coins, _lastGameOverPresentation.coinsCollected },
            { AnalyticsEventNames.Params.RewardCoins, _lastGameOverPresentation.baseRewardCoins },
            { AnalyticsEventNames.Params.RewardPremium, _lastGameOverPresentation.baseRewardPremiumCurrency },
            { AnalyticsEventNames.Params.RewardXp, _lastGameOverPresentation.baseRewardXp },
            { AnalyticsEventNames.Params.ComboModifier, _lastGameOverPresentation.comboModifier },
            { AnalyticsEventNames.Params.ContinuesRemaining, _lastGameOverPresentation.continuesRemaining }
        };
    }

    private void ShowGameOverUI()
    {
        if (gameOverUI == null || scoreManager == null)
            return;

        AwardRunRewardsOnce();
        _gameOverContinueAvailableAt = Time.unscaledTime + Mathf.Max(0f, continueUnlockDelaySeconds);
        _gameOverContinueDelayLogged = false;
        _lastGameOverPresentation = BuildGameOverPresentation();

        gameOverUI.Show(_lastGameOverPresentation);

        LogAnalyticsEvent(AnalyticsEventNames.GameOverShown, BuildGameOverAnalyticsParams());
        if (_lastGameOverPresentation.canDoubleRewards)
        {
            LogAnalyticsEvent(AnalyticsEventNames.GameOverDoubleRewardsOffered, new Dictionary<string, object>
            {
                { AnalyticsEventNames.Params.Source, "game_over" }
            });
        }

        mainMenuUI?.Hide();
        hudController?.Hide();
        pauseMenuUI?.Hide();
    }

    public void NotifyGameOverContinueDelayCompleted()
    {
        if (_gameOverContinueDelayLogged)
            return;

        _gameOverContinueDelayLogged = true;
        LogAnalyticsEvent(AnalyticsEventNames.GameOverContinueDelayCompleted, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Source, "game_over" }
        });
    }

    private void HandleContinueAdResult(RewardedAdResult result)
    {
        _adInProgress = false;

        if (result != RewardedAdResult.Rewarded)
        {
            if (logStateChanges)
                Debug.Log($"GameManager: Rewarded ad unavailable or skipped ({result}). No continue.");

            LogAnalyticsEvent(AnalyticsEventNames.AdSkipped, new Dictionary<string, object>
            {
                { AnalyticsEventNames.Params.Source, "continue" },
                { AnalyticsEventNames.Params.ContinueIndex, continuesUsed + 1 },
                { AnalyticsEventNames.Params.AdType, "rewarded" }
            });

            return;
        }

        LogAnalyticsEvent(AnalyticsEventNames.AdCompleted, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Source, "continue" },
            { AnalyticsEventNames.Params.ContinueIndex, continuesUsed + 1 },
            { AnalyticsEventNames.Params.AdType, "rewarded" }
        });

        continuesUsed++;
        if (continuesUsed > maxContinuesPerRun)
        {
            continuesUsed = maxContinuesPerRun;
        }

        LogAnalyticsEvent(AnalyticsEventNames.ContinueUsed, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.ContinueIndex, continuesUsed },
            { AnalyticsEventNames.Params.Source, "game_over" }
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
        if (playerColliders != null && playerColliders.Length > 0)
        {
            foreach (var collider in playerColliders)
            {
                collider.enabled = enabled;
            }
        }
    }

    private void AwardRunRewardsOnce()
    {
        if (_runRewardsGranted)
            return;

        _runRewardsGranted = true;
        _lastRunRewards = CalculateRunRewards();
        ApplyRunRewards(_lastRunRewards, 1, false);
        _lastGameOverPresentation = BuildGameOverPresentation();

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
        _lastGameOverPresentation = BuildGameOverPresentation();

        LogAnalyticsEvent(AnalyticsEventNames.GameOverDoubleRewardsBonusGranted, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Source, "game_over" },
            { AnalyticsEventNames.Params.Amount, _lastRunRewards.coins + _lastRunRewards.gems + _lastRunRewards.xp }
        });
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

            LogAnalyticsEvent(AnalyticsEventNames.AdBypassed, new Dictionary<string, object>
            {
                { AnalyticsEventNames.Params.Source, source },
                { AnalyticsEventNames.Params.Reason, "remove_ads" },
                { AnalyticsEventNames.Params.AdType, "interstitial" }
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

            LogAnalyticsEvent(AnalyticsEventNames.AdBypassed, new Dictionary<string, object>
            {
                { AnalyticsEventNames.Params.Source, source },
                { AnalyticsEventNames.Params.Reason, "interstitials_disabled" },
                { AnalyticsEventNames.Params.AdType, "interstitial" }
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

            LogAnalyticsEvent(AnalyticsEventNames.AdNotReady, new Dictionary<string, object>
            {
                { AnalyticsEventNames.Params.Source, source },
                { AnalyticsEventNames.Params.Reason, "service_missing" },
                { AnalyticsEventNames.Params.AdType, "interstitial" }
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

            LogAnalyticsEvent(AnalyticsEventNames.AdNotReady, new Dictionary<string, object>
            {
                { AnalyticsEventNames.Params.Source, source },
                { AnalyticsEventNames.Params.AdType, "interstitial" }
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

        LogAnalyticsEvent(AnalyticsEventNames.AdShown, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Source, source },
            { AnalyticsEventNames.Params.AdType, "interstitial" }
        });

        _services.InterstitialAds.ShowInterstitialAd(result =>
        {
            _interstitialInProgress = false;

            if (result == InterstitialAdResult.Completed)
            {
                LogAnalyticsEvent(AnalyticsEventNames.AdCompleted, new Dictionary<string, object>
                {
                    { AnalyticsEventNames.Params.Source, source },
                    { AnalyticsEventNames.Params.AdType, "interstitial" }
                });
            }
            else
            {
                LogAnalyticsEvent(AnalyticsEventNames.AdSkipped, new Dictionary<string, object>
                {
                    { AnalyticsEventNames.Params.Source, source },
                    { AnalyticsEventNames.Params.AdType, "interstitial" },
                    { AnalyticsEventNames.Params.Result, result.ToString() }
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
