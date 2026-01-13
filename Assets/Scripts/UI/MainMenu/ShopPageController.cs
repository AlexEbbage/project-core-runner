using System.Collections.Generic;
using UnityEngine;

public class ShopPageController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PlayerProfile profile;
    [SerializeField] private ShopDatabase shopDatabase;

    [Header("Tabs")]
    [SerializeField] private ShopTab selectedTab = ShopTab.Skins;

    [Header("Content")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private ShopItemCardView itemCardPrefab;

    [Header("Modal")]
    [SerializeField] private ShopItemDetailsModal detailsModal;

    private readonly List<GameObject> _spawnedItems = new();

    public void SelectTab(ShopTab tab)
    {
        selectedTab = tab;
        BuildContent();
    }

    public void OnShopItemClicked(ShopItemDefinition item)
    {
        if (detailsModal == null || item == null)
            return;

        detailsModal.Show(item, () => OnBuyConfirmed(item));
    }

    public void OnBuyConfirmed(ShopItemDefinition item)
    {
        if (profile == null || item == null)
            return;

        if (!profile.TrySpend(item.currencyType, item.price))
            return;

        profile.UnlockItem(item.id);
        detailsModal.Hide();
    }

    private void BuildContent()
    {
        ClearContent();

        if (shopDatabase == null || itemCardPrefab == null)
            return;

        foreach (var item in shopDatabase.GetItemsForTab(selectedTab))
        {
            if (item == null)
                continue;

            var instance = Instantiate(itemCardPrefab, contentRoot);
            instance.Initialize(item);
            _spawnedItems.Add(instance.gameObject);
        }
    }

    private void ClearContent()
    {
        foreach (var item in _spawnedItems)
        {
            if (item != null)
                Destroy(item);
        }

        _spawnedItems.Clear();
    }
}
