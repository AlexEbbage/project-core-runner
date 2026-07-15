using UnityEngine;

namespace CoreRacer.Meta.Ships
{
    [CreateAssetMenu(menuName = "Core Racer/Ships/Ship Definition")]
    public sealed class ShipDefinition : UnlockableDefinition
    {
        public ShipStats BaseStats;
    }
}
