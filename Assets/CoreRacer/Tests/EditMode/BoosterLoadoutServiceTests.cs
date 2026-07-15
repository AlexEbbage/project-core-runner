using System.Collections.Generic;
using CoreRacer.Meta.Boosters;
using CoreRacer.Meta.Profile;
using CoreRacer.Services.Save;
using NUnit.Framework;
using UnityEngine;

namespace CoreRacer.Tests.EditMode
{
    public sealed class BoosterLoadoutServiceTests
    {
        [Test]
        public void Toggle_ReplacesOnlyTheExistingChoiceFromTheSameFamily()
        {
            var catalog = CreateCatalog();
            try
            {
                catalog.Boosters.Add(Definition("shield_long", BoosterFamily.Survival, BoosterEffectType.StartShield, 3f));
                var profile = CreateProfile();
                var loadout = new BoosterLoadoutService(profile, catalog);

                Assert.IsTrue(loadout.TryToggle("start_shield"));
                Assert.IsTrue(loadout.TryToggle("coin_boost"));
                Assert.IsTrue(loadout.TryToggle("shield_long"));

                CollectionAssert.AreEquivalent(new[] { "coin_boost", "shield_long" }, profile.State.EquippedBoosterIds);
                Assert.IsFalse(loadout.IsEquipped("start_shield"));
                Assert.IsTrue(loadout.IsEquipped("shield_long"));
            }
            finally
            {
                Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void Resolve_CombinesOneValidatedBoosterPerFamily()
        {
            var catalog = CreateCatalog();
            try
            {
                var modifiers = BoosterLoadoutResolver.Resolve(catalog, new List<string>
                {
                    "start_shield",
                    "coin_boost",
                    "score_boost",
                    "missing"
                });

                Assert.AreEqual(1f, modifiers.StartShieldSeconds);
                Assert.AreEqual(2f, modifiers.CoinMultiplier);
                Assert.AreEqual(2f, modifiers.ScoreMultiplier);
            }
            finally
            {
                Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void ToggleEquippedBooster_UnequipsItAndPersistsTheEmptyFamily()
        {
            var catalog = CreateCatalog();
            try
            {
                var profile = CreateProfile();
                var loadout = new BoosterLoadoutService(profile, catalog);
                loadout.TryToggle("score_boost");

                Assert.IsTrue(loadout.TryToggle("score_boost"));
                Assert.IsEmpty(profile.State.EquippedBoosterIds);
            }
            finally
            {
                Object.DestroyImmediate(catalog);
            }
        }

        private static BoosterCatalog CreateCatalog()
        {
            var catalog = ScriptableObject.CreateInstance<BoosterCatalog>();
            catalog.Boosters.Add(Definition("start_shield", BoosterFamily.Survival, BoosterEffectType.StartShield, 1f));
            catalog.Boosters.Add(Definition("coin_boost", BoosterFamily.Economy, BoosterEffectType.CoinMultiplier, 2f));
            catalog.Boosters.Add(Definition("score_boost", BoosterFamily.Score, BoosterEffectType.ScoreMultiplier, 2f));
            return catalog;
        }

        private static BoosterDefinition Definition(string id, BoosterFamily family, BoosterEffectType effect, float value)
        {
            return new BoosterDefinition { Id = id, DisplayName = id, Family = family, EffectType = effect, Value = value };
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
    }
}
