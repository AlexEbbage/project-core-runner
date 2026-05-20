using System;
using System.Globalization;

namespace CoreRacer.Services.LiveOps
{
    public sealed class LocalRemoteConfigService : IRemoteConfigService
    {
        private readonly RemoteConfigDefaultsConfig _defaults;

        public LocalRemoteConfigService(RemoteConfigDefaultsConfig defaults)
        {
            _defaults = defaults;
            IsReady = true;
        }

        public bool IsReady { get; private set; }

        public void Refresh(Action<bool> completed = null)
        {
            IsReady = true;
            completed?.Invoke(true);
        }

        public string GetString(string key, string fallback = "")
        {
            if (_defaults != null && _defaults.TryGet(key, out var value))
                return value;
            return fallback;
        }

        public int GetInt(string key, int fallback = 0)
        {
            return int.TryParse(GetString(key, fallback.ToString(CultureInfo.InvariantCulture)), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                ? value
                : fallback;
        }

        public float GetFloat(string key, float fallback = 0f)
        {
            return float.TryParse(GetString(key, fallback.ToString(CultureInfo.InvariantCulture)), NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                ? value
                : fallback;
        }

        public bool GetBool(string key, bool fallback = false)
        {
            var raw = GetString(key, fallback ? "true" : "false");
            return bool.TryParse(raw, out var value) ? value : fallback;
        }
    }
}
