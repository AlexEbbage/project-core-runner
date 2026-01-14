// LevelPlayRewardedAdService.cs
using System;
using System.Collections;
using Unity.Services.LevelPlay;
using UnityEngine;

/// <summary>
/// LevelPlay rewarded ads adapter using the instance-based LevelPlay API (SDK 8.5.0+).
/// Attach to a GameObject (e.g., ServicesRoot). Set Android App Key + Rewarded Ad Unit Id.
/// </summary>
public class LevelPlayRewardedAdService : MonoBehaviour, IRewardedAdService
{
    [Header("LevelPlay")]
    [SerializeField] private string androidAppKey = "YOUR_LEVELPLAY_ANDROID_APP_KEY";
    [Tooltip("Rewarded ad unit id (required for the new LevelPlay instance-based API).")]
    [SerializeField] private string rewardedAdUnitId = "YOUR_LEVELPLAY_REWARDED_UNIT_ID";
    [SerializeField] private bool initializeOnStart = true;
    [SerializeField] private bool logEvents = true;
    [Header("Debugging")]
    [Tooltip("Enable the LevelPlay integration test suite (only works in Editor/Development builds).")]
    [SerializeField] private bool enableTestSuite = false;
    [Tooltip("Fail-safe timeout (seconds) in case the SDK never calls back.")]
    [SerializeField] private float showTimeoutSeconds = 30f;
    private Action<bool> _pendingCallback;
    private bool _isShowing;
    private Coroutine _showTimeoutRoutine;
    private bool _rewardEarned;
    private bool _initRequested;

    private LevelPlayRewardedAd _rewardedAd;

    private void Start()
    {
        if (initializeOnStart)
            Initialize();
    }

    public void Initialize()
    {
        if (_initRequested)
            return;

        _initRequested = true;

        if (string.IsNullOrWhiteSpace(androidAppKey))
        {
            Debug.LogWarning("LevelPlayRewardedAdService: Android app key is required to initialize LevelPlay.");
            return;
        }

        LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed += SdkInitializationFailedEvent;

        if (ShouldRunTestSuite())
            LevelPlay.SetMetaData("is_test_suite", "enable");

        LevelPlay.Init(androidAppKey);
    }

    private void SdkInitializationFailedEvent(LevelPlayInitError obj)
    {
        Debug.Log($"FAILED - ErrorCode: {obj.ErrorCode}, ErrorMessage: {obj.ErrorMessage}");
    }

    private void SdkInitializationCompletedEvent(LevelPlayConfiguration obj)
    {
        Debug.Log($"SUCESS - IsAdQualityEnabled: {obj.IsAdQualityEnabled}");

        CreateRewardedAd();
        LoadRewardedAd();
        TryLaunchTestSuite();
    }

    public bool IsRewardedAdReady()
    {
        if (_rewardedAd == null)
        {
            Initialize();
            return false;
        }

        return _rewardedAd.IsAdReady();
    }

    public void ShowRewardedAd(Action<bool> onCompleted)
    {
        if (_isShowing)
        {
            if (logEvents) Debug.Log("LevelPlayRewardedAdService: already showing.");
            return;
        }

        if (_rewardedAd == null)
        {
            Initialize();
            if (_rewardedAd == null)
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
        _rewardEarned = false;

        // Fail-safe: if the SDK never calls back, complete as failure.
        StartShowTimeout();

        _rewardedAd.ShowAd();
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
        UnregisterRewardedAd();
        _rewardedAd = null;
        LevelPlay.OnInitSuccess -= SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed -= SdkInitializationFailedEvent;
    }

    private void CreateRewardedAd()
    {
        if (_rewardedAd != null)
            return;

        if (string.IsNullOrWhiteSpace(rewardedAdUnitId))
        {
            Debug.LogWarning("LevelPlayRewardedAdService: Rewarded ad unit id is required.");
            return;
        }

        _rewardedAd = new LevelPlayRewardedAd(rewardedAdUnitId);
        RegisterRewardedAdEvents();
    }

    private void RegisterRewardedAdEvents()
    {
        if (_rewardedAd == null)
            return;

        _rewardedAd.OnAdLoaded += RewardedOnAdLoadedEvent;
        _rewardedAd.OnAdLoadFailed += RewardedOnAdLoadFailedEvent;
        _rewardedAd.OnAdDisplayed += RewardedOnAdDisplayedEvent;
        _rewardedAd.OnAdDisplayFailed += RewardedOnAdDisplayFailedEvent;
        _rewardedAd.OnAdRewarded += RewardedOnAdRewardedEvent;
        _rewardedAd.OnAdClosed += RewardedOnAdClosedEvent;
        _rewardedAd.OnAdClicked += RewardedOnAdClickedEvent;
        _rewardedAd.OnAdInfoChanged += RewardedOnAdInfoChangedEvent;
    }

    private void UnregisterRewardedAd()
    {
        if (_rewardedAd == null)
            return;

        _rewardedAd.OnAdLoaded -= RewardedOnAdLoadedEvent;
        _rewardedAd.OnAdLoadFailed -= RewardedOnAdLoadFailedEvent;
        _rewardedAd.OnAdDisplayed -= RewardedOnAdDisplayedEvent;
        _rewardedAd.OnAdDisplayFailed -= RewardedOnAdDisplayFailedEvent;
        _rewardedAd.OnAdRewarded -= RewardedOnAdRewardedEvent;
        _rewardedAd.OnAdClosed -= RewardedOnAdClosedEvent;
        _rewardedAd.OnAdClicked -= RewardedOnAdClickedEvent;
        _rewardedAd.OnAdInfoChanged -= RewardedOnAdInfoChangedEvent;
    }

    private void LoadRewardedAd()
    {
        if (_rewardedAd == null)
            return;

        _rewardedAd.LoadAd();
    }

    private void RewardedOnAdLoadedEvent(LevelPlayAdInfo adInfo)
    {
        if (logEvents) Debug.Log($"LevelPlayRewardedAdService: Rewarded loaded ({adInfo.AdUnitId}).");
    }

    private void RewardedOnAdLoadFailedEvent(LevelPlayAdError error)
    {
        if (logEvents) Debug.LogWarning($"LevelPlayRewardedAdService: Rewarded load failed: {error.ErrorMessage}");
    }

    private void RewardedOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
    {
        if (logEvents) Debug.Log($"LevelPlayRewardedAdService: Rewarded displayed ({adInfo.AdUnitId}).");
    }

    private void RewardedOnAdDisplayFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        if (logEvents) Debug.LogWarning($"LevelPlayRewardedAdService: Rewarded display failed: {error.ErrorMessage}");
        Complete(false);
        LoadRewardedAd();
    }

    private void RewardedOnAdRewardedEvent(LevelPlayAdInfo adInfo, LevelPlayReward adReward)
    {
        if (logEvents) Debug.Log($"LevelPlayRewardedAdService: Rewarded earned ({adReward.Name}:{adReward.Amount}).");
        _rewardEarned = true;
        Complete(true);
    }

    private void RewardedOnAdClosedEvent(LevelPlayAdInfo adInfo)
    {
        if (logEvents) Debug.Log("LevelPlayRewardedAdService: Rewarded closed.");

        if (_rewardEarned)
        {
            Complete(true);
        }
        else if (logEvents)
        {
            Debug.Log("LevelPlayRewardedAdService: Awaiting reward callback after close.");
        }

        LoadRewardedAd();
    }

    private void RewardedOnAdClickedEvent(LevelPlayAdInfo adInfo)
    {
        if (logEvents) Debug.Log("LevelPlayRewardedAdService: Rewarded clicked.");
    }

    private void RewardedOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
    {
        if (logEvents) Debug.Log("LevelPlayRewardedAdService: Rewarded ad info changed.");
    }

    private bool ShouldRunTestSuite()
    {
        if (!enableTestSuite)
            return false;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        return true;
#else
        if (logEvents) Debug.Log("LevelPlayRewardedAdService: Test suite disabled for non-development builds.");
        return false;
#endif
    }

    private void TryLaunchTestSuite()
    {
        if (!ShouldRunTestSuite())
            return;

        LevelPlay.LaunchTestSuite();
    }
}
