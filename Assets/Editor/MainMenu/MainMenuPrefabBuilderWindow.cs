using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

public sealed class MainMenuPrefabBuilderWindow : EditorWindow
{
    private const string PanelSpriteKey = "MainMenuPrefabBuilder.PanelSprite";
    private const string ButtonSpriteKey = "MainMenuPrefabBuilder.ButtonSprite";
    private const string ProgressFillSpriteKey = "MainMenuPrefabBuilder.ProgressFillSprite";
    private const string FontKey = "MainMenuPrefabBuilder.Font";
    private const string TextColorKey = "MainMenuPrefabBuilder.TextColor";
    private const string PaddingKey = "MainMenuPrefabBuilder.Padding";
    private const string SpacingKey = "MainMenuPrefabBuilder.Spacing";
    private const string ClickSfxKey = "MainMenuPrefabBuilder.ClickSfx";
    private const string TopBarHeightKey = "MainMenuPrefabBuilder.TopBarHeight";
    private const string BottomBarHeightKey = "MainMenuPrefabBuilder.BottomBarHeight";

    private Sprite panelSprite;
    private Sprite buttonSprite;
    private Sprite progressFillSprite;
    private TMP_FontAsset font;
    private Color textColor = Color.white;
    private int padding = 8;
    private int spacing = 12;
    private AudioClip clickSfx;
    private float topBarHeight = 160f;
    private float bottomBarHeight = 150f;

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
        buttonSprite = LoadAsset<Sprite>(ButtonSpriteKey);
        progressFillSprite = LoadAsset<Sprite>(ProgressFillSpriteKey);
        font = LoadAsset<TMP_FontAsset>(FontKey);
        clickSfx = LoadAsset<AudioClip>(ClickSfxKey);

        textColor = LoadColor(TextColorKey, Color.white);
        padding = EditorPrefs.GetInt(PaddingKey, 8);
        spacing = EditorPrefs.GetInt(SpacingKey, 12);
        topBarHeight = EditorPrefs.GetFloat(TopBarHeightKey, 160f);
        bottomBarHeight = EditorPrefs.GetFloat(BottomBarHeightKey, 150f);
    }

    private void OnDisable()
    {
        SaveAsset(PanelSpriteKey, panelSprite);
        SaveAsset(ButtonSpriteKey, buttonSprite);
        SaveAsset(ProgressFillSpriteKey, progressFillSprite);
        SaveAsset(FontKey, font);
        SaveAsset(ClickSfxKey, clickSfx);

        SaveColor(TextColorKey, textColor);
        EditorPrefs.SetInt(PaddingKey, padding);
        EditorPrefs.SetInt(SpacingKey, spacing);
        EditorPrefs.SetFloat(TopBarHeightKey, topBarHeight);
        EditorPrefs.SetFloat(BottomBarHeightKey, bottomBarHeight);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Style Settings", EditorStyles.boldLabel);
        panelSprite = (Sprite)EditorGUILayout.ObjectField("Panel Sprite", panelSprite, typeof(Sprite), false);
        buttonSprite = (Sprite)EditorGUILayout.ObjectField("Button Sprite", buttonSprite, typeof(Sprite), false);
        progressFillSprite = (Sprite)EditorGUILayout.ObjectField("Progress Fill Sprite", progressFillSprite, typeof(Sprite), false);
        font = (TMP_FontAsset)EditorGUILayout.ObjectField("Font", font, typeof(TMP_FontAsset), false);
        textColor = EditorGUILayout.ColorField("Text Color", textColor);
        padding = EditorGUILayout.IntField("Padding", padding);
        spacing = EditorGUILayout.IntField("Spacing", spacing);
        clickSfx = (AudioClip)EditorGUILayout.ObjectField("Click SFX", clickSfx, typeof(AudioClip), false);
        topBarHeight = EditorGUILayout.FloatField("Top Bar Height", topBarHeight);
        bottomBarHeight = EditorGUILayout.FloatField("Bottom Bar Height", bottomBarHeight);

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
            buttonSprite,
            progressFillSprite,
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
        buttonSprite = style.ButtonSprite;
        progressFillSprite = style.ProgressFillSprite;
        font = style.Font;
        textColor = style.TextColor;
        padding = style.Padding;
        spacing = style.Spacing;
        clickSfx = style.ClickSfx;
        topBarHeight = style.TopBarHeight;
        bottomBarHeight = style.BottomBarHeight;
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
