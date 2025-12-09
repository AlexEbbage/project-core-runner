using UnityEngine;

public static class AdsConfig
{
    private const string RemoveAdsKey = "RemoveAds";

    public static bool RemoveAds
    {
        get => PlayerPrefs.GetInt(RemoveAdsKey, 0) == 1;
        set
        {
            PlayerPrefs.SetInt(RemoveAdsKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
