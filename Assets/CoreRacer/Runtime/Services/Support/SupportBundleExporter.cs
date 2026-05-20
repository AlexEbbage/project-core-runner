using System.Text;
using CoreRacer.Meta.Economy;
using CoreRacer.Services.Logging;
using CoreRacer.Services.Save;
using UnityEngine;

namespace CoreRacer.Services.Support
{
    public sealed class SupportBundleExporter
    {
        private readonly ISaveStorage _storage;
        private readonly CrashBreadcrumbBuffer _breadcrumbs;
        private readonly EconomyLedger _ledger;

        public SupportBundleExporter(ISaveStorage storage, CrashBreadcrumbBuffer breadcrumbs = null, EconomyLedger ledger = null)
        {
            _storage = storage;
            _breadcrumbs = breadcrumbs;
            _ledger = ledger;
        }

        public string BuildTextBundle(PlayerSupportInfo info)
        {
            var sb = new StringBuilder();
            sb.AppendLine("CORE RACER SUPPORT BUNDLE");
            sb.AppendLine(JsonUtility.ToJson(info, true));
            sb.AppendLine("\nBREADCRUMBS");
            if (_breadcrumbs != null)
                foreach (var entry in _breadcrumbs.Snapshot())
                    sb.AppendLine(entry.ToString());
            sb.AppendLine("\nRECENT ECONOMY TRANSACTIONS");
            if (_ledger != null)
                foreach (var tx in _ledger.Entries)
                    sb.AppendLine($"{tx.UtcIso} {tx.Reason} {tx.Currency} {tx.Amount} {tx.BalanceBefore}->{tx.BalanceAfter}");
            sb.AppendLine("\nSAVE PRESENCE");
            sb.AppendLine("Profile: " + (_storage != null && _storage.Exists(SaveKeys.PlayerProfile)));
            sb.AppendLine("Consent: " + (_storage != null && _storage.Exists(SaveKeys.Consent)));
            return sb.ToString();
        }
    }
}
