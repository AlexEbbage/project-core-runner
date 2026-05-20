using System;
using CoreRacer.Services.Save;

namespace CoreRacer.Services.Notifications
{
    public sealed class NotificationPermissionService
    {
        private const string SaveKey = "core_racer_notification_permission";
        private readonly ISaveStorage _storage;
        public NotificationPermissionState State { get; private set; }
        public event Action<NotificationPermissionState> Changed;

        public NotificationPermissionService(ISaveStorage storage)
        {
            _storage = storage;
            State = Load();
        }

        public void MarkPromptShown()
        {
            if (State == NotificationPermissionState.Unknown)
                Set(NotificationPermissionState.NotRequested);
        }

        public void Set(NotificationPermissionState state)
        {
            State = state;
            _storage?.Save(SaveKey, state.ToString());
            Changed?.Invoke(State);
        }

        private NotificationPermissionState Load()
        {
            if (_storage == null || !_storage.Exists(SaveKey)) return NotificationPermissionState.Unknown;
            NotificationPermissionState state;
            return Enum.TryParse(_storage.Load(SaveKey), out state) ? state : NotificationPermissionState.Unknown;
        }
    }
}
