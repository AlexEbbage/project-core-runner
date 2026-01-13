// LevelPlayRewardedAdService.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// LevelPlay rewarded ads adapter that avoids compile errors by using reflection.
/// Supports both:
/// - New Unity LevelPlay package (Unity.Services.LevelPlay.*)
/// - Legacy ironSource plugin (IronSource.Agent / IronSourceEvents)
///
/// Attach to a GameObject (e.g., ServicesRoot). Set Android App Key.
/// </summary>
public class LevelPlayRewardedAdService : MonoBehaviour, IRewardedAdService
{
    [Header("LevelPlay")]
    [SerializeField] private string androidAppKey = "YOUR_LEVELPLAY_ANDROID_APP_KEY";
    [SerializeField] private bool initializeOnStart = true;
    [SerializeField] private bool logEvents = true;
    [Tooltip("Fail-safe timeout (seconds) in case the SDK never calls back.")]
    [SerializeField] private float showTimeoutSeconds = 30f;

    private Action<bool> _pendingCallback;
    private bool _isShowing;
    private Coroutine _showTimeoutRoutine;

    // Keep NEW LevelPlay event delegates alive so we can unsubscribe cleanly.
    private readonly Dictionary<string, Delegate> _newEventDelegates = new Dictionary<string, Delegate>();

    // Which backend did we detect?
    private enum Backend { None, NewLevelPlay, LegacyIronSource }
    private Backend _backend = Backend.None;

    // Reflection caches (new LevelPlay)
    private Type _lpSdkType;
    private Type _lpRewardedType;
    private MethodInfo _lpInitializeMethod;
    private MethodInfo _lpIsAvailableMethod;
    private MethodInfo _lpShowMethod;

    // Reflection caches (legacy IronSource)
    private Type _isAgentType;
    private MethodInfo _isInitMethod;
    private MethodInfo _isIsAvailableMethod;
    private MethodInfo _isShowMethod;

    private Type _isEventsType;
    private EventInfo _evRewarded;
    private EventInfo _evShowFail;
    private EventInfo _evClosed;

    // Keep delegates alive (so event unsubscription works)
    private Delegate _dOnRewarded;
    private Delegate _dOnShowFail;
    private Delegate _dOnClosed;

    private void Start()
    {
        if (initializeOnStart)
            Initialize();
    }

    public void Initialize()
    {
        if (_backend != Backend.None) return;

        // Try NEW LevelPlay first
        if (TryBindNewLevelPlay())
        {
            _backend = Backend.NewLevelPlay;
            CallNewLevelPlayInitialize();
            HookNewLevelPlayEvents();
            if (logEvents) Debug.Log("LevelPlayRewardedAdService: Using NEW Unity LevelPlay backend.");
            return;
        }

        // Fallback to legacy IronSource
        if (TryBindLegacyIronSource())
        {
            _backend = Backend.LegacyIronSource;
            CallLegacyInitialize();
            HookLegacyIronSourceEvents();
            if (logEvents) Debug.Log("LevelPlayRewardedAdService: Using LEGACY IronSource backend.");
            return;
        }

        _backend = Backend.None;
        if (logEvents) Debug.LogWarning("LevelPlayRewardedAdService: No LevelPlay/IronSource backend found. Ads will be unavailable.");
    }

    public bool IsRewardedAdReady()
    {
        if (logEvents) Debug.Log($"LevelPlayRewardedAdService: IsRewardedAdReady backend: {_backend}");
        if (_backend == Backend.None)
        {
            Initialize();
            if (_backend == Backend.None)
                return false;
        }

        switch (_backend)
        {
            case Backend.NewLevelPlay:
                return CallNewIsAvailable();
            case Backend.LegacyIronSource:
                return CallLegacyIsAvailable();
            default:
                return false;
        }
    }

    public void ShowRewardedAd(Action<bool> onCompleted)
    {
        if (_isShowing)
        {
            if (logEvents) Debug.Log("LevelPlayRewardedAdService: already showing.");
            return;
        }

        if (_backend == Backend.None)
        {
            Initialize();
            if (_backend == Backend.None)
            {
                onCompleted?.Invoke(false);
                return;
            }
        }

        if (!IsRewardedAdReady())
        {
            if (logEvents) Debug.Log("LevelPlayRewardedAdService: rewarded not ready.");
            onCompleted?.Invoke(false);
            return;
        }

        _pendingCallback = onCompleted;
        _isShowing = true;

        // Fail-safe: if the SDK never calls back, complete as failure.
        StartShowTimeout();

        switch (_backend)
        {
            case Backend.NewLevelPlay:
                CallNewShow();
                break;
            case Backend.LegacyIronSource:
                CallLegacyShow();
                break;
        }
    }

    private void Complete(bool success)
    {
        if (!_isShowing && _pendingCallback == null) return;

        _isShowing = false;
        StopShowTimeout();

        var cb = _pendingCallback;
        _pendingCallback = null;

        cb?.Invoke(success);
    }

    private void StartShowTimeout()
    {
        StopShowTimeout();
        if (showTimeoutSeconds <= 0f) return;
        _showTimeoutRoutine = StartCoroutine(ShowTimeoutRoutine(showTimeoutSeconds));
    }

    private void StopShowTimeout()
    {
        if (_showTimeoutRoutine == null) return;
        StopCoroutine(_showTimeoutRoutine);
        _showTimeoutRoutine = null;
    }

    private IEnumerator ShowTimeoutRoutine(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);

        if (_isShowing && _pendingCallback != null)
        {
            if (logEvents) Debug.LogWarning("LevelPlayRewardedAdService: Show timed out. Completing as failure.");
            Complete(false);
        }
    }

    // ---------------------------
    // New LevelPlay (Unity.Services.LevelPlay)
    // ---------------------------

    private bool TryBindNewLevelPlay()
    {
        // Types we look for:
        // Unity.Services.LevelPlay.LevelPlaySDK (Initialize)
        // Unity.Services.LevelPlay.LevelPlayRewardedAd (IsAdAvailable, ShowAd)
        _lpSdkType = FindType("Unity.Services.LevelPlay.LevelPlaySDK");
        _lpRewardedType = FindType("Unity.Services.LevelPlay.LevelPlayRewardedAd");

        if (_lpSdkType == null || _lpRewardedType == null)
            return false;

        _lpInitializeMethod = _lpSdkType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static);
        _lpIsAvailableMethod = _lpRewardedType.GetMethod("IsAdAvailable", BindingFlags.Public | BindingFlags.Static);
        _lpShowMethod = _lpRewardedType.GetMethod("ShowAd", BindingFlags.Public | BindingFlags.Static);

        return _lpInitializeMethod != null && _lpIsAvailableMethod != null && _lpShowMethod != null;
    }

    private void CallNewLevelPlayInitialize()
    {
#if UNITY_ANDROID
        try
        {
            _lpInitializeMethod.Invoke(null, new object[] { androidAppKey });
        }
        catch (Exception ex)
        {
            if (logEvents) Debug.LogWarning($"LevelPlayRewardedAdService: New LevelPlay Initialize failed: {ex.Message}");
        }
#else
        if (logEvents) Debug.Log("LevelPlayRewardedAdService: New LevelPlay initialize skipped (not Android).");
#endif
    }

    private bool CallNewIsAvailable()
    {
        try
        {
            var result = _lpIsAvailableMethod.Invoke(null, null);
            return result is bool b && b;
        }
        catch
        {
            return false;
        }
    }

    private void CallNewShow()
    {
        try
        {
            _lpShowMethod.Invoke(null, null);
        }
        catch (Exception ex)
        {
            if (logEvents) Debug.LogWarning($"LevelPlayRewardedAdService: New LevelPlay ShowAd failed: {ex.Message}");
            Complete(false);
        }
    }

    private void HookNewLevelPlayEvents()
    {
        // Event names differ across versions; we’ll try a few.
        // We only need "reward granted" and "failed to show/closed without reward".
        bool hookedAny = false;

        hookedAny |= TryHookStaticEvent(_lpRewardedType, "OnAdRewarded", OnRewardedSuccess_New);
        hookedAny |= TryHookStaticEvent(_lpRewardedType, "OnAdFailedToShow", OnRewardedFailed_New);
        hookedAny |= TryHookStaticEvent(_lpRewardedType, "OnAdClosed", OnRewardedClosed_New);

        if (!hookedAny && logEvents)
        {
            Debug.LogWarning("LevelPlayRewardedAdService: No New LevelPlay rewarded events were hooked. Check SDK version/signatures.");
        }
    }

    private void OnRewardedSuccess_New(object _)
    {
        if (logEvents) Debug.Log("LevelPlayRewardedAdService: New rewarded success.");
        Complete(true);
    }

    private void OnRewardedFailed_New(object _)
    {
        if (logEvents) Debug.Log("LevelPlayRewardedAdService: New rewarded failed to show.");
        Complete(false);
    }

    private void OnRewardedClosed_New(object _)
    {
        // Some SDKs may close without reward; if reward already completed, Complete() is a no-op.
        if (logEvents) Debug.Log("LevelPlayRewardedAdService: New rewarded closed.");
        Complete(false);
    }

    private bool TryHookStaticEvent(Type declaringType, string eventName, Action<object> handler)
    {
        try
        {
            var ev = declaringType.GetEvent(eventName, BindingFlags.Public | BindingFlags.Static);
            if (ev == null)
            {
                if (logEvents) Debug.Log($"LevelPlayRewardedAdService: New LevelPlay event not found: {declaringType.FullName}.{eventName}");
                return false;
            }

            // Create delegate compatible with event handler type that calls handler(arg)
            var del = CreateDelegateForEvent(ev.EventHandlerType, handler);
            if (del == null)
            {
                if (logEvents) Debug.LogWarning($"LevelPlayRewardedAdService: Could not create delegate for event: {declaringType.FullName}.{eventName} ({ev.EventHandlerType})");
                return false;
            }

            ev.AddEventHandler(null, del);
            _newEventDelegates[$"{declaringType.FullName}.{eventName}"] = del;

            if (logEvents) Debug.Log($"LevelPlayRewardedAdService: Hooked New LevelPlay event: {declaringType.FullName}.{eventName}");

            return true;
        }
        catch (Exception ex)
        {
            if (logEvents) Debug.LogWarning($"LevelPlayRewardedAdService: Failed to hook New LevelPlay event {declaringType?.FullName}.{eventName}: {ex.Message}");
            return false;
        }
    }

    private static Delegate CreateDelegateForEvent(Type eventHandlerType, Action<object> handler)
    {
        // Handle common signatures:
        // Action
        // Action<T>
        // EventHandler
        var invoke = eventHandlerType.GetMethod("Invoke");
        var pars = invoke.GetParameters();

        if (pars.Length == 0)
        {
            Action a = () => handler(null);
            return Delegate.CreateDelegate(eventHandlerType, a.Target, a.Method);
        }

        if (pars.Length == 1)
        {
            // Wrap param into object
            return Delegate.CreateDelegate(eventHandlerType, handler.Target,
                handler.Method, throwOnBindFailure: false);
        }

        // Fallback: create a dynamic method is overkill; we just won’t hook.
        return null;
    }

    // ---------------------------
    // Legacy ironSource (IronSource.Agent / IronSourceEvents)
    // ---------------------------

    private bool TryBindLegacyIronSource()
    {
        // Types we look for:
        // IronSource.Agent (init, isRewardedVideoAvailable, showRewardedVideo)
        // IronSourceEvents (onRewardedVideoAdRewardedEvent, onRewardedVideoAdShowFailedEvent, onRewardedVideoAdClosedEvent)
        _isAgentType = FindType("IronSource.Agent");
        _isEventsType = FindType("IronSourceEvents");

        if (_isAgentType == null || _isEventsType == null)
            return false;

        _isInitMethod = _isAgentType.GetMethod("init", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
        _isIsAvailableMethod = _isAgentType.GetMethod("isRewardedVideoAvailable", BindingFlags.Public | BindingFlags.Static);
        _isShowMethod = _isAgentType.GetMethod("showRewardedVideo", BindingFlags.Public | BindingFlags.Static);

        if (_isInitMethod == null || _isIsAvailableMethod == null || _isShowMethod == null)
            return false;

        // Events (names may vary; try the most common)
        _evRewarded = _isEventsType.GetEvent("onRewardedVideoAdRewardedEvent", BindingFlags.Public | BindingFlags.Static);
        _evShowFail = _isEventsType.GetEvent("onRewardedVideoAdShowFailedEvent", BindingFlags.Public | BindingFlags.Static);
        _evClosed = _isEventsType.GetEvent("onRewardedVideoAdClosedEvent", BindingFlags.Public | BindingFlags.Static);

        return true;
    }

    private void CallLegacyInitialize()
    {
#if UNITY_ANDROID
        try
        {
            _isInitMethod.Invoke(null, new object[] { androidAppKey });
        }
        catch (Exception ex)
        {
            if (logEvents) Debug.LogWarning($"LevelPlayRewardedAdService: Legacy init failed: {ex.Message}");
        }
#else
        if (logEvents) Debug.Log("LevelPlayRewardedAdService: Legacy init skipped (not Android).");
#endif
    }

    private bool CallLegacyIsAvailable()
    {
        try
        {
            var result = _isIsAvailableMethod.Invoke(null, null);
            return result is bool b && b;
        }
        catch
        {
            return false;
        }
    }

    private void CallLegacyShow()
    {
        try
        {
            _isShowMethod.Invoke(null, null);
        }
        catch (Exception ex)
        {
            if (logEvents) Debug.LogWarning($"LevelPlayRewardedAdService: Legacy show failed: {ex.Message}");
            Complete(false);
        }
    }

    private void HookLegacyIronSourceEvents()
    {
        // Rewarded
        if (_evRewarded != null)
        {
            _dOnRewarded = CreateCompatibleDelegate(_evRewarded.EventHandlerType, nameof(OnLegacyRewarded));
            if (_dOnRewarded != null) _evRewarded.AddEventHandler(null, _dOnRewarded);
        }

        // Show failed
        if (_evShowFail != null)
        {
            _dOnShowFail = CreateCompatibleDelegate(_evShowFail.EventHandlerType, nameof(OnLegacyShowFailed));
            if (_dOnShowFail != null) _evShowFail.AddEventHandler(null, _dOnShowFail);
        }

        // Closed
        if (_evClosed != null)
        {
            _dOnClosed = CreateCompatibleDelegate(_evClosed.EventHandlerType, nameof(OnLegacyClosed));
            if (_dOnClosed != null) _evClosed.AddEventHandler(null, _dOnClosed);
        }
    }

    private void OnLegacyRewarded()
    {
        if (logEvents) Debug.Log("LevelPlayRewardedAdService: Legacy rewarded success.");
        Complete(true);
    }

    private void OnLegacyShowFailed()
    {
        if (logEvents) Debug.Log("LevelPlayRewardedAdService: Legacy rewarded failed to show.");
        Complete(false);
    }

    private void OnLegacyClosed()
    {
        if (logEvents) Debug.Log("LevelPlayRewardedAdService: Legacy rewarded closed.");
        Complete(false);
    }

    private Delegate CreateCompatibleDelegate(Type handlerType, string methodNameOnThis)
    {
        var mi = GetType().GetMethod(methodNameOnThis, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (mi == null) return null;

        try
        {
            return Delegate.CreateDelegate(handlerType, this, mi);
        }
        catch
        {
            return null;
        }
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

    private void UnhookNewLevelPlayEvents()
    {
        if (_lpRewardedType == null) return;

        foreach (var kvp in _newEventDelegates)
        {
            try
            {
                // kvp.Key is "FullTypeName.EventName"
                var lastDot = kvp.Key.LastIndexOf('.');
                if (lastDot <= 0) continue;

                var typeName = kvp.Key.Substring(0, lastDot);
                var eventName = kvp.Key.Substring(lastDot + 1);

                var declaringType = FindType(typeName);
                if (declaringType == null) continue;

                var ev = declaringType.GetEvent(eventName, BindingFlags.Public | BindingFlags.Static);
                if (ev == null) continue;

                ev.RemoveEventHandler(null, kvp.Value);
            }
            catch
            {
                // Best effort only
            }
        }

        _newEventDelegates.Clear();
    }

    private void OnDestroy()
    {
        StopShowTimeout();

        // Clean up New LevelPlay event handlers (best effort)
        try
        {
            if (_backend == Backend.NewLevelPlay)
            {
                UnhookNewLevelPlayEvents();
            }
        }
        catch { /* ignore */ }

        // Clean up legacy event handlers (best effort)
        try
        {
            if (_backend == Backend.LegacyIronSource && _isEventsType != null)
            {
                if (_evRewarded != null && _dOnRewarded != null) _evRewarded.RemoveEventHandler(null, _dOnRewarded);
                if (_evShowFail != null && _dOnShowFail != null) _evShowFail.RemoveEventHandler(null, _dOnShowFail);
                if (_evClosed != null && _dOnClosed != null) _evClosed.RemoveEventHandler(null, _dOnClosed);
            }
        }
        catch { /* ignore */ }
    }
}
