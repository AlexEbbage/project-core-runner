using System;
using CoreRacer.Common.Time;

namespace CoreRacer.Gameplay.Run
{
    public sealed class RunLifecycleService
    {
        private readonly RunStateMachine _stateMachine;
        private readonly IGameClock _clock;
        private readonly RunSession _session = new RunSession();

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

        public void StartNewRun(string levelId, string shipId)
        {
            if (!_stateMachine.TrySetState(RunState.Running))
                return;

            _session.Reset(levelId, shipId, UnityEngine.Time.realtimeSinceStartup);
            _clock.TimeScale = 1f;
            RunStarted?.Invoke();
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

        public void EndRun(RunEndReason reason)
        {
            _clock.TimeScale = 1f;
            _stateMachine.TrySetState(RunState.GameOver);
            RunEnded?.Invoke(reason);
        }

        public void ReturnToMenu()
        {
            _clock.TimeScale = 1f;
            _stateMachine.TrySetState(RunState.ReturningToMenu);
            _stateMachine.TrySetState(RunState.MainMenu);
        }
    }
}
