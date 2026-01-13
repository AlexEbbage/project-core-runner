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
    [Tooltip("Rewarded ad unit id (required for the new LevelPlay instance-based API).")]
    [SerializeField] private string rewardedAdUnitId = "YOUR_LEVELPLAY_REWARDED_UNIT_ID";
    [SerializeField] private bool initializeOnStart = true;
    [SerializeField] private bool logEvents = true;
    [Tooltip("Fail-safe timeout (seconds) in case the SDK never calls back.")]
    [SerializeField] private float showTimeoutSeconds = 30f;

    private Action<bool> _pendingCallback;
    private bool _isShowing;
    private Coroutine _showTimeoutRoutine;
    private IRewardedAdAdapter _adapter;

    private interface IRewardedAdAdapter
    {
        bool Initialize();
        bool IsReady();
        void Show();
        void Destroy();
    }

    private void Start()
    {
        if (initializeOnStart)
            Initialize();
    }

    public void Initialize()
    {
        if (_adapter != null)
            return;

        var newAdapter = new NewLevelPlayRewardedAdAdapter(androidAppKey, rewardedAdUnitId, logEvents, Complete);
        if (newAdapter.Initialize())
        {
            _adapter = newAdapter;
            if (logEvents) Debug.Log("LevelPlayRewardedAdService: Using NEW Unity LevelPlay backend.");
            return;
        }

        var legacyAdapter = new LegacyIronSourceRewardedAdAdapter(androidAppKey, logEvents, Complete);
        if (legacyAdapter.Initialize())
        {
            _adapter = legacyAdapter;
            if (logEvents) Debug.Log("LevelPlayRewardedAdService: Using LEGACY IronSource backend.");
            return;
        }

        if (logEvents) Debug.LogWarning("LevelPlayRewardedAdService: No LevelPlay/IronSource backend found. Ads will be unavailable.");
    }

    public bool IsRewardedAdReady()
    {
        if (logEvents) Debug.Log($"LevelPlayRewardedAdService: IsRewardedAdReady backend: {_adapter?.GetType().Name ?? "None"}");
        if (_adapter == null)
        {
            Initialize();
            if (_adapter == null)
                return false;
        }

        return _adapter.IsReady();
    }

    public void ShowRewardedAd(Action<bool> onCompleted)
    {
        if (_isShowing)
        {
            if (logEvents) Debug.Log("LevelPlayRewardedAdService: already showing.");
            return;
        }

        if (_adapter == null)
        {
            Initialize();
            if (_adapter == null)
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

        _adapter.Show();
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

    private void OnDestroy()
    {
        StopShowTimeout();
        _adapter?.Destroy();
        _adapter = null;
    }

    private sealed class NewLevelPlayRewardedAdAdapter : IRewardedAdAdapter
    {
        private struct NewEventSubscription
        {
            public EventInfo EventInfo;
            public object Target;
            public Delegate Handler;
        }

        private readonly string _androidAppKey;
        private readonly string _rewardedAdUnitId;
        private readonly bool _logEvents;
        private readonly Action<bool> _complete;

        private readonly List<NewEventSubscription> _newEventSubscriptions = new List<NewEventSubscription>();

        private Type _lpSdkType;
        private Type _lpRewardedType;
        private MethodInfo _lpInitializeMethod;
        private MethodInfo _lpIsAvailableMethod;
        private MethodInfo _lpShowMethod;
        private MethodInfo _lpLoadMethod;
        private object _lpRewardedInstance;
        private bool _lpUsesInstance;

        public NewLevelPlayRewardedAdAdapter(string androidAppKey, string rewardedAdUnitId, bool logEvents, Action<bool> complete)
        {
            _androidAppKey = androidAppKey;
            _rewardedAdUnitId = rewardedAdUnitId;
            _logEvents = logEvents;
            _complete = complete;
        }

        public bool Initialize()
        {
            if (!TryBindNewLevelPlay())
                return false;

            CallNewLevelPlayInitialize();
            HookNewLevelPlayEvents();
            return true;
        }

        public bool IsReady()
        {
            try
            {
                if (_lpUsesInstance && _lpRewardedInstance == null)
                {
                    CreateNewLevelPlayInstance();
                }

                if (_lpUsesInstance && _lpRewardedInstance == null)
                    return false;

                var target = _lpIsAvailableMethod.IsStatic ? null : _lpRewardedInstance;
                var result = _lpIsAvailableMethod.Invoke(target, null);
                return result is bool b && b;
            }
            catch
            {
                return false;
            }
        }

        public void Show()
        {
            try
            {
                if (_lpUsesInstance && _lpRewardedInstance == null)
                {
                    CreateNewLevelPlayInstance();
                }

                if (_lpUsesInstance && _lpRewardedInstance == null)
                {
                    throw new InvalidOperationException("LevelPlay rewarded instance is missing.");
                }

                var target = _lpShowMethod.IsStatic ? null : _lpRewardedInstance;
                _lpShowMethod.Invoke(target, null);
            }
            catch (Exception ex)
            {
                if (_logEvents) Debug.LogWarning($"LevelPlayRewardedAdService: New LevelPlay ShowAd failed: {ex.Message}");
                _complete(false);
            }
        }

        public void Destroy()
        {
            UnhookNewLevelPlayEvents();
        }

        private bool TryBindNewLevelPlay()
        {
            _lpSdkType = FindType("Unity.Services.LevelPlay.LevelPlaySDK")
                ?? FindTypeByName("LevelPlaySDK");
            _lpRewardedType = FindType("Unity.Services.LevelPlay.LevelPlayRewardedAd")
                ?? FindTypeByName("LevelPlayRewardedAd");

            if (_lpSdkType == null || _lpRewardedType == null)
                return false;

            _lpInitializeMethod = _lpSdkType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static);
            if (_lpInitializeMethod == null)
                return false;

            _lpIsAvailableMethod = _lpRewardedType.GetMethod("IsAdAvailable", BindingFlags.Public | BindingFlags.Static)
                ?? _lpRewardedType.GetMethod("IsAdAvailable", BindingFlags.Public | BindingFlags.Instance);
            _lpShowMethod = _lpRewardedType.GetMethod("ShowAd", BindingFlags.Public | BindingFlags.Static)
                ?? _lpRewardedType.GetMethod("ShowAd", BindingFlags.Public | BindingFlags.Instance);
            _lpLoadMethod = _lpRewardedType.GetMethod("LoadAd", BindingFlags.Public | BindingFlags.Static)
                ?? _lpRewardedType.GetMethod("LoadAd", BindingFlags.Public | BindingFlags.Instance);

            if (_lpIsAvailableMethod == null || _lpShowMethod == null)
                return false;

            _lpUsesInstance = !_lpIsAvailableMethod.IsStatic || !_lpShowMethod.IsStatic;
            return true;
        }

        private void CallNewLevelPlayInitialize()
        {
#if UNITY_ANDROID
            try
            {
                _lpInitializeMethod.Invoke(null, new object[] { _androidAppKey });
                if (_lpUsesInstance)
                {
                    CreateNewLevelPlayInstance();
                }
                else
                {
                    LoadNewLevelPlayAd();
                }
            }
            catch (Exception ex)
            {
                if (_logEvents) Debug.LogWarning($"LevelPlayRewardedAdService: New LevelPlay Initialize failed: {ex.Message}");
            }
#else
            if (_logEvents) Debug.Log("LevelPlayRewardedAdService: New LevelPlay initialize skipped (not Android).");
#endif
        }

        private void HookNewLevelPlayEvents()
        {
            bool hookedAny = false;

            var target = _lpUsesInstance ? _lpRewardedInstance : null;
            var flags = _lpUsesInstance ? BindingFlags.Public | BindingFlags.Instance : BindingFlags.Public | BindingFlags.Static;

            if (_lpUsesInstance && target == null)
            {
                if (_logEvents) Debug.LogWarning("LevelPlayRewardedAdService: Cannot hook instance events without a rewarded ad instance.");
                return;
            }

            hookedAny |= TryHookEvent(_lpRewardedType, target, flags, "OnAdRewarded", OnRewardedSuccess);
            hookedAny |= TryHookEvent(_lpRewardedType, target, flags, "OnAdFailedToShow", OnRewardedFailed);
            hookedAny |= TryHookEvent(_lpRewardedType, target, flags, "OnAdDisplayFailed", OnRewardedFailed);
            hookedAny |= TryHookEvent(_lpRewardedType, target, flags, "OnAdClosed", OnRewardedClosed);

            if (!hookedAny && _logEvents)
            {
                Debug.LogWarning("LevelPlayRewardedAdService: No New LevelPlay rewarded events were hooked. Check SDK version/signatures.");
            }
        }

        private void OnRewardedSuccess(object _)
        {
            if (_logEvents) Debug.Log("LevelPlayRewardedAdService: New rewarded success.");
            _complete(true);
            LoadNewLevelPlayAd();
        }

        private void OnRewardedFailed(object _)
        {
            if (_logEvents) Debug.Log("LevelPlayRewardedAdService: New rewarded failed to show.");
            _complete(false);
            LoadNewLevelPlayAd();
        }

        private void OnRewardedClosed(object _)
        {
            if (_logEvents) Debug.Log("LevelPlayRewardedAdService: New rewarded closed.");
            _complete(false);
            LoadNewLevelPlayAd();
        }

        private bool TryHookEvent(Type declaringType, object target, BindingFlags bindingFlags, string eventName, Action<object> handler)
        {
            try
            {
                var ev = declaringType.GetEvent(eventName, bindingFlags);
                if (ev == null)
                {
                    if (_logEvents) Debug.Log($"LevelPlayRewardedAdService: New LevelPlay event not found: {declaringType.FullName}.{eventName}");
                    return false;
                }

                var del = CreateDelegateForEvent(ev.EventHandlerType, handler);
                if (del == null)
                {
                    if (_logEvents) Debug.LogWarning($"LevelPlayRewardedAdService: Could not create delegate for event: {declaringType.FullName}.{eventName} ({ev.EventHandlerType})");
                    return false;
                }

                ev.AddEventHandler(target, del);
                _newEventSubscriptions.Add(new NewEventSubscription
                {
                    EventInfo = ev,
                    Target = target,
                    Handler = del
                });

                if (_logEvents) Debug.Log($"LevelPlayRewardedAdService: Hooked New LevelPlay event: {declaringType.FullName}.{eventName}");

                return true;
            }
            catch (Exception ex)
            {
                if (_logEvents) Debug.LogWarning($"LevelPlayRewardedAdService: Failed to hook New LevelPlay event {declaringType?.FullName}.{eventName}: {ex.Message}");
                return false;
            }
        }

        private void LoadNewLevelPlayAd()
        {
            if (_lpLoadMethod == null)
                return;

            var target = _lpLoadMethod.IsStatic ? null : _lpRewardedInstance;
            if (target == null && !_lpLoadMethod.IsStatic)
                return;

            try
            {
                _lpLoadMethod.Invoke(target, null);
            }
            catch (Exception ex)
            {
                if (_logEvents) Debug.LogWarning($"LevelPlayRewardedAdService: New LevelPlay LoadAd failed: {ex.Message}");
            }
        }

        private void CreateNewLevelPlayInstance()
        {
            if (_lpRewardedInstance != null)
                return;

            if (string.IsNullOrWhiteSpace(_rewardedAdUnitId))
            {
                if (_logEvents) Debug.LogWarning("LevelPlayRewardedAdService: Rewarded ad unit id is required for LevelPlay instance API.");
                return;
            }

            try
            {
                var ctor = _lpRewardedType.GetConstructor(new[] { typeof(string) });
                if (ctor != null)
                {
                    _lpRewardedInstance = ctor.Invoke(new object[] { _rewardedAdUnitId });
                }
                else
                {
                    _lpRewardedInstance = Activator.CreateInstance(_lpRewardedType);
                }
            }
            catch (Exception ex)
            {
                if (_logEvents) Debug.LogWarning($"LevelPlayRewardedAdService: Failed to create LevelPlay rewarded instance: {ex.Message}");
                _lpRewardedInstance = null;
            }

            if (_lpRewardedInstance != null)
            {
                LoadNewLevelPlayAd();
            }
        }

        private void UnhookNewLevelPlayEvents()
        {
            if (_newEventSubscriptions.Count == 0) return;

            foreach (var subscription in _newEventSubscriptions)
            {
                try
                {
                    subscription.EventInfo.RemoveEventHandler(subscription.Target, subscription.Handler);
                }
                catch
                {
                    // Best effort only
                }
            }

            _newEventSubscriptions.Clear();
        }
    }

    private sealed class LegacyIronSourceRewardedAdAdapter : IRewardedAdAdapter
    {
        private readonly string _androidAppKey;
        private readonly bool _logEvents;
        private readonly Action<bool> _complete;

        private Type _isAgentType;
        private MethodInfo _isInitMethod;
        private MethodInfo _isIsAvailableMethod;
        private MethodInfo _isShowMethod;

        private Type _isEventsType;
        private EventInfo _evRewarded;
        private EventInfo _evShowFail;
        private EventInfo _evClosed;

        private Delegate _dOnRewarded;
        private Delegate _dOnShowFail;
        private Delegate _dOnClosed;

        public LegacyIronSourceRewardedAdAdapter(string androidAppKey, bool logEvents, Action<bool> complete)
        {
            _androidAppKey = androidAppKey;
            _logEvents = logEvents;
            _complete = complete;
        }

        public bool Initialize()
        {
            if (!TryBindLegacyIronSource())
                return false;

            CallLegacyInitialize();
            HookLegacyIronSourceEvents();
            return true;
        }

        public bool IsReady()
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

        public void Show()
        {
            try
            {
                _isShowMethod.Invoke(null, null);
            }
            catch (Exception ex)
            {
                if (_logEvents) Debug.LogWarning($"LevelPlayRewardedAdService: Legacy show failed: {ex.Message}");
                _complete(false);
            }
        }

        public void Destroy()
        {
            try
            {
                if (_isEventsType != null)
                {
                    if (_evRewarded != null && _dOnRewarded != null) _evRewarded.RemoveEventHandler(null, _dOnRewarded);
                    if (_evShowFail != null && _dOnShowFail != null) _evShowFail.RemoveEventHandler(null, _dOnShowFail);
                    if (_evClosed != null && _dOnClosed != null) _evClosed.RemoveEventHandler(null, _dOnClosed);
                }
            }
            catch
            {
                // Best effort only
            }
        }

        private bool TryBindLegacyIronSource()
        {
            _isAgentType = FindType("IronSource.Agent");
            _isEventsType = FindType("IronSourceEvents");

            if (_isAgentType == null || _isEventsType == null)
                return false;

            _isInitMethod = _isAgentType.GetMethod("init", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
            _isIsAvailableMethod = _isAgentType.GetMethod("isRewardedVideoAvailable", BindingFlags.Public | BindingFlags.Static);
            _isShowMethod = _isAgentType.GetMethod("showRewardedVideo", BindingFlags.Public | BindingFlags.Static);

            if (_isInitMethod == null || _isIsAvailableMethod == null || _isShowMethod == null)
                return false;

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
                _isInitMethod.Invoke(null, new object[] { _androidAppKey });
            }
            catch (Exception ex)
            {
                if (_logEvents) Debug.LogWarning($"LevelPlayRewardedAdService: Legacy init failed: {ex.Message}");
            }
#else
            if (_logEvents) Debug.Log("LevelPlayRewardedAdService: Legacy init skipped (not Android).");
#endif
        }

        private void HookLegacyIronSourceEvents()
        {
            if (_evRewarded != null)
            {
                _dOnRewarded = CreateCompatibleDelegate(_evRewarded.EventHandlerType, nameof(OnLegacyRewarded));
                if (_dOnRewarded != null) _evRewarded.AddEventHandler(null, _dOnRewarded);
            }

            if (_evShowFail != null)
            {
                _dOnShowFail = CreateCompatibleDelegate(_evShowFail.EventHandlerType, nameof(OnLegacyShowFailed));
                if (_dOnShowFail != null) _evShowFail.AddEventHandler(null, _dOnShowFail);
            }

            if (_evClosed != null)
            {
                _dOnClosed = CreateCompatibleDelegate(_evClosed.EventHandlerType, nameof(OnLegacyClosed));
                if (_dOnClosed != null) _evClosed.AddEventHandler(null, _dOnClosed);
            }
        }

        private void OnLegacyRewarded()
        {
            if (_logEvents) Debug.Log("LevelPlayRewardedAdService: Legacy rewarded success.");
            _complete(true);
        }

        private void OnLegacyShowFailed()
        {
            if (_logEvents) Debug.Log("LevelPlayRewardedAdService: Legacy rewarded failed to show.");
            _complete(false);
        }

        private void OnLegacyClosed()
        {
            if (_logEvents) Debug.Log("LevelPlayRewardedAdService: Legacy rewarded closed.");
            _complete(false);
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
    }

    private static Delegate CreateDelegateForEvent(Type eventHandlerType, Action<object> handler)
    {
        var invoke = eventHandlerType.GetMethod("Invoke");
        var pars = invoke.GetParameters();

        if (pars.Length == 0)
        {
            Action a = () => handler(null);
            return Delegate.CreateDelegate(eventHandlerType, a.Target, a.Method);
        }

        if (pars.Length == 1)
        {
            return Delegate.CreateDelegate(eventHandlerType, handler.Target,
                handler.Method, throwOnBindFailure: false);
        }

        return null;
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
            catch
            {
                // ignore
            }
        }
        return null;
    }

    private static Type FindTypeByName(string typeName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try
            {
                types = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types;
            }
            catch
            {
                continue;
            }

            if (types == null) continue;

            foreach (var type in types)
            {
                if (type == null) continue;
                if (string.Equals(type.Name, typeName, StringComparison.Ordinal))
                    return type;
            }
        }

        return null;
    }
}
