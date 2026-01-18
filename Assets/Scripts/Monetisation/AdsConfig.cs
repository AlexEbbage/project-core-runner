using UnityEngine;

public static class AdsConfig
{
    private const string RemoveAdsKey = "RemoveAds";
    private const string InterstitialsEnabledKey = "InterstitialsEnabled";

    public static bool RemoveAds
    {
        get => PlayerPrefs.GetInt(RemoveAdsKey, 0) == 1;
        set
        {
            PlayerPrefs.SetInt(RemoveAdsKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static bool InterstitialsEnabled
    {
        get => PlayerPrefs.GetInt(InterstitialsEnabledKey, 0) == 1;
        set
        {
            PlayerPrefs.SetInt(InterstitialsEnabledKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
