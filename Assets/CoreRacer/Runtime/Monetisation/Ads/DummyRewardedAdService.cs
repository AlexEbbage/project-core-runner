using System;
using UnityEngine;

namespace CoreRacer.Monetisation.Ads
{
    public sealed class DummyRewardedAdService : MonoBehaviour, IRewardedAdService
    {
        [SerializeField] private bool alwaysReady = true;
        [SerializeField] private bool alwaysReward = true;

        public bool IsRewardedAdReady() => alwaysReady;

        public void ShowRewardedAd(AdPlacement placement, Action<RewardedAdResult> onCompleted)
        {
            Debug.Log($"Dummy rewarded ad shown: {placement}");
            onCompleted?.Invoke(alwaysReady && alwaysReward ? RewardedAdResult.Rewarded : RewardedAdResult.NotReady);
        }
    }
}
