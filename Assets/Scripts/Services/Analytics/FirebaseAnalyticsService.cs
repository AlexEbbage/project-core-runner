// FirebaseAnalyticsService.cs
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Firebase Analytics adapter that avoids hard compile-time dependencies by using reflection.
/// - If Firebase is present, it will initialize FirebaseApp (dependencies check) and then log events.
/// - If Firebase is missing, it will safely no-op (optionally logging to Console).
///
/// Attach to a GameObject (e.g., ServicesRoot).
/// </summary>
public class FirebaseAnalyticsService : MonoBehaviour, IAnalyticsService
{
    [SerializeField] private bool logToConsoleWhenFirebaseMissing = true;
    [SerializeField] private int maxQueuedEvents = 50;
    [SerializeField] private int maxParametersPerEvent = 25;

    private bool _firebaseReady;
    private bool _firebaseMissing;

    // Type/method caches
    private Type _firebaseAppType;
    private Type _dependencyStatusType;
    private Type _firebaseAnalyticsType;
    private Type _firebaseParameterType;

    private object _dependencyStatusAvailableValue;

    private System.Reflection.MethodInfo _checkAndFixDependenciesAsync;
    private System.Reflection.MethodInfo _logEventString;
    private System.Reflection.MethodInfo _logEventWithParams;

    private readonly Queue<(string name, Dictionary<string, object> parameters)> _queuedEvents =
        new Queue<(string name, Dictionary<string, object> parameters)>();

    private void Awake()
    {
        TryBindFirebaseTypes();
        TryInitializeFirebase();
    }

    public void LogEvent(string eventName)
    {
        LogEvent(eventName, null);
    }

    public void LogEvent(string eventName, Dictionary<string, object> parameters)
    {
        if (string.IsNullOrEmpty(eventName))
            return;

        eventName = SanitizeName(eventName, fallbackPrefix: "event_");
        parameters = SanitizeParameters(parameters);

        if (_firebaseMissing)
        {
            if (logToConsoleWhenFirebaseMissing)
                Debug.Log($"[Analytics/Firebase Missing] {eventName} ({parameters?.Count ?? 0} params)");
            return;
        }

        if (!_firebaseReady)
        {
            var queueLimit = Math.Max(0, maxQueuedEvents);
            if (_queuedEvents.Count >= queueLimit)
            {
                if (_queuedEvents.Count > 0)
                    _queuedEvents.Dequeue();

                if (logToConsoleWhenFirebaseMissing)
                    Debug.LogWarning($"Firebase not ready. Dropping oldest event to queue '{eventName}'. Queue limit reached.");
            }

            _queuedEvents.Enqueue((eventName, parameters));
            return;
        }

        TryLogEventNow(eventName, parameters);
    }

    private void TryBindFirebaseTypes()
    {
        // Firebase.FirebaseApp
        _firebaseAppType = FindType("Firebase.FirebaseApp");
        _dependencyStatusType = FindType("Firebase.DependencyStatus");

        // Firebase.Analytics.FirebaseAnalytics
        _firebaseAnalyticsType = FindType("Firebase.Analytics.FirebaseAnalytics");
        _firebaseParameterType = FindType("Firebase.Analytics.Parameter");

        if (_firebaseAppType == null || _dependencyStatusType == null || _firebaseAnalyticsType == null)
        {
            _firebaseMissing = true;
            return;
        }

        _checkAndFixDependenciesAsync = _firebaseAppType.GetMethod("CheckAndFixDependenciesAsync",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        _logEventString = _firebaseAnalyticsType.GetMethod("LogEvent",
            new[] { typeof(string) });

        // Overload: LogEvent(string, Parameter[])
        if (_firebaseParameterType != null)
        {
            var parameterArrayType = _firebaseParameterType.MakeArrayType();
            _logEventWithParams = _firebaseAnalyticsType.GetMethod("LogEvent",
                new[] { typeof(string), parameterArrayType });
        }

        // Cache DependencyStatus.Available
        try
        {
            _dependencyStatusAvailableValue = Enum.Parse(_dependencyStatusType, "Available");
        }
        catch
        {
            _dependencyStatusAvailableValue = null;
        }
    }

    private void TryInitializeFirebase()
    {
        if (_firebaseMissing)
            return;

        if (_checkAndFixDependenciesAsync == null)
        {
            // If the method is missing, assume Firebase is already initialized.
            _firebaseReady = true;
            FlushQueue();
            return;
        }

        try
        {
            // Task<DependencyStatus> task = FirebaseApp.CheckAndFixDependenciesAsync();
            var taskObj = _checkAndFixDependenciesAsync.Invoke(null, null);
            if (taskObj == null)
            {
                _firebaseReady = true;
                FlushQueue();
                return;
            }

            // task.ContinueWith(t => { ... });
            var taskType = taskObj.GetType();
            var continueWith = taskType.GetMethod("ContinueWith",
                new[] { typeof(Action<>).MakeGenericType(taskType) });

            if (continueWith == null)
            {
                // Can't attach; assume ready and proceed.
                _firebaseReady = true;
                FlushQueue();
                return;
            }

            // Build Action<Task<DependencyStatus>> via lambda target method on this instance.
            Action<object> onCompleted = (completedTaskObj) =>
            {
                try
                {
                    // completedTask.Result
                    var resultProp = completedTaskObj.GetType().GetProperty("Result");
                    var status = resultProp != null ? resultProp.GetValue(completedTaskObj) : null;

                    if (_dependencyStatusAvailableValue != null && status != null && status.Equals(_dependencyStatusAvailableValue))
                    {
                        _firebaseReady = true;
                        FlushQueue();
                    }
                    else
                    {
                        _firebaseReady = false;
                        if (logToConsoleWhenFirebaseMissing)
                            Debug.LogWarning($"Firebase dependencies not available (status={status}). Analytics disabled.");
                    }
                }
                catch (Exception ex)
                {
                    _firebaseReady = false;
                    if (logToConsoleWhenFirebaseMissing)
                        Debug.LogWarning($"Firebase init callback failed. Analytics disabled. {ex.Message}");
                }
            };

            // Convert Action<object> into Action<Task<DependencyStatus>> compatible delegate
            var actionGenericType = typeof(Action<>).MakeGenericType(taskType);
            var del = Delegate.CreateDelegate(actionGenericType, onCompleted.Target, onCompleted.Method);

            continueWith.Invoke(taskObj, new object[] { del });
        }
        catch (Exception ex)
        {
            _firebaseReady = false;
            if (logToConsoleWhenFirebaseMissing)
                Debug.LogWarning($"Firebase init failed. Analytics disabled. {ex.Message}");
        }
    }

    private void FlushQueue()
    {
        while (_queuedEvents.Count > 0)
        {
            var (name, parameters) = _queuedEvents.Dequeue();
            TryLogEventNow(name, parameters);
        }
    }

    private void TryLogEventNow(string eventName, Dictionary<string, object> parameters)
    {
        try
        {
            if (parameters == null || parameters.Count == 0 || _logEventWithParams == null || _firebaseParameterType == null)
            {
                _logEventString?.Invoke(null, new object[] { eventName });
                return;
            }

            var paramArray = Array.CreateInstance(_firebaseParameterType, parameters.Count);
            int i = 0;

            foreach (var kvp in parameters)
            {
                var p = CreateFirebaseParameter(kvp.Key, kvp.Value);
                if (p != null)
                {
                    paramArray.SetValue(p, i);
                    i++;
                }
            }

            // Trim if some params were null
            if (i != parameters.Count)
            {
                var trimmed = Array.CreateInstance(_firebaseParameterType, i);
                Array.Copy(paramArray, trimmed, i);
                paramArray = trimmed;
            }

            _logEventWithParams.Invoke(null, new object[] { eventName, paramArray });
        }
        catch (Exception ex)
        {
            if (logToConsoleWhenFirebaseMissing)
                Debug.LogWarning($"Firebase LogEvent failed for '{eventName}'. {ex.Message}");
        }
    }

    private object CreateFirebaseParameter(string key, object value)
    {
        if (string.IsNullOrEmpty(key) || value == null || _firebaseParameterType == null)
            return null;

        try
        {
            // Common ctor overloads:
            // Parameter(string name, string value)
            // Parameter(string name, long value)
            // Parameter(string name, double value)
            var ctors = _firebaseParameterType.GetConstructors();
            foreach (var ctor in ctors)
            {
                var ps = ctor.GetParameters();
                if (ps.Length != 2) continue;
                if (ps[0].ParameterType != typeof(string)) continue;

                var valueType = ps[1].ParameterType;

                if (valueType == typeof(string))
                    return ctor.Invoke(new object[] { key, CoerceToString(value) });

                if (valueType == typeof(long))
                {
                    if (TryCoerceLong(value, out long l))
                        return ctor.Invoke(new object[] { key, l });
                }

                if (valueType == typeof(double))
                {
                    if (TryCoerceDouble(value, out double d))
                        return ctor.Invoke(new object[] { key, d });
                }
            }

            // Fallback: try string
            return Activator.CreateInstance(_firebaseParameterType, new object[] { key, CoerceToString(value) });
        }
        catch
        {
            return null;
        }
    }

    private static bool TryCoerceLong(object value, out long result)
    {
        switch (value)
        {
            case long l: result = l; return true;
            case int i: result = i; return true;
            case short s: result = s; return true;
            case byte b: result = b; return true;
            case bool bo: result = bo ? 1 : 0; return true;
            case float f: result = (long)f; return true;
            case double d: result = (long)d; return true;
            case string str when long.TryParse(str, out var parsed): result = parsed; return true;
            default: result = 0; return false;
        }
    }

    private static bool TryCoerceDouble(object value, out double result)
    {
        switch (value)
        {
            case double d: result = d; return true;
            case float f: result = f; return true;
            case int i: result = i; return true;
            case long l: result = l; return true;
            case bool bo: result = bo ? 1.0 : 0.0; return true;
            case string str when double.TryParse(str, out var parsed): result = parsed; return true;
            default: result = 0.0; return false;
        }
    }

    private Dictionary<string, object> SanitizeParameters(Dictionary<string, object> parameters)
    {
        if (parameters == null || parameters.Count == 0)
            return parameters;

        var sanitized = new Dictionary<string, object>(parameters.Count);
        int count = 0;
        foreach (var kvp in parameters)
        {
            if (count >= Math.Max(0, maxParametersPerEvent))
                break;

            var key = SanitizeName(kvp.Key, fallbackPrefix: "param_");
            if (string.IsNullOrEmpty(key))
                continue;

            var value = CoerceParameterValue(kvp.Value);
            if (value == null)
                continue;

            sanitized[key] = value;
            count++;
        }

        return sanitized;
    }

    private static string SanitizeName(string name, string fallbackPrefix)
    {
        if (string.IsNullOrWhiteSpace(name))
            return fallbackPrefix + "unnamed";

        const int maxLength = 40;
        var trimmed = name.Trim();

        var sb = new System.Text.StringBuilder(trimmed.Length);
        for (int i = 0; i < trimmed.Length; i++)
        {
            char c = trimmed[i];
            if ((c >= 'a' && c <= 'z') ||
                (c >= 'A' && c <= 'Z') ||
                (c >= '0' && c <= '9') ||
                c == '_')
            {
                sb.Append(c);
            }
            else
            {
                sb.Append('_');
            }
        }

        var sanitized = sb.ToString();
        if (sanitized.Length == 0 || !(sanitized[0] >= 'a' && sanitized[0] <= 'z'))
            sanitized = fallbackPrefix + sanitized;

        if (sanitized.StartsWith("firebase_", StringComparison.OrdinalIgnoreCase) ||
            sanitized.StartsWith("google_", StringComparison.OrdinalIgnoreCase))
            sanitized = "app_" + sanitized;

        if (sanitized.Length > maxLength)
            sanitized = sanitized.Substring(0, maxLength);

        return sanitized;
    }

    private static object CoerceParameterValue(object value)
    {
        if (value == null)
            return null;

        if (value is DateTime dt)
            return dt.ToString("o");

        if (value is string)
            return CoerceToString(value);

        return value;
    }

    private static string CoerceToString(object value)
    {
        if (value == null)
            return string.Empty;

        if (value is DateTime dt)
            return dt.ToString("o");

        var str = value.ToString();
        const int maxLength = 100;
        return str != null && str.Length > maxLength ? str.Substring(0, maxLength) : str;
    }

    private static Type FindType(string fullName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var t = asm.GetType(fullName, throwOnError: false);
                if (t != null) return t;
            }
            catch { /* ignore */ }
        }

        return null;
    }
}
