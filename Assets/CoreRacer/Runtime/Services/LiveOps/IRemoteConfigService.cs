using System;

namespace CoreRacer.Services.LiveOps
{
    public interface IRemoteConfigService
    {
        bool IsReady { get; }
        void Refresh(Action<bool> completed = null);
        string GetString(string key, string fallback = "");
        int GetInt(string key, int fallback = 0);
        float GetFloat(string key, float fallback = 0f);
        bool GetBool(string key, bool fallback = false);
    }
}
