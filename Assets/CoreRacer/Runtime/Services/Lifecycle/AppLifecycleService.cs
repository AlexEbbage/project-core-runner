using System;
using CoreRacer.Bootstrap;
using CoreRacer.Services.Logging;
using CoreRacer.Services.Save;
using UnityEngine;

namespace CoreRacer.Services.Lifecycle
{
    public sealed class AppLifecycleService : MonoBehaviour
    {
        public AppLifecycleState State { get; private set; } = AppLifecycleState.Starting;
        public event Action<AppLifecycleState> StateChanged;

        private IGameLogger _logger;
        private ISaveStorage _saveStorage;

        private void Awake()
        {
            GameServices.TryGet(out _logger);
            GameServices.TryGet(out _saveStorage);
            SetState(AppLifecycleState.Foreground);
        }

        private void OnApplicationPause(bool paused)
        {
            SetState(paused ? AppLifecycleState.Background : AppLifecycleState.Foreground);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) SetState(AppLifecycleState.Background);
            else SetState(AppLifecycleState.Foreground);
        }

        private void OnApplicationQuit()
        {
            SetState(AppLifecycleState.Quitting);
        }

        private void SetState(AppLifecycleState state)
        {
            if (State == state) return;
            State = state;
            if (state == AppLifecycleState.Background || state == AppLifecycleState.Quitting)
                PlayerPrefs.Save();
            _logger?.Info(LogCategory.System, "App lifecycle changed: " + state, this);
            StateChanged?.Invoke(state);
        }
    }
}
