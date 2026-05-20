using System;

namespace CoreRacer.Meta.Economy
{
    [Serializable]
    public struct CurrencyAmount
    {
        public CurrencyType Currency;
        public int Amount;

        public CurrencyType Type => Currency;

        public CurrencyAmount(CurrencyType currency, int amount)
        {
            Currency = currency;
            Amount = amount;
        }
    }
}
