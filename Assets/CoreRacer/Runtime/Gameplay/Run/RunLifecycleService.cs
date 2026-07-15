using System;
using CoreRacer.Common.Time;

namespace CoreRacer.Gameplay.Run
{
    public sealed class RunLifecycleService
    {
        private readonly RunStateMachine _stateMachine;
        private readonly IGameClock _clock;
        private readonly RunSession _session = new RunSession();
        private bool _endRaised;

        public RunSession Session => _session;
        public event Action RunStarted;
        public event Action RunPaused;
        public event Action RunResumed;
        public event Action<RunEndReason> RunEnded;

        public RunLifecycleService(RunStateMachine stateMachine, IGameClock clock)
        {
            _stateMachine = stateMachine;
            _clock = clock;
        }

        public bool StartNewRun(string levelId, string shipId)
        {
            // RunStateMachine allows idempotent same-state transitions for general callers.
            // Starting a run is not idempotent: a second Play click must not reset the active session.
            if (_stateMachine.State != RunState.MainMenu && _stateMachine.State != RunState.Starting)
                return false;

            if (!_stateMachine.TrySetState(RunState.Running))
                return false;

            _endRaised = false;
            _session.Reset(levelId, shipId, UnityEngine.Time.realtimeSinceStartup);
            _clock.TimeScale = 1f;
            RunStarted?.Invoke();
            return true;
        }

        public void Pause()
        {
            if (!_stateMachine.TrySetState(RunState.Paused))
                return;
            _clock.TimeScale = 0f;
            RunPaused?.Invoke();
        }

        public void Resume()
        {
            if (!_stateMachine.TrySetState(RunState.Running))
                return;
            _clock.TimeScale = 1f;
            RunResumed?.Invoke();
        }

        public void Crash()
        {
            _stateMachine.TrySetState(RunState.Crashed);
        }

        public bool EndRun(RunEndReason reason)
        {
            if (_endRaised || _stateMachine.State == RunState.GameOver || _stateMachine.State == RunState.MainMenu || _stateMachine.State == RunState.ReturningToMenu)
                return false;

            if (!_stateMachine.TrySetState(RunState.GameOver))
                return false;

            _endRaised = true;
            _clock.TimeScale = 1f;
            RunEnded?.Invoke(reason);
            return true;
        }

        public void ReturnToMenu()
        {
            _clock.TimeScale = 1f;
            _stateMachine.TrySetState(RunState.ReturningToMenu);
            _stateMachine.TrySetState(RunState.MainMenu);
        }
    }
}
