using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyLoginRewardPreviewView : MonoBehaviour
{
    [SerializeField] private DailyLoginRewardsManager rewardsManager;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private bool claimOnEnable = true;

    private bool _attemptedClaim;

    private void Awake()
    {
        if (rewardsManager == null)
            rewardsManager = FindFirstObjectByType<DailyLoginRewardsManager>();
    }

    private void OnEnable()
    {
        LocalizationService.LanguageChanged += Refresh;

        if (claimOnEnable && !_attemptedClaim)
            TryClaimOnOpen();

        Refresh();
    }

    private void OnDisable()
    {
        LocalizationService.LanguageChanged -= Refresh;
    }

    public void Refresh()
    {
        if (titleText != null)
            titleText.text = LocalizationService.Get("ui.daily_login_title", "Daily Login");

        if (rewardsManager == null)
        {
            SetText(rewardText, LocalizationService.Get("ui.daily_login_unavailable", "Rewards unavailable."));
            SetText(statusText, LocalizationService.Get("ui.daily_login_unavailable_detail", "Login rewards are not configured."));
            return;
        }

        DailyLoginRewardEntry reward = rewardsManager.GetNextRewardPreview(out int dayIndex);
        if (dayIndex <= 0)
        {
            SetText(rewardText, LocalizationService.Get("ui.daily_login_unavailable", "Rewards unavailable."));
            SetText(statusText, LocalizationService.Get("ui.daily_login_unavailable_detail", "Login rewards are not configured."));
            return;
        }

        string rewardLabel = DescribeReward(reward);
        SetText(rewardText, LocalizationService.Format("ui.daily_login_next_reward", "Next reward (Day {0}): {1}", dayIndex, rewardLabel));

        bool canClaim = rewardsManager.CanClaimToday();
        SetText(statusText, canClaim
            ? LocalizationService.Get("ui.daily_login_claim_available", "Claim available today.")
            : LocalizationService.Get("ui.daily_login_claimed", "Claimed today."));
    }

    private void TryClaimOnOpen()
    {
        _attemptedClaim = true;
        if (rewardsManager != null)
            rewardsManager.TryClaimReward();
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

        return view;
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
}
