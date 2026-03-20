using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HangarCosmeticItemView : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text actionText;
    [SerializeField] private GameObject lockedState;
    [SerializeField] private GameObject equippedState;
    [SerializeField] private Button actionButton;
    [SerializeField] private string lockedLabel = "LOCKED";
    [SerializeField] private string equipLabel = "EQUIP";
    [SerializeField] private string equippedLabel = "EQUIPPED";

    public string ItemId { get; private set; }

    private void Awake()
    {
        EnsureScaffold();
    }

    public void Initialize(string itemId, string displayName, Sprite icon, int cost, bool unlocked, bool equipped)
    {
        ItemId = itemId;
        EnsureScaffold();

        if (iconImage != null)
            iconImage.sprite = icon;
        if (nameText != null)
            nameText.text = displayName;
        if (priceText != null)
        {
            if (equipped)
                priceText.text = LocalizationService.Get("ui.hangar_status_equipped", "Equipped");
            else if (unlocked)
                priceText.text = LocalizationService.Get("ui.hangar_status_owned", "Unlocked");
            else if (cost > 0)
                priceText.text = LocalizationService.Format("ui.shop_price_soft", cost);
            else
                priceText.text = LocalizationService.Get("ui.hangar_status_locked", "Locked");
        }

        if (actionText != null)
            actionText.text = equipped
                ? LocalizationService.Get("ui.hangar_action_equipped", equippedLabel)
                : unlocked
                    ? LocalizationService.Get("ui.hangar_action_equip", equipLabel)
                    : LocalizationService.Get("ui.hangar_action_locked", lockedLabel);

        if (lockedState != null)
            lockedState.SetActive(!unlocked);
        if (equippedState != null)
            equippedState.SetActive(equipped);
        if (actionButton != null)
            actionButton.interactable = unlocked;

        if (backgroundImage != null)
        {
            backgroundImage.color = equipped
                ? new Color(0.98f, 0.5f, 0.18f, 0.95f)
                : unlocked
                    ? new Color(0.11f, 0.15f, 0.21f, 0.96f)
                    : new Color(0.18f, 0.18f, 0.2f, 0.92f);
        }
    }

    public void SetAction(System.Action action)
    {
        if (actionButton == null)
            return;

        actionButton.onClick.RemoveAllListeners();
        if (action != null)
            actionButton.onClick.AddListener(() => action());
    }

    private void EnsureScaffold()
    {
        RectTransform rootRect = transform as RectTransform;
        if (rootRect == null)
            return;

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>() ?? gameObject.AddComponent<Image>();

        if (GetComponent<LayoutElement>() == null)
        {
            LayoutElement layout = gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 110f;
        }

        if (GetComponent<HorizontalLayoutGroup>() == null)
        {
            HorizontalLayoutGroup group = gameObject.AddComponent<HorizontalLayoutGroup>();
            group.padding = new RectOffset(18, 18, 14, 14);
            group.spacing = 14f;
            group.childAlignment = TextAnchor.MiddleLeft;
            group.childControlWidth = false;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = false;
        }

        backgroundImage.color = new Color(0.11f, 0.15f, 0.21f, 0.96f);

        TMP_FontAsset font = TMP_Settings.defaultFontAsset;

        if (iconImage == null)
        {
            GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            iconObject.transform.SetParent(transform, false);
            LayoutElement layout = iconObject.GetComponent<LayoutElement>();
            layout.preferredWidth = 72f;
            layout.preferredHeight = 72f;
            iconImage = iconObject.GetComponent<Image>();
            iconImage.color = Color.white;
            iconImage.preserveAspect = true;
        }

        if (nameText == null || priceText == null || lockedState == null || equippedState == null)
        {
            GameObject bodyObject = new GameObject("Body", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            bodyObject.transform.SetParent(transform, false);
            LayoutElement bodyLayout = bodyObject.GetComponent<LayoutElement>();
            bodyLayout.flexibleWidth = 1f;
            VerticalLayoutGroup bodyGroup = bodyObject.GetComponent<VerticalLayoutGroup>();
            bodyGroup.spacing = 6f;
            bodyGroup.childControlWidth = true;
            bodyGroup.childControlHeight = false;
            bodyGroup.childForceExpandWidth = true;
            bodyGroup.childForceExpandHeight = false;

            if (nameText == null)
                nameText = CreateText(bodyObject.transform, "Name", font, 26f, FontStyles.Bold);

            if (priceText == null)
                priceText = CreateText(bodyObject.transform, "Price", font, 20f, FontStyles.Normal);

            if (lockedState == null)
            {
                TMP_Text lockedText = CreateText(bodyObject.transform, "LockedState", font, 18f, FontStyles.Bold);
                lockedText.text = LocalizationService.Get("ui.hangar_action_locked", lockedLabel);
                lockedText.color = new Color(1f, 0.78f, 0.32f, 1f);
                lockedState = lockedText.gameObject;
            }

            if (equippedState == null)
            {
                TMP_Text equippedText = CreateText(bodyObject.transform, "EquippedState", font, 18f, FontStyles.Bold);
                equippedText.text = LocalizationService.Get("ui.hangar_action_equipped", equippedLabel);
                equippedText.color = new Color(0.78f, 1f, 0.78f, 1f);
                equippedState = equippedText.gameObject;
            }
        }

        if (actionButton == null)
        {
            GameObject buttonObject = new GameObject("ActionButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonObject.transform.SetParent(transform, false);
            LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
            layout.preferredWidth = 150f;
            layout.preferredHeight = 46f;
            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.98f, 0.5f, 0.18f, 1f);
            actionButton = buttonObject.GetComponent<Button>();
            actionButton.targetGraphic = image;

            if (actionText == null)
            {
                actionText = CreateText(buttonObject.transform, "ActionText", font, 20f, FontStyles.Bold);
                actionText.alignment = TextAlignmentOptions.Center;
                RectTransform actionRect = actionText.rectTransform;
                actionRect.anchorMin = Vector2.zero;
                actionRect.anchorMax = Vector2.one;
                actionRect.offsetMin = Vector2.zero;
                actionRect.offsetMax = Vector2.zero;
            }
        }
        else if (actionText == null)
        {
            actionText = actionButton.GetComponentInChildren<TMP_Text>(true);
        }
    }

    private static TMP_Text CreateText(Transform parent, string objectName, TMP_FontAsset font, float fontSize, FontStyles style)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }
}
