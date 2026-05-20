using System;

namespace CoreRacer.UI
{
    public static class TimeFormatUtility
    {
        public static string FormatSeconds(float seconds)
        {
            var span = TimeSpan.FromSeconds(Math.Max(0, seconds));
            if (span.TotalHours >= 1)
                return $"{(int)span.TotalHours:0}:{span.Minutes:00}:{span.Seconds:00}";
            return $"{span.Minutes:0}:{span.Seconds:00}";
        }

        public static string FormatCompactDuration(TimeSpan span)
        {
            if (span.TotalDays >= 1) return $"{(int)span.TotalDays}d {span.Hours}h";
            if (span.TotalHours >= 1) return $"{(int)span.TotalHours}h {span.Minutes}m";
            if (span.TotalMinutes >= 1) return $"{(int)span.TotalMinutes}m";
            return $"{Math.Max(0, span.Seconds)}s";
        }
    }
}
