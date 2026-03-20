using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementsPageController : MonoBehaviour
{
    [SerializeField] private PlayerProfile profile;
    [SerializeField] private AchievementsConfig config;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private GameManager gameManager;

    private readonly List<GameObject> _spawnedObjects = new();

    private void Awake()
    {
        if (profile == null)
        {
            PlayerProfile[] profiles = Resources.FindObjectsOfTypeAll<PlayerProfile>();
            if (profiles != null && profiles.Length > 0)
                profile = profiles[0];
        }

        if (config == null)
        {
            AchievementsConfig[] configs = Resources.FindObjectsOfTypeAll<AchievementsConfig>();
            if (configs != null && configs.Length > 0)
                config = configs[0];
        }

        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        EnsureContentRoot();
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        EnsureContentRoot();
        ClearContent();

        if (profile == null || config == null || contentRoot == null)
            return;

        for (int i = 0; i < config.achievements.Count; i++)
        {
            AchievementDefinition achievement = config.achievements[i];
            if (achievement == null || string.IsNullOrWhiteSpace(achievement.id))
                continue;

            BuildAchievementCard(achievement);
        }
    }

    private void EnsureContentRoot()
    {
        if (contentRoot != null)
            return;

        RectTransform existing = GetComponentInChildren<RectTransform>(true);
        if (existing != null && existing != transform)
            contentRoot = existing;

        if (contentRoot != null)
            return;

        GameObject rootObject = new GameObject("AchievementsContent", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        rootObject.transform.SetParent(transform, false);
        contentRoot = rootObject.GetComponent<RectTransform>();
        contentRoot.anchorMin = new Vector2(0f, 0f);
        contentRoot.anchorMax = new Vector2(1f, 1f);
        contentRoot.offsetMin = new Vector2(48f, 48f);
        contentRoot.offsetMax = new Vector2(-48f, -48f);

        VerticalLayoutGroup layout = rootObject.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 14f;
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = rootObject.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private void BuildAchievementCard(AchievementDefinition achievement)
    {
        int progress = profile.GetAchievementMetricValue(achievement.metricType);
        int claimedTierCount = profile.GetClaimedAchievementTierCount(achievement.id);

        GameObject card = new GameObject($"{achievement.id}_Card", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter), typeof(LayoutElement));
        card.transform.SetParent(contentRoot, false);
        _spawnedObjects.Add(card);

        Image background = card.GetComponent<Image>();
        background.color = new Color(0.07f, 0.1f, 0.14f, 0.92f);

        LayoutElement layout = card.GetComponent<LayoutElement>();
        layout.preferredHeight = 180f;

        VerticalLayoutGroup group = card.GetComponent<VerticalLayoutGroup>();
        group.padding = new RectOffset(18, 18, 16, 16);
        group.spacing = 8f;
        group.childControlWidth = true;
        group.childControlHeight = false;
        group.childForceExpandWidth = true;
        group.childForceExpandHeight = false;

        ContentSizeFitter fitter = card.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        TMP_FontAsset font = TMP_Settings.defaultFontAsset;
        CreateText(card.transform, achievement.title, font, 28f, FontStyles.Bold);
        CreateText(card.transform, achievement.description, font, 20f, FontStyles.Normal);
        CreateText(card.transform, $"{LocalizationService.Get("ui.achievement_progress", "Progress")}: {progress}", font, 18f, FontStyles.Normal);

        for (int i = 0; i < achievement.tiers.Count; i++)
        {
            AchievementTierDefinition tier = achievement.tiers[i];
            if (tier == null)
                continue;

            BuildTierRow(card.transform, achievement, tier, i, progress, claimedTierCount, font);
        }
    }

    private void BuildTierRow(Transform parent, AchievementDefinition achievement, AchievementTierDefinition tier, int tierIndex, int progress, int claimedTierCount, TMP_FontAsset font)
    {
        GameObject row = new GameObject($"Tier_{tierIndex + 1}", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(parent, false);
        _spawnedObjects.Add(row);

        LayoutElement layout = row.GetComponent<LayoutElement>();
        layout.preferredHeight = 42f;

        HorizontalLayoutGroup group = row.GetComponent<HorizontalLayoutGroup>();
        group.spacing = 10f;
        group.childControlWidth = false;
        group.childControlHeight = true;
        group.childForceExpandWidth = false;
        group.childForceExpandHeight = false;

        TMP_Text label = CreateText(row.transform, $"Tier {tierIndex + 1}: {tier.targetValue} - {tier.rewardLabel}", font, 18f, FontStyles.Normal);
        label.alignment = TextAlignmentOptions.Left;
        LayoutElement labelLayout = label.gameObject.AddComponent<LayoutElement>();
        labelLayout.flexibleWidth = 1f;

        Button button = CreateButton(row.transform, font);
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>(true);

        bool claimed = tierIndex < claimedTierCount;
        bool claimable = !claimed && tierIndex == claimedTierCount && progress >= tier.targetValue;

        buttonText.text = claimed
            ? LocalizationService.Get("ui.achievement_claimed", "Claimed")
            : claimable
                ? LocalizationService.Get("ui.achievement_claim", "Claim")
                : LocalizationService.Get("ui.achievement_locked", "Locked");

        button.interactable = claimable;
        if (claimable)
        {
            button.onClick.AddListener(() =>
            {
                if (profile.TryClaimAchievementTier(achievement.id, tierIndex, tier, progress))
                {
                    gameManager?.LogAnalyticsEvent(AnalyticsEventNames.AchievementTierClaimed, new Dictionary<string, object>
                    {
                        { AnalyticsEventNames.Params.Source, "achievements" },
                        { AnalyticsEventNames.Params.AchievementId, achievement.id },
                        { AnalyticsEventNames.Params.TierIndex, tierIndex },
                        { AnalyticsEventNames.Params.RewardKind, tier.rewardType.ToString() },
                        { AnalyticsEventNames.Params.Amount, tier.rewardAmount }
                    });
                    Refresh();
                    FindFirstObjectByType<MainMenuController>()?.RefreshHubChrome();
                }
            });
        }
    }

    private void ClearContent()
    {
        for (int i = 0; i < _spawnedObjects.Count; i++)
        {
            if (_spawnedObjects[i] != null)
                Destroy(_spawnedObjects[i]);
        }

        _spawnedObjects.Clear();
    }

    private static TMP_Text CreateText(Transform parent, string value, TMP_FontAsset font, float fontSize, FontStyles style)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.font = font;
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = Color.white;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(Transform parent, TMP_FontAsset font)
    {
        GameObject buttonObject = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetParent(parent, false);

        LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
        layout.preferredWidth = 120f;
        layout.preferredHeight = 36f;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.98f, 0.5f, 0.18f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        TMP_Text label = CreateText(buttonObject.transform, LocalizationService.Get("ui.achievement_claim", "Claim"), font, 16f, FontStyles.Bold);
        label.alignment = TextAlignmentOptions.Center;
        RectTransform rect = label.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        return button;
    }
}
