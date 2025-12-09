using UnityEngine;
//using UnityEngine.Purchasing;

/// <summary>
/// Handles the "Remove Ads" non-consumable IAP.
/// Product ID: "remove_ads" (must match your store config).
/// Notifies listeners via OnRemoveAdsUnlocked when purchased/restored.
/// </summary>
public class RemoveAdsIAPManager : MonoBehaviour
{
    public const string Product_RemoveAds = "remove_ads";

    public static System.Action OnRemoveAdsUnlocked;

    //private static IStoreController storeController;
    //private static IExtensionProvider storeExtensionProvider;

    //[Header("Debug")]
    //[SerializeField] private bool autoInitializeOnStart = true;

    //private void Start()
    //{
    //    if (autoInitializeOnStart && storeController == null)
    //    {
    //        InitializePurchasing();
    //    }

    //    // If cached RemoveAds flag is already true, make sure UI knows
    //    if (AdsConfig.RemoveAds)
    //    {
    //        OnRemoveAdsUnlocked?.Invoke();
    //    }
    //}

    //public void InitializePurchasing()
    //{
    //    if (storeController != null) return;

    //    var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

    //    // Non-consumable product
    //    builder.AddProduct(Product_RemoveAds, ProductType.NonConsumable);

    //    UnityPurchasing.Initialize(this, builder);
    //}

    //// ---- Public API for UI ----

    public void BuyRemoveAds()
    {
        //if (storeController == null)
        //{
        //    Debug.LogWarning("IAP: Not initialized yet.");
        //    return;
        //}

        //storeController.InitiatePurchase(Product_RemoveAds);
    }

    /// <summary>
    /// Restore purchases (more relevant on iOS; safe on Android).
    /// </summary>
    public void RestorePurchases()
    {
//        if (storeExtensionProvider == null)
//        {
//            Debug.LogWarning("IAP: Cannot restore, not initialized.");
//            return;
//        }

//#if UNITY_IOS
//                var apple = storeExtensionProvider.GetExtension<IAppleExtensions>();
//                apple.RestoreTransactions(result => Debug.Log("RestoreTransactions: " + result));
//#else
//        var google = storeExtensionProvider.GetExtension<IGooglePlayStoreExtensions>();
//        google.RestoreTransactions(result => Debug.Log("RestoreTransactions: " + result));
//#endif
    }

    //// ---- IStoreListener ----

    //public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    //{
    //    storeController = controller;
    //    storeExtensionProvider = extensions;

    //    // Check ownership at startup
    //    Product product = storeController.products.WithID(Product_RemoveAds);
    //    if (product != null && product.hasReceipt)
    //    {
    //        UnlockRemoveAds();
    //    }
    //}

    //public void OnInitializeFailed(InitializationFailureReason error)
    //{
    //    Debug.LogError("IAP init failed: " + error);
    //}

    //public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    //{
    //    if (args.purchasedProduct.definition.id == Product_RemoveAds)
    //    {
    //        UnlockRemoveAds();
    //    }

    //    return PurchaseProcessingResult.Complete;
    //}

//#if UNITY_2022_1_OR_NEWER
//    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason, string message)
//    {
//        Debug.LogError($"Purchase failed: {reason}, {message}");
//    }
//#else
//            public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
//            {
//                Debug.LogError($"Purchase failed: {reason}");
//            }
//#endif

    private void UnlockRemoveAds()
    {
        if (AdsConfig.RemoveAds)
        {
            // Already unlocked
            OnRemoveAdsUnlocked?.Invoke();
            return;
        }

        AdsConfig.RemoveAds = true;
        Debug.Log("REMOVE ADS UNLOCKED");
        OnRemoveAdsUnlocked?.Invoke();
    }
}
