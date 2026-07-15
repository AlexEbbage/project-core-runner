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
                _iap.SetStoreAdapterAvailability(false);
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
            _iap.SetStoreAdapterAvailability(false);
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
            if (productId != IapProductIds.PremiumUser)
            {
                _iap?.CompletePurchase(productId, IapPurchaseResult.UnknownProduct);
                return;
            }

            BuyPremium();
        }

        public void BuyPremium()
        {
            _analytics?.Track(AnalyticsEventNames.PurchaseStarted);
            if (_controller == null)
            {
                LogWarning("Unity IAP controller is not initialized.");
                _iap?.CompletePurchase(IapProductIds.PremiumUser, IapPurchaseResult.NotInitialized);
                return;
            }

            _controller.InitiatePurchase(IapProductIds.PremiumUser);
        }

        public void RestorePurchases()
        {
            if (_controller == null || _extensions == null)
            {
                _iap?.CompleteRestore(IapPurchaseResult.NotInitialized);
                return;
            }

#if UNITY_IOS || UNITY_STANDALONE_OSX
            var apple = _extensions.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions((success, message) =>
            {
                if (success)
                    RestoreKnownReceipts();
                else
                    _iap?.CompleteRestore(IapPurchaseResult.Failed);
                LogInfo($"Restore purchases completed. success={success}, message={message}");
            });
#else
            // Google Play automatically restores non-consumables into the product receipt set.
            RestoreKnownReceipts();
#endif
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;
            _iap?.SetStoreAdapterAvailability(true);
            LogInfo("Unity IAP initialized.");

            var premium = controller.products.WithID(IapProductIds.PremiumUser);
            if (premium != null && premium.hasReceipt)
                _iap?.RestoreOwnedProduct(IapProductIds.PremiumUser);
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            HandleInitializationFailure(error + string.Empty);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            HandleInitializationFailure(error + " " + message);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var product = args != null ? args.purchasedProduct : null;
            var productId = product?.definition?.id;
            if (string.IsNullOrEmpty(productId) || string.IsNullOrEmpty(product.receipt))
            {
                _iap?.CompletePurchase(productId ?? IapProductIds.PremiumUser, IapPurchaseResult.Failed);
                return PurchaseProcessingResult.Complete;
            }

            _iap?.CompletePurchase(productId, IapPurchaseResult.Success);
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            var productId = product?.definition?.id ?? IapProductIds.PremiumUser;
            _iap?.CompletePurchase(productId, failureReason == PurchaseFailureReason.UserCancelled ? IapPurchaseResult.Cancelled : IapPurchaseResult.Failed);
            LogWarning($"Purchase failed: {productId}, reason={failureReason}");
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            var productId = product?.definition?.id ?? IapProductIds.PremiumUser;
            if (failureDescription == null)
            {
                _iap?.CompletePurchase(productId, IapPurchaseResult.Failed);
                LogWarning($"Purchase failed: {productId}, no failure description supplied.");
                return;
            }

            var reason = failureDescription.reason;
            _iap?.CompletePurchase(productId, reason == PurchaseFailureReason.UserCancelled ? IapPurchaseResult.Cancelled : IapPurchaseResult.Failed);
            LogWarning($"Purchase failed: {productId}, reason={reason}, message={failureDescription.message}");
        }

        private void RestoreKnownReceipts()
        {
            var premium = _controller?.products?.WithID(IapProductIds.PremiumUser);
            if (premium != null && premium.hasReceipt)
                _iap?.RestoreOwnedProduct(IapProductIds.PremiumUser);
            else
                _iap?.CompleteRestore(IapPurchaseResult.Success);
        }

        private void HandleInitializationFailure(string message)
        {
            LogWarning("Unity IAP initialization failed: " + message);
            _iap?.SetStoreAdapterAvailability(false);
            if (_iap != null)
            {
                if (_iap.IsPurchasePending)
                    _iap.CompletePurchase(IapProductIds.PremiumUser, IapPurchaseResult.NotInitialized);
                if (_iap.IsRestorePending)
                    _iap.CompleteRestore(IapPurchaseResult.NotInitialized);
            }
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
        private IapPurchaseService _iap;

        private void Start()
        {
            GameServices.TryGet(out _iap);
            if (_iap == null)
                return;

            _iap.SetStoreAdapterAvailability(false);
            _iap.PurchaseRequested += OnPurchaseRequested;
            _iap.RestoreRequested += OnRestoreRequested;
        }

        private void OnDestroy()
        {
            if (_iap == null)
                return;

            _iap.PurchaseRequested -= OnPurchaseRequested;
            _iap.RestoreRequested -= OnRestoreRequested;
            _iap.SetStoreAdapterAvailability(false);
        }

        public void BuyPremium()
        {
            Debug.LogWarning("[CoreRacer:IAP] CORE_RACER_UNITY_IAP is not enabled. Install/configure Unity IAP before release.", this);
            _iap?.CompletePurchase(IapProductIds.PremiumUser, IapPurchaseResult.NotInitialized);
        }

        public void RestorePurchases()
        {
            Debug.LogWarning("[CoreRacer:IAP] CORE_RACER_UNITY_IAP is not enabled. Install/configure Unity IAP before release.", this);
            _iap?.CompleteRestore(IapPurchaseResult.NotInitialized);
        }

        private void OnPurchaseRequested(string productId)
        {
            _iap?.CompletePurchase(productId, IapPurchaseResult.NotInitialized);
        }

        private void OnRestoreRequested()
        {
            _iap?.CompleteRestore(IapPurchaseResult.NotInitialized);
        }
    }
#endif
}
