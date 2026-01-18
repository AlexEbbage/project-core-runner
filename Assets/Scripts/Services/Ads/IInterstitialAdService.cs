using System;

public interface IInterstitialAdService
{
    bool IsInterstitialAdReady();
    void ShowInterstitialAd(Action<InterstitialAdResult> onCompleted);
}

public enum InterstitialAdResult
{
    Completed,
    Closed,
    NotReady,
    FailedToShow
}
