using System.Collections.Generic;
using CoreRacer.Meta.Shop;
using CoreRacer.Monetisation.Iap;
using UnityEditor;
using UnityEngine;

namespace CoreRacer.Editor.Validation
{
    public static class ProductCatalogValidator
    {
        [MenuItem("Tools/Core Racer/Validate Product Catalogues")]
        public static void ValidateProductCatalogues()
        {
            var issues = 0;
            var guids = AssetDatabase.FindAssets("t:ShopCatalog");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var catalog = AssetDatabase.LoadAssetAtPath<ShopCatalog>(path);
                issues += Validate(catalog, path);
            }
            Debug.Log($"Product catalogue validation complete. Issues: {issues}");
        }

        private static int Validate(ShopCatalog catalog, string path)
        {
            var issues = 0;
            if (catalog == null) return 0;
            var ids = new HashSet<string>();
            var hasPremium = false;
            var hasRestore = false;
            foreach (var item in catalog.Items)
            {
                if (item == null) { Debug.LogWarning($"Null shop item in {path}"); issues++; continue; }
                if (string.IsNullOrWhiteSpace(item.Id)) { Debug.LogWarning($"Shop item missing Id in {path}"); issues++; }
                else if (!ids.Add(item.Id)) { Debug.LogWarning($"Duplicate shop item Id '{item.Id}' in {path}"); issues++; }
                if (item.Kind == ShopItemKind.PremiumUser) hasPremium = true;
                if (item.Kind == ShopItemKind.RestorePurchases) hasRestore = true;
                if (item.Kind == ShopItemKind.Unlock && string.IsNullOrWhiteSpace(item.GrantItemId)) { Debug.LogWarning($"Unlock item '{item.Id}' has no GrantItemId."); issues++; }
                if (item.Price.Amount < 0 || item.CurrencyGrant.Amount < 0) { Debug.LogWarning($"Negative amount in shop item '{item.Id}'."); issues++; }
            }
            if (!hasPremium) { Debug.LogWarning($"{path} has no premium product entry. Expected product id '{IapProductIds.PremiumUser}' in platform catalogue."); issues++; }
            if (!hasRestore) { Debug.LogWarning($"{path} has no restore purchases item."); issues++; }
            return issues;
        }
    }
}
