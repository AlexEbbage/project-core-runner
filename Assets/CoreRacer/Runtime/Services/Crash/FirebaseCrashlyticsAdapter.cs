using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreRacer.Services.Crash
{
    public sealed class FirebaseCrashlyticsAdapter : MonoBehaviour, ICrashReportingService
    {
        public void SetUserId(string userId)
        {
#if CORE_RACER_FIREBASE_CRASHLYTICS
            Firebase.Crashlytics.Crashlytics.SetUserId(userId);
#else
            Debug.Log("[CrashlyticsAdapter] SetUserId " + userId, this);
#endif
        }

        public void SetCustomKey(string key, string value)
        {
#if CORE_RACER_FIREBASE_CRASHLYTICS
            Firebase.Crashlytics.Crashlytics.SetCustomKey(key, value);
#else
            Debug.Log($"[CrashlyticsAdapter] Key {key}={value}", this);
#endif
        }

        public void LogBreadcrumb(string message)
        {
#if CORE_RACER_FIREBASE_CRASHLYTICS
            Firebase.Crashlytics.Crashlytics.Log(message);
#else
            Debug.Log("[CrashlyticsAdapter] " + message, this);
#endif
        }

        public void RecordException(Exception exception, IReadOnlyDictionary<string, string> context = null)
        {
#if CORE_RACER_FIREBASE_CRASHLYTICS
            Firebase.Crashlytics.Crashlytics.LogException(exception);
#else
            Debug.LogException(exception, this);
#endif
        }
    }
}
