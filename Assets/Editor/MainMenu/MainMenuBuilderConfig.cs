using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class MainMenuBuilderConfig
{
    public TopBarConfig TopBar { get; }
    public IReadOnlyList<BottomNavButtonConfig> BottomNavButtons { get; }
    public HangarPageConfig HangarPage { get; }
    public ShopPageConfig ShopPage { get; }
    public IReadOnlyList<PlaceholderPageConfig> PlaceholderPages { get; }
    public ProgressionPageConfig ProgressionPage { get; }
    public IReadOnlyList<PageKind> PageOrder { get; }
    public MainMenuStyleConfig Style { get; }

    private MainMenuBuilderConfig(
        TopBarConfig topBar,
        IReadOnlyList<BottomNavButtonConfig> bottomNavButtons,
        HangarPageConfig hangarPage,
        ShopPageConfig shopPage,
        IReadOnlyList<PlaceholderPageConfig> placeholderPages,
        ProgressionPageConfig progressionPage,
        IReadOnlyList<PageKind> pageOrder,
        MainMenuStyleConfig style)
    {
        TopBar = topBar;
        BottomNavButtons = bottomNavButtons;
        HangarPage = hangarPage;
        ShopPage = shopPage;
        PlaceholderPages = placeholderPages;
        ProgressionPage = progressionPage;
        PageOrder = pageOrder;
        Style = style;
    }

    public static MainMenuBuilderConfig CreateDefault()
    {
        var topBar = new TopBarConfig(new List<CurrencyPillConfig>
        {
            new CurrencyPillConfig("SoftCurrencyPill", "Soft", null),
            new CurrencyPillConfig("PremiumCurrencyPill", "Premium", null),
            new CurrencyPillConfig("OtherCurrencyPill", "Other", null)
        });

        var bottomNavButtons = new List<BottomNavButtonConfig>
        {
            new BottomNavButtonConfig(MainPage.Shop, "ShopButton", "Shop", null),
            new BottomNavButtonConfig(MainPage.Hangar, "HangarButton", "Hangar", null),
            new BottomNavButtonConfig(MainPage.Play, "PlayButton", "Play", null),
            new BottomNavButtonConfig(MainPage.Challenges, "ChallengesButton", "Challenges", null),
            new BottomNavButtonConfig(MainPage.Progression, "ProgressionButton", "Progression", null)
        };

        var hangarPage = new HangarPageConfig(
            "HangarPage",
            "ShipDisplayArea",
            "StatsPanel",
            "SubTabBar",
            "ContentScroll",
            new List<ShipStatType>
            {
                ShipStatType.Speed,
                ShipStatType.Handling,
                ShipStatType.Stability,
                ShipStatType.Boost,
                ShipStatType.Energy
            },
            new List<TabButtonConfig>
            {
                new TabButtonConfig("UpgradesTabButton", "Upgrades", null),
                new TabButtonConfig("SkinsTabButton", "Skins", null),
                new TabButtonConfig("TrailsTabButton", "Trails", null),
                new TabButtonConfig("CoreFxTabButton", "Core FX", null)
            });

        var shopPage = new ShopPageConfig(
            "ShopPage",
            "SubTabBar",
            "ContentScroll",
            new List<TabButtonConfig>
            {
                new TabButtonConfig("SkinsTabButton", "Skins", null),
                new TabButtonConfig("ShipsTabButton", "Ships", null),
                new TabButtonConfig("TrailsTabButton", "Trails", null),
                new TabButtonConfig("CurrencyTabButton", "Currency", null)
            });

        var placeholderPages = new List<PlaceholderPageConfig>
        {
            new PlaceholderPageConfig(PageKind.Play, "PlayPage", "Play"),
            new PlaceholderPageConfig(PageKind.Challenges, "ChallengesPage", "Challenges")
        };

        var progressionPage = new ProgressionPageConfig(
            "ProgressionPage",
            "Progression",
            new List<ProgressionTabConfig>
            {
                new ProgressionTabConfig(ProgressionCadence.Daily, "DailyTab", "Daily", null),
                new ProgressionTabConfig(ProgressionCadence.Weekly, "WeeklyTab", "Weekly", null),
                new ProgressionTabConfig(ProgressionCadence.Monthly, "MonthlyTab", "Monthly", null)
            });

        var pageOrder = new List<PageKind>
        {
            PageKind.Hangar,
            PageKind.Play,
            PageKind.Shop,
            PageKind.Challenges,
            PageKind.Progression
        };

        var style = new MainMenuStyleConfig(
            null,
            Color.white,
            null,
            Color.white,
            new Color(1f, 1f, 1f, 0.2f),
            null,
            new Color(0.3f, 0.8f, 0.2f, 1f),
            new Color(1f, 1f, 1f, 0.2f),
            null,
            Color.white,
            8,
            12,
            null,
            160f,
            150f);

        return new MainMenuBuilderConfig(topBar, bottomNavButtons, hangarPage, shopPage, placeholderPages, progressionPage, pageOrder, style);
    }

    public static MainMenuBuilderConfig CreateStyleOverrides(MainMenuStyleConfig style)
    {
        return new MainMenuBuilderConfig(null, null, null, null, null, null, null, style);
    }

    public static MainMenuBuilderConfig Merge(MainMenuBuilderConfig overrides, MainMenuBuilderConfig defaults)
    {
        if (defaults == null)
            defaults = CreateDefault();

        if (overrides == null)
            return defaults;

        return new MainMenuBuilderConfig(
            MergeTopBar(overrides.TopBar, defaults.TopBar),
            overrides.BottomNavButtons ?? defaults.BottomNavButtons,
            MergeHangarPage(overrides.HangarPage, defaults.HangarPage),
            MergeShopPage(overrides.ShopPage, defaults.ShopPage),
            overrides.PlaceholderPages ?? defaults.PlaceholderPages,
            MergeProgressionPage(overrides.ProgressionPage, defaults.ProgressionPage),
            overrides.PageOrder ?? defaults.PageOrder,
            MergeStyle(overrides.Style, defaults.Style));
    }

    private static TopBarConfig MergeTopBar(TopBarConfig overrides, TopBarConfig defaults)
    {
        if (overrides == null)
            return defaults;

        return overrides.CurrencyPills == null
            ? defaults
            : new TopBarConfig(overrides.CurrencyPills);
    }

    private static HangarPageConfig MergeHangarPage(HangarPageConfig overrides, HangarPageConfig defaults)
    {
        if (overrides == null)
            return defaults;

        return new HangarPageConfig(
            GetStringOrDefault(overrides.Name, defaults.Name),
            GetStringOrDefault(overrides.ShipDisplayPanelName, defaults.ShipDisplayPanelName),
            GetStringOrDefault(overrides.StatsPanelName, defaults.StatsPanelName),
            GetStringOrDefault(overrides.SubTabBarName, defaults.SubTabBarName),
            GetStringOrDefault(overrides.ContentScrollName, defaults.ContentScrollName),
            overrides.StatTypes ?? defaults.StatTypes,
            overrides.Tabs ?? defaults.Tabs);
    }

    private static ShopPageConfig MergeShopPage(ShopPageConfig overrides, ShopPageConfig defaults)
    {
        if (overrides == null)
            return defaults;

        return new ShopPageConfig(
            GetStringOrDefault(overrides.Name, defaults.Name),
            GetStringOrDefault(overrides.SubTabBarName, defaults.SubTabBarName),
            GetStringOrDefault(overrides.ContentScrollName, defaults.ContentScrollName),
            overrides.Tabs ?? defaults.Tabs);
    }

    private static ProgressionPageConfig MergeProgressionPage(ProgressionPageConfig overrides, ProgressionPageConfig defaults)
    {
        if (overrides == null)
            return defaults;

        return new ProgressionPageConfig(
            GetStringOrDefault(overrides.Name, defaults.Name),
            GetStringOrDefault(overrides.TitleLabel, defaults.TitleLabel),
            overrides.Tabs ?? defaults.Tabs);
    }

    private static MainMenuStyleConfig MergeStyle(MainMenuStyleConfig overrides, MainMenuStyleConfig defaults)
    {
        if (overrides == null)
            return defaults;

        return new MainMenuStyleConfig(
            overrides.PanelSprite ?? defaults?.PanelSprite,
            overrides.PanelColor == default ? defaults?.PanelColor ?? Color.white : overrides.PanelColor,
            overrides.ButtonSprite ?? defaults?.ButtonSprite,
            overrides.ButtonColor == default ? defaults?.ButtonColor ?? Color.white : overrides.ButtonColor,
            overrides.ButtonFallbackColor == default ? defaults?.ButtonFallbackColor ?? new Color(1f, 1f, 1f, 0.2f) : overrides.ButtonFallbackColor,
            overrides.ProgressFillSprite ?? defaults?.ProgressFillSprite,
            overrides.ProgressFillColor == default ? defaults?.ProgressFillColor ?? new Color(0.3f, 0.8f, 0.2f, 1f) : overrides.ProgressFillColor,
            overrides.ProgressBackgroundColor == default ? defaults?.ProgressBackgroundColor ?? new Color(1f, 1f, 1f, 0.2f) : overrides.ProgressBackgroundColor,
            overrides.Font ?? defaults?.Font,
            overrides.TextColor == default ? defaults?.TextColor ?? Color.white : overrides.TextColor,
            overrides.Padding <= 0 ? defaults?.Padding ?? 8 : overrides.Padding,
            overrides.Spacing <= 0 ? defaults?.Spacing ?? 12 : overrides.Spacing,
            overrides.ClickSfx ?? defaults?.ClickSfx,
            overrides.TopBarHeight <= 0f ? defaults?.TopBarHeight ?? 160f : overrides.TopBarHeight,
            overrides.BottomBarHeight <= 0f ? defaults?.BottomBarHeight ?? 150f : overrides.BottomBarHeight);
    }

    private static string GetStringOrDefault(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }
}

public sealed class MainMenuStyleConfig
{
    public Sprite PanelSprite { get; }
    public Color PanelColor { get; }
    public Sprite ButtonSprite { get; }
    public Color ButtonColor { get; }
    public Color ButtonFallbackColor { get; }
    public Sprite ProgressFillSprite { get; }
    public Color ProgressFillColor { get; }
    public Color ProgressBackgroundColor { get; }
    public TMP_FontAsset Font { get; }
    public Color TextColor { get; }
    public int Padding { get; }
    public int Spacing { get; }
    public AudioClip ClickSfx { get; }
    public float TopBarHeight { get; }
    public float BottomBarHeight { get; }

    public MainMenuStyleConfig(
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
        int padding,
        int spacing,
        AudioClip clickSfx,
        float topBarHeight,
        float bottomBarHeight)
    {
        PanelSprite = panelSprite;
        PanelColor = panelColor;
        ButtonSprite = buttonSprite;
        ButtonColor = buttonColor;
        ButtonFallbackColor = buttonFallbackColor;
        ProgressFillSprite = progressFillSprite;
        ProgressFillColor = progressFillColor;
        ProgressBackgroundColor = progressBackgroundColor;
        Font = font;
        TextColor = textColor;
        Padding = padding;
        Spacing = spacing;
        ClickSfx = clickSfx;
        TopBarHeight = topBarHeight;
        BottomBarHeight = bottomBarHeight;
    }
}

public sealed class TopBarConfig
{
    public IReadOnlyList<CurrencyPillConfig> CurrencyPills { get; }

    public TopBarConfig(IReadOnlyList<CurrencyPillConfig> currencyPills)
    {
        CurrencyPills = currencyPills;
    }
}

public sealed class CurrencyPillConfig
{
    public string Name { get; }
    public string Label { get; }
    public Sprite Icon { get; }

    public CurrencyPillConfig(string name, string label, Sprite icon)
    {
        Name = name;
        Label = label;
        Icon = icon;
    }
}

public sealed class BottomNavButtonConfig
{
    public MainPage Page { get; }
    public string Name { get; }
    public string Label { get; }
    public Sprite Icon { get; }

    public BottomNavButtonConfig(MainPage page, string name, string label, Sprite icon)
    {
        Page = page;
        Name = name;
        Label = label;
        Icon = icon;
    }
}

public sealed class TabButtonConfig
{
    public string Name { get; }
    public string Label { get; }
    public Sprite Icon { get; }

    public TabButtonConfig(string name, string label, Sprite icon)
    {
        Name = name;
        Label = label;
        Icon = icon;
    }
}

public sealed class ProgressionTabConfig
{
    public ProgressionCadence Cadence { get; }
    public string Name { get; }
    public string Label { get; }
    public Sprite Icon { get; }

    public ProgressionTabConfig(ProgressionCadence cadence, string name, string label, Sprite icon)
    {
        Cadence = cadence;
        Name = name;
        Label = label;
        Icon = icon;
    }
}

public sealed class HangarPageConfig
{
    public string Name { get; }
    public string ShipDisplayPanelName { get; }
    public string StatsPanelName { get; }
    public string SubTabBarName { get; }
    public string ContentScrollName { get; }
    public IReadOnlyList<ShipStatType> StatTypes { get; }
    public IReadOnlyList<TabButtonConfig> Tabs { get; }

    public HangarPageConfig(
        string name,
        string shipDisplayPanelName,
        string statsPanelName,
        string subTabBarName,
        string contentScrollName,
        IReadOnlyList<ShipStatType> statTypes,
        IReadOnlyList<TabButtonConfig> tabs)
    {
        Name = name;
        ShipDisplayPanelName = shipDisplayPanelName;
        StatsPanelName = statsPanelName;
        SubTabBarName = subTabBarName;
        ContentScrollName = contentScrollName;
        StatTypes = statTypes;
        Tabs = tabs;
    }
}

public sealed class ShopPageConfig
{
    public string Name { get; }
    public string SubTabBarName { get; }
    public string ContentScrollName { get; }
    public IReadOnlyList<TabButtonConfig> Tabs { get; }

    public ShopPageConfig(
        string name,
        string subTabBarName,
        string contentScrollName,
        IReadOnlyList<TabButtonConfig> tabs)
    {
        Name = name;
        SubTabBarName = subTabBarName;
        ContentScrollName = contentScrollName;
        Tabs = tabs;
    }
}

public sealed class PlaceholderPageConfig
{
    public PageKind Kind { get; }
    public string Name { get; }
    public string Label { get; }

    public PlaceholderPageConfig(PageKind kind, string name, string label)
    {
        Kind = kind;
        Name = name;
        Label = label;
    }
}

public sealed class ProgressionPageConfig
{
    public string Name { get; }
    public string TitleLabel { get; }
    public IReadOnlyList<ProgressionTabConfig> Tabs { get; }

    public ProgressionPageConfig(string name, string titleLabel, IReadOnlyList<ProgressionTabConfig> tabs)
    {
        Name = name;
        TitleLabel = titleLabel;
        Tabs = tabs;
    }
}

public enum PageKind
{
    Hangar,
    Play,
    Shop,
    Challenges,
    Progression
}
