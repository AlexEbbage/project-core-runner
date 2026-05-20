using System;
using CoreRacer.Monetisation.Premium;

namespace CoreRacer.Monetisation.Iap
{
    public enum IapPurchaseResult
    {
        Success,
        Failed,
        Cancelled,
        NotInitialized,
        UnknownProduct
    }

    /// <summary>
    /// SDK-agnostic purchase facade. Wire Unity IAP/Google Play callbacks into CompletePurchase/RestoreOwnedProduct.
    /// </summary>
    public sealed class IapPurchaseService
    {
        private readonly PremiumEntitlementService _premium;
        public event Action<string> PurchaseRequested;
        public event Action RestoreRequested;
        public event Action<string, IapPurchaseResult> PurchaseCompleted;

        public IapPurchaseService(PremiumEntitlementService premium)
        {
            _premium = premium;
        }

        
        public void BuyPremium()
        {
            PurchaseRequested?.Invoke(IapProductIds.PremiumUser);
        }

        public void RestorePurchases()
        {
            RestoreRequested?.Invoke();
        }

        public void CompletePurchase(string productId)
        {
            if (productId == IapProductIds.PremiumUser)
            {
                _premium.GrantPremium();
                PurchaseCompleted?.Invoke(productId, IapPurchaseResult.Success);
                return;
            }

            PurchaseCompleted?.Invoke(productId, IapPurchaseResult.UnknownProduct);
        }

        public void RestoreOwnedProduct(string productId)
        {
            // Important: never revoke existing premium on failed restore. Only grant on positive ownership.
            CompletePurchase(productId);
        }
    }
}
