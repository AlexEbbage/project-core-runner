namespace CoreRacer.Monetisation.Ads
{
    public enum AdPlacement
    {
        ContinueRun,
        DoubleRunRewards,
        DailyLoginDoubleReward,
        MidRunRewardedOffer,
        Interstitial
    }

    public enum RewardedAdResult
    {
        Rewarded,
        ClosedBeforeReward,
        NotReady,
        FailedToShow,
        BypassedByPremium
    }

    public enum InterstitialAdResult
    {
        Completed,
        Closed,
        NotReady,
        FailedToShow,
        BypassedByPremium
    }
}
