using System;

namespace CoreRacer.Services.Logging
{
    [Serializable]
    public sealed class LogEntry
    {
        public LogLevel Level;
        public LogCategory Category;
        public string Message;
        public string Context;
        public string UtcTimestamp;
        public int Frame;

        public LogEntry() { }

        public LogEntry(LogLevel level, LogCategory category, string message, string context, string utcTimestamp, int frame)
        {
            Level = level;
            Category = category;
            Message = message;
            Context = context;
            UtcTimestamp = utcTimestamp;
            Frame = frame;
        }

        public override string ToString()
        {
            return $"[{UtcTimestamp}] [{Level}] [{Category}] {Message}";
        }
    }
}
