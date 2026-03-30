using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyLoginRewardPreviewView : MonoBehaviour
{
    [SerializeField] private DailyLoginRewardsManager rewardsManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button claimButton;
    [SerializeField] private TMP_Text claimButtonText;
    [SerializeField] private Button doubleClaimButton;
    [SerializeField] private TMP_Text doubleClaimButtonText;

    private void Awake()
    {
        if (rewardsManager == null)
            rewardsManager = FindFirstObjectByType<DailyLoginRewardsManager>();

        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

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
            SetClaimButtons(false, false);
            return;
        }

        DailyLoginRewardEntry reward = rewardsManager.GetNextRewardPreview(out int dayIndex);
        if (dayIndex <= 0)
        {
            SetText(rewardText, LocalizationService.Get("ui.daily_login_unavailable", "Rewards unavailable."));
            SetText(statusText, LocalizationService.Get("ui.daily_login_unavailable_detail", "Login rewards are not configured."));
            SetClaimButtons(false, false);
            return;
        }

        string rewardLabel = DescribeReward(reward);
        SetText(rewardText, LocalizationService.Format("ui.daily_login_next_reward", "Next reward (Day {0}): {1}", dayIndex, rewardLabel));

        bool canClaim = rewardsManager.CanClaimToday();
        bool canDoubleClaim = canClaim && gameManager != null && gameManager.CanShowDailyLoginDoubleRewardAd();
        SetText(statusText, canClaim
            ? LocalizationService.Get("ui.daily_login_claim_available", "Claim available today.")
            : LocalizationService.Get("ui.daily_login_claimed", "Claimed today."));
        SetClaimButtons(canClaim, canDoubleClaim);
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

    public void TryClaimDouble()
    {
        if (rewardsManager == null || gameManager == null || !rewardsManager.CanClaimToday())
            return;

        bool started = gameManager.TryShowDailyLoginDoubleRewardAd(result =>
        {
            if (result == RewardedAdResult.Rewarded && rewardsManager.TryClaimReward(true))
            {
                FindFirstObjectByType<ProgressionTasksController>()?.Refresh();
                FindFirstObjectByType<MainMenuController>()?.RefreshHubChrome();
            }

            Refresh();
        });

        if (!started)
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

    private void SetClaimButtons(bool canClaim, bool canDoubleClaim)
    {
        if (claimButton != null)
            claimButton.interactable = canClaim;

        if (claimButtonText != null)
        {
            claimButtonText.text = canClaim
                ? LocalizationService.Get("ui.daily_login_claim_button", "Claim")
                : LocalizationService.Get("ui.daily_login_claimed_button", "Claimed");
        }

        if (doubleClaimButton != null)
        {
            doubleClaimButton.gameObject.SetActive(canDoubleClaim);
            doubleClaimButton.interactable = canDoubleClaim;
        }

        if (doubleClaimButtonText != null)
        {
            doubleClaimButtonText.text = canDoubleClaim
                ? LocalizationService.Get("ui.daily_login_claim_double_button", "Claim x2")
                : LocalizationService.Get("ui.daily_login_claim_double_unavailable", "Claim x2");
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
        GameObject buttonRow = new("ButtonRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        buttonRow.transform.SetParent(root.transform, false);
        HorizontalLayoutGroup buttonRowLayout = buttonRow.GetComponent<HorizontalLayoutGroup>();
        buttonRowLayout.spacing = 8f;
        buttonRowLayout.childAlignment = TextAnchor.MiddleLeft;
        buttonRowLayout.childControlWidth = false;
        buttonRowLayout.childControlHeight = true;
        buttonRowLayout.childForceExpandWidth = false;
        buttonRowLayout.childForceExpandHeight = false;
        LayoutElement buttonRowElement = buttonRow.GetComponent<LayoutElement>();
        buttonRowElement.preferredHeight = 44f;

        view.claimButton = CreateButton(buttonRow.transform, template, out view.claimButtonText, "ClaimButton");
        view.claimButton.onClick.AddListener(view.TryClaim);
        view.doubleClaimButton = CreateButton(buttonRow.transform, template, out view.doubleClaimButtonText, "ClaimDoubleButton");
        view.doubleClaimButton.onClick.AddListener(view.TryClaimDouble);

        return view;
    }

    private void EnsureScaffold()
    {
        if (claimButton != null)
        {
            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(TryClaim);
        }

        if (doubleClaimButton != null)
        {
            doubleClaimButton.onClick.RemoveAllListeners();
            doubleClaimButton.onClick.AddListener(TryClaimDouble);
        }
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

    private static Button CreateButton(Transform parent, TMP_Text template, out TMP_Text buttonText, string objectName)
    {
        GameObject buttonObject = new(objectName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
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
