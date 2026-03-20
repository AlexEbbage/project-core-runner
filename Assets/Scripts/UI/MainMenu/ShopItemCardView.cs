using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemCardView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text buyButtonText;

    public ShopItemDefinition Item { get; private set; }

    private void Awake()
    {
        EnsureScaffold();

        if (buyButtonText == null && buyButton != null)
            buyButtonText = buyButton.GetComponentInChildren<TMP_Text>(true);
    }

    public void Initialize(ShopItemDefinition item, string priceLabel, string actionLabel, bool canPurchase, System.Action onPressed)
    {
        Item = item;

        if (iconImage != null)
            iconImage.sprite = item != null ? item.icon : null;
        if (nameText != null)
            nameText.text = item != null ? item.displayName : LocalizationService.Get("ui.item_default", "Item");
        if (priceText != null)
            priceText.text = string.IsNullOrWhiteSpace(priceLabel) ? " " : priceLabel;
        if (buyButton != null)
        {
            buyButton.interactable = item != null && canPurchase;
            buyButton.onClick.RemoveAllListeners();
            if (onPressed != null)
                buyButton.onClick.AddListener(() => onPressed());
        }
        if (buyButtonText != null)
            buyButtonText.text = string.IsNullOrWhiteSpace(actionLabel)
                ? LocalizationService.Get("ui.shop_action_default", "View")
                : actionLabel;
    }

    private void EnsureScaffold()
    {
        RectTransform root = transform as RectTransform;
        if (root == null)
            root = gameObject.AddComponent<RectTransform>();

        if (iconImage != null && nameText != null && priceText != null && buyButton != null)
            return;

        root.sizeDelta = new Vector2(0f, 180f);

        Image background = GetComponent<Image>();
        if (background == null)
            background = gameObject.AddComponent<Image>();
        background.color = new Color(0.09f, 0.12f, 0.16f, 0.94f);

        LayoutElement layout = GetComponent<LayoutElement>();
        if (layout == null)
            layout = gameObject.AddComponent<LayoutElement>();
        layout.preferredHeight = 180f;

        HorizontalLayoutGroup horizontalLayout = GetComponent<HorizontalLayoutGroup>();
        if (horizontalLayout == null)
            horizontalLayout = gameObject.AddComponent<HorizontalLayoutGroup>();
        horizontalLayout.padding = new RectOffset(18, 18, 18, 18);
        horizontalLayout.spacing = 18f;
        horizontalLayout.childAlignment = TextAnchor.MiddleLeft;
        horizontalLayout.childControlWidth = false;
        horizontalLayout.childControlHeight = true;
        horizontalLayout.childForceExpandWidth = false;
        horizontalLayout.childForceExpandHeight = false;

        TMP_FontAsset font = TMP_Settings.defaultFontAsset;

        if (iconImage == null)
        {
            GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            iconObject.transform.SetParent(transform, false);
            LayoutElement iconLayout = iconObject.GetComponent<LayoutElement>();
            iconLayout.preferredWidth = 96f;
            iconLayout.preferredHeight = 96f;
            iconImage = iconObject.GetComponent<Image>();
            iconImage.color = new Color(0.95f, 0.6f, 0.24f, 0.9f);
        }

        GameObject textColumn = new GameObject("TextColumn", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        textColumn.transform.SetParent(transform, false);
        VerticalLayoutGroup textLayout = textColumn.GetComponent<VerticalLayoutGroup>();
        textLayout.spacing = 8f;
        textLayout.childControlHeight = false;
        textLayout.childControlWidth = true;
        textLayout.childForceExpandWidth = true;
        textLayout.childForceExpandHeight = false;
        LayoutElement textColumnLayout = textColumn.GetComponent<LayoutElement>();
        textColumnLayout.flexibleWidth = 1f;

        if (nameText == null)
            nameText = CreateText(textColumn.transform, font, 30f, FontStyles.Bold);

        if (priceText == null)
            priceText = CreateText(textColumn.transform, font, 22f, FontStyles.Normal);

        if (buyButton == null)
        {
            GameObject buttonObject = new GameObject("ActionButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonObject.transform.SetParent(transform, false);
            LayoutElement buttonLayout = buttonObject.GetComponent<LayoutElement>();
            buttonLayout.preferredWidth = 190f;
            buttonLayout.preferredHeight = 72f;

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(0.98f, 0.5f, 0.18f, 1f);

            buyButton = buttonObject.GetComponent<Button>();
            buyButton.targetGraphic = buttonImage;

            buyButtonText = CreateText(buttonObject.transform, font, 22f, FontStyles.Bold);
            buyButtonText.alignment = TextAlignmentOptions.Center;
            RectTransform buyTextRect = buyButtonText.rectTransform;
            buyTextRect.anchorMin = Vector2.zero;
            buyTextRect.anchorMax = Vector2.one;
            buyTextRect.offsetMin = Vector2.zero;
            buyTextRect.offsetMax = Vector2.zero;
        }
    }

    private static TMP_Text CreateText(Transform parent, TMP_FontAsset font, float fontSize, FontStyles fontStyle)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = Color.white;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
        return text;
    }
}
