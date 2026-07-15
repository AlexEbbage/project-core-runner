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
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            State = _repository.Load() ?? PlayerProfileDefaults.CreateNew();
        }

        /// <summary>Persists without broadcasting. Prefer Mutate/TryMutate for state changes.</summary>
        public void Save()
        {
            _repository.Save(State);
        }

        public void CommitExternalMutation()
        {
            Commit();
        }

        /// <summary>Applies one grouped mutation, persists once, and broadcasts once.</summary>
        public void Mutate(Action<PlayerProfileState> mutation)
        {
            if (mutation == null)
                return;

            mutation(State);
            Commit();
        }

        /// <summary>
        /// Applies and commits a conditional grouped mutation. Returning false leaves storage and listeners untouched.
        /// Use this for claims and purchases so currency, entitlement, and consumed markers are one profile commit.
        /// </summary>
        public bool TryMutate(Func<PlayerProfileState, bool> mutation)
        {
            if (mutation == null || !mutation(State))
                return false;

            Commit();
            return true;
        }

        public void AddCurrency(CurrencyType type, int amount)
        {
            if (amount <= 0)
                return;

            Mutate(state => state.Wallet.Add(type, amount));
        }

        public bool TrySpend(CurrencyAmount price)
        {
            return TryMutate(state => state.Wallet.TrySpend(price));
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0)
                return;

            Mutate(state => ApplyExperience(state, amount));
        }

        public void ApplyExperience(PlayerProfileState state, int amount)
        {
            if (state == null || amount <= 0)
                return;

            state.Experience += amount;
            while (state.Experience >= ExperienceForNextLevel(state.Level))
            {
                state.Experience -= ExperienceForNextLevel(state.Level);
                state.Level++;
            }
        }

        public int GetUpgradeLevel(List<SerializableIntById> list, string id)
        {
            if (list == null || string.IsNullOrWhiteSpace(id))
                return 0;

            for (int i = 0; i < list.Count; i++)
                if (list[i].Id == id)
                    return list[i].Value;
            return 0;
        }

        public void SetUpgradeLevel(List<SerializableIntById> list, string id, int level)
        {
            if (list == null || string.IsNullOrWhiteSpace(id))
                return;

            Mutate(_ => SetIntById(list, id, Math.Max(0, level)));
        }

        public bool UnlockItem(string id)
        {
            var changed = false;
            TryMutate(state =>
            {
                changed = state.Inventory.Unlock(id);
                return changed;
            });
            return changed;
        }

        public void RecordRun(int score, int coins, float distance, int powerups)
        {
            Mutate(state => ApplyRunRecord(state, score, coins, distance, powerups));
        }

        public void ApplyRunRecord(PlayerProfileState state, int score, int coins, float distance, int powerups)
        {
            if (state == null)
                return;

            state.TotalRuns++;
            state.TotalCoinsCollected += Math.Max(0, coins);
            state.TotalPowerupsCollected += Math.Max(0, powerups);
            if (score > state.BestScore) state.BestScore = score;
            if (distance > state.BestDistance) state.BestDistance = distance;
        }

        public int ExperienceForNextLevel(int level)
        {
            return 500 + Math.Max(0, level - 1) * 250;
        }

        private static void SetIntById(List<SerializableIntById> list, string id, int value)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].Id != id)
                    continue;

                list[i] = new SerializableIntById(id, value);
                return;
            }

            list.Add(new SerializableIntById(id, value));
        }

        private void Commit()
        {
            Save();
            Changed?.Invoke();
        }
    }
}
