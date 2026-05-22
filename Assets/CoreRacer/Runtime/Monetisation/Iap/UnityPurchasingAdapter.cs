using CoreRacer.Bootstrap;
using CoreRacer.Services.Analytics;
using CoreRacer.Services.Logging;
using UnityEngine;

#if CORE_RACER_UNITY_IAP
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
#endif

namespace CoreRacer.Monetisation.Iap
{
#if CORE_RACER_UNITY_IAP
    public sealed class UnityPurchasingAdapter : MonoBehaviour, IDetailedStoreListener
    {
        private IStoreController _controller;
        private IExtensionProvider _extensions;
        private IapPurchaseService _iap;
        private IAnalyticsService _analytics;
        private IGameLogger _logger;

        private void Start()
        {
            GameServices.TryGet(out _iap);
            if (_iap != null)
            {
                _iap.PurchaseRequested += OnPurchaseRequested;
                _iap.RestoreRequested += RestorePurchases;
            }
            GameServices.TryGet(out _analytics);
            GameServices.TryGet(out _logger);
            Initialize();
        }

        private void OnDestroy()
        {
            if (_iap == null)
                return;

            _iap.PurchaseRequested -= OnPurchaseRequested;
            _iap.RestoreRequested -= RestorePurchases;
        }

        public void Initialize()
        {
            if (_controller != null)
                return;

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            builder.AddProduct(IapProductIds.PremiumUser, ProductType.NonConsumable);
            UnityPurchasing.Initialize(this, builder);
        }

        private void OnPurchaseRequested(string productId)
        {
            if (productId == IapProductIds.PremiumUser)
                BuyPremium();
        }

        public void BuyPremium()
        {
            _analytics?.Track(AnalyticsEventNames.PurchaseStarted);
            if (_controller == null)
            {
                LogWarning("Unity IAP controller is not initialized.");
                return;
            }
            _controller.InitiatePurchase(IapProductIds.PremiumUser);
        }

        public void RestorePurchases()
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            var apple = _extensions.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions((success, message) =>
            {
                LogInfo($"Restore purchases completed. success={success}, message={message}");
            });
#else
            LogInfo("Restore is handled automatically by Google Play for non-consumables.");
#endif
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;
            LogInfo("Unity IAP initialized.");

            var premium = controller.products.WithID(IapProductIds.PremiumUser);
            if (premium != null && premium.hasReceipt)
                _iap?.RestoreOwnedProduct(IapProductIds.PremiumUser);
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            LogWarning("Unity IAP initialization failed: " + error);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            LogWarning("Unity IAP initialization failed: " + error + " " + message);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            _iap?.CompletePurchase(args.purchasedProduct.definition.id);
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            LogWarning($"Purchase failed: {product?.definition?.id}, reason={failureReason}");
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            LogWarning($"Purchase failed: {product?.definition?.id}, reason={failureDescription.reason}, message={failureDescription.message}");
        }

        private void LogInfo(string message)
        {
            if (_logger != null) _logger.Info(LogCategory.Iap, message, this);
            else Debug.Log("[CoreRacer:IAP] " + message, this);
        }

        private void LogWarning(string message)
        {
            if (_logger != null) _logger.Warn(LogCategory.Iap, message, this);
            else Debug.LogWarning("[CoreRacer:IAP] " + message, this);
        }
    }
#else
    public sealed class UnityPurchasingAdapter : MonoBehaviour
    {
        public void BuyPremium()
        {
            Debug.LogWarning("[CoreRacer:IAP] CORE_RACER_UNITY_IAP is not enabled. Install/configure Unity IAP before release.", this);
        }

        public void RestorePurchases()
        {
            Debug.LogWarning("[CoreRacer:IAP] CORE_RACER_UNITY_IAP is not enabled. Install/configure Unity IAP before release.", this);
        }
    }
#endif
}
