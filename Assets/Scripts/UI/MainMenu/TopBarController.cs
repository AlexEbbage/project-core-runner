using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopBarController : MonoBehaviour
{
    [Header("Profile")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image xpProgressBar;

    [Header("Currency")]
    [SerializeField] private TMP_Text softCurrencyText;
    [SerializeField] private TMP_Text premiumCurrencyText;

    [Header("Navigation")]
    [SerializeField] private MainMenuController menuController;

    public void SetLevel(int level, float xpNormalized)
    {
        if (levelText != null)
            levelText.text = $"Lv {Mathf.Max(1, level)}";

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
}
