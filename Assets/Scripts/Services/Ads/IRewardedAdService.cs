using System;

public interface IRewardedAdService
{
    bool IsRewardedAdReady();
    void ShowRewardedAd(Action<RewardedAdResult> onCompleted);
}

public enum RewardedAdResult
{
    Rewarded,
    ClosedBeforeReward,
    NotReady,
    FailedToShow
}
