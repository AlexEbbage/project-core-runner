using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Dummy implementation of IRewardedAdService.
/// Simulates a rewarded ad with a short delay and always succeeds.
/// Use this during development; later you can replace it with a real ad SDK.
/// Attach to a GameObject (e.g., ServicesRoot).
/// </summary>
public class DummyRewardedAdService : MonoBehaviour, IRewardedAdService
{
    [Header("Simulation")]
    [Tooltip("Simulated ad duration in seconds.")]
    [SerializeField] private float simulatedAdDuration = 2f;

    [Tooltip("If false, all ads will 'fail' and user gets no reward.")]
    [SerializeField] private bool simulateSuccess = true;

    [SerializeField] private bool logEvents = true;

    private bool _isShowing;

    public bool IsRewardedAdReady()
    {
        return !_isShowing;
    }

    public void ShowRewardedAd(Action<RewardedAdResult> onCompleted)
    {
        if (_isShowing)
        {
            if (logEvents)
                Debug.Log("DummyRewardedAdService: Ad is already showing.");
            return;
        }

        if (logEvents)
            Debug.Log("DummyRewardedAdService: Showing simulated rewarded ad...");

        StartCoroutine(SimulateAdCoroutine(onCompleted));
    }

    private IEnumerator SimulateAdCoroutine(Action<RewardedAdResult> onCompleted)
    {
        _isShowing = true;
        yield return new WaitForSeconds(simulatedAdDuration);
        _isShowing = false;

        bool result = simulateSuccess;
        if (logEvents)
            Debug.Log($"DummyRewardedAdService: Ad completed. Success={result}");

        onCompleted?.Invoke(result ? RewardedAdResult.Rewarded : RewardedAdResult.FailedToShow);
    }
}
