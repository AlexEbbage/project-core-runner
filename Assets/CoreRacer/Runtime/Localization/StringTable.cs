using System.Collections.Generic;
using UnityEngine;

namespace CoreRacer.Localization
{
    [CreateAssetMenu(menuName = "Core Racer/Localization/String Table")]
    public sealed class StringTable : ScriptableObject
    {
        [System.Serializable]
        public sealed class Entry
        {
            public string Key;
            public string Value;
        }

        public List<Entry> Entries = new List<Entry>();

        public string Get(string key)
        {
            for (var i = 0; i < Entries.Count; i++)
            {
                if (Entries[i] != null && Entries[i].Key == key)
                    return Entries[i].Value;
            }

            return key;
        }
    }
}
