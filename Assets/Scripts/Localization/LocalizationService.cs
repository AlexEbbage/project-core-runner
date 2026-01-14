using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LocalizationEntry
{
    public string key;
    public string value;
}

[Serializable]
public class LocalizationFile
{
    public LocalizationEntry[] entries;
}

public static class LocalizationService
{
    public const string DefaultLanguage = "en";
    private const string PlayerPrefsKey = "Localization.Language";

    private static Dictionary<string, string> _entries;
    private static Dictionary<string, string> _fallbackEntries;
    private static string _currentLanguage;
    private static bool _initialized;

    public static event Action LanguageChanged;

    public static string CurrentLanguage
    {
        get
        {
            Initialize();
            return _currentLanguage ?? DefaultLanguage;
        }
    }

    public static void SetLanguage(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            languageCode = DefaultLanguage;

        Initialize();

        if (string.Equals(_currentLanguage, languageCode, StringComparison.OrdinalIgnoreCase))
            return;

        _currentLanguage = languageCode;
        _entries = LoadEntries(languageCode);
        PlayerPrefs.SetString(PlayerPrefsKey, _currentLanguage);
        PlayerPrefs.Save();
        LanguageChanged?.Invoke();
    }

    public static string Get(string key, string fallback = null)
    {
        Initialize();

        if (string.IsNullOrEmpty(key))
            return fallback ?? string.Empty;

        if (_entries != null && _entries.TryGetValue(key, out var value))
            return value;

        if (_fallbackEntries != null && _fallbackEntries.TryGetValue(key, out var fallbackValue))
            return fallbackValue;

        return fallback ?? key;
    }

    public static string Format(string key, params object[] args)
    {
        var value = Get(key, null);
        if (string.IsNullOrEmpty(value))
            return key;

        try
        {
            return string.Format(value, args);
        }
        catch (FormatException)
        {
            return value;
        }
    }

    private static void Initialize()
    {
        if (_initialized)
            return;

        _fallbackEntries = LoadEntries(DefaultLanguage);
        if (PlayerPrefs.HasKey(PlayerPrefsKey))
            _currentLanguage = PlayerPrefs.GetString(PlayerPrefsKey, DefaultLanguage);
        else
            _currentLanguage = GetSystemLanguageCode();
        _entries = LoadEntries(_currentLanguage);
        _initialized = true;
    }

    private static Dictionary<string, string> LoadEntries(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            return new Dictionary<string, string>();

        var asset = Resources.Load<TextAsset>($"Localization/{languageCode}");
        if (asset == null || string.IsNullOrWhiteSpace(asset.text))
            return new Dictionary<string, string>();

        var file = JsonUtility.FromJson<LocalizationFile>(asset.text);
        var entries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (file?.entries == null)
            return entries;

        foreach (var entry in file.entries)
        {
            if (entry == null || string.IsNullOrEmpty(entry.key))
                continue;

            entries[entry.key] = entry.value ?? string.Empty;
        }

        return entries;
    }

    private static string GetSystemLanguageCode()
    {
        switch (Application.systemLanguage)
        {
            case SystemLanguage.Spanish:
                return "es";
            default:
                return DefaultLanguage;
        }
    }
}
