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
    [SerializeField] private RemoveAdsIAPManager removeAdsIAPManager;

    [SerializeField] private GameManager gameManager;

    private readonly List<GameObject> _spawnedItems = new();
    public ShopTab CurrentTab => selectedTab;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (profile == null)
        {
            PlayerProfile[] profiles = Resources.FindObjectsOfTypeAll<PlayerProfile>();
            if (profiles != null && profiles.Length > 0)
                profile = profiles[0];
        }

        if (shopDatabase == null)
        {
            shopDatabase = Resources.Load<ShopDatabase>("ShopDatabase");

            if (shopDatabase == null)
            {
                ShopDatabase[] databases = Resources.FindObjectsOfTypeAll<ShopDatabase>();
                if (databases != null && databases.Length > 0)
                    shopDatabase = databases[0];
            }
        }

        if (hangarPageController == null)
            hangarPageController = FindFirstObjectByType<HangarPageController>();

        if (removeAdsIAPManager == null)
            removeAdsIAPManager = FindFirstObjectByType<RemoveAdsIAPManager>();
    }

    private void OnEnable()
    {
        BuildContent();
    }

    public void Initialize(
        PlayerProfile runtimeProfile,
        ShopDatabase runtimeShopDatabase,
        Transform runtimeContentRoot,
        ShopItemDetailsModal runtimeDetailsModal,
        RemoveAdsIAPManager runtimeRemoveAdsIapManager,
        HangarPageController runtimeHangarPageController,
        GameManager runtimeGameManager)
    {
        profile = runtimeProfile;
        shopDatabase = runtimeShopDatabase;
        contentRoot = runtimeContentRoot;
        detailsModal = runtimeDetailsModal;
        removeAdsIAPManager = runtimeRemoveAdsIapManager;
        hangarPageController = runtimeHangarPageController;
        gameManager = runtimeGameManager;
        BuildContent();
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

        ShopItemState state = GetState(item);
        detailsModal.Show(
            item,
            BuildDescription(item, state),
            BuildPriceLabel(item, state),
            BuildActionLabel(item, state),
            state.CanPurchase,
            state.CanPurchase ? () => OnBuyConfirmed(item) : null);
    }

    public void OnBuyConfirmed(ShopItemDefinition item)
    {
        if (profile == null || item == null)
            return;

        ShopItemState state = GetState(item);
        if (!state.CanPurchase)
        {
            OnShopItemClicked(item);
            return;
        }

        switch (item.action)
        {
            case ShopItemAction.OpenRemoveAdsPurchase:
                removeAdsIAPManager?.BuyRemoveAds();
                BuildContent();
                FindFirstObjectByType<MainMenuUI>()?.RefreshShopView();
                return;
            case ShopItemAction.RestorePurchases:
                removeAdsIAPManager?.RestorePurchases();
                BuildContent();
                FindFirstObjectByType<MainMenuUI>()?.RefreshShopView();
                return;
        }

        if (!profile.TrySpend(item.currencyType, item.price))
        {
            OnShopItemClicked(item);
            return;
        }

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
        FindFirstObjectByType<MainMenuUI>()?.RefreshShopView();
    }

    private void BuildContent()
    {
        ClearContent();

        if (shopDatabase == null || contentRoot == null)
            return;

        foreach (ShopItemDefinition item in shopDatabase.GetItemsForTab(selectedTab))
        {
            if (item == null)
                continue;

            ShopItemState state = GetState(item);
            ShopItemCardView instance = itemCardPrefab != null
                ? Instantiate(itemCardPrefab, contentRoot)
                : CreateRuntimeItemCard(contentRoot);
            instance.Initialize(item, BuildPriceLabel(item, state), BuildActionLabel(item, state), true, () => OnShopItemClicked(item));
            _spawnedItems.Add(instance.gameObject);
        }
    }

    private void ClearContent()
    {
        foreach (GameObject item in _spawnedItems)
        {
            if (item != null)
                Destroy(item);
        }

        _spawnedItems.Clear();
    }

    private ShopItemState GetState(ShopItemDefinition item)
    {
        if (item == null)
            return default;

        bool isOwned = item.action switch
        {
            ShopItemAction.OpenRemoveAdsPurchase => AdsConfig.RemoveAds,
            ShopItemAction.RestorePurchases => AdsConfig.RemoveAds,
            _ => profile != null && !string.IsNullOrEmpty(item.id) && profile.HasUnlocked(item.id)
        };

        bool canAfford = item.action != ShopItemAction.UnlockItem || CanAfford(item);
        bool canPurchase = item.action switch
        {
            ShopItemAction.OpenRemoveAdsPurchase => removeAdsIAPManager != null && !AdsConfig.RemoveAds,
            ShopItemAction.RestorePurchases => removeAdsIAPManager != null && !AdsConfig.RemoveAds,
            _ => !isOwned && canAfford
        };

        return new ShopItemState
        {
            IsOwned = isOwned,
            CanAfford = canAfford,
            CanPurchase = canPurchase
        };
    }

    private bool CanAfford(ShopItemDefinition item)
    {
        if (profile == null || item == null)
            return false;

        return item.currencyType == ShopCurrencyType.Soft
            ? profile.softCurrency >= item.price
            : profile.premiumCurrency >= item.price;
    }

    private static string BuildPriceLabel(ShopItemDefinition item, ShopItemState state)
    {
        if (item == null)
            return " ";

        if (state.IsOwned)
            return LocalizationService.Get("ui.shop_owned", "Owned");

        return item.action switch
        {
            ShopItemAction.OpenRemoveAdsPurchase => LocalizationService.Get("ui.shop_price_premium", "Premium"),
            ShopItemAction.RestorePurchases => LocalizationService.Get("ui.shop_price_restore", "Account"),
            _ => item.currencyType == ShopCurrencyType.Soft
                ? LocalizationService.Format("ui.shop_price_soft", item.price)
                : LocalizationService.Format("ui.shop_price_premium_currency", item.price)
        };
    }

    private static string BuildActionLabel(ShopItemDefinition item, ShopItemState state)
    {
        if (item == null)
            return LocalizationService.Get("ui.shop_action_default", "View");

        if (state.IsOwned)
            return LocalizationService.Get("ui.shop_action_owned", "Owned");

        return item.action switch
        {
            ShopItemAction.OpenRemoveAdsPurchase => LocalizationService.Get("ui.shop_action_remove_ads", "Go Premium"),
            ShopItemAction.RestorePurchases => LocalizationService.Get("ui.shop_action_restore", "Restore"),
            _ => state.CanPurchase
                ? LocalizationService.Get("ui.shop_action_buy", "Buy")
                : LocalizationService.Get("ui.shop_action_need_more", "Need More")
        };
    }

    private static string BuildDescription(ShopItemDefinition item, ShopItemState state)
    {
        if (item == null)
            return string.Empty;

        if (state.IsOwned)
            return AppendStatus(item.description, LocalizationService.Get("ui.shop_status_owned", "Already owned on this profile."));

        if (item.action == ShopItemAction.OpenRemoveAdsPurchase)
            return AppendStatus(item.description, LocalizationService.Get("ui.shop_status_remove_ads", "Unlock ad-free runs and retire interrupting ads."));

        if (item.action == ShopItemAction.RestorePurchases)
            return AppendStatus(item.description, LocalizationService.Get("ui.shop_status_restore", "Restore prior premium purchases on this account."));

        if (state.CanPurchase)
            return AppendStatus(item.description, LocalizationService.Get("ui.shop_status_ready", "Ready to purchase."));

        return item.currencyType == ShopCurrencyType.Soft
            ? AppendStatus(item.description, LocalizationService.Get("ui.shop_status_need_soft", "You need more coins for this item."))
            : AppendStatus(item.description, LocalizationService.Get("ui.shop_status_need_premium", "You need more gems for this item."));
    }

    private static string AppendStatus(string baseDescription, string status)
    {
        if (string.IsNullOrWhiteSpace(baseDescription))
            return status ?? string.Empty;

        if (string.IsNullOrWhiteSpace(status))
            return baseDescription;

        return $"{baseDescription}\n\n{status}";
    }

    private static ShopItemCardView CreateRuntimeItemCard(Transform parent)
    {
        GameObject cardObject = new GameObject("ShopItemCard", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.LayoutElement), typeof(UnityEngine.UI.HorizontalLayoutGroup), typeof(ShopItemCardView));
        cardObject.transform.SetParent(parent, false);
        return cardObject.GetComponent<ShopItemCardView>();
    }

    private struct ShopItemState
    {
        public bool IsOwned;
        public bool CanAfford;
        public bool CanPurchase;
    }
}
