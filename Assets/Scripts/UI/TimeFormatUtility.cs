using UnityEngine;

public static class TimeFormatUtility
{
    public static string FormatElapsedTime(float elapsedTime)
    {
        if (elapsedTime < 3600f)
        {
            int totalSeconds = Mathf.FloorToInt(elapsedTime);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"{minutes:00}:{seconds:00}";
        }

        int totalSecondsLong = Mathf.FloorToInt(elapsedTime);
        int hours = totalSecondsLong / 3600;
        int remainingSeconds = totalSecondsLong % 3600;
        int minutesLong = remainingSeconds / 60;
        return $"{hours:00}:{minutesLong:00}";
    }
}
