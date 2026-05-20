using System;
using System.Collections.Generic;

namespace CoreRacer.Services.Crash
{
    public interface ICrashReportingService
    {
        void SetUserId(string userId);
        void SetCustomKey(string key, string value);
        void LogBreadcrumb(string message);
        void RecordException(Exception exception, IReadOnlyDictionary<string, string> context = null);
    }
}
