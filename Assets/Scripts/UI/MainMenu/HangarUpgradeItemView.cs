using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HangarUpgradeItemView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button upgradeButton;

    public ShipUpgradeDefinition Definition { get; private set; }
    public int CurrentLevel { get; private set; }

    public void Initialize(ShipUpgradeDefinition definition, int currentLevel, int cost, bool canUpgrade)
    {
        Definition = definition;
        CurrentLevel = currentLevel;

        if (iconImage != null)
            iconImage.sprite = definition != null ? definition.icon : null;

        if (nameText != null)
            nameText.text = definition != null ? definition.displayName : "Upgrade";

        if (levelText != null)
            levelText.text = $"Lv {currentLevel}";

        if (costText != null)
            costText.text = cost.ToString();

        if (upgradeButton != null)
            upgradeButton.interactable = canUpgrade;
    }
}
