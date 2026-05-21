using CoreRacer.Bootstrap;
using CoreRacer.Common.Time;
using CoreRacer.Config.Run;
using CoreRacer.FTUE;
using CoreRacer.Meta.Profile;
using CoreRacer.Monetisation.Ads;
using CoreRacer.Services.Analytics;
using UnityEngine;

namespace CoreRacer.Gameplay.Run
{
    public sealed class RunController : MonoBehaviour
    {
        [SerializeField] private RunSceneReferences references;
        [SerializeField] private RunConfig config;
        [SerializeField] private string defaultLevelId = "hex_sector_01";
        private string _selectedLevelId;

        private RunStateMachine _stateMachine;
        private RunLifecycleService _lifecycle;
        private RunRewardService _rewards;
        private RunContinueService _continues;
        private RewardedAdController _rewardedAds;
        private GameAnalytics _analytics;
        private TutorialService _tutorial;
        private RunResult _lastResult;

        public RunState State => _stateMachine != null ? _stateMachine.State : RunState.None;

        private void Awake()
        {
            if (config == null)
                config = ScriptableObject.CreateInstance<RunConfig>();

            var registry = GameServices.Registry;
            var clock = registry != null && registry.TryGet<IGameClock>(out var c) ? c : new UnityGameClock();
            var profile = registry != null && registry.TryGet<PlayerProfileService>(out var p) ? p : null;
            registry?.TryGet(out _rewardedAds);
            registry?.TryGet(out _analytics);
            registry?.TryGet(out _tutorial);

            _stateMachine = new RunStateMachine();
            _lifecycle = new RunLifecycleService(_stateMachine, clock);
            if (profile != null)
                _rewards = new RunRewardService(profile, config.Rewards);
            _continues = new RunContinueService(references.Player, references.PlayerHealth, config.Continues);

            _lifecycle.RunStarted += OnRunStarted;
            _lifecycle.RunEnded += OnRunEnded;
            if (references.PlayerHealth != null)
                references.PlayerHealth.Died += () => HandlePlayerDeath();
        }

        private void Start()
        {
            ShowMainMenu();
        }

        public void StartRun()
        {
            var shipId = "starter_runner";
            if (GameServices.TryGet<PlayerProfileService>(out var profile))
                shipId = profile.State.SelectedShipId;

            var levelId = string.IsNullOrWhiteSpace(_selectedLevelId) ? defaultLevelId : _selectedLevelId;
            _analytics?.RunStarted(levelId, shipId);
            _lifecycle.StartNewRun(levelId, shipId);
            _tutorial?.Notify(TutorialStepKind.WaitForRunStarted, "play");
        }

        public void SetSelectedLevelId(string levelId)
        {
            if (!string.IsNullOrWhiteSpace(levelId))
                _selectedLevelId = levelId;
        }

        public void PauseRun() => _lifecycle.Pause();
        public void ResumeRun() => _lifecycle.Resume();

        public void ReturnToMenu()
        {
            StopRuntimeSystems();
            _lifecycle.ReturnToMenu();
            ShowMainMenu();
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
                references.Hud?.Hide();
                references.PauseMenu?.Hide();
                references.GameOver?.ShowContinueOffer();
            }
            else
            {
                _lifecycle.EndRun(RunEndReason.PlayerDeath);
            }
        }

        public void ContinueRun()
        {
            if (State != RunState.ContinueOffered)
                return;

            if (_rewardedAds == null)
            {
                ApplyContinue();
                return;
            }

            _rewardedAds.ShowOrBypass(AdPlacement.ContinueRun, result =>
            {
                if (result == RewardedAdResult.Rewarded || result == RewardedAdResult.BypassedByPremium)
                    ApplyContinue();
                else
                    _lifecycle.EndRun(RunEndReason.PlayerDeath);
            });
        }

        public void DeclineContinue()
        {
            if (State != RunState.ContinueOffered && State != RunState.Crashed)
                return;

            _lifecycle.EndRun(RunEndReason.PlayerDeath);
        }

        public void DoubleRunRewards()
        {
            if (_lastResult.Score <= 0 || _lastResult.Coins <= 0)
                return;

            if (_rewardedAds == null)
            {
                GrantDoubleRewards();
                return;
            }

            _rewardedAds.ShowOrBypass(AdPlacement.DoubleRunRewards, result =>
            {
                if (result == RewardedAdResult.Rewarded || result == RewardedAdResult.BypassedByPremium)
                    GrantDoubleRewards();
            });
        }

        private void ApplyContinue()
        {
            _continues.ContinueRun(_lifecycle.Session);
            UnityEngine.Time.timeScale = 1f;
            _stateMachine.TrySetState(RunState.Running);
            references.Hud?.Show();
            references.GameOver?.Hide();
            references.PauseMenu?.Hide();
            references.Player?.BeginRun();
            references.ObstacleWorld?.BeginRun();
            references.PickupWorld?.BeginRun();
        }

        private void OnRunStarted()
        {
            references.MainMenu?.Hide();
            references.PauseMenu?.Hide();
            references.GameOver?.Hide();
            references.Player?.BeginRun();
            references.PlayerHealth?.ResetHealth();
            references.ScoreTracker?.BeginRun();
            references.CurrencyTracker?.BeginRun();
            references.StatsTracker?.BeginRun();
            references.ObstacleWorld?.BeginRun();
            references.PickupWorld?.BeginRun();
            references.Hud?.Show();
            references.GameOver?.Hide();
        }

        private void OnRunEnded(RunEndReason reason)
        {
            StopRuntimeSystems();
            references.Hud?.Hide();
            references.PauseMenu?.Hide();

            _lastResult = _rewards != null
                ? _rewards.BuildResult(references.ScoreTracker.CurrentScore, references.CurrencyTracker.Coins, references.StatsTracker.Distance, references.StatsTracker.Duration, references.StatsTracker.PowerupsCollected, reason, false)
                : default;
            _rewards?.Grant(_lastResult);
            _analytics?.RunEnded(_lastResult);
            references.GameOver?.Show(_lastResult);
        }

        private void GrantDoubleRewards()
        {
            if (_rewards == null)
                return;

            var extra = _rewards.BuildResult(
                references.ScoreTracker.CurrentScore,
                references.CurrencyTracker.Coins,
                references.StatsTracker.Distance,
                references.StatsTracker.Duration,
                references.StatsTracker.PowerupsCollected,
                _lastResult.EndReason,
                false);
            _rewards.Grant(extra);
            references.GameOver?.ShowDoubleRewardGranted(extra);
        }

        private void StopRuntimeSystems()
        {
            references.Player?.EndRun();
            references.ScoreTracker?.EndRun();
            references.StatsTracker?.EndRun();
            references.ObstacleWorld?.EndRun();
            references.PickupWorld?.EndRun();
        }

        private void ShowMainMenu()
        {
            references.Hud?.Hide();
            references.GameOver?.Hide();
            references.PauseMenu?.Hide();
            references.MainMenu?.Show();
        }
    }
}
