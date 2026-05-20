using System;
using UnityEngine;

namespace CoreRacer.Monetisation.Ads
{
    /// <summary>
    /// Dependency-safe LevelPlay adapter. Keep this class in scenes, then enable CORE_RACER_LEVELPLAY
    /// and add the SDK-specific calls in the guarded section matching your installed LevelPlay version.
    /// Without the symbol it behaves as not-ready, which is safe for editor imports and CI validation.
    /// </summary>
    public sealed class LevelPlayRewardedAdServiceAdapter : MonoBehaviour, IRewardedAdService
    {
        [SerializeField] private string androidAppKey;
        [SerializeField] private string iosAppKey;
        [SerializeField] private string rewardedUnitId;

        public string RewardedUnitId => rewardedUnitId;

        public bool IsRewardedAdReady()
        {
#if CORE_RACER_LEVELPLAY
            return false;
#else
            return false;
#endif
        }

        public void ShowRewardedAd(AdPlacement placement, Action<RewardedAdResult> onCompleted)
        {
#if CORE_RACER_LEVELPLAY
            onCompleted?.Invoke(RewardedAdResult.FailedToShow);
#else
            Debug.LogWarning("LevelPlay is not enabled for this build. Use DummyRewardedAdService for editor testing or enable CORE_RACER_LEVELPLAY after wiring the SDK.");
            onCompleted?.Invoke(RewardedAdResult.NotReady);
#endif
        }
    }
}
