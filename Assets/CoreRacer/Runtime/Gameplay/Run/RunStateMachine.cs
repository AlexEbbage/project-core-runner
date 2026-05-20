using System;

namespace CoreRacer.Gameplay.Run
{
    public sealed class RunStateMachine
    {
        public RunState State { get; private set; } = RunState.MainMenu;
        public event Action<RunState, RunState> StateChanged;

        public bool TrySetState(RunState next)
        {
            if (!CanTransition(State, next))
                return false;

            var previous = State;
            State = next;
            StateChanged?.Invoke(previous, next);
            return true;
        }

        public bool CanTransition(RunState current, RunState next)
        {
            if (current == next) return true;

            switch (current)
            {
                case RunState.None:
                    return next == RunState.MainMenu;
                case RunState.MainMenu:
                    return next == RunState.Starting || next == RunState.Running;
                case RunState.Starting:
                    return next == RunState.Running || next == RunState.MainMenu;
                case RunState.Running:
                    return next == RunState.Paused || next == RunState.Crashed || next == RunState.GameOver || next == RunState.ReturningToMenu;
                case RunState.Paused:
                    return next == RunState.Running || next == RunState.ReturningToMenu;
                case RunState.Crashed:
                    return next == RunState.ContinueOffered || next == RunState.GameOver;
                case RunState.ContinueOffered:
                    return next == RunState.Running || next == RunState.GameOver;
                case RunState.GameOver:
                    return next == RunState.ReturningToMenu || next == RunState.MainMenu;
                case RunState.ReturningToMenu:
                    return next == RunState.MainMenu;
                default:
                    return false;
            }
        }
    }
}
