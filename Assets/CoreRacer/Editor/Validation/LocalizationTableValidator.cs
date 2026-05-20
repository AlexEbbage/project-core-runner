using System.Collections.Generic;
using CoreRacer.Localization;
using UnityEditor;
using UnityEngine;

namespace CoreRacer.Editor.Validation
{
    public static class LocalizationTableValidator
    {
        [MenuItem("Tools/Core Racer/Validate Localization Tables")]
        public static void ValidateLocalizationTables()
        {
            var guids = AssetDatabase.FindAssets("t:StringTable");
            var seen = new HashSet<string>();
            var issues = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var table = AssetDatabase.LoadAssetAtPath<StringTable>(path);
                seen.Clear();
                foreach (var entry in table.Entries)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.Key))
                    {
                        Debug.LogWarning($"Localization empty key in {path}"); issues++; continue;
                    }
                    if (!seen.Add(entry.Key))
                    {
                        Debug.LogWarning($"Localization duplicate key '{entry.Key}' in {path}"); issues++;
                    }
                    if (string.IsNullOrWhiteSpace(entry.Value))
                    {
                        Debug.LogWarning($"Localization missing value for '{entry.Key}' in {path}"); issues++;
                    }
                }
            }
            Debug.Log($"Localization validation complete. Issues: {issues}");
        }
    }
}
