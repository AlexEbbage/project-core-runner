using System;

public interface IRewardedAdService
{
    bool IsRewardedAdReady();
    void ShowRewardedAd(Action<bool> onCompleted);
}
