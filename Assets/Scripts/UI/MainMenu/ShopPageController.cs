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
    [SerializeField] private HangarPageController hangarPageController;

    private readonly List<GameObject> _spawnedItems = new();
    [SerializeField] private GameManager gameManager;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
    }

    public void SelectTab(ShopTab tab)
    {
        selectedTab = tab;
        BuildContent();
    }

    public void OnShopItemClicked(ShopItemDefinition item)
    {
        if (detailsModal == null || item == null)
            return;

        bool isUnlocked = IsItemUnlocked(item);
        detailsModal.Show(item, isUnlocked, isUnlocked ? null : () => OnBuyConfirmed(item));
    }

    public void OnBuyConfirmed(ShopItemDefinition item)
    {
        if (profile == null || item == null)
            return;

        if (IsItemUnlocked(item))
            return;

        if (!profile.TrySpend(item.currencyType, item.price))
            return;

        profile.UnlockItem(item.id);
        gameManager?.LogAnalyticsEvent(AnalyticsEventNames.ShopPurchase, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Type, item.tab.ToString() },
            { AnalyticsEventNames.Params.Id, item.id },
            { AnalyticsEventNames.Params.Price, item.price }
        });
        detailsModal.Hide();
        BuildContent();
        hangarPageController?.RefreshContent();
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
            instance.Initialize(item, IsItemUnlocked(item));
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

    private bool IsItemUnlocked(ShopItemDefinition item)
    {
        if (profile == null || item == null || string.IsNullOrEmpty(item.id))
            return false;

        return profile.HasUnlocked(item.id);
    }
}
