using System;
using UnityEngine;

namespace CoreRacer.Services.Support
{
    [Serializable]
    public sealed class PlayerSupportInfo
    {
        public string AppVersion;
        public string UnityVersion;
        public string DeviceModel;
        public string OperatingSystem;
        public string SessionId;
        public string PlayerId;
        public string CreatedUtcIso;

        public static PlayerSupportInfo Create(string playerId = null, string sessionId = null)
        {
            return new PlayerSupportInfo
            {
                AppVersion = Application.version,
                UnityVersion = Application.unityVersion,
                DeviceModel = SystemInfo.deviceModel,
                OperatingSystem = SystemInfo.operatingSystem,
                SessionId = sessionId ?? Guid.NewGuid().ToString("N"),
                PlayerId = playerId ?? "local_player",
                CreatedUtcIso = DateTimeOffset.UtcNow.ToString("o")
            };
        }
    }
}
