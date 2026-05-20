using System.Collections.Generic;
using UnityEngine;

namespace CoreRacer.Services.Analytics
{
    /// <summary>
    /// Dependency-safe Firebase adapter. It compiles without Firebase installed. When the Firebase SDK is
    /// installed, define CORE_RACER_FIREBASE and replace the guarded conversion with your SDK version's API.
    /// </summary>
    public sealed class FirebaseAnalyticsServiceAdapter : IAnalyticsService
    {
        public void Track(string eventName, IReadOnlyDictionary<string, object> parameters = null)
        {
#if CORE_RACER_FIREBASE
            Debug.Log($"Firebase analytics enabled event: {eventName}");
#else
            Debug.Log($"Firebase analytics adapter disabled event: {eventName}");
#endif
        }
    }
}
