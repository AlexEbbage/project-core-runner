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
    }

    [CreateAssetMenu(menuName = "Core Racer/Environment/Run Zone Catalog")]
    public sealed class RunZoneCatalog : ScriptableObject
    {
        public List<RunZoneDefinition> Zones = new List<RunZoneDefinition>();
        public RunZoneDefinition GetDefault() => Zones.Count > 0 ? Zones[0] : null;
    }

    public sealed class RunZoneManagerV2 : MonoBehaviour
    {
        [SerializeField] private RunZoneCatalog catalog;
        [SerializeField] private MeshRenderer tunnelRenderer;

        public void ApplyDefaultZone()
        {
            var zone = catalog != null ? catalog.GetDefault() : null;
            if (zone != null) Apply(zone);
        }

        public void Apply(RunZoneDefinition zone)
        {
            if (zone == null) return;
            if (tunnelRenderer != null && zone.TunnelMaterial != null)
                tunnelRenderer.sharedMaterial = zone.TunnelMaterial;
            RenderSettings.ambientLight = zone.AmbientColor;
            RenderSettings.fog = true;
            RenderSettings.fogColor = zone.FogColor;
            RenderSettings.fogDensity = zone.FogDensity;
        }
    }
}
