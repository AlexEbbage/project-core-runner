using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Dummy implementation of IInterstitialAdService.
/// Simulates an interstitial ad with a short delay and always succeeds.
/// Use this during development; later you can replace it with a real ad SDK.
/// Attach to a GameObject (e.g., ServicesRoot).
/// </summary>
public class DummyInterstitialAdService : MonoBehaviour, IInterstitialAdService
{
    [Header("Simulation")]
    [Tooltip("Simulated ad duration in seconds.")]
    [SerializeField] private float simulatedAdDuration = 2f;

    [Tooltip("If false, all ads will 'fail' and user gets no ad completion.")]
    [SerializeField] private bool simulateSuccess = true;

    [SerializeField] private bool logEvents = true;

    private bool _isShowing;

    public bool IsInterstitialAdReady()
    {
        return !_isShowing;
    }

    public void ShowInterstitialAd(Action<InterstitialAdResult> onCompleted)
    {
        if (_isShowing)
        {
            if (logEvents)
                Debug.Log("DummyInterstitialAdService: Ad is already showing.");
            return;
        }

        if (logEvents)
            Debug.Log("DummyInterstitialAdService: Showing simulated interstitial ad...");

        StartCoroutine(SimulateAdCoroutine(onCompleted));
    }

    private IEnumerator SimulateAdCoroutine(Action<InterstitialAdResult> onCompleted)
    {
        _isShowing = true;
        yield return new WaitForSeconds(simulatedAdDuration);
        _isShowing = false;

        bool result = simulateSuccess;
        if (logEvents)
            Debug.Log($"DummyInterstitialAdService: Ad completed. Success={result}");

        onCompleted?.Invoke(result ? InterstitialAdResult.Completed : InterstitialAdResult.FailedToShow);
    }
}
