using UnityEngine;
using UnityEngine.UI;

public class SettingsModalController : MonoBehaviour
{
    private const float PanelWidth = 420f;
    private const float PanelHeight = 320f;

    private static SettingsModalController _instance;

    private GameObject _root;
    private Toggle _musicToggle;
    private Toggle _sfxToggle;
    private Toggle _vibrationToggle;
    private AudioManager _audioManager;
    private float _lastMusicVolume = 1f;
    private float _lastSfxVolume = 1f;

    public static void ShowModal()
    {
        EnsureInstance();
        _instance.Show();
    }

    public static void HideModal()
    {
        if (_instance == null)
            return;

        _instance.Hide();
    }

    private static void EnsureInstance()
    {
        if (_instance != null)
            return;

        _instance = FindFirstObjectByType<SettingsModalController>();

        if (_instance == null)
        {
            var host = new GameObject("SettingsModalController");
            _instance = host.AddComponent<SettingsModalController>();
            _instance.BuildUI();
        }
    }

    private void Awake()
    {
        if (_instance == null)
            _instance = this;

        if (_root == null)
            BuildUI();
    }

    private void BuildUI()
    {
        _audioManager = FindFirstObjectByType<AudioManager>();

        _root = new GameObject("SettingsModalCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = _root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        var scaler = _root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        var dimmer = CreateImage("Dimmer", _root.transform, new Color(0f, 0f, 0f, 0.6f));
        StretchRect(dimmer.GetComponent<RectTransform>());

        var panel = CreateImage("Panel", _root.transform, new Color(0.08f, 0.08f, 0.08f, 0.95f));
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(PanelWidth, PanelHeight);
        panelRect.anchoredPosition = Vector2.zero;

        var title = CreateText("Title", panel.transform, "Settings", 28);
        var titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -20f);
        titleRect.sizeDelta = new Vector2(PanelWidth - 40f, 40f);

        var closeButton = CreateButton("CloseButton", panel.transform, "X");
        var closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-12f, -12f);
        closeRect.sizeDelta = new Vector2(36f, 36f);
        closeButton.onClick.AddListener(Hide);

        var toggleContainer = new GameObject("Toggles", typeof(RectTransform));
        toggleContainer.transform.SetParent(panel.transform, false);
        var containerRect = toggleContainer.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(PanelWidth - 60f, 180f);
        containerRect.anchoredPosition = new Vector2(0f, -10f);

        var layout = toggleContainer.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        var fitter = toggleContainer.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _musicToggle = CreateToggle(toggleContainer.transform, "Music");
        _sfxToggle = CreateToggle(toggleContainer.transform, "SFX");
        _vibrationToggle = CreateToggle(toggleContainer.transform, "Vibration");

        _musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
        _sfxToggle.onValueChanged.AddListener(OnSfxToggleChanged);
        _vibrationToggle.onValueChanged.AddListener(OnVibrationToggleChanged);

        _root.SetActive(false);
    }

    public void Show()
    {
        if (_root == null)
            BuildUI();

        _root.SetActive(true);
        RefreshFromSettings();
    }

    public void Hide()
    {
        if (_root != null)
            _root.SetActive(false);
    }

    private void RefreshFromSettings()
    {
        _lastMusicVolume = SettingsData.MusicVolume > 0f ? SettingsData.MusicVolume : _lastMusicVolume;
        _lastSfxVolume = SettingsData.SfxVolume > 0f ? SettingsData.SfxVolume : _lastSfxVolume;

        if (_musicToggle != null)
            _musicToggle.SetIsOnWithoutNotify(SettingsData.MusicVolume > 0.01f);
        if (_sfxToggle != null)
            _sfxToggle.SetIsOnWithoutNotify(SettingsData.SfxVolume > 0.01f);
        if (_vibrationToggle != null)
            _vibrationToggle.SetIsOnWithoutNotify(SettingsData.VibrateEnabled);

        ApplyAudio();
    }

    private void OnMusicToggleChanged(bool enabled)
    {
        if (enabled)
        {
            SettingsData.MusicVolume = Mathf.Clamp01(_lastMusicVolume <= 0f ? 1f : _lastMusicVolume);
        }
        else
        {
            _lastMusicVolume = SettingsData.MusicVolume;
            SettingsData.MusicVolume = 0f;
        }

        ApplyAudio();
    }

    private void OnSfxToggleChanged(bool enabled)
    {
        if (enabled)
        {
            SettingsData.SfxVolume = Mathf.Clamp01(_lastSfxVolume <= 0f ? 1f : _lastSfxVolume);
        }
        else
        {
            _lastSfxVolume = SettingsData.SfxVolume;
            SettingsData.SfxVolume = 0f;
        }

        ApplyAudio();
    }

    private void OnVibrationToggleChanged(bool enabled)
    {
        SettingsData.VibrateEnabled = enabled;
    }

    private void ApplyAudio()
    {
        if (_audioManager == null)
            _audioManager = FindFirstObjectByType<AudioManager>();

        if (_audioManager == null)
            return;

        _audioManager.SetMusicVolume(SettingsData.MusicVolume);
        _audioManager.SetSfxVolume(SettingsData.SfxVolume);
    }

    public static Button CreateCogButton(Transform parent, Vector2 anchoredPosition)
    {
        var buttonObject = new GameObject("SettingsButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        var rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.sizeDelta = new Vector2(64f, 64f);
        rect.anchoredPosition = anchoredPosition;

        var image = buttonObject.GetComponent<Image>();
        image.sprite = GetBuiltinSprite("UI/Skin/UISprite.psd");
        image.type = Image.Type.Sliced;
        image.color = new Color(1f, 1f, 1f, 0.9f);

        var button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(ShowModal);

        var label = CreateText("Icon", buttonObject.transform, "⚙", 32);
        var labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        return button;
    }

    private static GameObject CreateImage(string name, Transform parent, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        var image = obj.GetComponent<Image>();
        image.color = color;
        image.sprite = GetBuiltinSprite("UI/Skin/Background.psd");
        image.type = Image.Type.Sliced;
        return obj;
    }

    private static Button CreateButton(string name, Transform parent, string labelText)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        var image = obj.GetComponent<Image>();
        image.sprite = GetBuiltinSprite("UI/Skin/UISprite.psd");
        image.type = Image.Type.Sliced;
        image.color = new Color(1f, 1f, 1f, 0.85f);
        var button = obj.GetComponent<Button>();
        button.targetGraphic = image;

        var label = CreateText("Label", obj.transform, labelText, 20);
        var labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        return button;
    }

    private static Toggle CreateToggle(Transform parent, string labelText)
    {
        var toggleObject = new GameObject($"{labelText}Toggle", typeof(RectTransform), typeof(Toggle));
        toggleObject.transform.SetParent(parent, false);
        var rect = toggleObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(320f, 40f);

        var toggle = toggleObject.GetComponent<Toggle>();

        var background = CreateImage("Background", toggleObject.transform, Color.white);
        var backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.5f);
        backgroundRect.anchorMax = new Vector2(0f, 0.5f);
        backgroundRect.pivot = new Vector2(0f, 0.5f);
        backgroundRect.sizeDelta = new Vector2(28f, 28f);
        backgroundRect.anchoredPosition = new Vector2(0f, 0f);

        var checkmark = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
        checkmark.transform.SetParent(background.transform, false);
        var checkmarkImage = checkmark.GetComponent<Image>();
        checkmarkImage.sprite = GetBuiltinSprite("UI/Skin/Checkmark.psd");
        checkmarkImage.color = Color.black;
        var checkmarkRect = checkmark.GetComponent<RectTransform>();
        checkmarkRect.anchorMin = Vector2.zero;
        checkmarkRect.anchorMax = Vector2.one;
        checkmarkRect.offsetMin = Vector2.zero;
        checkmarkRect.offsetMax = Vector2.zero;

        toggle.graphic = checkmarkImage;
        toggle.targetGraphic = background.GetComponent<Image>();

        var label = CreateText("Label", toggleObject.transform, labelText, 22);
        var labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0.5f);
        labelRect.anchorMax = new Vector2(1f, 0.5f);
        labelRect.pivot = new Vector2(0f, 0.5f);
        labelRect.anchoredPosition = new Vector2(40f, 0f);
        labelRect.sizeDelta = new Vector2(-40f, 30f);

        return toggle;
    }

    private static GameObject CreateText(string name, Transform parent, string textValue, int fontSize)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(parent, false);
        var text = obj.GetComponent<Text>();
        text.text = textValue;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return obj;
    }

    private static Sprite GetBuiltinSprite(string path)
    {
        return Resources.GetBuiltinResource<Sprite>(path);
    }

    private static void StretchRect(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
