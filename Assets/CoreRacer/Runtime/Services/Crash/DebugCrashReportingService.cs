using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreRacer.Services.Crash
{
    public sealed class DebugCrashReportingService : ICrashReportingService
    {
        public void SetUserId(string userId) => Debug.Log("[Crash] UserId: " + userId);
        public void SetCustomKey(string key, string value) => Debug.Log($"[Crash] Key {key}={value}");
        public void LogBreadcrumb(string message) => Debug.Log("[Crash] Breadcrumb: " + message);
        public void RecordException(Exception exception, IReadOnlyDictionary<string, string> context = null) => Debug.LogException(exception);
    }
}
