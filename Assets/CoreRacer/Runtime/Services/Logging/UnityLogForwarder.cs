using UnityEngine;

namespace CoreRacer.Services.Logging
{
    public sealed class UnityLogForwarder : MonoBehaviour
    {
        private IGameLogger _logger;
        private bool _isForwarding;

        public void Initialize(IGameLogger logger)
        {
            _logger = logger;
        }

        private void OnEnable()
        {
            Application.logMessageReceived += OnUnityLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= OnUnityLog;
        }

        private void OnUnityLog(string condition, string stackTrace, LogType type)
        {
            if (_logger == null || _isForwarding)
                return;

            if (!condition.Contains("CoreRacer"))
                return;

            _isForwarding = true;
            var level = type == LogType.Warning ? LogLevel.Warning :
                type == LogType.Error || type == LogType.Exception || type == LogType.Assert ? LogLevel.Error : LogLevel.Debug;
            _logger.Log(level, LogCategory.General, condition + (string.IsNullOrEmpty(stackTrace) ? string.Empty : "\n" + stackTrace));
            _isForwarding = false;
        }
    }
}
