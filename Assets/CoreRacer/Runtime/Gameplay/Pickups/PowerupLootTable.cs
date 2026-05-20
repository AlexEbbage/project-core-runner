using CoreRacer.Gameplay.Powerups;
using UnityEngine;

namespace CoreRacer.Gameplay.Pickups
{
    public sealed class PowerupLootTable
    {
        private readonly PickupGenerationConfig _config;

        public PowerupLootTable(PickupGenerationConfig config)
        {
            _config = config;
        }

        public PowerupType Roll()
        {
            if (_config.PowerupLootTable == null || _config.PowerupLootTable.Count == 0)
                return PowerupType.Shield;

            float total = 0f;
            for (int i = 0; i < _config.PowerupLootTable.Count; i++)
                total += Mathf.Max(0f, _config.PowerupLootTable[i].Weight);

            if (total <= 0f)
                return PowerupType.Shield;

            var roll = Random.value * total;
            for (int i = 0; i < _config.PowerupLootTable.Count; i++)
            {
                roll -= Mathf.Max(0f, _config.PowerupLootTable[i].Weight);
                if (roll <= 0f)
                    return _config.PowerupLootTable[i].Type;
            }

            return _config.PowerupLootTable[_config.PowerupLootTable.Count - 1].Type;
        }
    }
}
