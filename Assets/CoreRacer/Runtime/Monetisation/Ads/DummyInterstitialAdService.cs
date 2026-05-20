using System;
using UnityEngine;

namespace CoreRacer.Monetisation.Ads
{
    public sealed class DummyInterstitialAdService : MonoBehaviour, IInterstitialAdService
    {
        [SerializeField] private bool alwaysReady = true;

        public bool IsInterstitialAdReady() => alwaysReady;

        public void ShowInterstitialAd(AdPlacement placement, Action<InterstitialAdResult> onCompleted)
        {
            Debug.Log($"Dummy interstitial ad shown: {placement}");
            onCompleted?.Invoke(alwaysReady ? InterstitialAdResult.Completed : InterstitialAdResult.NotReady);
        }
    }
}
