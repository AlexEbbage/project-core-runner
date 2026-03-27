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
    private int _currentSoftCurrency;
    private int _currentPremiumCurrency;
    private bool _hasSnapshot;

    private void Awake()
    {
        if (mainMenuUI == null)
            mainMenuUI = FindFirstObjectByType<MainMenuUI>();

        if (menuController == null)
            menuController = FindFirstObjectByType<MainMenuController>();
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

        if (!_hasSnapshot)
        {
            _currentLevel = Mathf.Max(1, profile.level);
            _currentSoftCurrency = Mathf.Max(0, profile.softCurrency);
            _currentPremiumCurrency = Mathf.Max(0, profile.premiumCurrency);
            _hasSnapshot = true;
        }

        SetLevel(profile.level, GetFallbackXpNormalized(profile));
        SetSoftCurrency(profile.softCurrency);
        SetPremiumCurrency(profile.premiumCurrency);
    }

    public void SetLevel(int level, float xpNormalized)
    {
        int previousLevel = _currentLevel;
        _currentLevel = Mathf.Max(1, level);
        if (levelText != null)
            levelText.text = LocalizationService.Format("ui.level_prefix", _currentLevel);

        if (xpProgressBar != null)
            xpProgressBar.fillAmount = Mathf.Clamp01(xpNormalized);

        if (_hasSnapshot && _currentLevel > previousLevel && levelText != null)
            UiMotion.PulseScale(levelText.rectTransform);
    }

    public void SetSoftCurrency(int amount)
    {
        int previousAmount = _currentSoftCurrency;
        _currentSoftCurrency = Mathf.Max(0, amount);
        if (softCurrencyText != null)
            softCurrencyText.text = _currentSoftCurrency.ToString();

        if (_hasSnapshot && _currentSoftCurrency > previousAmount && softCurrencyText != null)
            UiMotion.PulseBadge(softCurrencyText.rectTransform);
    }

    public void SetPremiumCurrency(int amount)
    {
        int previousAmount = _currentPremiumCurrency;
        _currentPremiumCurrency = Mathf.Max(0, amount);
        if (premiumCurrencyText != null)
            premiumCurrencyText.text = _currentPremiumCurrency.ToString();

        if (_hasSnapshot && _currentPremiumCurrency > previousAmount && premiumCurrencyText != null)
            UiMotion.PulseBadge(premiumCurrencyText.rectTransform);
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
