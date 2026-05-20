namespace CoreRacer.Services.Notifications
{
    public sealed class NoOpPushNotificationService : IPushNotificationService
    {
        public void ScheduleDailyReminder(string title, string body, System.TimeSpan localTime) { }
        public void ClearAll() { }
    }
}
