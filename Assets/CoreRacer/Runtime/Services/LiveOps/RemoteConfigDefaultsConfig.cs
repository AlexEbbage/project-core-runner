using System.Collections.Generic;
using UnityEngine;

namespace CoreRacer.Services.LiveOps
{
    [CreateAssetMenu(menuName = "Core Racer/LiveOps/Remote Config Defaults")]
    public sealed class RemoteConfigDefaultsConfig : ScriptableObject
    {
        public List<RemoteConfigValue> Values = new List<RemoteConfigValue>();

        public bool TryGet(string key, out string value)
        {
            for (int i = 0; i < Values.Count; i++)
            {
                var item = Values[i];
                if (item != null && item.Key == key)
                {
                    value = item.Value;
                    return true;
                }
            }

            value = string.Empty;
            return false;
        }
    }
}
