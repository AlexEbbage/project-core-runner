using UnityEngine;

namespace CoreRacer.Gameplay.Vfx
{
    [System.Serializable]
    public sealed class VfxDefinition
    {
        public VfxEventId Id;
        public VfxPooledInstance Prefab;
        public int InitialPoolSize = 4;
        public bool DisableOnLowQuality;
    }
}
