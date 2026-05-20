using System;
using CoreRacer.Services.Save;

namespace CoreRacer.Monetisation.Premium
{
    public sealed class PremiumEntitlementService
    {
        private readonly ISaveStorage _storage;
        public bool HasPremium => _storage.Load(SaveKeys.PremiumEntitlement) == "1";
        public event Action<bool> PremiumChanged;

        public PremiumEntitlementService(ISaveStorage storage)
        {
            _storage = storage;
        }

        public void GrantPremium()
        {
            _storage.Save(SaveKeys.PremiumEntitlement, "1");
            PremiumChanged?.Invoke(true);
        }

        public void RevokeForTestingOnly()
        {
            _storage.Delete(SaveKeys.PremiumEntitlement);
            PremiumChanged?.Invoke(false);
        }
    }
}
