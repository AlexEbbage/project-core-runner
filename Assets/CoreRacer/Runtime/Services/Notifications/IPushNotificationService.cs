using System;

namespace CoreRacer.Services.Notifications
{
    public interface IPushNotificationService
    {
        void ScheduleDailyReminder(string title, string body, TimeSpan localTime);
        void ClearAll();
    }
}
