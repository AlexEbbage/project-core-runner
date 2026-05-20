using System;
using System.Collections.Generic;
using CoreRacer.Services.Save;

namespace CoreRacer.Meta.Economy
{
    [System.Serializable]
    internal sealed class EconomyLedgerSaveData
    {
        public List<EconomyTransaction> Entries = new List<EconomyTransaction>();
    }

    public sealed class EconomyLedger
    {
        private const string SaveKey = "core_racer_economy_ledger";
        private readonly ISaveStorage _storage;
        private readonly JsonSaveSerializer _serializer;
        private readonly int _maxEntries;
        private readonly List<EconomyTransaction> _entries = new List<EconomyTransaction>();

        public IReadOnlyList<EconomyTransaction> Entries => _entries;

        public EconomyLedger(ISaveStorage storage, JsonSaveSerializer serializer, int maxEntries = 250)
        {
            _storage = storage;
            _serializer = serializer;
            _maxEntries = maxEntries;
            Load();
        }

        public void Record(string reason, CurrencyType currency, int amount, int balanceBefore, int balanceAfter, string sourceId = null)
        {
            _entries.Add(new EconomyTransaction
            {
                Id = Guid.NewGuid().ToString("N"),
                Reason = reason,
                Currency = currency,
                Amount = amount,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                SourceId = sourceId,
                UtcIso = DateTimeOffset.UtcNow.ToString("o")
            });
            while (_entries.Count > _maxEntries) _entries.RemoveAt(0);
            Save();
        }

        private void Load()
        {
            if (_storage == null || !_storage.Exists(SaveKey)) return;
            var data = _serializer.Deserialize<EconomyLedgerSaveData>(_storage.Load(SaveKey));
            if (data != null && data.Entries != null) _entries.AddRange(data.Entries);
        }

        private void Save()
        {
            _storage?.Save(SaveKey, _serializer.Serialize(new EconomyLedgerSaveData { Entries = _entries }));
        }
    }
}
