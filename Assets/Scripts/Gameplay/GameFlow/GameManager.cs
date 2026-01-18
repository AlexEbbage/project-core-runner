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

    [Header("References - Services")]
    [SerializeField] private MonoBehaviour rewardedAdServiceBehaviour;
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
    private IAnalyticsService _analytics;
    private IPushNotificationService _pushNotifications;
    private bool _adInProgress;
    private float _elapsedTime;
    private bool _gameTimerEnabled;
    private GameStateMachine _stateMachine;
    private GameServicesFacade _services;
    private bool _runCurrencyAwarded;

    private Vector3 _lastDeathPosition;
    private Vector3 _lastDeathForward;

    public GameState CurrentState => _stateMachine != null ? _stateMachine.CurrentState : GameState.Menu;

    public int ContinuesUsed => continuesUsed;

    public float GetElapsedGameTime => _elapsedTime;

    public int MaxContinuesPerRun => maxContinuesPerRun;

    public bool GameTimerEnabled => _gameTimerEnabled;

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

        _services = new GameServicesFacade(audioManager, vfxManager, _rewardedAdService, _analytics);
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
        AwardRunCurrencyOnce();
        StartNewRunWithFade(StartNewRunFromGameOver);
    }

    public void OnMenuButtonPressedFromGameOver()
    {
        if (_stateMachine.CurrentState != GameState.GameOver)
            return;

        _services?.Audio?.PlayButtonClick();
        AwardRunCurrencyOnce();
        ReturnToMenuWithFade(() =>
        {
            _services?.Audio?.PlayMenuMusic();
            runZoneManager?.OnRunEnded();
            GoToMenu();
        });
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
        AwardRunCurrencyOnce();
        ReturnToMenuWithFade(() =>
        {
            _services?.Audio?.PlayMenuMusic();
            runZoneManager?.OnRunEnded();
            statsTracker?.EndRun();
            GoToMenu();
        });
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

    // --- flow ---

    private void GoToMenu()
    {
        TransitionToState(GameState.Menu, 1f);

        continuesUsed = 0;
        _adInProgress = false;
        _gameTimerEnabled = false;

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

    private void StartNewRun()
    {
        TransitionToState(GameState.Playing, 1f);

        _elapsedTime = 0;
        _gameTimerEnabled = false;
        _runCurrencyAwarded = false;

        // Show and enable the player now we're playing
        SetPlayerVisible(true);
        SetPlayerCollidable(true);

        mainMenuUI?.Hide();
        gameOverUI?.Hide();
        pauseMenuUI?.Hide();
        hudController?.Show();

        playerHealth?.ResetHealth();
        playerVisual.SetVisible(true);
        scoreManager?.ResetRun();
        currencyManager?.ResetRun();
        speedController?.ResetForNewRun();
        statsTracker?.ResetRunStats();
        runZoneManager?.OnResetRun();

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
        if (continuesUsed >= maxContinuesPerRun)
        {
            AwardRunCurrencyOnce();
        }

        ShowGameOverUI();
    }

    private void ShowGameOverUI()
    {
        if (gameOverUI == null || scoreManager == null)
            return;

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

    private void AwardRunCurrencyOnce()
    {
        if (_runCurrencyAwarded)
            return;

        _runCurrencyAwarded = true;

        if (playerProfile == null || currencyManager == null)
            return;

        int coinsToAward = currencyManager.CurrentCoins;
        if (coinsToAward <= 0)
            return;

        playerProfile.softCurrency += coinsToAward;
        playerProfile.Save();
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
        public IAnalyticsService Analytics { get; }

        public GameServicesFacade(AudioManager audio, VfxManager vfx, IRewardedAdService rewardedAds, IAnalyticsService analytics)
        {
            Audio = audio;
            Vfx = vfx;
            RewardedAds = rewardedAds;
            Analytics = analytics;
        }
    }

    // --- Analytics ---

    private void LogAnalyticsEvent(string eventName, Dictionary<string, object> parameters = null)
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
