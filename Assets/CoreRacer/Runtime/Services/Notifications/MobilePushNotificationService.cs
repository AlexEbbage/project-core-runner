using System;
using UnityEngine;

#if CORE_RACER_MOBILE_NOTIFICATIONS && UNITY_ANDROID
using Unity.Notifications.Android;
#endif

#if CORE_RACER_MOBILE_NOTIFICATIONS && UNITY_IOS
using Unity.Notifications.iOS;
#endif

namespace CoreRacer.Services.Notifications
{
    /// <summary>
    /// Dependency-safe mobile notification adapter. It avoids package-specific APIs so the project compiles
    /// until you install Unity Mobile Notifications and add the guarded implementation.
    /// </summary>
    public sealed class MobilePushNotificationService : MonoBehaviour, IPushNotificationService
    {
        [SerializeField] private bool logRequests = true;
        [SerializeField] private string androidChannelId = "core_racer_daily";
        [SerializeField] private string androidChannelName = "Daily reminders";
        [SerializeField] private string androidChannelDescription = "Daily Core Racer reminders.";

#if CORE_RACER_MOBILE_NOTIFICATIONS && UNITY_ANDROID
        private bool _androidChannelRegistered;
#endif

        public void ScheduleDailyReminder(string title, string body, TimeSpan localTime)
        {
#if CORE_RACER_MOBILE_NOTIFICATIONS
            SchedulePlatformReminder(title, body, localTime);
#endif
            if (logRequests)
                Debug.Log($"[MobilePushNotificationService] Schedule daily reminder {localTime:hh\\:mm} - {title}: {body}", this);
        }

        public void ClearAll()
        {
#if CORE_RACER_MOBILE_NOTIFICATIONS
            ClearPlatformNotifications();
#endif
            if (logRequests)
                Debug.Log("[MobilePushNotificationService] Clear all notifications", this);
        }

#if CORE_RACER_MOBILE_NOTIFICATIONS
        private void SchedulePlatformReminder(string title, string body, TimeSpan localTime)
        {
#if UNITY_ANDROID
            EnsureAndroidChannel();
            var notification = new AndroidNotification
            {
                Title = title,
                Text = body,
                FireTime = NextLocalDateTime(localTime),
                RepeatInterval = TimeSpan.FromDays(1),
                ShouldAutoCancel = true
            };
            AndroidNotificationCenter.SendNotification(notification, androidChannelId);
#elif UNITY_IOS
            var fireTime = NextLocalDateTime(localTime);
            var notification = new iOSNotification
            {
                Identifier = "core_racer_daily_reminder",
                Title = title,
                Body = body,
                ShowInForeground = false,
                Trigger = new iOSNotificationCalendarTrigger
                {
                    Hour = fireTime.Hour,
                    Minute = fireTime.Minute,
                    Second = 0,
                    Repeats = true
                }
            };
            iOSNotificationCenter.ScheduleNotification(notification);
#endif
        }

        private static DateTime NextLocalDateTime(TimeSpan localTime)
        {
            var now = DateTime.Now;
            var next = new DateTime(now.Year, now.Month, now.Day, localTime.Hours, localTime.Minutes, 0, DateTimeKind.Local);
            return next <= now ? next.AddDays(1) : next;
        }

        private void ClearPlatformNotifications()
        {
#if UNITY_ANDROID
            AndroidNotificationCenter.CancelAllScheduledNotifications();
            AndroidNotificationCenter.CancelAllDisplayedNotifications();
#elif UNITY_IOS
            iOSNotificationCenter.RemoveAllScheduledNotifications();
            iOSNotificationCenter.RemoveAllDeliveredNotifications();
#endif
        }

#if UNITY_ANDROID
        private void EnsureAndroidChannel()
        {
            if (_androidChannelRegistered)
                return;

            var channel = new AndroidNotificationChannel
            {
                Id = androidChannelId,
                Name = androidChannelName,
                Importance = Importance.Default,
                Description = androidChannelDescription
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
            _androidChannelRegistered = true;
        }
#endif
#endif
    }
}
