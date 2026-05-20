using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreRacer.Services.Logging
{
    public sealed class GameLogger : IGameLogger
    {
        private readonly int _entryCapacity;
        private readonly Queue<LogEntry> _entries;
        private readonly CrashBreadcrumbBuffer _breadcrumbs;

        public event Action<LogEntry> EntryLogged;
        public LogLevel MinimumLevel { get; set; }

        public GameLogger(LogLevel minimumLevel = LogLevel.Info, int entryCapacity = 250, int breadcrumbCapacity = 100)
        {
            MinimumLevel = minimumLevel;
            _entryCapacity = Math.Max(25, entryCapacity);
            _entries = new Queue<LogEntry>(_entryCapacity);
            _breadcrumbs = new CrashBreadcrumbBuffer(breadcrumbCapacity);
        }

        public void Log(LogLevel level, LogCategory category, string message, Object context = null)
        {
            if (level < MinimumLevel)
                return;

            var entry = new LogEntry(
                level,
                category,
                message ?? string.Empty,
                context != null ? context.name : string.Empty,
                DateTimeOffset.UtcNow.ToString("o"),
                UnityEngine.Time.frameCount);

            while (_entries.Count >= _entryCapacity)
                _entries.Dequeue();

            _entries.Enqueue(entry);
            _breadcrumbs.Add(entry);
            WriteToUnityConsole(entry, context);
            EntryLogged?.Invoke(entry);
        }

        public void Exception(LogCategory category, Exception exception, Object context = null)
        {
            if (exception == null)
            {
                Log(LogLevel.Error, category, "Unknown exception", context);
                return;
            }

            Log(LogLevel.Error, category, exception.GetType().Name + ": " + exception.Message + "\n" + exception.StackTrace, context);
        }

        public IReadOnlyList<LogEntry> GetRecentEntries()
        {
            return new List<LogEntry>(_entries);
        }

        public IReadOnlyList<LogEntry> GetBreadcrumbs()
        {
            return _breadcrumbs.Snapshot();
        }

        private static void WriteToUnityConsole(LogEntry entry, Object context)
        {
            var message = $"[CoreRacer:{entry.Category}] {entry.Message}";
            switch (entry.Level)
            {
                case LogLevel.Warning:
                    Debug.LogWarning(message, context);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    Debug.LogError(message, context);
                    break;
                default:
                    Debug.Log(message, context);
                    break;
            }
        }
    }
}
