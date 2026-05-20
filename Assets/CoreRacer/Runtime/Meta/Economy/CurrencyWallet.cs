using System;

namespace CoreRacer.Meta.Economy
{
    [Serializable]
    public sealed class CurrencyWallet
    {
        public int Soft;
        public int Premium;

        public int Get(CurrencyType type)
        {
            return type == CurrencyType.Premium ? Premium : Soft;
        }

        public bool CanSpend(CurrencyAmount price)
        {
            return price.Amount <= 0 || Get(price.Currency) >= price.Amount;
        }

        public void Add(CurrencyAmount amount)
        {
            Add(amount.Currency, amount.Amount);
        }

        public void Add(CurrencyType type, int amount)
        {
            if (amount <= 0) return;
            if (type == CurrencyType.Premium) Premium += amount;
            else Soft += amount;
        }

        public bool TrySpend(CurrencyAmount price)
        {
            if (!CanSpend(price))
                return false;

            if (price.Amount <= 0)
                return true;

            if (price.Currency == CurrencyType.Premium) Premium -= price.Amount;
            else Soft -= price.Amount;
            return true;
        }
    }
}
