using System;
using System.Collections.Generic;
using CoreRacer.Meta.Economy;
using CoreRacer.Meta.Progression;

namespace CoreRacer.Meta.Profile
{
    public sealed class PlayerProfileService
    {
        private readonly PlayerProfileRepository _repository;

        public PlayerProfileState State { get; private set; }
        public event Action Changed;

        public PlayerProfileService(PlayerProfileRepository repository)
        {
            _repository = repository;
            State = _repository.Load();
        }

        public void Save()
        {
            _repository.Save(State);
        }

        public void AddCurrency(CurrencyType type, int amount)
        {
            State.Wallet.Add(type, amount);
            Commit();
        }

        public bool TrySpend(CurrencyAmount price)
        {
            if (!State.Wallet.TrySpend(price))
                return false;
            Commit();
            return true;
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0) return;
            State.Experience += amount;
            while (State.Experience >= ExperienceForNextLevel(State.Level))
            {
                State.Experience -= ExperienceForNextLevel(State.Level);
                State.Level++;
            }
            Commit();
        }

        public int GetUpgradeLevel(List<SerializableIntById> list, string id)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i].Id == id)
                    return list[i].Value;
            return 0;
        }

        public void SetUpgradeLevel(List<SerializableIntById> list, string id, int level)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == id)
                {
                    list[i] = new SerializableIntById(id, level);
                    Commit();
                    return;
                }
            }
            list.Add(new SerializableIntById(id, level));
            Commit();
        }

        public bool UnlockItem(string id)
        {
            var changed = State.Inventory.Unlock(id);
            if (changed) Commit();
            return changed;
        }

        public void RecordRun(int score, int coins, float distance, int powerups)
        {
            State.TotalRuns++;
            State.TotalCoinsCollected += Math.Max(0, coins);
            State.TotalPowerupsCollected += Math.Max(0, powerups);
            if (score > State.BestScore) State.BestScore = score;
            if (distance > State.BestDistance) State.BestDistance = distance;
            Commit();
        }

        public int ExperienceForNextLevel(int level)
        {
            return 500 + Math.Max(0, level - 1) * 250;
        }

        private void Commit()
        {
            Save();
            Changed?.Invoke();
        }
    }
}
