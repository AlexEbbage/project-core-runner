using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

#if CORE_RACER_FIREBASE
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
#endif

namespace CoreRacer.Services.Analytics
{
    /// <summary>
    /// Dependency-safe Firebase adapter. It compiles without Firebase installed and sends events only when
    /// CORE_RACER_FIREBASE is enabled for a project with verified Firebase Analytics assemblies.
    /// </summary>
    public sealed class FirebaseAnalyticsServiceAdapter : MonoBehaviour, IAnalyticsService
    {
#if CORE_RACER_FIREBASE
        private bool _initialized;
        private bool _initializationFailed;

        private void Awake()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.Result != DependencyStatus.Available)
                {
                    _initializationFailed = true;
                    Debug.LogWarning("[FirebaseAnalytics] Firebase dependencies are unavailable: " + (task.IsFaulted ? task.Exception?.Message : task.Result.ToString()), this);
                    return;
                }

                _initialized = true;
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            });
        }
#endif

        public void Track(string eventName, IReadOnlyDictionary<string, object> parameters = null)
        {
#if CORE_RACER_FIREBASE
            if (string.IsNullOrWhiteSpace(eventName))
                return;

            if (_initializationFailed)
                return;

            if (!_initialized)
            {
                Debug.Log($"[FirebaseAnalytics] Dropped event before initialization: {eventName}", this);
                return;
            }

            var firebaseParameters = ToFirebaseParameters(parameters);
            if (firebaseParameters.Length == 0)
            {
                FirebaseAnalytics.LogEvent(eventName);
                return;
            }

            FirebaseAnalytics.LogEvent(eventName, firebaseParameters);
#else
            Debug.Log($"Firebase analytics adapter disabled event: {eventName}");
#endif
        }

#if CORE_RACER_FIREBASE
        private static Parameter[] ToFirebaseParameters(IReadOnlyDictionary<string, object> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return new Parameter[0];

            var result = new List<Parameter>(parameters.Count);
            foreach (var pair in parameters)
            {
                if (string.IsNullOrWhiteSpace(pair.Key) || pair.Value == null)
                    continue;

                switch (pair.Value)
                {
                    case int value:
                        result.Add(new Parameter(pair.Key, value));
                        break;
                    case long value:
                        result.Add(new Parameter(pair.Key, value));
                        break;
                    case float value:
                        result.Add(new Parameter(pair.Key, value));
                        break;
                    case double value:
                        result.Add(new Parameter(pair.Key, value));
                        break;
                    case bool value:
                        result.Add(new Parameter(pair.Key, value ? 1L : 0L));
                        break;
                    case string value:
                        result.Add(new Parameter(pair.Key, value));
                        break;
                    default:
                        result.Add(new Parameter(pair.Key, System.Convert.ToString(pair.Value, CultureInfo.InvariantCulture)));
                        break;
                }
            }

            return result.ToArray();
        }
#endif
    }
}
