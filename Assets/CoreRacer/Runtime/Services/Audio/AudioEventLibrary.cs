using System.Collections.Generic;
using UnityEngine;

namespace CoreRacer.Services.Audio
{
    [CreateAssetMenu(menuName = "Core Racer/Audio/Audio Event Library")]
    public sealed class AudioEventLibrary : ScriptableObject
    {
        public List<AudioEventDefinition> Events = new List<AudioEventDefinition>();

        public bool TryGet(AudioEventId id, out AudioEventDefinition definition)
        {
            for (int i = 0; i < Events.Count; i++)
            {
                var item = Events[i];
                if (item != null && item.Id == id)
                {
                    definition = item;
                    return true;
                }
            }
            definition = null;
            return false;
        }
    }
}
