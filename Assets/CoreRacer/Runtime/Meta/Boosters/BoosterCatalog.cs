using System.Collections.Generic;
using CoreRacer.Meta.Economy;
using UnityEngine;

namespace CoreRacer.Meta.Boosters
{
    public enum BoosterEffectType
    {
        StartShield,
        CoinMultiplier,
        ScoreMultiplier,
        ExtraContinue,
        MagnetBoost
    }

    [System.Serializable]
    public sealed class BoosterDefinition
    {
        public string Id;
        public string DisplayName;
        public Sprite Icon;
        public BoosterEffectType EffectType;
        public float Value = 1f;
        public CurrencyAmount Price;
    }

    [CreateAssetMenu(menuName = "Core Racer/Boosters/Booster Catalog")]
    public sealed class BoosterCatalog : ScriptableObject
    {
        public List<BoosterDefinition> Boosters = new List<BoosterDefinition>();

        public BoosterDefinition Get(string id)
        {
            for (int i = 0; i < Boosters.Count; i++)
                if (Boosters[i] != null && Boosters[i].Id == id)
                    return Boosters[i];
            return null;
        }
    }
}
