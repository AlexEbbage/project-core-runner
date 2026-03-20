using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopBarController : MonoBehaviour
{
    [Header("Profile")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image xpProgressBar;
    [SerializeField] private Button settingsButton;
    [SerializeField] private MainMenuUI mainMenuUI;

    [Header("Currency")]
    [SerializeField] private TMP_Text softCurrencyText;
    [SerializeField] private TMP_Text premiumCurrencyText;

    [Header("Navigation")]
    [SerializeField] private MainMenuController menuController;

    private int _currentLevel = 1;

    private void Awake()
    {
        if (mainMenuUI == null)
            mainMenuUI = FindFirstObjectByType<MainMenuUI>();
    }

    private void OnEnable()
    {
        LocalizationService.LanguageChanged += HandleLanguageChanged;
    }

    private void OnDisable()
    {
        LocalizationService.LanguageChanged -= HandleLanguageChanged;
    }

    public void RefreshFromProfile(PlayerProfile profile)
    {
        if (profile == null)
            return;

        SetLevel(profile.level, GetFallbackXpNormalized(profile));
        SetSoftCurrency(profile.softCurrency);
        SetPremiumCurrency(profile.premiumCurrency);
    }

    public void SetLevel(int level, float xpNormalized)
    {
        _currentLevel = Mathf.Max(1, level);
        if (levelText != null)
            levelText.text = LocalizationService.Format("ui.level_prefix", _currentLevel);

        if (xpProgressBar != null)
            xpProgressBar.fillAmount = Mathf.Clamp01(xpNormalized);
    }

    public void SetSoftCurrency(int amount)
    {
        if (softCurrencyText != null)
            softCurrencyText.text = amount.ToString();
    }

    public void SetPremiumCurrency(int amount)
    {
        if (premiumCurrencyText != null)
            premiumCurrencyText.text = amount.ToString();
    }

    public void OnSoftCurrencyClicked()
    {
        if (menuController != null)
            menuController.ShowShopPage(ShopTab.Currency);
    }

    public void OnPremiumCurrencyClicked()
    {
        if (menuController != null)
            menuController.ShowShopPage(ShopTab.Currency);
    }

    public void OnSettingsClicked()
    {
        mainMenuUI?.ShowSettings();
    }

    private void HandleLanguageChanged()
    {
        if (levelText != null)
            levelText.text = LocalizationService.Format("ui.level_prefix", _currentLevel);
    }

    private static float GetFallbackXpNormalized(PlayerProfile profile)
    {
        if (profile == null)
            return 0f;

        return profile.GetXpProgressNormalized();
    }
}
