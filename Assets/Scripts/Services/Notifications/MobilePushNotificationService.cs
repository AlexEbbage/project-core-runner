using System;
using UnityEngine;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

public class MobilePushNotificationService : MonoBehaviour, IPushNotificationService
{
    [Header("Reminder Settings")]
    [SerializeField] private string channelId = "reengagement";
    [SerializeField] private string channelName = "Re-engagement";
    [SerializeField] private string channelDescription = "Reminders to return to the game.";
    [SerializeField] private string reminderTitle = "We miss you!";
    [SerializeField] private string reminderBody = "Jump back in for another run.";
    [Min(0f)]
    [SerializeField] private float reminderDelayHours = 24f;
    [SerializeField] private bool scheduleOnAppExit = true;

    private bool _initialized;

    public void Initialize()
    {
        if (_initialized)
            return;

#if UNITY_ANDROID
        var channel = new AndroidNotificationChannel
        {
            Id = channelId,
            Name = channelName,
            Description = channelDescription,
            Importance = Importance.Default
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
        TryRequestAndroidPermission();
#endif


#if UNITY_EDITOR
        Debug.Log("MobilePushNotificationService: Initialized.");
#endif

        _initialized = true;
    }

    public void ScheduleReturnReminder()
    {
        if (!_initialized)
        {
            Initialize();
        }

        if (reminderDelayHours <= 0f)
        {
#if UNITY_EDITOR
            Debug.LogWarning("MobilePushNotificationService: reminderDelayHours must be greater than zero to schedule.");
#endif
            return;
        }

        CancelAll();
        var fireTime = DateTime.Now.AddHours(reminderDelayHours);

#if UNITY_ANDROID
        var notification = new AndroidNotification
        {
            Title = reminderTitle,
            Text = reminderBody,
            FireTime = fireTime
        };
        AndroidNotificationCenter.SendNotification(notification, channelId);
#endif


#if UNITY_EDITOR
        Debug.Log($"MobilePushNotificationService: Scheduled reminder in {reminderDelayHours} hours.");
#endif
    }

    public void CancelAll()
    {
#if UNITY_ANDROID
        AndroidNotificationCenter.CancelAllNotifications();
#endif


#if UNITY_EDITOR
        Debug.Log("MobilePushNotificationService: Cleared scheduled notifications.");
#endif
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (!scheduleOnAppExit)
            return;

        if (isPaused)
        {
            ScheduleReturnReminder();
        }
        else
        {
            CancelAll();
        }
    }

    private void OnApplicationQuit()
    {
        if (scheduleOnAppExit)
        {
            ScheduleReturnReminder();
        }
    }

#if UNITY_ANDROID
    private void TryRequestAndroidPermission()
    {
        try
        {
            var type = typeof(AndroidNotificationCenter);
            var requestMethod = type.GetMethod("RequestPermission")
                ?? type.GetMethod("RequestNotificationPermission");
            requestMethod?.Invoke(null, null);
        }
        catch (Exception ex)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"MobilePushNotificationService: Failed to request Android permission. {ex.Message}");
#endif
        }
    }
#endif
}
