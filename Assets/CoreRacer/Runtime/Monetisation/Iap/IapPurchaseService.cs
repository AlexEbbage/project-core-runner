using System;
using CoreRacer.Monetisation.Premium;
using CoreRacer.Services.Metrics;

namespace CoreRacer.Monetisation.Iap
{
    public enum IapPurchaseResult
    {
        Success,
        Failed,
        Cancelled,
        NotInitialized,
        UnknownProduct,
        AlreadyPending
    }

    /// <summary>
    /// SDK-agnostic purchase facade. Store adapters subscribe to requests and must complete every request.
    /// Entitlements are granted only from a successful store callback/positive restore result.
    /// </summary>
    public sealed class IapPurchaseService
    {
        private readonly PremiumEntitlementService _premium;
        private readonly AdIapAnalytics _analytics;
        private string _pendingProductId;
        private bool _restorePending;
        private bool _storeAdapterAvailable;

        public event Action<string> PurchaseRequested;
        public event Action RestoreRequested;
        public event Action<string, IapPurchaseResult> PurchaseCompleted;
        public event Action<IapPurchaseResult> RestoreCompleted;

        public IapPurchaseService(PremiumEntitlementService premium, AdIapAnalytics analytics = null)
        {
            _premium = premium;
            _analytics = analytics;
        }

        public bool HasPremium => _premium != null && _premium.HasPremium;
        public bool HasStoreAdapter => _storeAdapterAvailable;
        public bool IsPurchasePending => !string.IsNullOrEmpty(_pendingProductId);
        public bool IsRestorePending => _restorePending;

        public void SetStoreAdapterAvailability(bool available)
        {
            _storeAdapterAvailable = available;
            if (available)
                return;

            if (IsPurchasePending)
                CompletePurchase(_pendingProductId, IapPurchaseResult.NotInitialized);
            if (IsRestorePending)
                CompleteRestore(IapPurchaseResult.NotInitialized);
        }

        public void BuyPremium()
        {
            TryBuyPremium();
        }

        public bool TryBuyPremium()
        {
            if (HasPremium)
            {
                PurchaseCompleted?.Invoke(IapProductIds.PremiumUser, IapPurchaseResult.Success);
                return false;
            }

            if (IsPurchasePending)
            {
                PurchaseCompleted?.Invoke(IapProductIds.PremiumUser, IapPurchaseResult.AlreadyPending);
                return false;
            }

            if (!HasStoreAdapter || PurchaseRequested == null)
            {
                CompletePurchase(IapProductIds.PremiumUser, IapPurchaseResult.NotInitialized);
                return false;
            }

            _pendingProductId = IapProductIds.PremiumUser;
            _analytics?.PurchaseStarted(IapProductIds.PremiumUser);
            try
            {
                PurchaseRequested.Invoke(IapProductIds.PremiumUser);
                return true;
            }
            catch
            {
                CompletePurchase(IapProductIds.PremiumUser, IapPurchaseResult.Failed);
                return false;
            }
        }

        public void RestorePurchases()
        {
            TryRestorePurchases();
        }

        public bool TryRestorePurchases()
        {
            if (_restorePending)
                return false;

            if (!HasStoreAdapter || RestoreRequested == null)
            {
                CompleteRestore(IapPurchaseResult.NotInitialized);
                return false;
            }

            _restorePending = true;
            try
            {
                RestoreRequested.Invoke();
                return true;
            }
            catch
            {
                CompleteRestore(IapPurchaseResult.Failed);
                return false;
            }
        }

        public void CompletePurchase(string productId)
        {
            CompletePurchase(productId, IapPurchaseResult.Success);
        }

        public void CompletePurchase(string productId, IapPurchaseResult result)
        {
            if (string.Equals(_pendingProductId, productId, StringComparison.Ordinal))
                _pendingProductId = null;

            if (result != IapPurchaseResult.Success)
            {
                _analytics?.PurchaseFailed(productId, result.ToString());
                PurchaseCompleted?.Invoke(productId, result);
                return;
            }

            if (productId == IapProductIds.PremiumUser)
            {
                _premium?.GrantPremium();
                PurchaseCompleted?.Invoke(productId, IapPurchaseResult.Success);
                return;
            }

            _analytics?.PurchaseFailed(productId, IapPurchaseResult.UnknownProduct.ToString());
            PurchaseCompleted?.Invoke(productId, IapPurchaseResult.UnknownProduct);
        }

        public void CompleteRestore(IapPurchaseResult result)
        {
            _restorePending = false;
            RestoreCompleted?.Invoke(result);
        }

        public void RestoreOwnedProduct(string productId)
        {
            if (productId == IapProductIds.PremiumUser)
            {
                _premium?.GrantPremium();
                PurchaseCompleted?.Invoke(productId, IapPurchaseResult.Success);
                CompleteRestore(IapPurchaseResult.Success);
                return;
            }

            CompleteRestore(IapPurchaseResult.UnknownProduct);
        }
    }
}
