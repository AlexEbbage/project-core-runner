using System;
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

    private Action<bool> _pendingCallback;
    private bool _isShowing;

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
            if (logEvents) Debug.Log("LevelPlayRewardedAdService: Using NEW Unity.Services.LevelPlay backend.");
            return;
        }

        // Try LEGACY ironSource plugin
        if (TryBindLegacyIronSource())
        {
            _backend = Backend.LegacyIronSource;
            CallLegacyInit();
            HookLegacyEvents();
            if (logEvents) Debug.Log("LevelPlayRewardedAdService: Using LEGACY IronSource backend.");
            return;
        }

        _backend = Backend.None;
        Debug.LogWarning("LevelPlayRewardedAdService: No LevelPlay/IronSource SDK detected. Rewarded ads unavailable.");
    }

    public bool IsRewardedAdReady()
    {
        if (_isShowing) return false;

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

    // ----------------------------
    // NEW LevelPlay (Unity.Services.LevelPlay)
    // ----------------------------
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
        catch (Exception e)
        {
            Debug.LogWarning("LevelPlayRewardedAdService: New LevelPlay Initialize failed: " + e);
        }
#endif
    }

    private bool CallNewIsAvailable()
    {
        try
        {
            object result = _lpIsAvailableMethod.Invoke(null, null);
            return result is bool b && b;
        }
        catch (Exception e)
        {
            Debug.LogWarning("LevelPlayRewardedAdService: New IsAdAvailable failed: " + e);
            return false;
        }
    }

    private void CallNewShow()
    {
        try
        {
            _lpShowMethod.Invoke(null, null);
        }
        catch (Exception e)
        {
            Debug.LogWarning("LevelPlayRewardedAdService: New ShowAd failed: " + e);
            Complete(false);
        }
    }

    private void HookNewLevelPlayEvents()
    {
        // Event names differ across versions; we’ll try a few.
        // We only need "reward granted" and "failed to show/closed without reward".
        TryHookStaticEvent(_lpRewardedType, "OnAdRewarded", OnRewardedSuccess_New);
        TryHookStaticEvent(_lpRewardedType, "OnAdFailedToShow", OnRewardedFailed_New);
        TryHookStaticEvent(_lpRewardedType, "OnAdClosed", OnRewardedClosed_New);
    }

    private void OnRewardedSuccess_New(object arg)
    {
        if (logEvents) Debug.Log("LevelPlayRewardedAdService: Rewarded SUCCESS (new).");
        Complete(true);
    }

    private void OnRewardedFailed_New(object arg)
    {
        if (logEvents) Debug.Log("LevelPlayRewardedAdService: Rewarded FAILED TO SHOW (new).");
        Complete(false);
    }

    private void OnRewardedClosed_New(object arg)
    {
        // Some SDKs only fire close; if it closes without reward we treat as fail
        if (_isShowing)
        {
            if (logEvents) Debug.Log("LevelPlayRewardedAdService: Rewarded CLOSED (new).");
            // Don’t auto-fail if reward already completed
            // If still pending, count as fail:
            if (_pendingCallback != null)
                Complete(false);
        }
    }

    // ----------------------------
    // LEGACY ironSource (IronSource.Agent + IronSourceEvents)
    // ----------------------------
    private bool TryBindLegacyIronSource()
    {
        _isAgentType = FindType("IronSource.Agent");
        if (_isAgentType == null) return false;

        _isInitMethod = _isAgentType.GetMethod("init", BindingFlags.Public | BindingFlags.Static);
        _isIsAvailableMethod = _isAgentType.GetMethod("isRewardedVideoAvailable", BindingFlags.Public | BindingFlags.Static);
        _isShowMethod = _isAgentType.GetMethod("showRewardedVideo", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null)
                      ?? _isAgentType.GetMethod("showRewardedVideo", BindingFlags.Public | BindingFlags.Static);

        if (_isInitMethod == null || _isIsAvailableMethod == null || _isShowMethod == null)
            return false;

        _isEventsType = FindType("IronSourceEvents");
        if (_isEventsType == null)
        {
            // Some legacy versions use IronSourceRewardedVideoEvents
            _isEventsType = FindType("IronSourceRewardedVideoEvents");
        }

        // Events we want: rewarded, show fail, closed.
        // Names vary. We'll try multiple.
        if (_isEventsType != null)
        {
            _evRewarded = FindEventAny(_isEventsType,
                "onRewardedVideoAdRewardedEvent",
                "onAdRewardedEvent",
                "onRewardedVideoAdRewarded");

            _evShowFail = FindEventAny(_isEventsType,
                "onRewardedVideoAdShowFailedEvent",
                "onAdShowFailedEvent",
                "onRewardedVideoAdShowFailed");

            _evClosed = FindEventAny(_isEventsType,
                "onRewardedVideoAdClosedEvent",
                "onAdClosedEvent",
                "onRewardedVideoAdClosed");
        }

        return true;
    }

    private void CallLegacyInit()
    {
#if UNITY_ANDROID
        try
        {
            // init(string appKey)
            _isInitMethod.Invoke(null, new object[] { androidAppKey });
        }
        catch (Exception e)
        {
            Debug.LogWarning("LevelPlayRewardedAdService: Legacy init failed: " + e);
        }
#endif
    }

    private bool CallLegacyIsAvailable()
    {
        try
        {
            object result = _isIsAvailableMethod.Invoke(null, null);
            return result is bool b && b;
        }
        catch (Exception e)
        {
            Debug.LogWarning("LevelPlayRewardedAdService: Legacy isRewardedVideoAvailable failed: " + e);
            return false;
        }
    }

    private void CallLegacyShow()
    {
        try
        {
            // Some versions require placement name, some don’t.
            if (_isShowMethod.GetParameters().Length == 1)
                _isShowMethod.Invoke(null, new object[] { "DefaultRewardedVideo" });
            else
                _isShowMethod.Invoke(null, null);
        }
        catch (Exception e)
        {
            Debug.LogWarning("LevelPlayRewardedAdService: Legacy showRewardedVideo failed: " + e);
            Complete(false);
        }
    }

    private void HookLegacyEvents()
    {
        if (_isEventsType == null) return;

        // Rewarded success
        if (_evRewarded != null)
        {
            _dOnRewarded = CreateCompatibleDelegate(_evRewarded.EventHandlerType, nameof(OnRewardedSuccess_Legacy));
            _evRewarded.AddEventHandler(null, _dOnRewarded);
        }

        // Show fail
        if (_evShowFail != null)
        {
            _dOnShowFail = CreateCompatibleDelegate(_evShowFail.EventHandlerType, nameof(OnRewardedFailed_Legacy));
            _evShowFail.AddEventHandler(null, _dOnShowFail);
        }

        // Closed
        if (_evClosed != null)
        {
            _dOnClosed = CreateCompatibleDelegate(_evClosed.EventHandlerType, nameof(OnRewardedClosed_Legacy));
            _evClosed.AddEventHandler(null, _dOnClosed);
        }
    }

    private void OnDestroy()
    {
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

    private void OnRewardedSuccess_Legacy()
    {
        if (logEvents) Debug.Log("LevelPlayRewardedAdService: Rewarded SUCCESS (legacy).");
        Complete(true);
    }

    private void OnRewardedFailed_Legacy()
    {
        if (logEvents) Debug.Log("LevelPlayRewardedAdService: Rewarded FAILED TO SHOW (legacy).");
        Complete(false);
    }

    private void OnRewardedClosed_Legacy()
    {
        if (_isShowing)
        {
            if (logEvents) Debug.Log("LevelPlayRewardedAdService: Rewarded CLOSED (legacy).");
            if (_pendingCallback != null)
                Complete(false);
        }
    }

    // ----------------------------
    // Completion
    // ----------------------------
    private void Complete(bool success)
    {
        if (!_isShowing && _pendingCallback == null) return;

        _isShowing = false;

        var cb = _pendingCallback;
        _pendingCallback = null;

        cb?.Invoke(success);
    }

    // ----------------------------
    // Helpers
    // ----------------------------
    private static Type FindType(string fullName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type t = asm.GetType(fullName);
            if (t != null) return t;
        }
        return null;
    }

    private static EventInfo FindEventAny(Type type, params string[] names)
    {
        foreach (var n in names)
        {
            var ev = type.GetEvent(n, BindingFlags.Public | BindingFlags.Static);
            if (ev != null) return ev;
        }
        return null;
    }

    private bool TryHookStaticEvent(Type declaringType, string eventName, Action<object> handler)
    {
        try
        {
            var ev = declaringType.GetEvent(eventName, BindingFlags.Public | BindingFlags.Static);
            if (ev == null) return false;

            // Create delegate compatible with event handler type that calls handler(arg)
            var del = CreateDelegateForEvent(ev.EventHandlerType, handler);
            ev.AddEventHandler(null, del);
            return true;
        }
        catch
        {
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

    private Delegate CreateCompatibleDelegate(Type handlerType, string methodNameOnThis)
    {
        // Try to bind to a parameterless method on this component (works for Action-style events).
        var mi = GetType().GetMethod(methodNameOnThis, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (mi == null) return null;

        try
        {
            return Delegate.CreateDelegate(handlerType, this, mi);
        }
        catch
        {
            // If signature mismatched, fall back to parameterless wrapper if possible
            return null;
        }
    }
}
