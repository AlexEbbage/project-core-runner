using CoreRacer.Meta.Economy;
using UnityEngine;

namespace CoreRacer.Meta.Ships
{
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
}
