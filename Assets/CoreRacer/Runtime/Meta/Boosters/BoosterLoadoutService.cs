using System;
using System.Collections.Generic;
using CoreRacer.Meta.Profile;
using UnityEngine;

namespace CoreRacer.Meta.Boosters
{
    public struct BoosterRunModifiers
    {
        public float ScoreMultiplier;
        public float CoinMultiplier;
        public float StartShieldSeconds;

        public static BoosterRunModifiers Default => new BoosterRunModifiers
        {
            ScoreMultiplier = 1f,
            CoinMultiplier = 1f,
            StartShieldSeconds = 0f
        };
    }

    public static class BoosterLoadoutResolver
    {
        public static BoosterRunModifiers Resolve(BoosterCatalog catalog, IList<string> equippedIds)
        {
            var modifiers = BoosterRunModifiers.Default;
            if (catalog == null || equippedIds == null)
                return modifiers;

            var appliedFamilies = new HashSet<BoosterFamily>();
            for (var i = 0; i < equippedIds.Count; i++)
            {
                var booster = catalog.Get(equippedIds[i]);
                if (booster == null || !appliedFamilies.Add(booster.Family))
                    continue;

                switch (booster.EffectType)
                {
                    case BoosterEffectType.StartShield:
                        modifiers.StartShieldSeconds = Mathf.Max(modifiers.StartShieldSeconds, booster.Value);
                        break;
                    case BoosterEffectType.CoinMultiplier:
                        modifiers.CoinMultiplier = Mathf.Max(modifiers.CoinMultiplier, booster.Value);
                        break;
                    case BoosterEffectType.ScoreMultiplier:
                        modifiers.ScoreMultiplier = Mathf.Max(modifiers.ScoreMultiplier, booster.Value);
                        break;
                }
            }

            return modifiers;
        }
    }

    public sealed class BoosterLoadoutService
    {
        private readonly PlayerProfileService _profile;
        private readonly BoosterCatalog _catalog;

        public BoosterLoadoutService(PlayerProfileService profile, BoosterCatalog catalog)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public bool IsEquipped(string boosterId)
        {
            return !string.IsNullOrWhiteSpace(boosterId) && _profile.State.EquippedBoosterIds.Contains(boosterId);
        }

        public bool TryToggle(string boosterId)
        {
            var booster = _catalog.Get(boosterId);
            if (booster == null)
                return false;

            _profile.Mutate(state =>
            {
                var equipped = state.EquippedBoosterIds;
                var wasEquipped = equipped.Contains(booster.Id);
                for (var i = equipped.Count - 1; i >= 0; i--)
                {
                    var existing = _catalog.Get(equipped[i]);
                    if (existing == null || existing.Family == booster.Family)
                        equipped.RemoveAt(i);
                }

                if (!wasEquipped)
                    equipped.Add(booster.Id);
            });
            return true;
        }
    }
}
