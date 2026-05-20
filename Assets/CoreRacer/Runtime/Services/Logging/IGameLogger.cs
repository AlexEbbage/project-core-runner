using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreRacer.Services.Logging
{
    public interface IGameLogger
    {
        event Action<LogEntry> EntryLogged;
        LogLevel MinimumLevel { get; set; }
        void Log(LogLevel level, LogCategory category, string message, UnityEngine.Object context = null);
        void Exception(LogCategory category, Exception exception, UnityEngine.Object context = null);
        IReadOnlyList<LogEntry> GetRecentEntries();
        IReadOnlyList<LogEntry> GetBreadcrumbs();
    }
}
