using System;

namespace CoreRacer.Monetisation.Ads
{
    public interface IRewardedAdService
    {
        bool IsRewardedAdReady();
        void ShowRewardedAd(AdPlacement placement, Action<RewardedAdResult> onCompleted);
    }
}
