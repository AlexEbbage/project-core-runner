using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class RunUiPrefabBuilder
{
    private const string HudPrefabRoot = "Assets/Prefabs/UI/HUD";
    private const string OverlayPrefabRoot = "Assets/Prefabs/UI/Overlays";

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
        var canvas = CreateCanvasRoot("HUDCanvas");
        var uiRoot = CreateRect("UIRoot", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var hudElements = BuildHudTopBar(uiRoot);
        var hudView = hudElements.root.gameObject.AddComponent<HudView>();
        AssignHudView(hudView, hudElements);
        return canvas;
    }

    private static GameObject BuildRunUiCanvas(RewardRowView rewardRowPrefab)
    {
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
        topBar.sizeDelta = new Vector2(0, 220f);
        var layout = topBar.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 24f;
        layout.padding = new RectOffset(32, 32, 24, 24);
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        var leftCluster = CreateRect("LeftCluster", topBar, Vector2.zero, Vector2.one, new Vector2(0, 0.5f), new Vector2(0, 0));
        var leftLayout = leftCluster.gameObject.AddComponent<VerticalLayoutGroup>();
        leftLayout.childAlignment = TextAnchor.MiddleLeft;
        leftLayout.spacing = 8f;
        leftLayout.childForceExpandHeight = false;
        leftLayout.childForceExpandWidth = false;
        leftCluster.gameObject.AddComponent<LayoutElement>().preferredWidth = 320f;

        var scoreText = CreateText("ScoreText", leftCluster, "0", 64, TextAlignmentOptions.Left);
        var bestText = CreateText("BestText", leftCluster, "Best 0", 28, TextAlignmentOptions.Left);

        var centerCluster = CreateRect("CenterCluster", topBar, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero);
        var centerLayout = centerCluster.gameObject.AddComponent<VerticalLayoutGroup>();
        centerLayout.childAlignment = TextAnchor.MiddleCenter;
        centerLayout.childForceExpandHeight = false;
        centerLayout.childForceExpandWidth = true;
        centerCluster.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

        var progressBar = CreateProgressSlider("RunProgressBar", centerCluster, new Vector2(520f, 22f));
        progressBar.minValue = 0f;
        progressBar.maxValue = 1f;
        progressBar.value = 0f;
        AddLayoutSize(progressBar.GetComponent<RectTransform>(), 520f, 22f);

        var rightCluster = CreateRect("RightCluster", topBar, Vector2.zero, Vector2.one, new Vector2(1, 0.5f), Vector2.zero);
        rightCluster.gameObject.AddComponent<LayoutElement>().preferredWidth = 200f;
        var rightLayout = rightCluster.gameObject.AddComponent<HorizontalLayoutGroup>();
        rightLayout.childAlignment = TextAnchor.MiddleRight;
        rightLayout.childForceExpandWidth = false;
        rightLayout.childForceExpandHeight = false;

        var pauseButtonRoot = CreateButton("PauseButton", rightCluster, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), Vector2.zero);
        pauseButtonRoot.sizeDelta = new Vector2(140f, 80f);
        var pauseLabel = pauseButtonRoot.GetComponentInChildren<TMP_Text>();
        if (pauseLabel != null)
            pauseLabel.text = "Pause";
        pauseButtonRoot.gameObject.AddComponent<LayoutElement>().preferredWidth = 140f;

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
        contentLayout.spacing = 12f;
        contentLayout.padding = new RectOffset(12, 12, 12, 12);
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
        image.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);

        var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.spacing = 18f;
        layout.padding = new RectOffset(24, 24, 32, 32);
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
        background.color = new Color(1f, 1f, 1f, 0.2f);

        var checkmark = CreateImage("Checkmark", toggleRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(28f, 28f));
        checkmark.color = new Color(0.3f, 0.9f, 0.4f, 1f);
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
        tmp.color = Color.white;
        return tmp;
    }

    private static Slider CreateProgressSlider(string name, Transform parent, Vector2 sizeDelta)
    {
        var root = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        root.sizeDelta = sizeDelta;
        var background = root.gameObject.AddComponent<Image>();
        background.color = new Color(1f, 1f, 1f, 0.2f);

        var fillArea = CreateRect("FillArea", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var fill = CreateImage("Fill", fillArea, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f), Vector2.zero, Vector2.zero);
        fill.color = new Color(0.3f, 0.8f, 0.2f, 1f);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = 1f;

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
        image.color = new Color(1f, 1f, 1f, 0.2f);
        var button = rect.gameObject.AddComponent<Button>();
        var handler = rect.gameObject.AddComponent<UiInteractionHandler>();
        button.onClick.AddListener(handler.HandleClick);
        var label = CreateText("Label", rect, name, 24);
        label.raycastTarget = false;
        return rect;
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
