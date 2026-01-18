using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemCardView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;

    public ShopItemDefinition Item { get; private set; }

    public void Initialize(ShopItemDefinition item, bool isUnlocked)
    {
        Item = item;

        if (iconImage != null)
            iconImage.sprite = item != null ? item.icon : null;
        if (nameText != null)
            nameText.text = item != null ? item.displayName : LocalizationService.Get("ui.item_default", "Item");
        if (priceText != null)
            priceText.text = item != null
                ? (isUnlocked ? LocalizationService.Get("ui.unlocked", "Unlocked") : item.price.ToString())
                : "0";
        if (buyButton != null)
            buyButton.interactable = item != null && item.price >= 0 && !isUnlocked;
    }
}
