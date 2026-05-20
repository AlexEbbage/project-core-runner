using System.Collections.Generic;
using UnityEngine;

namespace CoreRacer.Gameplay.Vfx
{
    [CreateAssetMenu(menuName = "Core Racer/VFX/VFX Library")]
    public sealed class VfxLibrary : ScriptableObject
    {
        public List<VfxDefinition> Effects = new List<VfxDefinition>();

        public bool TryGet(VfxEventId id, out VfxDefinition definition)
        {
            for (int i = 0; i < Effects.Count; i++)
            {
                var effect = Effects[i];
                if (effect != null && effect.Id == id)
                {
                    definition = effect;
                    return true;
                }
            }
            definition = null;
            return false;
        }
    }
}
