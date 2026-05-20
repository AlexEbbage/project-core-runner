using System;
using UnityEngine;

namespace CoreRacer.Services.Notifications
{
    /// <summary>
    /// Dependency-safe mobile notification adapter. It avoids package-specific APIs so the project compiles
    /// until you install Unity Mobile Notifications and add the guarded implementation.
    /// </summary>
    public sealed class MobilePushNotificationService : MonoBehaviour, IPushNotificationService
    {
        [SerializeField] private bool logRequests = true;

        public void ScheduleDailyReminder(string title, string body, TimeSpan localTime)
        {
#if CORE_RACER_MOBILE_NOTIFICATIONS
            // Add Unity.Notifications.Android / Unity.Notifications.iOS implementation here after package install.
#endif
            if (logRequests)
                Debug.Log($"[MobilePushNotificationService] Schedule daily reminder {localTime:hh\\:mm} - {title}: {body}", this);
        }

        public void ClearAll()
        {
#if CORE_RACER_MOBILE_NOTIFICATIONS
            // Cancel platform notifications here after package install.
#endif
            if (logRequests)
                Debug.Log("[MobilePushNotificationService] Clear all notifications", this);
        }
    }
}
