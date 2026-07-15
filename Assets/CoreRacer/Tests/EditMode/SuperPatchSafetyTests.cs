using System;
using System.Collections.Generic;
using CoreRacer.Common.Time;
using CoreRacer.Config.Run;
using CoreRacer.Gameplay.Run;
using CoreRacer.Meta.Economy;
using CoreRacer.Meta.Profile;
using CoreRacer.Monetisation.Iap;
using CoreRacer.Monetisation.Premium;
using CoreRacer.Services.Save;
using NUnit.Framework;

namespace CoreRacer.Tests.EditMode
{
    public sealed class SuperPatchSafetyTests
    {
        [Test]
        public void RunLifecycle_EndsOnlyOnce()
        {
            var state = new RunStateMachine();
            var lifecycle = new RunLifecycleService(state, new FixedClock());
            var ended = 0;
            lifecycle.RunEnded += _ => ended++;

            Assert.IsTrue(lifecycle.StartNewRun("hex_sector_01", "starter_runner"));
            Assert.IsFalse(lifecycle.StartNewRun("hex_sector_01", "starter_runner"));

            Assert.IsTrue(lifecycle.EndRun(RunEndReason.PlayerDeath));
            Assert.IsFalse(lifecycle.EndRun(RunEndReason.PlayerDeath));
            Assert.AreEqual(1, ended);
            Assert.AreEqual(RunState.GameOver, state.State);
        }

        [Test]
        public void DoubleRewardBonus_DoesNotRecordSecondRun()
        {
            var profile = CreateProfile();
            var service = new RunRewardService(profile, new RunRewardConfig
            {
                XpPerScorePoint = 0.1f,
                PremiumCurrencyPerCoins = 100
            });
            var settled = service.BuildResult(1000, 250, 100f, 20f, 2, RunEndReason.PlayerDeath, false);

            service.Grant(settled);
            service.GrantBonus(service.BuildBonusResult(settled));

            Assert.AreEqual(1, profile.State.TotalRuns);
            Assert.AreEqual(250, profile.State.TotalCoinsCollected);
            Assert.AreEqual(2, profile.State.TotalPowerupsCollected);
            Assert.AreEqual(500, profile.State.Wallet.Soft);
            Assert.AreEqual(4, profile.State.Wallet.Premium);
        }

        [Test]
        public void PurchaseUnlock_DoesNotSpendWhenUnlockCannotComplete()
        {
            var profile = CreateProfile();
            var rewards = new RewardGrantService(profile);
            var purchases = new PurchaseService(profile, rewards);
            profile.AddCurrency(CurrencyType.Soft, 50);

            var result = purchases.TryPurchaseUnlock("locked_skin", new CurrencyAmount(CurrencyType.Soft, 100));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(PurchaseFailureReason.InsufficientCurrency, result.FailureReason);
            Assert.AreEqual(50, profile.State.Wallet.Soft);
            Assert.IsFalse(profile.State.Inventory.IsUnlocked("locked_skin"));
        }

        [Test]
        public void IapWithoutStoreAdapter_FailsClosed()
        {
            var premium = new PremiumEntitlementService(new MemorySaveStorage());
            var iap = new IapPurchaseService(premium);
            var result = IapPurchaseResult.Success;
            iap.PurchaseCompleted += (_, completed) => result = completed;

            Assert.IsFalse(iap.TryBuyPremium());
            Assert.AreEqual(IapPurchaseResult.NotInitialized, result);
            Assert.IsFalse(premium.HasPremium);
            Assert.IsFalse(iap.IsPurchasePending);
        }

        [Test]
        public void SafeSave_BackupPreservesPreviousKnownGoodValue()
        {
            var memory = new MemorySaveStorage();
            var safe = new SafeSaveStorage(memory);
            safe.Save("profile", "version-one");
            safe.Save("profile", "version-two");

            memory.Save("profile", "corrupted");

            Assert.AreEqual("version-one", safe.Load("profile"));
        }

        [Test]
        public void Migration_RepairsNullCollectionsAndNegativeCounters()
        {
            var state = new PlayerProfileState
            {
                Level = -1,
                Experience = -20,
                TotalRuns = -3,
                Inventory = null,
                ClaimedTasks = null,
                ShipUpgradeLevels = null
            };

            var migrated = new ProfileMigrationService().Migrate(state);

            Assert.AreEqual(1, migrated.Level);
            Assert.AreEqual(0, migrated.Experience);
            Assert.AreEqual(0, migrated.TotalRuns);
            Assert.NotNull(migrated.Inventory);
            Assert.NotNull(migrated.Inventory.UnlockedIds);
            Assert.NotNull(migrated.ClaimedTasks);
            Assert.NotNull(migrated.ShipUpgradeLevels);
        }

        private static PlayerProfileService CreateProfile()
        {
            var repository = new PlayerProfileRepository(new MemorySaveStorage(), new JsonSaveSerializer(), new ProfileMigrationService());
            return new PlayerProfileService(repository);
        }

        private sealed class MemorySaveStorage : ISaveStorage
        {
            private readonly Dictionary<string, string> _values = new Dictionary<string, string>();
            public bool Exists(string key) => _values.ContainsKey(key);
            public string Load(string key) => _values.TryGetValue(key, out var value) ? value : null;
            public void Save(string key, string value) => _values[key] = value;
            public void Delete(string key) => _values.Remove(key);
        }

        private sealed class FixedClock : IGameClock
        {
            public float DeltaTime => 0.016f;
            public float UnscaledDeltaTime => 0.016f;
            public float TimeScale { get; set; } = 1f;
            public DateTimeOffset UtcNow => new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.Zero);
        }
    }
}
