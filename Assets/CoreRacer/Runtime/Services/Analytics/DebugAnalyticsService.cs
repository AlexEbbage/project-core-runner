using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CoreRacer.Services.Analytics
{
    public sealed class DebugAnalyticsService : IAnalyticsService
    {
        public bool LogEvents = true;

        public void Track(string eventName, IReadOnlyDictionary<string, object> parameters = null)
        {
            if (!LogEvents)
                return;

            var sb = new StringBuilder();
            sb.Append("Analytics: ").Append(eventName);
            if (parameters != null)
            {
                foreach (var pair in parameters)
                    sb.Append(" | ").Append(pair.Key).Append("=").Append(pair.Value);
            }

            Debug.Log(sb.ToString());
        }
    }
}
