using System;
using CoreRacer.Services.Save;

namespace CoreRacer.Services.Accessibility
{
    public sealed class AccessibilitySettingsService
    {
        private const string SaveKey = "core_racer_comfort_settings";
        private readonly ISaveStorage _storage;
        private readonly JsonSaveSerializer _serializer;

        public ComfortSettingsState State { get; private set; }
        public event Action<ComfortSettingsState> Changed;

        public AccessibilitySettingsService(ISaveStorage storage, JsonSaveSerializer serializer)
        {
            _storage = storage;
            _serializer = serializer;
            State = Load();
        }

        public void Update(Action<ComfortSettingsState> mutate)
        {
            mutate?.Invoke(State);
            Save();
            Changed?.Invoke(State);
        }

        public void ResetDefaults()
        {
            State = new ComfortSettingsState();
            Save();
            Changed?.Invoke(State);
        }

        private ComfortSettingsState Load()
        {
            if (_storage == null || !_storage.Exists(SaveKey)) return new ComfortSettingsState();
            return _serializer.Deserialize<ComfortSettingsState>(_storage.Load(SaveKey)) ?? new ComfortSettingsState();
        }

        private void Save()
        {
            _storage?.Save(SaveKey, _serializer.Serialize(State));
        }
    }
}
