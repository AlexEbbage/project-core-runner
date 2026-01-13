using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HangarCosmeticItemView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private GameObject lockedState;
    [SerializeField] private GameObject equippedState;
    [SerializeField] private Button actionButton;

    public string ItemId { get; private set; }

    public void Initialize(string itemId, string displayName, Sprite icon, int cost, bool unlocked, bool equipped)
    {
        ItemId = itemId;

        if (iconImage != null)
            iconImage.sprite = icon;
        if (nameText != null)
            nameText.text = displayName;
        if (priceText != null)
            priceText.text = cost.ToString();

        if (lockedState != null)
            lockedState.SetActive(!unlocked);
        if (equippedState != null)
            equippedState.SetActive(equipped);
        if (actionButton != null)
            actionButton.interactable = unlocked || cost <= 0;
    }
}
