using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class RunUiPrefabBuilder
{
    private const string HudPrefabRoot = "Assets/Prefabs/UI/HUD";
    private const string OverlayPrefabRoot = "Assets/Prefabs/UI/Overlays";
    private const string PanelSpriteKey = "MainMenuPrefabBuilder.PanelSprite";
    private const string PanelColorKey = "MainMenuPrefabBuilder.PanelColor";
    private const string ButtonSpriteKey = "MainMenuPrefabBuilder.ButtonSprite";
    private const string ButtonColorKey = "MainMenuPrefabBuilder.ButtonColor";
    private const string ButtonFallbackColorKey = "MainMenuPrefabBuilder.ButtonFallbackColor";
    private const string ProgressFillSpriteKey = "MainMenuPrefabBuilder.ProgressFillSprite";
    private const string ProgressFillColorKey = "MainMenuPrefabBuilder.ProgressFillColor";
    private const string ProgressBackgroundColorKey = "MainMenuPrefabBuilder.ProgressBackgroundColor";
    private const string FontKey = "MainMenuPrefabBuilder.Font";
    private const string TextColorKey = "MainMenuPrefabBuilder.TextColor";
    private const string ClickSfxKey = "MainMenuPrefabBuilder.ClickSfx";
    private const string HudTopBarHeightKey = "MainMenuPrefabBuilder.HudTopBarHeight";
    private const string HudPaddingKey = "MainMenuPrefabBuilder.HudPadding";
    private const string HudSpacingKey = "MainMenuPrefabBuilder.HudSpacing";
    private const string HudProgressWidthKey = "MainMenuPrefabBuilder.HudProgressWidth";
    private const string HudProgressHeightKey = "MainMenuPrefabBuilder.HudProgressHeight";
    private const string HudScoreFontSizeKey = "MainMenuPrefabBuilder.HudScoreFontSize";
    private const string HudBestFontSizeKey = "MainMenuPrefabBuilder.HudBestFontSize";
    private const string HudPauseButtonWidthKey = "MainMenuPrefabBuilder.HudPauseButtonWidth";
    private const string HudPauseButtonHeightKey = "MainMenuPrefabBuilder.HudPauseButtonHeight";

    private static SharedStyleSettings _sharedStyle;
    private static HudLayoutSettings _hudLayout;
    private static bool _styleLoaded;

    private readonly struct HudElements
    {
        public readonly RectTransform root;
        public readonly TMP_Text scoreText;
        public readonly TMP_Text bestText;
        public readonly Slider progressBar;
        public readonly Button pauseButton;

        public HudElements(RectTransform root, TMP_Text scoreText, TMP_Text bestText, Slider progressBar, Button pauseButton)
        {
            this.root = root;
            this.scoreText = scoreText;
            this.bestText = bestText;
            this.progressBar = progressBar;
            this.pauseButton = pauseButton;
        }
    }

    private readonly struct SharedStyleSettings
    {
        public readonly Sprite panelSprite;
        public readonly Color panelColor;
        public readonly Sprite buttonSprite;
        public readonly Color buttonColor;
        public readonly Color buttonFallbackColor;
        public readonly Sprite progressFillSprite;
        public readonly Color progressFillColor;
        public readonly Color progressBackgroundColor;
        public readonly TMP_FontAsset font;
        public readonly Color textColor;
        public readonly AudioClip clickSfx;

        public SharedStyleSettings(
            Sprite panelSprite,
            Color panelColor,
            Sprite buttonSprite,
            Color buttonColor,
            Color buttonFallbackColor,
            Sprite progressFillSprite,
            Color progressFillColor,
            Color progressBackgroundColor,
            TMP_FontAsset font,
            Color textColor,
            AudioClip clickSfx)
        {
            this.panelSprite = panelSprite;
            this.panelColor = panelColor;
            this.buttonSprite = buttonSprite;
            this.buttonColor = buttonColor;
            this.buttonFallbackColor = buttonFallbackColor;
            this.progressFillSprite = progressFillSprite;
            this.progressFillColor = progressFillColor;
            this.progressBackgroundColor = progressBackgroundColor;
            this.font = font;
            this.textColor = textColor;
            this.clickSfx = clickSfx;
        }
    }

    private readonly struct HudLayoutSettings
    {
        public readonly float topBarHeight;
        public readonly int padding;
        public readonly int spacing;
        public readonly float progressWidth;
        public readonly float progressHeight;
        public readonly int scoreFontSize;
        public readonly int bestFontSize;
        public readonly float pauseButtonWidth;
        public readonly float pauseButtonHeight;

        public HudLayoutSettings(
            float topBarHeight,
            int padding,
            int spacing,
            float progressWidth,
            float progressHeight,
            int scoreFontSize,
            int bestFontSize,
            float pauseButtonWidth,
            float pauseButtonHeight)
        {
            this.topBarHeight = topBarHeight;
            this.padding = padding;
            this.spacing = spacing;
            this.progressWidth = progressWidth;
            this.progressHeight = progressHeight;
            this.scoreFontSize = scoreFontSize;
            this.bestFontSize = bestFontSize;
            this.pauseButtonWidth = pauseButtonWidth;
            this.pauseButtonHeight = pauseButtonHeight;
        }
    }

    private readonly struct PauseOverlayElements
    {
        public readonly RectTransform root;
        public readonly CanvasGroup canvasGroup;
        public readonly Button continueButton;
        public readonly Button settingsButton;
        public readonly Button mainMenuButton;

        public PauseOverlayElements(RectTransform root, CanvasGroup canvasGroup, Button continueButton, Button settingsButton, Button mainMenuButton)
        {
            this.root = root;
            this.canvasGroup = canvasGroup;
            this.continueButton = continueButton;
            this.settingsButton = settingsButton;
            this.mainMenuButton = mainMenuButton;
        }
    }

    private readonly struct SettingsOverlayElements
    {
        public readonly RectTransform root;
        public readonly CanvasGroup canvasGroup;
        public readonly Toggle musicToggle;
        public readonly Toggle sfxToggle;
        public readonly Toggle vibrateToggle;
        public readonly Toggle inputToggle;
        public readonly Button backButton;

        public SettingsOverlayElements(RectTransform root, CanvasGroup canvasGroup, Toggle musicToggle, Toggle sfxToggle, Toggle vibrateToggle, Toggle inputToggle, Button backButton)
        {
            this.root = root;
            this.canvasGroup = canvasGroup;
            this.musicToggle = musicToggle;
            this.sfxToggle = sfxToggle;
            this.vibrateToggle = vibrateToggle;
            this.inputToggle = inputToggle;
            this.backButton = backButton;
        }
    }

    private readonly struct DeathContinueOverlayElements
    {
        public readonly RectTransform root;
        public readonly CanvasGroup canvasGroup;
        public readonly TMP_Text currentScoreText;
        public readonly Button watchAdButton;
        public readonly Button backToBaseButton;

        public DeathContinueOverlayElements(RectTransform root, CanvasGroup canvasGroup, TMP_Text currentScoreText, Button watchAdButton, Button backToBaseButton)
        {
            this.root = root;
            this.canvasGroup = canvasGroup;
            this.currentScoreText = currentScoreText;
            this.watchAdButton = watchAdButton;
            this.backToBaseButton = backToBaseButton;
        }
    }

    private readonly struct ResultsOverlayElements
    {
        public readonly RectTransform root;
        public readonly CanvasGroup canvasGroup;
        public readonly RectTransform rewardsContent;
        public readonly Button doubleRewardsButton;
        public readonly Slider timerBar;
        public readonly Button backToHubButton;

        public ResultsOverlayElements(RectTransform root, CanvasGroup canvasGroup, RectTransform rewardsContent, Button doubleRewardsButton, Slider timerBar, Button backToHubButton)
        {
            this.root = root;
            this.canvasGroup = canvasGroup;
            this.rewardsContent = rewardsContent;
            this.doubleRewardsButton = doubleRewardsButton;
            this.timerBar = timerBar;
            this.backToHubButton = backToHubButton;
        }
    }

    [MenuItem("Tools/UI/Build HUD + Overlays UI")]
    public static void BuildHudAndOverlaysUI()
    {
        ReloadSharedStyle();
        EnsureFolder(HudPrefabRoot);
        EnsureFolder(OverlayPrefabRoot);

        var rewardRowPrefab = BuildRewardRowPrefab();
        var rewardRowPath = Path.Combine(OverlayPrefabRoot, "RewardRow.prefab");
        PrefabUtility.SaveAsPrefabAsset(rewardRowPrefab.gameObject, rewardRowPath);
        Object.DestroyImmediate(rewardRowPrefab.gameObject);

        var rewardRowViewPrefab = AssetDatabase.LoadAssetAtPath<RewardRowView>(rewardRowPath);

        var hudCanvas = BuildHudCanvas();
        PrefabUtility.SaveAsPrefabAsset(hudCanvas, Path.Combine(HudPrefabRoot, "HUDCanvas.prefab"));
        Object.DestroyImmediate(hudCanvas);

        var runCanvas = BuildRunUiCanvas(rewardRowViewPrefab);
        PrefabUtility.SaveAsPrefabAsset(runCanvas, Path.Combine(OverlayPrefabRoot, "RunOverlaysCanvas.prefab"));
        Object.DestroyImmediate(runCanvas);
    }

    private static GameObject BuildHudCanvas()
    {
        EnsureStyleLoaded();
        var canvas = CreateCanvasRoot("HUDCanvas");
        var uiRoot = CreateRect("UIRoot", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var hudElements = BuildHudTopBar(uiRoot);
        var hudView = hudElements.root.gameObject.AddComponent<HudView>();
        AssignHudView(hudView, hudElements);
        return canvas;
    }

    private static GameObject BuildRunUiCanvas(RewardRowView rewardRowPrefab)
    {
        EnsureStyleLoaded();
        var canvas = CreateCanvasRoot("RunOverlaysCanvas");
        var uiRoot = CreateRect("UIRoot", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var hudElements = BuildHudTopBar(uiRoot);
        var overlaysRoot = CreateRect("OverlaysRoot", uiRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        var pauseOverlay = BuildPauseOverlay(overlaysRoot);
        var settingsOverlay = BuildSettingsOverlay(overlaysRoot);
        var deathOverlay = BuildDeathContinueOverlay(overlaysRoot);
        var resultsOverlay = BuildResultsOverlay(overlaysRoot);

        var hudView = hudElements.root.gameObject.AddComponent<HudView>();
        AssignHudView(hudView, hudElements);

        var pauseView = pauseOverlay.root.gameObject.AddComponent<PauseMenuView>();
        AssignPauseView(pauseView, pauseOverlay);

        var settingsView = settingsOverlay.root.gameObject.AddComponent<SettingsMenuView>();
        AssignSettingsView(settingsView, settingsOverlay);

        var deathView = deathOverlay.root.gameObject.AddComponent<DeathContinueView>();
        AssignDeathContinueView(deathView, deathOverlay);

        var resultsView = resultsOverlay.root.gameObject.AddComponent<ResultsView>();
        AssignResultsView(resultsView, resultsOverlay, rewardRowPrefab);

        var controller = uiRoot.gameObject.AddComponent<RunUiController>();
        SetSerializedReference(controller, "hudView", hudView);
        SetSerializedReference(controller, "pauseMenuView", pauseView);
        SetSerializedReference(controller, "settingsMenuView", settingsView);
        SetSerializedReference(controller, "deathContinueView", deathView);
        SetSerializedReference(controller, "resultsView", resultsView);

        return canvas;
    }

    private static GameObject CreateCanvasRoot(string name)
    {
        var canvas = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvasComponent = canvas.GetComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        var eventSystem = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }

        return canvas;
    }

    private static HudElements BuildHudTopBar(Transform parent)
    {
        var hudRoot = CreateRect("HudRoot", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var topBar = CreateRect("TopHudBar", hudRoot, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        topBar.sizeDelta = new Vector2(0, _hudLayout.topBarHeight);
        var layout = topBar.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = _hudLayout.spacing;
        layout.padding = new RectOffset(_hudLayout.padding, _hudLayout.padding, _hudLayout.padding, _hudLayout.padding);
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        var leftCluster = CreateRect("LeftCluster", topBar, Vector2.zero, Vector2.one, new Vector2(0, 0.5f), new Vector2(0, 0));
        var leftLayout = leftCluster.gameObject.AddComponent<VerticalLayoutGroup>();
        leftLayout.childAlignment = TextAnchor.MiddleLeft;
        leftLayout.spacing = 8f;
        leftLayout.childForceExpandHeight = false;
        leftLayout.childForceExpandWidth = false;
        leftCluster.gameObject.AddComponent<LayoutElement>().preferredWidth = 320f;

        var scoreText = CreateText("ScoreText", leftCluster, "0", _hudLayout.scoreFontSize, TextAlignmentOptions.Left);
        var bestText = CreateText("BestText", leftCluster, "Best 0", _hudLayout.bestFontSize, TextAlignmentOptions.Left);

        var centerCluster = CreateRect("CenterCluster", topBar, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero);
        var centerLayout = centerCluster.gameObject.AddComponent<VerticalLayoutGroup>();
        centerLayout.childAlignment = TextAnchor.MiddleCenter;
        centerLayout.childForceExpandHeight = false;
        centerLayout.childForceExpandWidth = true;
        centerCluster.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

        var progressBar = CreateProgressSlider("RunProgressBar", centerCluster, new Vector2(_hudLayout.progressWidth, _hudLayout.progressHeight));
        progressBar.minValue = 0f;
        progressBar.maxValue = 1f;
        progressBar.value = 0f;
        AddLayoutSize(progressBar.GetComponent<RectTransform>(), _hudLayout.progressWidth, _hudLayout.progressHeight);

        var rightCluster = CreateRect("RightCluster", topBar, Vector2.zero, Vector2.one, new Vector2(1, 0.5f), Vector2.zero);
        rightCluster.gameObject.AddComponent<LayoutElement>().preferredWidth = 200f;
        var rightLayout = rightCluster.gameObject.AddComponent<HorizontalLayoutGroup>();
        rightLayout.childAlignment = TextAnchor.MiddleRight;
        rightLayout.childForceExpandWidth = false;
        rightLayout.childForceExpandHeight = false;

        var pauseButtonRoot = CreateButton("PauseButton", rightCluster, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), Vector2.zero);
        pauseButtonRoot.sizeDelta = new Vector2(_hudLayout.pauseButtonWidth, _hudLayout.pauseButtonHeight);
        var pauseLabel = pauseButtonRoot.GetComponentInChildren<TMP_Text>();
        if (pauseLabel != null)
            pauseLabel.text = "Pause";
        pauseButtonRoot.gameObject.AddComponent<LayoutElement>().preferredWidth = _hudLayout.pauseButtonWidth;

        return new HudElements(hudRoot, scoreText, bestText, progressBar, pauseButtonRoot.GetComponent<Button>());
    }

    private static PauseOverlayElements BuildPauseOverlay(Transform parent)
    {
        var overlay = CreateOverlayRoot("PauseOverlay", parent);
        var panel = CreatePanel(overlay, new Vector2(720f, 760f));
        CreateText("TitleText", panel, "PAUSED", 48, TextAlignmentOptions.Center);

        var buttonRoot = CreateRect("Buttons", panel, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero);
        var layout = buttonRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 18f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var continueButton = CreateButton("ContinueButton", buttonRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        continueButton.sizeDelta = new Vector2(420f, 90f);
        AddLayoutSize(continueButton, 420f, 90f);
        var settingsButton = CreateButton("SettingsButton", buttonRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        settingsButton.sizeDelta = new Vector2(420f, 90f);
        AddLayoutSize(settingsButton, 420f, 90f);
        var mainMenuButton = CreateButton("MainMenuButton", buttonRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        mainMenuButton.sizeDelta = new Vector2(420f, 90f);
        AddLayoutSize(mainMenuButton, 420f, 90f);

        return new PauseOverlayElements(overlay, overlay.GetComponent<CanvasGroup>(), continueButton.GetComponent<Button>(), settingsButton.GetComponent<Button>(), mainMenuButton.GetComponent<Button>());
    }

    private static SettingsOverlayElements BuildSettingsOverlay(Transform parent)
    {
        var overlay = CreateOverlayRoot("SettingsOverlay", parent);
        var panel = CreatePanel(overlay, new Vector2(760f, 900f));
        CreateText("TitleText", panel, "SETTINGS", 48, TextAlignmentOptions.Center);

        var toggleRoot = CreateRect("ToggleList", panel, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero);
        var layout = toggleRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 14f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var musicToggle = CreateToggleRow(toggleRoot, "ToggleRow_Music", "Music");
        var sfxToggle = CreateToggleRow(toggleRoot, "ToggleRow_Sfx", "SFX");
        var vibrateToggle = CreateToggleRow(toggleRoot, "ToggleRow_Vibrate", "Vibrate");
        var inputToggle = CreateToggleRow(toggleRoot, "ToggleRow_Input", "Input");

        var backButton = CreateButton("BackButton", panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        backButton.sizeDelta = new Vector2(360f, 84f);
        AddLayoutSize(backButton, 360f, 84f);

        return new SettingsOverlayElements(overlay, overlay.GetComponent<CanvasGroup>(), musicToggle, sfxToggle, vibrateToggle, inputToggle, backButton.GetComponent<Button>());
    }

    private static DeathContinueOverlayElements BuildDeathContinueOverlay(Transform parent)
    {
        var overlay = CreateOverlayRoot("DeathContinueOverlay", parent);
        var panel = CreatePanel(overlay, new Vector2(760f, 760f));
        CreateText("TitleText", panel, "CONTINUE?", 48, TextAlignmentOptions.Center);
        var currentScoreText = CreateText("CurrentScoreText", panel, "Score: 0", 32, TextAlignmentOptions.Center);

        var buttonRoot = CreateRect("Buttons", panel, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero);
        var layout = buttonRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 18f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var watchAdButton = CreateButton("WatchAdContinueButton", buttonRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        watchAdButton.sizeDelta = new Vector2(460f, 92f);
        AddLayoutSize(watchAdButton, 460f, 92f);
        var backToBaseButton = CreateButton("BackToBaseButton", buttonRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        backToBaseButton.sizeDelta = new Vector2(460f, 92f);
        AddLayoutSize(backToBaseButton, 460f, 92f);

        return new DeathContinueOverlayElements(overlay, overlay.GetComponent<CanvasGroup>(), currentScoreText, watchAdButton.GetComponent<Button>(), backToBaseButton.GetComponent<Button>());
    }

    private static ResultsOverlayElements BuildResultsOverlay(Transform parent)
    {
        var overlay = CreateOverlayRoot("ResultsOverlay", parent);
        var panel = CreatePanel(overlay, new Vector2(900f, 1200f));
        CreateText("TitleText", panel, "RESULTS", 50, TextAlignmentOptions.Center);

        var rewardsScroll = BuildRewardsScroll(panel);

        var doubleRewardsButton = CreateButton("DoubleRewardsAdButton", panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        doubleRewardsButton.sizeDelta = new Vector2(560f, 92f);
        AddLayoutSize(doubleRewardsButton, 560f, 92f);
        var doubleLabel = doubleRewardsButton.GetComponentInChildren<TMP_Text>();
        if (doubleLabel != null)
            doubleLabel.text = "Watch Ad for Double";

        var timerBar = CreateProgressSlider("TimerBar", panel, new Vector2(720f, 20f));
        timerBar.minValue = 0f;
        timerBar.maxValue = 1f;
        timerBar.value = 1f;
        AddLayoutSize(timerBar.GetComponent<RectTransform>(), 720f, 20f);

        var backToHubButton = CreateButton("BackToHubButton", panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        backToHubButton.sizeDelta = new Vector2(360f, 84f);
        AddLayoutSize(backToHubButton, 360f, 84f);
        backToHubButton.gameObject.SetActive(false);

        return new ResultsOverlayElements(overlay, overlay.GetComponent<CanvasGroup>(), rewardsScroll, doubleRewardsButton.GetComponent<Button>(), timerBar, backToHubButton.GetComponent<Button>());
    }

    private static RectTransform BuildRewardsScroll(Transform parent)
    {
        var scrollRoot = CreateRect("RewardsScroll", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        scrollRoot.sizeDelta = new Vector2(720f, 420f);
        AddLayoutSize(scrollRoot, 720f, 420f);
        var scrollImage = scrollRoot.gameObject.AddComponent<Image>();
        scrollImage.color = new Color(1f, 1f, 1f, 0.08f);

        var scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        var viewport = CreateRect("Viewport", scrollRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var viewportImage = viewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.02f);
        viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

        var content = CreateRect("Content", viewport, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        var contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        contentLayout.childAlignment = TextAnchor.UpperCenter;
        contentLayout.spacing = Mathf.Max(0f, _hudLayout.spacing);
        contentLayout.padding = new RectOffset(_hudLayout.padding, _hudLayout.padding, _hudLayout.padding, _hudLayout.padding);
        contentLayout.childForceExpandHeight = false;
        contentLayout.childForceExpandWidth = true;
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewport;
        scrollRect.content = content;

        return content;
    }

    private static RectTransform BuildRewardRowPrefab()
    {
        var root = CreateRect("RewardRow", null, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        root.sizeDelta = new Vector2(680f, 84f);
        AddLayoutSize(root, 680f, 84f);
        var layout = root.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.spacing = 16f;
        layout.padding = new RectOffset(12, 12, 12, 12);
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;

        var icon = CreateImage("IconImage", root, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(64f, 64f));
        icon.raycastTarget = false;
        var label = CreateText("LabelText", root, "XP", 28, TextAlignmentOptions.Left);
        var value = CreateText("ValueText", root, "+0", 28, TextAlignmentOptions.Right);
        value.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

        var view = root.gameObject.AddComponent<RewardRowView>();
        SetSerializedReference(view, "iconImage", icon);
        SetSerializedReference(view, "labelText", label);
        SetSerializedReference(view, "valueText", value);

        return root;
    }

    private static RectTransform CreateOverlayRoot(string name, Transform parent)
    {
        var root = CreateRect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var group = root.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        var dimmer = CreateImage("Dimmer", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Vector2.zero);
        dimmer.color = new Color(0f, 0f, 0f, 0.6f);
        dimmer.raycastTarget = true;

        root.gameObject.SetActive(false);
        return root;
    }

    private static RectTransform CreatePanel(RectTransform parent, Vector2 size)
    {
        var panel = CreateRect("Panel", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        panel.sizeDelta = size;
        var image = panel.gameObject.AddComponent<Image>();
        ApplyPanelSprite(image);
        image.color = _sharedStyle.panelSprite != null
            ? _sharedStyle.panelColor
            : new Color(0.12f, 0.12f, 0.12f, 0.9f);

        var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.spacing = Mathf.Max(0f, _hudLayout.spacing);
        layout.padding = new RectOffset(_hudLayout.padding, _hudLayout.padding, _hudLayout.padding, _hudLayout.padding);
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        return panel;
    }

    private static Toggle CreateToggleRow(Transform parent, string name, string label)
    {
        var row = CreateRect(name, parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero);
        row.sizeDelta = new Vector2(620f, 72f);
        AddLayoutSize(row, 620f, 72f);
        var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.spacing = 16f;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;

        var toggleRoot = CreateRect("Toggle", row, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero);
        toggleRoot.sizeDelta = new Vector2(48f, 48f);
        var background = toggleRoot.gameObject.AddComponent<Image>();
        ApplyButtonSprite(background);

        var checkmark = CreateImage("Checkmark", toggleRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(28f, 28f));
        checkmark.color = _sharedStyle.progressFillColor;
        checkmark.raycastTarget = false;

        var toggle = toggleRoot.gameObject.AddComponent<Toggle>();
        toggle.targetGraphic = background;
        toggle.graphic = checkmark;
        toggle.isOn = true;

        CreateText("Label", row, label, 28, TextAlignmentOptions.Left);
        return toggle;
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        if (anchorMin != anchorMax)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        if (parent != null)
            rect.SetParent(parent, false);
        return rect;
    }

    private static void AddLayoutSize(RectTransform rect, float width, float height)
    {
        if (rect == null)
            return;

        var layout = rect.gameObject.GetComponent<LayoutElement>() ?? rect.gameObject.AddComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.preferredHeight = height;
    }

    private static Image CreateImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        var rect = CreateRect(name, parent, anchorMin, anchorMax, pivot, anchoredPosition);
        rect.sizeDelta = sizeDelta;
        return rect.gameObject.AddComponent<Image>();
    }

    private static TMP_Text CreateText(string name, Transform parent, string text, int fontSize, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        var rect = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        var tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        ApplyTextStyle(tmp);
        return tmp;
    }

    private static Slider CreateProgressSlider(string name, Transform parent, Vector2 sizeDelta)
    {
        var root = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        root.sizeDelta = sizeDelta;
        var background = root.gameObject.AddComponent<Image>();
        ApplyPanelSprite(background);
        background.color = _sharedStyle.progressBackgroundColor;

        var fillArea = CreateRect("FillArea", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var fill = CreateImage("Fill", fillArea, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f), Vector2.zero, Vector2.zero);
        fill.color = _sharedStyle.progressFillColor;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = 1f;
        ApplyProgressFillSprite(fill);

        var slider = root.gameObject.AddComponent<Slider>();
        slider.fillRect = fill.rectTransform;
        slider.targetGraphic = background;
        slider.transition = Selectable.Transition.ColorTint;
        slider.interactable = false;
        return slider;
    }

    private static RectTransform CreateButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition)
    {
        var rect = CreateRect(name, parent, anchorMin, anchorMax, pivot, anchoredPosition);
        var image = rect.gameObject.AddComponent<Image>();
        ApplyButtonSprite(image);
        var button = rect.gameObject.AddComponent<Button>();
        var handler = rect.gameObject.AddComponent<UiInteractionHandler>();
        ApplyClickSfx(handler);
        button.onClick.AddListener(handler.HandleClick);
        var label = CreateText("Label", rect, name, 24);
        label.raycastTarget = false;
        return rect;
    }

    private static void EnsureStyleLoaded()
    {
        if (_styleLoaded)
            return;

        ReloadSharedStyle();
    }

    private static SharedStyleSettings LoadSharedStyle()
    {
        var panelSprite = LoadAsset<Sprite>(PanelSpriteKey);
        var buttonSprite = LoadAsset<Sprite>(ButtonSpriteKey);
        var progressFillSprite = LoadAsset<Sprite>(ProgressFillSpriteKey);
        var font = LoadAsset<TMP_FontAsset>(FontKey);
        var clickSfx = LoadAsset<AudioClip>(ClickSfxKey);

        return new SharedStyleSettings(
            panelSprite,
            LoadColor(PanelColorKey, Color.white),
            buttonSprite,
            LoadColor(ButtonColorKey, Color.white),
            LoadColor(ButtonFallbackColorKey, new Color(1f, 1f, 1f, 0.2f)),
            progressFillSprite,
            LoadColor(ProgressFillColorKey, new Color(0.3f, 0.8f, 0.2f, 1f)),
            LoadColor(ProgressBackgroundColorKey, new Color(1f, 1f, 1f, 0.2f)),
            font,
            LoadColor(TextColorKey, Color.white),
            clickSfx);
    }

    private static HudLayoutSettings LoadHudLayout()
    {
        return new HudLayoutSettings(
            Mathf.Max(0f, EditorPrefs.GetFloat(HudTopBarHeightKey, 220f)),
            Mathf.Max(0, EditorPrefs.GetInt(HudPaddingKey, 24)),
            Mathf.Max(0, EditorPrefs.GetInt(HudSpacingKey, 24)),
            Mathf.Max(0f, EditorPrefs.GetFloat(HudProgressWidthKey, 520f)),
            Mathf.Max(0f, EditorPrefs.GetFloat(HudProgressHeightKey, 22f)),
            Mathf.Max(0, EditorPrefs.GetInt(HudScoreFontSizeKey, 64)),
            Mathf.Max(0, EditorPrefs.GetInt(HudBestFontSizeKey, 28)),
            Mathf.Max(0f, EditorPrefs.GetFloat(HudPauseButtonWidthKey, 140f)),
            Mathf.Max(0f, EditorPrefs.GetFloat(HudPauseButtonHeightKey, 80f)));
    }

    private static T LoadAsset<T>(string key) where T : Object
    {
        var guid = EditorPrefs.GetString(key, string.Empty);
        if (string.IsNullOrWhiteSpace(guid))
            return null;

        var path = AssetDatabase.GUIDToAssetPath(guid);
        return string.IsNullOrWhiteSpace(path) ? null : AssetDatabase.LoadAssetAtPath<T>(path);
    }

    private static Color LoadColor(string key, Color fallback)
    {
        var value = EditorPrefs.GetString(key, string.Empty);
        return ColorUtility.TryParseHtmlString(value, out var color) ? color : fallback;
    }

    private static void ApplyTextStyle(TMP_Text text)
    {
        if (text == null)
            return;

        if (_sharedStyle.font != null)
            text.font = _sharedStyle.font;
        text.color = _sharedStyle.textColor;
    }

    private static void ApplyPanelSprite(Image image)
    {
        if (image == null)
            return;

        if (_sharedStyle.panelSprite != null)
        {
            image.sprite = _sharedStyle.panelSprite;
            image.type = Image.Type.Sliced;
        }

        image.color = _sharedStyle.panelColor;
    }

    private static void ApplyButtonSprite(Image image)
    {
        if (image == null)
            return;

        if (_sharedStyle.buttonSprite != null)
        {
            image.sprite = _sharedStyle.buttonSprite;
            image.type = Image.Type.Sliced;
            image.color = _sharedStyle.buttonColor;
        }
        else
        {
            image.color = _sharedStyle.buttonFallbackColor;
        }
    }

    private static void ApplyProgressFillSprite(Image image)
    {
        if (image == null)
            return;

        if (_sharedStyle.progressFillSprite != null)
        {
            image.sprite = _sharedStyle.progressFillSprite;
            image.type = Image.Type.Filled;
        }
    }

    private static void ApplyClickSfx(UiInteractionHandler handler)
    {
        if (handler == null || _sharedStyle.clickSfx == null)
            return;

        SetSerializedReference(handler, "clickSfxOverride", _sharedStyle.clickSfx);
    }

    private static void ReloadSharedStyle()
    {
        _sharedStyle = LoadSharedStyle();
        _hudLayout = LoadHudLayout();
        _styleLoaded = true;
    }

    private static void AssignHudView(HudView view, HudElements elements)
    {
        SetSerializedReference(view, "scoreText", elements.scoreText);
        SetSerializedReference(view, "bestText", elements.bestText);
        SetSerializedReference(view, "progressBar", elements.progressBar);
        SetSerializedReference(view, "pauseButton", elements.pauseButton);
    }

    private static void AssignPauseView(PauseMenuView view, PauseOverlayElements elements)
    {
        SetSerializedReference(view, "canvasGroup", elements.canvasGroup);
        SetSerializedReference(view, "continueButton", elements.continueButton);
        SetSerializedReference(view, "settingsButton", elements.settingsButton);
        SetSerializedReference(view, "mainMenuButton", elements.mainMenuButton);
    }

    private static void AssignSettingsView(SettingsMenuView view, SettingsOverlayElements elements)
    {
        SetSerializedReference(view, "canvasGroup", elements.canvasGroup);
        SetSerializedReference(view, "musicToggle", elements.musicToggle);
        SetSerializedReference(view, "sfxToggle", elements.sfxToggle);
        SetSerializedReference(view, "vibrateToggle", elements.vibrateToggle);
        SetSerializedReference(view, "inputToggle", elements.inputToggle);
        SetSerializedReference(view, "backButton", elements.backButton);
    }

    private static void AssignDeathContinueView(DeathContinueView view, DeathContinueOverlayElements elements)
    {
        SetSerializedReference(view, "canvasGroup", elements.canvasGroup);
        SetSerializedReference(view, "currentScoreText", elements.currentScoreText);
        SetSerializedReference(view, "watchAdContinueButton", elements.watchAdButton);
        SetSerializedReference(view, "backToBaseButton", elements.backToBaseButton);
    }

    private static void AssignResultsView(ResultsView view, ResultsOverlayElements elements, RewardRowView rewardRowPrefab)
    {
        SetSerializedReference(view, "canvasGroup", elements.canvasGroup);
        SetSerializedReference(view, "rewardsContentRoot", elements.rewardsContent);
        SetSerializedReference(view, "rewardRowPrefab", rewardRowPrefab);
        SetSerializedReference(view, "doubleRewardsButton", elements.doubleRewardsButton);
        SetSerializedReference(view, "timerBar", elements.timerBar);
        SetSerializedReference(view, "backToHubButton", elements.backToHubButton);
    }

    private static void SetSerializedReference(Object target, string propertyName, object value)
    {
        var serializedObject = new SerializedObject(target);
        var property = serializedObject.FindProperty(propertyName);
        if (property == null)
            return;

        if (value is Object unityObject)
        {
            property.objectReferenceValue = unityObject;
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        var parent = Path.GetDirectoryName(path);
        var folderName = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent ?? "Assets", folderName);
    }
}
