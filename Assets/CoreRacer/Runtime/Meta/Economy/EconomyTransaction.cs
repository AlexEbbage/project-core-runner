using System;

namespace CoreRacer.Meta.Economy
{
    [Serializable]
    public sealed class EconomyTransaction
    {
        public string Id;
        public string Reason;
        public CurrencyType Currency;
        public int Amount;
        public int BalanceBefore;
        public int BalanceAfter;
        public string SourceId;
        public string UtcIso;
    }
}
