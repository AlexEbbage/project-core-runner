using CoreRacer.Bootstrap;
using CoreRacer.Common.Time;
using CoreRacer.Config.Gameplay;
using CoreRacer.Config.Run;
using CoreRacer.FTUE;
using CoreRacer.Gameplay.Camera;
using CoreRacer.Gameplay.Environment;
using CoreRacer.Meta.Boosters;
using CoreRacer.Meta.Levels;
using CoreRacer.Meta.Profile;
using CoreRacer.Meta.Ships;
using CoreRacer.Monetisation.Ads;
using CoreRacer.Services.Analytics;
using UnityEngine;

namespace CoreRacer.Gameplay.Run
{
    public sealed class RunController : MonoBehaviour
    {
        [Header("Run Composition")]
        [SerializeField] private RunSceneReferences references;
        [SerializeField] private RunConfig config;
        [SerializeField] private LevelRoadmapConfigV2 levelRoadmap;
        [SerializeField] private SpeedScalingConfigV2 speedScalingConfig;
        [SerializeField] private CameraFovController cameraFovController;
        [SerializeField] private RunZoneManagerV2 zoneManager;
        [SerializeField] private TunnelWallGeneratorV2 tunnelWallGenerator;
        [SerializeField] private ShipDatabase shipDatabase;
        [SerializeField] private BoosterCatalog boosterCatalog;
        [SerializeField] private string defaultLevelId = "hex_sector_01";

        private string _selectedLevelId;
        private LevelDefinition _selectedLevel;
        private RunStateMachine _stateMachine;
        private RunLifecycleService _lifecycle;
        private RunRewardService _rewards;
        private RunContinueService _continues;
        private RewardedAdController _rewardedAds;
        private GameAnalytics _analytics;
        private TutorialService _tutorial;
        private RunResult _lastResult;
        private bool _continueRequestInFlight;
        private bool _doubleRewardRequestInFlight;
        private BoosterRunModifiers _activeBoosterModifiers;

        public RunState State => _stateMachine != null ? _stateMachine.State : RunState.None;
        public RunResult LastResult => _lastResult;
        public string CurrentRunId => _lifecycle != null && _lifecycle.Session != null ? _lifecycle.Session.RunId : string.Empty;
        public BoosterCatalog BoosterCatalog => boosterCatalog;
        public BoosterRunModifiers ActiveBoosterModifiers => _activeBoosterModifiers;

        private void Awake()
        {
            if (config == null)
                config = ScriptableObject.CreateInstance<RunConfig>();
            if (cameraFovController == null)
                cameraFovController = FindObjectOfType<CameraFovController>();
            if (zoneManager == null)
                zoneManager = FindObjectOfType<RunZoneManagerV2>();
            if (tunnelWallGenerator == null)
                tunnelWallGenerator = FindObjectOfType<TunnelWallGeneratorV2>();

            ResolveServices();
            _activeBoosterModifiers = BoosterRunModifiers.Default;

            var clock = GameServices.TryGet<IGameClock>(out var serviceClock) ? serviceClock : new UnityGameClock();
            GameServices.TryGet<PlayerProfileService>(out var profile);

            _stateMachine = new RunStateMachine();
            _lifecycle = new RunLifecycleService(_stateMachine, clock);
            if (profile != null)
                _rewards = new RunRewardService(profile, config.Rewards);

            _continues = new RunContinueService(references != null ? references.Player : null, references != null ? references.PlayerHealth : null, config.Continues);
            _lifecycle.RunStarted += OnRunStarted;
            _lifecycle.RunEnded += OnRunEnded;
            if (references != null && references.PlayerHealth != null)
                references.PlayerHealth.Died += HandlePlayerDeath;
        }

        private void Start()
        {
            ShowMainMenu();
        }

        private void OnDestroy()
        {
            if (_lifecycle != null)
            {
                _lifecycle.RunStarted -= OnRunStarted;
                _lifecycle.RunEnded -= OnRunEnded;
            }

            if (references != null && references.PlayerHealth != null)
                references.PlayerHealth.Died -= HandlePlayerDeath;
        }

        private void Update()
        {
            if (State != RunState.Running || references == null || references.Player == null || references.Player.Motor == null || speedScalingConfig == null)
                return;

            var elapsed = references.StatsTracker != null ? references.StatsTracker.Duration : 0f;
            var combo = references.ScoreTracker != null ? references.ScoreTracker.Combo : 0f;
            var startingSpeed = _selectedLevel != null && _selectedLevel.StartingSpeed > 0f
                ? _selectedLevel.StartingSpeed
                : speedScalingConfig.BaseForwardSpeed;
            var speed = speedScalingConfig.EvaluateForwardSpeed(elapsed, combo, startingSpeed);
            references.Player.Motor.ForwardSpeed = speed;

            if (cameraFovController != null)
            {
                var range = Mathf.Max(0.01f, speedScalingConfig.MaxForwardSpeed - startingSpeed);
                cameraFovController.SetSpeedIntensity((speed - startingSpeed) / range);
            }
        }

        public void StartRun()
        {
            TryStartRun();
        }

        public bool TryStartRun()
        {
            Debug.Log($"[CoreRacer.Run] Start requested (state={State})", this);
            if (State != RunState.MainMenu && State != RunState.Starting)
            {
                Debug.LogWarning($"[CoreRacer.Run] Start rejected: a run cannot start from state {State}.", this);
                return false;
            }

            ResolveServices();
            ResolveSelectedLevel();
            if (!ValidateCoreRunStart())
                return false;

            var shipId = "starter_runner";
            PlayerProfileService profile = null;
            if (GameServices.TryGet(out profile))
            {
                shipId = profile.State.SelectedShipId;
                if (_rewards == null)
                    _rewards = new RunRewardService(profile, config.Rewards);
                ApplyShipProgression(profile);
                references?.PlayerCosmetics?.Apply(profile.State);
                _activeBoosterModifiers = BoosterLoadoutResolver.Resolve(boosterCatalog, profile.State.EquippedBoosterIds);
            }
            else
                _activeBoosterModifiers = BoosterRunModifiers.Default;

            var levelId = _selectedLevel != null ? _selectedLevel.Id : (string.IsNullOrWhiteSpace(_selectedLevelId) ? defaultLevelId : _selectedLevelId);
            Debug.Log($"[CoreRacer.Run] Selected level: {levelId}", this);
            ApplySelectedRunDefinition();
            Time.timeScale = 1f;
            if (!_lifecycle.StartNewRun(levelId, shipId))
            {
                Debug.LogError($"[CoreRacer.Run] Start failed from state {State}. Return to the menu before starting another run.", this);
                return false;
            }

            _analytics?.RunStarted(levelId, shipId);
            _tutorial?.Notify(TutorialStepKind.WaitForRunStarted, "play");
            Debug.Log($"[CoreRacer.Run] Run started successfully (runId={CurrentRunId}, level={levelId}, ship={shipId})", this);
            return true;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public bool TryQuickPlayCoreRun()
        {
            if (levelRoadmap == null || levelRoadmap.Levels == null || levelRoadmap.Levels.Count == 0)
            {
                Debug.LogError("Quick Play failed: the run controller has no level roadmap entries.", this);
                return false;
            }

            var knownLevel = levelRoadmap.Levels[0];
            if (knownLevel == null || string.IsNullOrWhiteSpace(knownLevel.Id))
            {
                Debug.LogError("Quick Play failed: the first roadmap entry is missing or has no id.", this);
                return false;
            }

            SetSelectedLevel(knownLevel);
            return TryStartRun();
        }
#endif

        public void SetSelectedLevelId(string levelId)
        {
            if (string.IsNullOrWhiteSpace(levelId))
                return;

            _selectedLevelId = levelId;
            _selectedLevel = levelRoadmap != null ? levelRoadmap.Get(levelId) : null;
        }

        public void SetSelectedLevel(LevelDefinition level)
        {
            if (level == null)
                return;

            _selectedLevel = level;
            _selectedLevelId = level.Id;
        }

        public void PauseRun() => _lifecycle?.Pause();
        public void ResumeRun() => _lifecycle?.Resume();

        public void ReturnToMenu()
        {
            _continueRequestInFlight = false;
            _doubleRewardRequestInFlight = false;
            StopRuntimeSystems();
            ClearRunBoosters();
            _lifecycle?.ReturnToMenu();
            ShowMainMenu();
        }

        public void RetryRun()
        {
            if (State != RunState.GameOver)
                return;

            _lifecycle.ReturnToMenu();
            StartRun();
        }

        public void HandlePlayerDeath()
        {
            if (State != RunState.Running)
                return;

            _lifecycle.Crash();
            _tutorial?.Notify(TutorialStepKind.WaitForCrash, "continue");
            StopRuntimeSystems();
            if (_continues.CanContinue(_lifecycle.Session))
            {
                _stateMachine.TrySetState(RunState.ContinueOffered);
                UnityEngine.Time.timeScale = 0f;
                references?.Hud?.Hide();
                references?.PauseMenu?.Hide();
                references?.GameOver?.ShowContinueOffer();
            }
            else
            {
                _lifecycle.EndRun(RunEndReason.PlayerDeath);
            }
        }

        public bool ContinueRun()
        {
            if (State != RunState.ContinueOffered || _continueRequestInFlight)
                return false;

            ResolveServices();
            if (_rewardedAds == null || !_rewardedAds.CanShow(AdPlacement.ContinueRun))
            {
                references?.GameOver?.ShowContinueUnavailable();
                _lifecycle.EndRun(RunEndReason.PlayerDeath);
                return false;
            }

            _continueRequestInFlight = true;
            references?.GameOver?.SetContinuePending(true);
            _rewardedAds.ShowOrBypass(AdPlacement.ContinueRun, result =>
            {
                if (!_continueRequestInFlight)
                    return;

                _continueRequestInFlight = false;
                references?.GameOver?.SetContinuePending(false);
                if (State != RunState.ContinueOffered)
                    return;

                if (_rewardedAds.ShouldGrantReward(result))
                    ApplyContinue();
                else
                {
                    references?.GameOver?.ShowContinueUnavailable();
                    _lifecycle.EndRun(RunEndReason.PlayerDeath);
                }
            });
            return true;
        }

        public void DeclineContinue()
        {
            if (State != RunState.ContinueOffered && State != RunState.Crashed)
                return;

            _continueRequestInFlight = false;
            _lifecycle.EndRun(RunEndReason.PlayerDeath);
        }

        public bool DoubleRunRewards()
        {
            if (State != RunState.GameOver || _lifecycle == null || !_lifecycle.Session.RewardsGranted || _lifecycle.Session.DoubleRewardsGranted || _doubleRewardRequestInFlight)
                return false;

            if (_lastResult.Coins <= 0 && _lastResult.Experience <= 0 && _lastResult.PremiumCurrency <= 0)
                return false;

            ResolveServices();
            if (_rewardedAds == null || !_rewardedAds.CanShow(AdPlacement.DoubleRunRewards))
            {
                references?.GameOver?.ShowDoubleRewardUnavailable();
                return false;
            }

            _doubleRewardRequestInFlight = true;
            references?.GameOver?.SetDoubleRewardPending(true);
            _rewardedAds.ShowOrBypass(AdPlacement.DoubleRunRewards, result =>
            {
                if (!_doubleRewardRequestInFlight)
                    return;

                _doubleRewardRequestInFlight = false;
                references?.GameOver?.SetDoubleRewardPending(false);
                if (State != RunState.GameOver || _lifecycle.Session.DoubleRewardsGranted)
                    return;

                if (_rewardedAds.ShouldGrantReward(result))
                    GrantDoubleRewards();
                else
                    references?.GameOver?.ShowDoubleRewardUnavailable();
            });
            return true;
        }

        private void ApplyContinue()
        {
            if (!_stateMachine.TrySetState(RunState.Running))
                return;

            _continues.ContinueRun(_lifecycle.Session);
            UnityEngine.Time.timeScale = 1f;
            references?.Hud?.Show();
            references?.GameOver?.Hide();
            references?.PauseMenu?.Hide();
            references?.Player?.BeginRun();
            references?.ObstacleWorld?.BeginRun();
            references?.PickupWorld?.BeginRun();
        }

        private void OnRunStarted()
        {
            _continueRequestInFlight = false;
            _doubleRewardRequestInFlight = false;
            _lastResult = default;
            ApplySelectedRunDefinition();
            references?.Powerups?.ClearAll();
            references?.MainMenu?.Hide();
            references?.PauseMenu?.Hide();
            references?.GameOver?.Hide();
            references?.Player?.BeginRun();
            references?.PlayerHealth?.ResetHealth();
            references?.ScoreTracker?.BeginRun();
            references?.CurrencyTracker?.BeginRun();
            references?.StatsTracker?.BeginRun();
            ApplyRunBoosters();
            references?.ObstacleWorld?.BeginRun();
            references?.PickupWorld?.BeginRun();
            references?.Hud?.Show();
            Debug.Log("[CoreRacer.Run] Gameplay root activated", this);
        }

        private void OnRunEnded(RunEndReason reason)
        {
            if (_lifecycle.Session.RewardsGranted)
            {
                Debug.LogWarning($"[CoreRacer.Run] Duplicate end ignored (runId={CurrentRunId}, reason={reason})", this);
                return;
            }

            StopRuntimeSystems();
            ClearRunBoosters();
            references?.Hud?.Hide();
            references?.PauseMenu?.Hide();

            _lastResult = _rewards != null
                ? _rewards.BuildResult(
                    references?.ScoreTracker != null ? references.ScoreTracker.CurrentScore : 0,
                    references?.CurrencyTracker != null ? references.CurrencyTracker.Coins : 0,
                    references?.StatsTracker != null ? references.StatsTracker.Distance : 0f,
                    references?.StatsTracker != null ? references.StatsTracker.Duration : 0f,
                    references?.StatsTracker != null ? references.StatsTracker.PowerupsCollected : 0,
                    reason,
                    false)
                : default;

            _rewards?.Grant(_lastResult);
            _lifecycle.Session.RewardsGranted = true;
            _analytics?.RunEnded(_lastResult);
            references?.GameOver?.Show(_lastResult);
            Debug.Log($"[CoreRacer.Run] Run ended (runId={CurrentRunId}, reason={reason})", this);
        }

        private void GrantDoubleRewards()
        {
            if (_rewards == null || _lifecycle.Session.DoubleRewardsGranted)
                return;

            var bonus = _rewards.BuildBonusResult(_lastResult);
            _rewards.GrantBonus(bonus);
            _lifecycle.Session.DoubleRewardsGranted = true;
            references?.GameOver?.ShowDoubleRewardGranted(bonus);
        }

        private void ResolveSelectedLevel()
        {
            if (_selectedLevel != null)
                return;

            var id = string.IsNullOrWhiteSpace(_selectedLevelId) ? defaultLevelId : _selectedLevelId;
            _selectedLevel = levelRoadmap != null ? levelRoadmap.Get(id) : null;
            if (_selectedLevel == null && levelRoadmap != null && levelRoadmap.Levels != null && levelRoadmap.Levels.Count > 0)
            {
                _selectedLevel = levelRoadmap.Levels[0];
                _selectedLevelId = _selectedLevel != null ? _selectedLevel.Id : string.Empty;
                if (_selectedLevel != null)
                    Debug.LogWarning($"[CoreRacer.Run] Selected level '{id}' was unavailable; using default '{_selectedLevelId}'.", this);
            }
        }

        private void ApplySelectedRunDefinition()
        {
            ResolveSelectedLevel();
            var sides = _selectedLevel != null ? _selectedLevel.TunnelSides : 6;
            var difficulty = _selectedLevel != null ? _selectedLevel.DifficultyMultiplier : 1f;
            var speed = _selectedLevel != null && _selectedLevel.StartingSpeed > 0f
                ? _selectedLevel.StartingSpeed
                : (speedScalingConfig != null ? speedScalingConfig.BaseForwardSpeed : 10f);

            references?.ObstacleWorld?.ConfigureForRun(sides, difficulty);
            references?.PickupWorld?.ConfigureForRun(sides);
            if (references?.Player != null && references.Player.Motor != null)
                references.Player.Motor.ForwardSpeed = speed;
            if (tunnelWallGenerator != null)
            {
                tunnelWallGenerator.SetTarget(references != null && references.Player != null ? references.Player.transform : null);
                tunnelWallGenerator.ConfigureSides(sides);
            }

            if (zoneManager != null)
            {
                if (_selectedLevel != null && !string.IsNullOrWhiteSpace(_selectedLevel.ZoneId))
                    zoneManager.ApplyZone(_selectedLevel.ZoneId);
                else
                    zoneManager.ApplyDefaultZone();
            }
        }


        private void ApplyShipProgression(PlayerProfileService profile)
        {
            if (profile == null || references == null)
                return;

            var speedMultiplier = 1f;
            var handlingMultiplier = 1f;
            var ship = shipDatabase != null ? shipDatabase.GetShip(profile.State.SelectedShipId) : null;
            if (ship != null)
            {
                // Ship stats use 50 as the neutral design baseline and remain deliberately bounded.
                speedMultiplier = StatToMultiplier(ship.BaseStats.Speed);
                handlingMultiplier = StatToMultiplier(ship.BaseStats.Handling);
            }

            var handlingLevel = profile.GetUpgradeLevel(profile.State.ShipUpgradeLevels, UpgradeType.Handling.ToString());
            var comboLevel = profile.GetUpgradeLevel(profile.State.ShipUpgradeLevels, UpgradeType.ComboMultiplier.ToString());
            var radiusLevel = profile.GetUpgradeLevel(profile.State.ShipUpgradeLevels, UpgradeType.PickupRadius.ToString());
            var shieldLevel = profile.GetUpgradeLevel(profile.State.ShipUpgradeLevels, UpgradeType.ShieldRecharge.ToString());

            references.Player?.Motor?.SetShipModifiers(speedMultiplier, handlingMultiplier * (1f + handlingLevel * 0.05f));
            references.ScoreTracker?.SetShipComboMultiplier(1f + comboLevel * 0.05f);
            references.PlayerHealth?.SetMaxHealthBonus(shieldLevel * 0.2f);

            var magnet = references.Player != null ? references.Player.GetComponent<CoreRacer.Gameplay.Pickups.PickupMagnetController>() : null;
            magnet?.SetUpgradeRadiusMultiplier(1f + radiusLevel * 0.1f);
        }

        private void ApplyRunBoosters()
        {
            references?.ScoreTracker?.SetRunScoreMultiplier(_activeBoosterModifiers.ScoreMultiplier);
            references?.CurrencyTracker?.SetRunCoinMultiplier(_activeBoosterModifiers.CoinMultiplier);
            if (_activeBoosterModifiers.StartShieldSeconds > 0f)
                references?.PlayerHealth?.Revive(_activeBoosterModifiers.StartShieldSeconds);

            Debug.Log($"[CoreRacer.Boosters] Applied run loadout: score x{_activeBoosterModifiers.ScoreMultiplier:0.##}, coins x{_activeBoosterModifiers.CoinMultiplier:0.##}, shield {_activeBoosterModifiers.StartShieldSeconds:0.##}s.", this);
        }

        private static float StatToMultiplier(float stat)
        {
            return Mathf.Clamp(1f + (stat - 50f) * 0.005f, 0.75f, 1.25f);
        }

        private void ResolveServices()
        {
            if (_rewardedAds == null) GameServices.TryGet(out _rewardedAds);
            if (_analytics == null) GameServices.TryGet(out _analytics);
            if (_tutorial == null) GameServices.TryGet(out _tutorial);
        }

        private bool ValidateCoreRunStart()
        {
            if (_lifecycle == null)
            {
                Debug.LogError("[CoreRacer.Run] Start failed: the run lifecycle is not initialized.", this);
                return false;
            }

            if (references == null)
            {
                Debug.LogError("[CoreRacer.Run] Start failed: RunController.references is not assigned.", this);
                return false;
            }

            var validation = references.ValidateReferences();
            for (var i = 0; i < validation.Errors.Count; i++)
                Debug.LogError("[CoreRacer.Run] Reference error: " + validation.Errors[i], references);
            for (var i = 0; i < validation.Warnings.Count; i++)
                Debug.LogWarning("[CoreRacer.Run] Reference warning: " + validation.Warnings[i], references);

            var valid = validation.IsValid;
            valid &= RequireCoreReference(references.Player != null && references.Player.Motor != null, "Player and Player.Motor");
            valid &= RequireCoreReference(zoneManager != null, "RunZoneManagerV2");
            valid &= RequireCoreReference(tunnelWallGenerator != null, "TunnelWallGeneratorV2");
            valid &= RequireCoreReference(references.ObstacleWorld != null, "ObstacleWorld");
            valid &= RequireCoreReference(references.PickupWorld != null, "PickupWorld");
            valid &= RequireCoreReference(references.Hud != null, "Hud");
            valid &= RequireCoreReference(references.GameOver != null, "GameOver");
            valid &= RequireCoreReference(references.MainMenu != null, "MainMenu");

            if (_selectedLevel == null)
            {
                Debug.LogError($"[CoreRacer.Run] Start failed: selected level '{_selectedLevelId}' was not found in the roadmap.", this);
                valid = false;
            }

            return valid;
        }

        private bool RequireCoreReference(bool present, string referenceName)
        {
            if (present)
                return true;

            Debug.LogError($"[CoreRacer.Run] Start failed: required reference '{referenceName}' is missing.", this);
            return false;
        }

        private void StopRuntimeSystems()
        {
            references?.Powerups?.ClearAll();
            references?.Player?.EndRun();
            references?.ScoreTracker?.EndRun();
            references?.StatsTracker?.EndRun();
            references?.ObstacleWorld?.EndRun();
            references?.PickupWorld?.EndRun();
        }

        private void ClearRunBoosters()
        {
            references?.ScoreTracker?.SetRunScoreMultiplier(1f);
            references?.CurrencyTracker?.SetRunCoinMultiplier(1f);
            _activeBoosterModifiers = BoosterRunModifiers.Default;
        }

        private void ShowMainMenu()
        {
            references?.Hud?.Hide();
            references?.GameOver?.Hide();
            references?.PauseMenu?.Hide();
            references?.MainMenu?.Show();
        }
    }
}
