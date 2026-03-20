using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyLoginRewardPreviewView : MonoBehaviour
{
    [SerializeField] private DailyLoginRewardsManager rewardsManager;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button claimButton;
    [SerializeField] private TMP_Text claimButtonText;

    private void Awake()
    {
        if (rewardsManager == null)
            rewardsManager = FindFirstObjectByType<DailyLoginRewardsManager>();

        EnsureScaffold();
    }

    private void OnEnable()
    {
        LocalizationService.LanguageChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        LocalizationService.LanguageChanged -= Refresh;
    }

    public void Refresh()
    {
        EnsureScaffold();

        if (titleText != null)
            titleText.text = LocalizationService.Get("ui.daily_login_title", "Daily Login");

        if (rewardsManager == null)
        {
            SetText(rewardText, LocalizationService.Get("ui.daily_login_unavailable", "Rewards unavailable."));
            SetText(statusText, LocalizationService.Get("ui.daily_login_unavailable_detail", "Login rewards are not configured."));
            SetClaimButton(false);
            return;
        }

        DailyLoginRewardEntry reward = rewardsManager.GetNextRewardPreview(out int dayIndex);
        if (dayIndex <= 0)
        {
            SetText(rewardText, LocalizationService.Get("ui.daily_login_unavailable", "Rewards unavailable."));
            SetText(statusText, LocalizationService.Get("ui.daily_login_unavailable_detail", "Login rewards are not configured."));
            SetClaimButton(false);
            return;
        }

        string rewardLabel = DescribeReward(reward);
        SetText(rewardText, LocalizationService.Format("ui.daily_login_next_reward", "Next reward (Day {0}): {1}", dayIndex, rewardLabel));

        bool canClaim = rewardsManager.CanClaimToday();
        SetText(statusText, canClaim
            ? LocalizationService.Get("ui.daily_login_claim_available", "Claim available today.")
            : LocalizationService.Get("ui.daily_login_claimed", "Claimed today."));
        SetClaimButton(canClaim);
    }

    public void TryClaim()
    {
        if (rewardsManager == null)
            return;

        if (rewardsManager.TryClaimReward())
        {
            FindFirstObjectByType<ProgressionTasksController>()?.Refresh();
            FindFirstObjectByType<MainMenuController>()?.RefreshHubChrome();
        }

        Refresh();
    }

    private static string DescribeReward(DailyLoginRewardEntry reward)
    {
        switch (reward.rewardType)
        {
            case DailyLoginRewardType.SoftCurrency:
                return LocalizationService.Format("ui.daily_login_reward_soft", "{0} soft currency", reward.amount);
            case DailyLoginRewardType.PremiumCurrency:
                return LocalizationService.Format("ui.daily_login_reward_premium", "{0} premium currency", reward.amount);
            case DailyLoginRewardType.Skin:
                return string.IsNullOrEmpty(reward.itemId)
                    ? LocalizationService.Get("ui.daily_login_reward_skin", "Skin")
                    : LocalizationService.Format("ui.daily_login_reward_skin_named", "Skin ({0})", reward.itemId);
            case DailyLoginRewardType.Item:
                return string.IsNullOrEmpty(reward.itemId)
                    ? LocalizationService.Get("ui.daily_login_reward_item", "Item")
                    : LocalizationService.Format("ui.daily_login_reward_item_named", "Item ({0})", reward.itemId);
            default:
                return LocalizationService.Get("ui.daily_login_reward_default", "Reward");
        }
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }

    private void SetClaimButton(bool interactable)
    {
        if (claimButton != null)
            claimButton.interactable = interactable;

        if (claimButtonText != null)
        {
            claimButtonText.text = interactable
                ? LocalizationService.Get("ui.daily_login_claim_button", "Claim")
                : LocalizationService.Get("ui.daily_login_claimed_button", "Claimed");
        }
    }

    public static DailyLoginRewardPreviewView Create(Transform parent, TMP_Text template)
    {
        GameObject root = new("DailyLoginRewardPreview", typeof(RectTransform));
        root.transform.SetParent(parent, false);

        var layout = root.AddComponent<LayoutElement>();
        layout.preferredHeight = 120f;

        var group = root.AddComponent<VerticalLayoutGroup>();
        group.childAlignment = TextAnchor.MiddleLeft;
        group.spacing = 6f;
        group.childControlHeight = true;
        group.childControlWidth = true;
        group.childForceExpandHeight = false;
        group.childForceExpandWidth = true;

        var view = root.AddComponent<DailyLoginRewardPreviewView>();
        view.titleText = CreateText("Title", root.transform, template, 26f, FontStyles.Bold);
        view.rewardText = CreateText("Reward", root.transform, template, 20f, FontStyles.Normal);
        view.statusText = CreateText("Status", root.transform, template, 18f, FontStyles.Italic);
        view.claimButton = CreateButton(root.transform, template, out view.claimButtonText);
        view.claimButton.onClick.AddListener(view.TryClaim);

        return view;
    }

    private void EnsureScaffold()
    {
        if (claimButton == null)
            return;

        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(TryClaim);
    }

    private static TMP_Text CreateText(string name, Transform parent, TMP_Text template, float fontSize, FontStyles style)
    {
        GameObject go = new(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var text = go.AddComponent<TextMeshProUGUI>();
        if (template != null)
        {
            text.font = template.font;
            text.color = template.color;
            text.alignment = template.alignment;
            text.enableWordWrapping = template.enableWordWrapping;
        }
        else
        {
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.enableWordWrapping = true;
        }

        text.fontSize = fontSize;
        text.fontStyle = style;
        return text;
    }

    private static Button CreateButton(Transform parent, TMP_Text template, out TMP_Text buttonText)
    {
        GameObject buttonObject = new("ClaimButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetParent(parent, false);

        LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
        layout.preferredWidth = 160f;
        layout.preferredHeight = 44f;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.98f, 0.5f, 0.18f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        buttonText = CreateText("ClaimText", buttonObject.transform, template, 18f, FontStyles.Bold);
        buttonText.alignment = TextAlignmentOptions.Center;
        RectTransform rect = buttonText.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return button;
    }
}
