using System.Collections.Generic;

namespace CoreRacer.Services.Logging
{
    public sealed class CrashBreadcrumbBuffer
    {
        private readonly int _capacity;
        private readonly Queue<LogEntry> _entries = new Queue<LogEntry>();

        public CrashBreadcrumbBuffer(int capacity = 100)
        {
            _capacity = capacity < 10 ? 10 : capacity;
        }

        public int Count => _entries.Count;

        public void Add(LogEntry entry)
        {
            if (entry == null)
                return;

            while (_entries.Count >= _capacity)
                _entries.Dequeue();

            _entries.Enqueue(entry);
        }

        public List<LogEntry> Snapshot()
        {
            return new List<LogEntry>(_entries);
        }

        public string ToMultilineString()
        {
            var lines = new System.Text.StringBuilder();
            foreach (var entry in _entries)
                lines.AppendLine(entry.ToString());
            return lines.ToString();
        }

        public void Clear()
        {
            _entries.Clear();
        }
    }
}
