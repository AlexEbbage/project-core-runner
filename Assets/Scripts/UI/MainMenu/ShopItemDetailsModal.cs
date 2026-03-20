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
    [SerializeField] private TMP_Text buyButtonText;

    private Action _onBuyConfirmed;

    private void Awake()
    {
        EnsureScaffold();

        if (buyButtonText == null && buyButton != null)
            buyButtonText = buyButton.GetComponentInChildren<TMP_Text>(true);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(Hide);
        if (buyButton != null)
            buyButton.onClick.AddListener(HandleBuyClicked);
    }

    public void Show(
        ShopItemDefinition item,
        string descriptionOverride,
        string priceLabel,
        string actionLabel,
        bool canPurchase,
        Action onBuyConfirmed)
    {
        _onBuyConfirmed = onBuyConfirmed;
        gameObject.SetActive(true);

        if (itemIcon != null)
            itemIcon.sprite = item != null ? item.icon : null;
        if (itemNameText != null)
            itemNameText.text = item != null ? item.displayName : "Item";
        if (descriptionText != null)
            descriptionText.text = string.IsNullOrWhiteSpace(descriptionOverride)
                ? (item != null ? item.description : string.Empty)
                : descriptionOverride;
        if (currencyIcon != null)
            currencyIcon.enabled = currencyIcon.sprite != null;
        if (priceText != null)
            priceText.text = string.IsNullOrWhiteSpace(priceLabel) ? " " : priceLabel;
        if (buyButton != null)
            buyButton.interactable = canPurchase;
        if (buyButtonText != null)
            buyButtonText.text = string.IsNullOrWhiteSpace(actionLabel)
                ? LocalizationService.Get("ui.shop_action_default", "View")
                : actionLabel;
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

    private void EnsureScaffold()
    {
        RectTransform root = transform as RectTransform;
        if (root == null)
            root = gameObject.AddComponent<RectTransform>();

        if (itemIcon != null && itemNameText != null && descriptionText != null && priceText != null && buyButton != null && cancelButton != null)
            return;

        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;

        Image backdrop = GetComponent<Image>();
        if (backdrop == null)
            backdrop = gameObject.AddComponent<Image>();
        backdrop.color = new Color(0f, 0f, 0f, 0.72f);

        TMP_FontAsset font = TMP_Settings.defaultFontAsset;

        GameObject panel = new GameObject("ModalPanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        panel.transform.SetParent(transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(760f, 620f);

        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0.08f, 0.11f, 0.15f, 0.98f);

        VerticalLayoutGroup layout = panel.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(28, 28, 28, 28);
        layout.spacing = 18f;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        if (itemIcon == null)
        {
            GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            iconObject.transform.SetParent(panel.transform, false);
            LayoutElement iconLayout = iconObject.GetComponent<LayoutElement>();
            iconLayout.preferredWidth = 128f;
            iconLayout.preferredHeight = 128f;
            itemIcon = iconObject.GetComponent<Image>();
            itemIcon.color = new Color(0.95f, 0.6f, 0.24f, 0.9f);
        }

        if (itemNameText == null)
            itemNameText = CreateText(panel.transform, font, 34f, FontStyles.Bold);

        if (descriptionText == null)
        {
            descriptionText = CreateText(panel.transform, font, 24f, FontStyles.Normal);
            descriptionText.enableWordWrapping = true;
        }

        if (priceText == null)
            priceText = CreateText(panel.transform, font, 24f, FontStyles.Bold);

        GameObject actions = new GameObject("Actions", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        actions.transform.SetParent(panel.transform, false);
        LayoutElement actionsLayout = actions.GetComponent<LayoutElement>();
        actionsLayout.preferredHeight = 84f;
        HorizontalLayoutGroup actionsGroup = actions.GetComponent<HorizontalLayoutGroup>();
        actionsGroup.spacing = 16f;
        actionsGroup.childControlWidth = true;
        actionsGroup.childControlHeight = true;
        actionsGroup.childForceExpandWidth = true;
        actionsGroup.childForceExpandHeight = true;

        if (buyButton == null)
            buyButton = CreateButton(actions.transform, font, "BUY", new Color(0.98f, 0.5f, 0.18f, 1f), out buyButtonText);

        if (cancelButton == null)
            cancelButton = CreateButton(actions.transform, font, "CLOSE", new Color(0.22f, 0.26f, 0.32f, 1f), out _);
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
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(Transform parent, TMP_FontAsset font, string label, Color color, out TMP_Text labelText)
    {
        GameObject buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetParent(parent, false);

        LayoutElement buttonLayout = buttonObject.GetComponent<LayoutElement>();
        buttonLayout.preferredHeight = 72f;
        buttonLayout.flexibleWidth = 1f;

        Image image = buttonObject.GetComponent<Image>();
        image.color = color;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        labelText = CreateText(buttonObject.transform, font, 22f, FontStyles.Bold);
        labelText.alignment = TextAlignmentOptions.Center;
        RectTransform labelRect = labelText.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        labelText.text = label;
        return button;
    }
}
