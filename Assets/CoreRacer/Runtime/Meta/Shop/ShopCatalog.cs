using System.Collections.Generic;
using CoreRacer.Meta.Economy;
using UnityEngine;

namespace CoreRacer.Meta.Shop
{
    public enum ShopItemKind
    {
        Unlock,
        CurrencyPack,
        PremiumUser,
        RestorePurchases
    }

    [System.Serializable]
    public sealed class ShopItemDefinition
    {
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;
        public ShopItemKind Kind = ShopItemKind.Unlock;
        public string GrantItemId;
        public CurrencyAmount Price;
        public CurrencyAmount CurrencyGrant;
        public bool IsFeatured;
        public bool IsConsumable;
    }

    [CreateAssetMenu(menuName = "Core Racer/Shop/Shop Catalog")]
    public sealed class ShopCatalog : ScriptableObject
    {
        public List<ShopItemDefinition> Items = new List<ShopItemDefinition>();

        public ShopItemDefinition Get(string id)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i] != null && Items[i].Id == id)
                    return Items[i];
            }
            return null;
        }
    }
}
