using System;

namespace CoreRacer.Services.Notifications
{
    public sealed class LocalNotificationScheduler
    {
        private readonly IPushNotificationService _notifications;
        private readonly NotificationTemplateConfig _templates;

        public LocalNotificationScheduler(IPushNotificationService notifications, NotificationTemplateConfig templates)
        {
            _notifications = notifications;
            _templates = templates;
        }

        public void ScheduleTemplate(string templateId)
        {
            var template = _templates != null ? _templates.Get(templateId) : null;
            if (template == null) return;
            _notifications?.ScheduleDailyReminder(template.Title, template.Body, new TimeSpan(template.Hour, template.Minute, 0));
        }

        public void ClearAll()
        {
            _notifications?.ClearAll();
        }
    }
}
