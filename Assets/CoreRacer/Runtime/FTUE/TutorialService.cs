using System;
using CoreRacer.Services.Analytics;
using CoreRacer.Services.Logging;
using CoreRacer.Services.Save;

namespace CoreRacer.FTUE
{
    public sealed class TutorialService
    {
        private const string SaveKey = "core_racer_tutorial_state";

        private readonly ISaveStorage _storage;
        private readonly JsonSaveSerializer _serializer;
        private readonly TutorialConfig _config;
        private readonly IAnalyticsService _analytics;
        private readonly IGameLogger _logger;

        public TutorialState State { get; private set; }
        public TutorialStepDefinition CurrentStep => _config != null && State.CurrentStepIndex >= 0 && State.CurrentStepIndex < _config.Steps.Count ? _config.Steps[State.CurrentStepIndex] : null;
        public event Action<TutorialStepDefinition> StepChanged;
        public event Action Completed;

        public TutorialService(ISaveStorage storage, JsonSaveSerializer serializer, TutorialConfig config, IAnalyticsService analytics = null, IGameLogger logger = null)
        {
            _storage = storage;
            _serializer = serializer;
            _config = config;
            _analytics = analytics;
            _logger = logger;
            State = Load();
        }

        public bool ShouldRunForFreshInstall()
        {
            return _config != null && _config.RunOnFreshInstall && !State.Completed;
        }

        public void Start()
        {
            if (_config == null || _config.Steps.Count == 0 || State.Completed)
                return;

            State.TutorialId = _config.TutorialId;
            State.Started = true;
            State.CurrentStepIndex = Math.Max(0, State.CurrentStepIndex);
            Save();
            _analytics?.Track(AnalyticsEventNames.TutorialStarted);
            _logger?.Info(LogCategory.Progression, "Tutorial started: " + _config.TutorialId);
            StepChanged?.Invoke(CurrentStep);
        }

        public void Notify(TutorialStepKind kind, string targetId = null)
        {
            var step = CurrentStep;
            if (step == null || step.Kind != kind)
                return;

            if (!string.IsNullOrWhiteSpace(step.HighlightTargetId) && !string.Equals(step.HighlightTargetId, targetId, StringComparison.OrdinalIgnoreCase))
                return;

            Advance();
        }

        public void Advance()
        {
            if (_config == null || State.Completed)
                return;

            var completedStep = CurrentStep;
            if (completedStep != null)
                _analytics?.Track(AnalyticsEventNames.TutorialStepCompleted, new System.Collections.Generic.Dictionary<string, object> { ["step_id"] = completedStep.Id, ["step_index"] = State.CurrentStepIndex });

            State.CurrentStepIndex++;
            if (State.CurrentStepIndex >= _config.Steps.Count || (CurrentStep != null && CurrentStep.Kind == TutorialStepKind.Complete))
            {
                Complete();
                return;
            }

            Save();
            StepChanged?.Invoke(CurrentStep);
        }

        public void Complete()
        {
            State.Completed = true;
            Save();
            _analytics?.Track(AnalyticsEventNames.TutorialCompleted);
            _logger?.Info(LogCategory.Progression, "Tutorial completed.");
            Completed?.Invoke();
        }

        public void ResetForTesting()
        {
            State = new TutorialState { TutorialId = _config != null ? _config.TutorialId : string.Empty };
            Save();
            _analytics?.Track(AnalyticsEventNames.TutorialReset);
        }

        private TutorialState Load()
        {
            if (_storage == null || !_storage.Exists(SaveKey))
                return new TutorialState { TutorialId = _config != null ? _config.TutorialId : string.Empty };

            var state = _serializer.Deserialize<TutorialState>(_storage.Load(SaveKey)) ?? new TutorialState();
            if (_config != null && !string.Equals(state.TutorialId, _config.TutorialId, StringComparison.Ordinal))
                return new TutorialState { TutorialId = _config.TutorialId };
            return state;
        }

        private void Save()
        {
            State.LastUpdatedUtcIso = DateTimeOffset.UtcNow.ToString("o");
            _storage?.Save(SaveKey, _serializer.Serialize(State));
        }
    }
}
