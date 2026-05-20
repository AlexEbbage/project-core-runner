using System.Collections.Generic;
using CoreRacer.Common.Validation;
using CoreRacer.Meta.Economy;
using UnityEngine;

namespace CoreRacer.Meta.Ships
{
    public enum ShipStatType { Speed, Handling, Stability, Boost, Energy }
    public enum UpgradeType { ComboMultiplier, PickupRadius, Handling, ShieldRecharge }

    [System.Serializable]
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

    [CreateAssetMenu(menuName = "Core Racer/Ships/Ship Definition")]
    public sealed class ShipDefinition : UnlockableDefinition
    {
        public ShipStats BaseStats;
    }

    [CreateAssetMenu(menuName = "Core Racer/Ships/Skin Definition")]
    public sealed class ShipSkinDefinition : UnlockableDefinition { }

    [CreateAssetMenu(menuName = "Core Racer/Ships/Trail Definition")]
    public sealed class ShipTrailDefinition : UnlockableDefinition { }

    [CreateAssetMenu(menuName = "Core Racer/Ships/Core FX Definition")]
    public sealed class ShipCoreFxDefinition : UnlockableDefinition { }

    [CreateAssetMenu(menuName = "Core Racer/Ships/Upgrade Definition")]
    public sealed class ShipUpgradeDefinition : ScriptableObject
    {
        public UpgradeType UpgradeType;
        public string DisplayName;
        public Sprite Icon;
        public int MaxLevel = 5;
        public CurrencyType Currency = CurrencyType.Soft;
        public int BaseCost = 100;
        public int CostIncrease = 50;

        public int GetCostForLevel(int level)
        {
            return Mathf.Max(0, BaseCost + CostIncrease * Mathf.Max(0, level));
        }
    }

    [CreateAssetMenu(menuName = "Core Racer/Ships/Ship Database")]
    public sealed class ShipDatabase : ScriptableObject, IValidatableConfig
    {
        public List<ShipDefinition> Ships = new List<ShipDefinition>();
        public List<ShipSkinDefinition> Skins = new List<ShipSkinDefinition>();
        public List<ShipTrailDefinition> Trails = new List<ShipTrailDefinition>();
        public List<ShipCoreFxDefinition> CoreFx = new List<ShipCoreFxDefinition>();
        public List<ShipUpgradeDefinition> Upgrades = new List<ShipUpgradeDefinition>();

        public ShipDefinition GetShip(string id) => FindById(Ships, id);
        public ShipSkinDefinition GetSkin(string id) => FindById(Skins, id);
        public ShipTrailDefinition GetTrail(string id) => FindById(Trails, id);
        public ShipCoreFxDefinition GetCoreFx(string id) => FindById(CoreFx, id);

        private static T FindById<T>(List<T> list, string id) where T : UnlockableDefinition
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i] != null && list[i].Id == id)
                    return list[i];
            return null;
        }

        public ValidationResult ValidateConfig()
        {
            var result = new ValidationResult();
            ValidateUnique(result, Ships, "ship");
            ValidateUnique(result, Skins, "skin");
            ValidateUnique(result, Trails, "trail");
            ValidateUnique(result, CoreFx, "core_fx");
            return result;
        }

        private static void ValidateUnique<T>(ValidationResult result, List<T> items, string label) where T : UnlockableDefinition
        {
            var ids = new HashSet<string>();
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null) continue;
                if (string.IsNullOrWhiteSpace(item.Id)) result.Error($"{label} has empty id: {item.name}");
                else if (!ids.Add(item.Id)) result.Error($"Duplicate {label} id: {item.Id}");
            }
        }
    }
}
