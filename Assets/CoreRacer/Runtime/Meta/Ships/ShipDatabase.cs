using System.Collections.Generic;
using CoreRacer.Common.Validation;
using UnityEngine;

namespace CoreRacer.Meta.Ships
{
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
            if (list == null)
                return null;
            for (var i = 0; i < list.Count; i++)
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
            if (items == null)
            {
                result.Error(label + " list is null.");
                return;
            }

            var ids = new HashSet<string>();
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null) continue;
                if (string.IsNullOrWhiteSpace(item.Id)) result.Error($"{label} has empty id: {item.name}");
                else if (!ids.Add(item.Id)) result.Error($"Duplicate {label} id: {item.Id}");
            }
        }
    }
}
