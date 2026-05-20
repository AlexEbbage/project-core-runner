using System;
using CoreRacer.Services.Save;
using UnityEngine;

namespace CoreRacer.Services.Settings
{
    public sealed class SettingsService
    {
        private readonly ISaveStorage _storage;
        private readonly JsonSaveSerializer _serializer;

        public GameSettingsState State { get; private set; }
        public event Action Changed;

        public SettingsService(ISaveStorage storage, JsonSaveSerializer serializer)
        {
            _storage = storage;
            _serializer = serializer;
            State = Load();
        }

        public GameSettingsState Load()
        {
            var json = _storage.Load(SaveKeys.Settings);
            return string.IsNullOrWhiteSpace(json)
                ? new GameSettingsState()
                : _serializer.Deserialize<GameSettingsState>(json) ?? new GameSettingsState();
        }

        public void SetMasterVolume(float value)
        {
            State.MasterVolume = Mathf.Clamp01(value);
            Save();
        }

        public void SetMusicVolume(float value)
        {
            State.MusicVolume = Mathf.Clamp01(value);
            Save();
        }

        public void SetSfxVolume(float value)
        {
            State.SfxVolume = Mathf.Clamp01(value);
            Save();
        }

        public void SetHaptics(bool value)
        {
            State.HapticsEnabled = value;
            Save();
        }

        public void SetHighQualityGraphics(bool value)
        {
            State.HighQualityGraphics = value;
            Save();
        }

        public void Save()
        {
            _storage.Save(SaveKeys.Settings, _serializer.Serialize(State));
            Changed?.Invoke();
        }
    }
}
