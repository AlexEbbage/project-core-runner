using System.Collections.Generic;
using UnityEngine;

namespace CoreRacer.Gameplay.Environment
{
    [System.Serializable]
    public sealed class RunZoneDefinition
    {
        public string Id = "neon_hex";
        public string DisplayName = "Neon Hex";
        public Material TunnelMaterial;
        public Color AmbientColor = Color.cyan;
        public Color FogColor = Color.black;
        public float FogDensity = 0.01f;
        public Color WallTint = new Color(0.24f, 0.32f, 0.48f, 1f);
        public Color HazardColor = Color.red;
        public Color AccentColor = Color.cyan;
    }

    [CreateAssetMenu(menuName = "Core Racer/Environment/Run Zone Catalog")]
    public sealed class RunZoneCatalog : ScriptableObject
    {
        public List<RunZoneDefinition> Zones = new List<RunZoneDefinition>();

        public RunZoneDefinition GetDefault()
        {
            return Zones != null && Zones.Count > 0 ? Zones[0] : null;
        }

        public RunZoneDefinition Get(string id)
        {
            if (Zones == null)
                return null;

            for (var i = 0; i < Zones.Count; i++)
            {
                if (Zones[i] != null && Zones[i].Id == id)
                    return Zones[i];
            }

            return GetDefault();
        }
    }
}
