using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemDetailsModal : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image currencyIcon;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button cancelButton;

    private Action _onBuyConfirmed;

    private void Awake()
    {
        if (cancelButton != null)
            cancelButton.onClick.AddListener(Hide);
        if (buyButton != null)
            buyButton.onClick.AddListener(HandleBuyClicked);
    }

    public void Show(ShopItemDefinition item, bool isUnlocked, Action onBuyConfirmed)
    {
        _onBuyConfirmed = onBuyConfirmed;
        gameObject.SetActive(true);

        if (itemIcon != null)
            itemIcon.sprite = item != null ? item.icon : null;
        if (itemNameText != null)
            itemNameText.text = item != null ? item.displayName : "Item";
        if (descriptionText != null)
            descriptionText.text = item != null ? item.description : string.Empty;
        if (priceText != null)
            priceText.text = item != null
                ? (isUnlocked ? LocalizationService.Get("ui.unlocked", "Unlocked") : item.price.ToString())
                : "0";
        if (buyButton != null)
            buyButton.interactable = !isUnlocked;
    }

    public void Hide()
    {
        _onBuyConfirmed = null;
        gameObject.SetActive(false);
    }

    private void HandleBuyClicked()
    {
        _onBuyConfirmed?.Invoke();
    }
}
