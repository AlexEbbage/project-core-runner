using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

public sealed class MainMenuPrefabBuilderWindow : EditorWindow
{
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
    private const string PaddingKey = "MainMenuPrefabBuilder.Padding";
    private const string SpacingKey = "MainMenuPrefabBuilder.Spacing";
    private const string ClickSfxKey = "MainMenuPrefabBuilder.ClickSfx";
    private const string TopBarHeightKey = "MainMenuPrefabBuilder.TopBarHeight";
    private const string BottomBarHeightKey = "MainMenuPrefabBuilder.BottomBarHeight";
    private const string HudTopBarHeightKey = "MainMenuPrefabBuilder.HudTopBarHeight";
    private const string HudPaddingKey = "MainMenuPrefabBuilder.HudPadding";
    private const string HudSpacingKey = "MainMenuPrefabBuilder.HudSpacing";
    private const string HudProgressWidthKey = "MainMenuPrefabBuilder.HudProgressWidth";
    private const string HudProgressHeightKey = "MainMenuPrefabBuilder.HudProgressHeight";
    private const string HudScoreFontSizeKey = "MainMenuPrefabBuilder.HudScoreFontSize";
    private const string HudBestFontSizeKey = "MainMenuPrefabBuilder.HudBestFontSize";
    private const string HudPauseButtonWidthKey = "MainMenuPrefabBuilder.HudPauseButtonWidth";
    private const string HudPauseButtonHeightKey = "MainMenuPrefabBuilder.HudPauseButtonHeight";

    private Sprite panelSprite;
    private Color panelColor = Color.white;
    private Sprite buttonSprite;
    private Color buttonColor = Color.white;
    private Color buttonFallbackColor = new Color(1f, 1f, 1f, 0.2f);
    private Sprite progressFillSprite;
    private Color progressFillColor = new Color(0.3f, 0.8f, 0.2f, 1f);
    private Color progressBackgroundColor = new Color(1f, 1f, 1f, 0.2f);
    private TMP_FontAsset font;
    private Color textColor = Color.white;
    private int padding = 8;
    private int spacing = 12;
    private AudioClip clickSfx;
    private float topBarHeight = 160f;
    private float bottomBarHeight = 150f;
    private float hudTopBarHeight = 220f;
    private int hudPadding = 24;
    private int hudSpacing = 24;
    private float hudProgressWidth = 520f;
    private float hudProgressHeight = 22f;
    private int hudScoreFontSize = 64;
    private int hudBestFontSize = 28;
    private float hudPauseButtonWidth = 140f;
    private float hudPauseButtonHeight = 80f;

    [MenuItem("Tools/UI/Build Main Menu UI")]
    public static void ShowWindow()
    {
        var window = GetWindow<MainMenuPrefabBuilderWindow>("Main Menu Prefab Builder");
        window.minSize = new Vector2(380, 420);
        window.Show();
    }

    private void OnEnable()
    {
        panelSprite = LoadAsset<Sprite>(PanelSpriteKey);
        panelColor = LoadColor(PanelColorKey, Color.white);
        buttonSprite = LoadAsset<Sprite>(ButtonSpriteKey);
        buttonColor = LoadColor(ButtonColorKey, Color.white);
        buttonFallbackColor = LoadColor(ButtonFallbackColorKey, new Color(1f, 1f, 1f, 0.2f));
        progressFillSprite = LoadAsset<Sprite>(ProgressFillSpriteKey);
        progressFillColor = LoadColor(ProgressFillColorKey, new Color(0.3f, 0.8f, 0.2f, 1f));
        progressBackgroundColor = LoadColor(ProgressBackgroundColorKey, new Color(1f, 1f, 1f, 0.2f));
        font = LoadAsset<TMP_FontAsset>(FontKey);
        clickSfx = LoadAsset<AudioClip>(ClickSfxKey);

        textColor = LoadColor(TextColorKey, Color.white);
        padding = EditorPrefs.GetInt(PaddingKey, 8);
        spacing = EditorPrefs.GetInt(SpacingKey, 12);
        topBarHeight = EditorPrefs.GetFloat(TopBarHeightKey, 160f);
        bottomBarHeight = EditorPrefs.GetFloat(BottomBarHeightKey, 150f);
        hudTopBarHeight = EditorPrefs.GetFloat(HudTopBarHeightKey, 220f);
        hudPadding = EditorPrefs.GetInt(HudPaddingKey, 24);
        hudSpacing = EditorPrefs.GetInt(HudSpacingKey, 24);
        hudProgressWidth = EditorPrefs.GetFloat(HudProgressWidthKey, 520f);
        hudProgressHeight = EditorPrefs.GetFloat(HudProgressHeightKey, 22f);
        hudScoreFontSize = EditorPrefs.GetInt(HudScoreFontSizeKey, 64);
        hudBestFontSize = EditorPrefs.GetInt(HudBestFontSizeKey, 28);
        hudPauseButtonWidth = EditorPrefs.GetFloat(HudPauseButtonWidthKey, 140f);
        hudPauseButtonHeight = EditorPrefs.GetFloat(HudPauseButtonHeightKey, 80f);
    }

    private void OnDisable()
    {
        SaveAsset(PanelSpriteKey, panelSprite);
        SaveColor(PanelColorKey, panelColor);
        SaveAsset(ButtonSpriteKey, buttonSprite);
        SaveColor(ButtonColorKey, buttonColor);
        SaveColor(ButtonFallbackColorKey, buttonFallbackColor);
        SaveAsset(ProgressFillSpriteKey, progressFillSprite);
        SaveColor(ProgressFillColorKey, progressFillColor);
        SaveColor(ProgressBackgroundColorKey, progressBackgroundColor);
        SaveAsset(FontKey, font);
        SaveAsset(ClickSfxKey, clickSfx);

        SaveColor(TextColorKey, textColor);
        EditorPrefs.SetInt(PaddingKey, padding);
        EditorPrefs.SetInt(SpacingKey, spacing);
        EditorPrefs.SetFloat(TopBarHeightKey, topBarHeight);
        EditorPrefs.SetFloat(BottomBarHeightKey, bottomBarHeight);
        EditorPrefs.SetFloat(HudTopBarHeightKey, hudTopBarHeight);
        EditorPrefs.SetInt(HudPaddingKey, hudPadding);
        EditorPrefs.SetInt(HudSpacingKey, hudSpacing);
        EditorPrefs.SetFloat(HudProgressWidthKey, hudProgressWidth);
        EditorPrefs.SetFloat(HudProgressHeightKey, hudProgressHeight);
        EditorPrefs.SetInt(HudScoreFontSizeKey, hudScoreFontSize);
        EditorPrefs.SetInt(HudBestFontSizeKey, hudBestFontSize);
        EditorPrefs.SetFloat(HudPauseButtonWidthKey, hudPauseButtonWidth);
        EditorPrefs.SetFloat(HudPauseButtonHeightKey, hudPauseButtonHeight);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Style Settings", EditorStyles.boldLabel);
        panelSprite = (Sprite)EditorGUILayout.ObjectField("Panel Sprite", panelSprite, typeof(Sprite), false);
        panelColor = EditorGUILayout.ColorField("Panel Tint", panelColor);
        buttonSprite = (Sprite)EditorGUILayout.ObjectField("Button Sprite", buttonSprite, typeof(Sprite), false);
        buttonColor = EditorGUILayout.ColorField("Button Tint", buttonColor);
        buttonFallbackColor = EditorGUILayout.ColorField("Button Fallback Tint", buttonFallbackColor);
        progressFillSprite = (Sprite)EditorGUILayout.ObjectField("Progress Fill Sprite", progressFillSprite, typeof(Sprite), false);
        progressFillColor = EditorGUILayout.ColorField("Progress Fill Tint", progressFillColor);
        progressBackgroundColor = EditorGUILayout.ColorField("Progress Track Tint", progressBackgroundColor);
        font = (TMP_FontAsset)EditorGUILayout.ObjectField("Font", font, typeof(TMP_FontAsset), false);
        textColor = EditorGUILayout.ColorField("Text Color", textColor);
        padding = EditorGUILayout.IntField("Padding", padding);
        spacing = EditorGUILayout.IntField("Spacing", spacing);
        clickSfx = (AudioClip)EditorGUILayout.ObjectField("Click SFX", clickSfx, typeof(AudioClip), false);
        topBarHeight = EditorGUILayout.FloatField("Top Bar Height", topBarHeight);
        bottomBarHeight = EditorGUILayout.FloatField("Bottom Bar Height", bottomBarHeight);

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("HUD Settings", EditorStyles.boldLabel);
        hudTopBarHeight = EditorGUILayout.FloatField("HUD Top Bar Height", hudTopBarHeight);
        hudPadding = EditorGUILayout.IntField("HUD Padding", hudPadding);
        hudSpacing = EditorGUILayout.IntField("HUD Spacing", hudSpacing);
        hudProgressWidth = EditorGUILayout.FloatField("HUD Progress Width", hudProgressWidth);
        hudProgressHeight = EditorGUILayout.FloatField("HUD Progress Height", hudProgressHeight);
        hudScoreFontSize = EditorGUILayout.IntField("HUD Score Font Size", hudScoreFontSize);
        hudBestFontSize = EditorGUILayout.IntField("HUD Best Font Size", hudBestFontSize);
        hudPauseButtonWidth = EditorGUILayout.FloatField("HUD Pause Button Width", hudPauseButtonWidth);
        hudPauseButtonHeight = EditorGUILayout.FloatField("HUD Pause Button Height", hudPauseButtonHeight);

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Build Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Build Main Menu Prefab"))
        {
            BuildPrefab(false);
        }

        if (GUILayout.Button("Rebuild Main Menu Prefab (Overwrite)"))
        {
            BuildPrefab(true);
        }

        if (GUILayout.Button("Reveal Prefab Folder"))
        {
            RevealPrefabFolder();
        }

        if (GUILayout.Button("Reset To Defaults"))
        {
            ResetToDefaults();
        }
    }

    private void BuildPrefab(bool rebuild)
    {
        var style = new MainMenuStyleConfig(
            panelSprite,
            panelColor,
            buttonSprite,
            buttonColor,
            buttonFallbackColor,
            progressFillSprite,
            progressFillColor,
            progressBackgroundColor,
            font,
            textColor,
            Mathf.Max(0, padding),
            Mathf.Max(0, spacing),
            clickSfx,
            Mathf.Max(0f, topBarHeight),
            Mathf.Max(0f, bottomBarHeight));

        var builderConfig = MainMenuBuilderConfig.CreateStyleOverrides(style);
        MainMenuPrefabBuilder.BuildMainMenuUI(builderConfig);

        if (rebuild)
            Debug.Log("Main menu prefabs rebuilt with updated style settings.");
        else
            Debug.Log("Main menu prefabs built with updated style settings.");
    }

    private void ResetToDefaults()
    {
        var style = MainMenuBuilderConfig.CreateDefault().Style;
        panelSprite = style.PanelSprite;
        panelColor = style.PanelColor;
        buttonSprite = style.ButtonSprite;
        buttonColor = style.ButtonColor;
        buttonFallbackColor = style.ButtonFallbackColor;
        progressFillSprite = style.ProgressFillSprite;
        progressFillColor = style.ProgressFillColor;
        progressBackgroundColor = style.ProgressBackgroundColor;
        font = style.Font;
        textColor = style.TextColor;
        padding = style.Padding;
        spacing = style.Spacing;
        clickSfx = style.ClickSfx;
        topBarHeight = style.TopBarHeight;
        bottomBarHeight = style.BottomBarHeight;
        hudTopBarHeight = 220f;
        hudPadding = 24;
        hudSpacing = 24;
        hudProgressWidth = 520f;
        hudProgressHeight = 22f;
        hudScoreFontSize = 64;
        hudBestFontSize = 28;
        hudPauseButtonWidth = 140f;
        hudPauseButtonHeight = 80f;
    }

    private static T LoadAsset<T>(string key) where T : Object
    {
        var guid = EditorPrefs.GetString(key, string.Empty);
        if (string.IsNullOrWhiteSpace(guid))
            return null;

        var path = AssetDatabase.GUIDToAssetPath(guid);
        return string.IsNullOrWhiteSpace(path) ? null : AssetDatabase.LoadAssetAtPath<T>(path);
    }

    private static void SaveAsset(string key, Object asset)
    {
        if (asset == null)
        {
            EditorPrefs.DeleteKey(key);
            return;
        }

        var path = AssetDatabase.GetAssetPath(asset);
        if (string.IsNullOrWhiteSpace(path))
            return;

        EditorPrefs.SetString(key, AssetDatabase.AssetPathToGUID(path));
    }

    private static Color LoadColor(string key, Color fallback)
    {
        var value = EditorPrefs.GetString(key, string.Empty);
        return ColorUtility.TryParseHtmlString(value, out var color) ? color : fallback;
    }

    private static void SaveColor(string key, Color value)
    {
        EditorPrefs.SetString(key, $"#{ColorUtility.ToHtmlStringRGBA(value)}");
    }

    private static void RevealPrefabFolder()
    {
        var path = MainMenuPrefabBuilder.PrefabRootPath;
        if (!AssetDatabase.IsValidFolder(path))
        {
            EditorUtility.DisplayDialog("Prefab Folder Missing", $"The folder '{path}' does not exist yet. Build the prefabs first.", "OK");
            return;
        }

        var fullPath = Path.GetFullPath(path);
        EditorUtility.RevealInFinder(fullPath);
    }
}
