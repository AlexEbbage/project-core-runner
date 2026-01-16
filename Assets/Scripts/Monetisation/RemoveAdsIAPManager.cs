using UnityEngine;
using UnityEngine.Purchasing;

/// <summary>
/// Handles the "Remove Ads" non-consumable IAP.
/// Product ID: "remove_ads" (must match your store config).
/// Notifies listeners via OnRemoveAdsUnlocked when purchased/restored.
/// </summary>
public class RemoveAdsIAPManager : MonoBehaviour, IStoreListener
{
    public const string Product_RemoveAds = "premium_user";

    public static System.Action OnRemoveAdsUnlocked;
    public static System.Action<string> OnProductPurchased;

    private static IStoreController storeController;
    private static IExtensionProvider storeExtensionProvider;

    [System.Serializable]
    public class ProductConfig
    {
        public string id;
        public ProductType type = ProductType.NonConsumable;
        public bool grantOnPurchase = true;
    }

    [Header("Config")]
    [SerializeField] private bool autoInitializeOnStart = true;
    [SerializeField] private ProductConfig[] products;

    private void Start()
    {
        if (autoInitializeOnStart && storeController == null)
        {
            InitializePurchasing();
        }

        if (AdsConfig.RemoveAds)
        {
            OnRemoveAdsUnlocked?.Invoke();
        }
    }

    public void InitializePurchasing()
    {
        if (storeController != null) return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        bool hasRemoveAds = false;

        if (products != null)
        {
            foreach (var product in products)
            {
                if (product == null || string.IsNullOrWhiteSpace(product.id))
                {
                    continue;
                }

                if (product.id == Product_RemoveAds)
                {
                    hasRemoveAds = true;
                }

                builder.AddProduct(product.id, product.type);
            }
        }

        if (!hasRemoveAds)
        {
            builder.AddProduct(Product_RemoveAds, ProductType.NonConsumable);
        }

        UnityPurchasing.Initialize(this, builder);
    }

    public void BuyRemoveAds()
    {
        BuyProduct(Product_RemoveAds);
    }

    public void BuyProduct(string productId)
    {
        if (storeController == null)
        {
            Debug.LogWarning("IAP: Not initialized yet.");
            return;
        }

        storeController.InitiatePurchase(productId);
    }

    /// <summary>
    /// Restore purchases (more relevant on iOS; safe on Android).
    /// </summary>
    public void RestorePurchases()
    {
        if (storeExtensionProvider == null)
        {
            Debug.LogWarning("IAP: Cannot restore, not initialized.");
            return;
        }

#if UNITY_IOS
        var apple = storeExtensionProvider.GetExtension<IAppleExtensions>();
        apple.RestoreTransactions(result => Debug.Log("RestoreTransactions: " + result));
#elif UNITY_ANDROID
        if (Application.isEditor)
        {
            Debug.Log("IAP: Restore purchases is not supported in the editor for Google Play.");
            return;
        }

        var google = storeExtensionProvider.GetExtension<IGooglePlayStoreExtensions>();
        google.RestoreTransactions((result, message) =>
            Debug.Log($"RestoreTransactions: {result}, {message}"));
#else
        Debug.Log("IAP: Restore not supported on this platform.");
#endif
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        storeExtensionProvider = extensions;

        foreach (var product in storeController.products.all)
        {
            if (product != null && product.hasReceipt)
            {
                GrantProduct(product.definition.id);
            }
        }
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError("IAP init failed: " + error);
    }

#if UNITY_2022_1_OR_NEWER
    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError("IAP init failed: " + error + ", " + message);
    }
#endif

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        GrantProduct(args.purchasedProduct.definition.id);
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        LogPurchaseFailed(reason.ToString(), null);
    }

#if UNITY_2022_1_OR_NEWER
    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason, string message)
    {
        LogPurchaseFailed(reason.ToString(), message);
    }
#endif

    private void LogPurchaseFailed(string reason, string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            Debug.LogError($"Purchase failed: {reason}");
            return;
        }

        Debug.LogError($"Purchase failed: {reason}, {message}");
    }

    private void GrantProduct(string productId)
    {
        if (!ShouldGrantProduct(productId))
        {
            return;
        }

        if (productId == Product_RemoveAds)
        {
            UnlockRemoveAds();
            return;
        }

        OnProductPurchased?.Invoke(productId);
    }

    private bool ShouldGrantProduct(string productId)
    {
        if (products == null || products.Length == 0)
        {
            return true;
        }

        foreach (var product in products)
        {
            if (product != null && product.id == productId)
            {
                return product.grantOnPurchase;
            }
        }

        return true;
    }

    private void UnlockRemoveAds()
    {
        if (AdsConfig.RemoveAds)
        {
            OnRemoveAdsUnlocked?.Invoke();
            return;
        }

        AdsConfig.RemoveAds = true;
        Debug.Log("REMOVE ADS UNLOCKED");
        OnRemoveAdsUnlocked?.Invoke();
    }
}
