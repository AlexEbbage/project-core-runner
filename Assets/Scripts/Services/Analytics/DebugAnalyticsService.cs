using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple analytics implementation that logs events to the Console.
/// Replace this with a Firebase-backed implementation later.
/// Attach to a GameObject (e.g., ServicesRoot).
/// </summary>
public class DebugAnalyticsService : MonoBehaviour, IAnalyticsService
{
    [SerializeField] private bool logEvents = true;

    public void LogEvent(string eventName)
    {
        if (!logEvents) return;
        Debug.Log($"[Analytics] Event: {eventName}");
    }

    public void LogEvent(string eventName, Dictionary<string, object> parameters)
    {
        if (!logEvents) return;

        if (parameters == null || parameters.Count == 0)
        {
            Debug.Log($"[Analytics] Event: {eventName} (no params)");
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append($"[Analytics] Event: {eventName} | ");

        foreach (var kvp in parameters)
        {
            sb.Append($"{kvp.Key}={kvp.Value}; ");
        }

        Debug.Log(sb.ToString());
    }
}
