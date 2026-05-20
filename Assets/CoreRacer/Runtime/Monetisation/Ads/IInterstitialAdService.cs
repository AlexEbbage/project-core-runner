using System;

namespace CoreRacer.Monetisation.Ads
{
    public interface IInterstitialAdService
    {
        bool IsInterstitialAdReady();
        void ShowInterstitialAd(AdPlacement placement, Action<InterstitialAdResult> onCompleted);
    }
}
