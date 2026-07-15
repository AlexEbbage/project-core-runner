using System;
using CoreRacer.Meta.Economy;
using UnityEngine;

namespace CoreRacer.Meta.Ships
{
    public enum ShipStatType { Speed, Handling, Stability, Boost, Energy }
    public enum UpgradeType { ComboMultiplier, PickupRadius, Handling, ShieldRecharge }

    [Serializable]
    public struct ShipStats
    {
        public float Speed;
        public float Handling;
        public float Stability;
        public float Boost;
        public float Energy;

        public float GetValue(ShipStatType type)
        {
            switch (type)
            {
                case ShipStatType.Speed: return Speed;
                case ShipStatType.Handling: return Handling;
                case ShipStatType.Stability: return Stability;
                case ShipStatType.Boost: return Boost;
                case ShipStatType.Energy: return Energy;
                default: return 0f;
            }
        }
    }

    public abstract class UnlockableDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public Sprite Icon;
        public CurrencyAmount Price;
        public GameObject Prefab;
    }
}
