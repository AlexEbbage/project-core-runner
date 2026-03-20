using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressionTaskRowView : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Image rewardIcon;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private GameObject completeOverlay;
    [SerializeField] private TMP_Text completeIndicator;
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionLabel;

    private void Awake()
    {
        EnsureScaffold();
    }

    public Image IconImage => iconImage;
    public TMP_Text DescriptionText => descriptionText;
    public TMP_Text ProgressText => progressText;
    public Slider ProgressSlider => progressSlider;
    public Image RewardIcon => rewardIcon;
    public TMP_Text RewardText => rewardText;
    public GameObject CompleteOverlay => completeOverlay;
    public TMP_Text CompleteIndicator => completeIndicator;
    public Button ActionButton => actionButton;
    public TMP_Text ActionLabel => actionLabel;

    public void SetAction(string label, bool interactable, System.Action action)
    {
        EnsureScaffold();

        if (actionLabel != null)
            actionLabel.text = label;

        if (actionButton != null)
        {
            actionButton.interactable = interactable;
            if (actionButton.TryGetComponent(out Image image))
            {
                image.color = interactable
                    ? new Color(0.98f, 0.5f, 0.18f, 1f)
                    : new Color(0.3f, 0.33f, 0.38f, 1f);
            }

            actionButton.onClick.RemoveAllListeners();
            if (action != null)
                actionButton.onClick.AddListener(() => action());
        }
    }

    private void EnsureScaffold()
    {
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>() ?? gameObject.AddComponent<Image>();

        if (GetComponent<LayoutElement>() == null)
        {
            LayoutElement layout = gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 106f;
        }

        if (GetComponent<HorizontalLayoutGroup>() == null)
        {
            HorizontalLayoutGroup group = gameObject.AddComponent<HorizontalLayoutGroup>();
            group.padding = new RectOffset(18, 18, 14, 14);
            group.spacing = 12f;
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
            layout.preferredWidth = 64f;
            layout.preferredHeight = 64f;
            iconImage = iconObject.GetComponent<Image>();
            iconImage.preserveAspect = true;
        }

        if (descriptionText == null || progressText == null || rewardText == null)
        {
            GameObject body = new GameObject("Body", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            body.transform.SetParent(transform, false);

            LayoutElement bodyLayout = body.GetComponent<LayoutElement>();
            bodyLayout.flexibleWidth = 1f;

            VerticalLayoutGroup group = body.GetComponent<VerticalLayoutGroup>();
            group.spacing = 6f;
            group.childControlWidth = true;
            group.childControlHeight = false;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;

            if (descriptionText == null)
                descriptionText = CreateText(body.transform, "Description", font, 24f, FontStyles.Bold);

            if (progressText == null)
                progressText = CreateText(body.transform, "Progress", font, 18f, FontStyles.Normal);

            if (progressSlider == null)
            {
                GameObject sliderObject = new GameObject("ProgressSlider", typeof(RectTransform), typeof(Slider), typeof(LayoutElement));
                sliderObject.transform.SetParent(body.transform, false);
                LayoutElement sliderLayout = sliderObject.GetComponent<LayoutElement>();
                sliderLayout.preferredHeight = 18f;
                progressSlider = sliderObject.GetComponent<Slider>();
                progressSlider.direction = Slider.Direction.LeftToRight;
                progressSlider.minValue = 0f;
                progressSlider.maxValue = 1f;
            }

            if (rewardText == null)
                rewardText = CreateText(body.transform, "Reward", font, 18f, FontStyles.Normal);

            if (completeIndicator == null)
            {
                completeIndicator = CreateText(body.transform, "CompleteIndicator", font, 18f, FontStyles.Bold);
                completeIndicator.text = LocalizationService.Get("ui.task_complete", "Completed");
                completeIndicator.gameObject.SetActive(false);
            }

            if (completeOverlay == null)
                completeOverlay = completeIndicator != null ? completeIndicator.gameObject : null;
        }

        if (actionButton == null)
        {
            GameObject buttonObject = new GameObject("ActionButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonObject.transform.SetParent(transform, false);
            LayoutElement buttonLayout = buttonObject.GetComponent<LayoutElement>();
            buttonLayout.preferredWidth = 140f;
            buttonLayout.preferredHeight = 42f;

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.98f, 0.5f, 0.18f, 1f);

            actionButton = buttonObject.GetComponent<Button>();
            actionButton.targetGraphic = image;

            if (actionLabel == null)
            {
                actionLabel = CreateText(buttonObject.transform, "ActionLabel", font, 18f, FontStyles.Bold);
                actionLabel.alignment = TextAlignmentOptions.Center;
                RectTransform rect = actionLabel.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
        }
    }

    private static TMP_Text CreateText(Transform parent, string name, TMP_FontAsset font, float size, FontStyles style)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.font = font;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = Color.white;
        text.raycastTarget = false;
        text.enableWordWrapping = true;
        return text;
    }
}
