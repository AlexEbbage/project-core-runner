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
    public PowerupUpgradeConfig.PowerupUpgradeEntry PowerupUpgradeEntry { get; private set; }
    public int CurrentLevel { get; private set; }
    public bool IsPowerupUpgrade => PowerupUpgradeEntry != null;

    private void OnEnable()
    {
        LocalizationService.LanguageChanged += UpdateLevelLabel;
    }

    private void OnDisable()
    {
        LocalizationService.LanguageChanged -= UpdateLevelLabel;
    }

    public void Initialize(ShipUpgradeDefinition definition, int currentLevel, int cost, bool canUpgrade)
    {
        Definition = definition;
        PowerupUpgradeEntry = null;
        CurrentLevel = currentLevel;

        if (iconImage != null)
            iconImage.sprite = definition != null ? definition.icon : null;

        if (nameText != null)
            nameText.text = definition != null
                ? definition.displayName
                : LocalizationService.Get("ui.upgrade_default", "Upgrade");

        UpdateLevelLabel();

        if (costText != null)
            costText.text = cost.ToString();

        if (upgradeButton != null)
            upgradeButton.interactable = canUpgrade;
    }

    public void InitializePowerupUpgrade(PowerupUpgradeConfig.PowerupUpgradeEntry entry, int currentLevel, int cost, bool canUpgrade)
    {
        PowerupUpgradeEntry = entry;
        Definition = null;
        CurrentLevel = currentLevel;

        if (iconImage != null)
            iconImage.sprite = entry != null ? entry.icon : null;

        if (nameText != null)
        {
            if (entry != null && !string.IsNullOrEmpty(entry.displayName))
            {
                nameText.text = entry.displayName;
            }
            else if (entry != null)
            {
                nameText.text = entry.powerupType.ToString();
            }
            else
            {
                nameText.text = LocalizationService.Get("ui.upgrade_default", "Upgrade");
            }
        }

        UpdateLevelLabel();

        if (costText != null)
            costText.text = cost.ToString();

        if (upgradeButton != null)
            upgradeButton.interactable = canUpgrade;
    }

    private void UpdateLevelLabel()
    {
        if (levelText != null)
            levelText.text = LocalizationService.Format("ui.level_prefix", CurrentLevel);
    }
}
