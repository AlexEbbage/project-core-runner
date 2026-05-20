using System.Collections.Generic;

namespace CoreRacer.Meta.Economy
{
    public sealed class EconomyAnomalyDetector
    {
        public List<string> Detect(IReadOnlyList<EconomyTransaction> entries)
        {
            var warnings = new List<string>();
            if (entries == null) return warnings;

            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e.BalanceBefore + e.Amount != e.BalanceAfter)
                    warnings.Add($"Ledger mismatch: {e.Id} {e.Currency} before {e.BalanceBefore} + {e.Amount} != after {e.BalanceAfter}");
                if (e.Amount > 1000000)
                    warnings.Add($"Large currency transaction: {e.Id} {e.Amount} {e.Currency}");
            }
            return warnings;
        }
    }
}
