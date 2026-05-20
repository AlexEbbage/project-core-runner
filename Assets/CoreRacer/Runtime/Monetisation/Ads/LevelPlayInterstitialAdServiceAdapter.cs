using System;
using UnityEngine;

namespace CoreRacer.Monetisation.Ads
{
    /// <summary>
    /// Dependency-safe LevelPlay interstitial adapter. Enable CORE_RACER_LEVELPLAY and wire the exact SDK API version in the guarded section.
    /// </summary>
    public sealed class LevelPlayInterstitialAdServiceAdapter : MonoBehaviour, IInterstitialAdService
    {
        [SerializeField] private string androidAppKey;
        [SerializeField] private string iosAppKey;
        [SerializeField] private string interstitialUnitId;

        public string InterstitialUnitId => interstitialUnitId;

        public bool IsInterstitialAdReady()
        {
#if CORE_RACER_LEVELPLAY
            return false;
#else
            return false;
#endif
        }

        public void ShowInterstitialAd(AdPlacement placement, Action<InterstitialAdResult> onCompleted)
        {
#if CORE_RACER_LEVELPLAY
            onCompleted?.Invoke(InterstitialAdResult.FailedToShow);
#else
            Debug.LogWarning("LevelPlay interstitials are not enabled for this build. Enable CORE_RACER_LEVELPLAY after wiring the SDK.", this);
            onCompleted?.Invoke(InterstitialAdResult.NotReady);
#endif
        }
    }
}
