using UnityEngine;

namespace CoreRacer.Gameplay.Environment
{
    public sealed class RunZoneManagerV2 : MonoBehaviour
    {
        [SerializeField] private RunZoneCatalog catalog;
        [SerializeField] private MeshRenderer tunnelRenderer;

        public void ApplyDefaultZone()
        {
            Apply(catalog != null ? catalog.GetDefault() : null);
        }

        public void ApplyZone(string zoneId)
        {
            Apply(catalog != null ? catalog.Get(zoneId) : null);
        }

        public void Apply(RunZoneDefinition zone)
        {
            if (zone == null)
                return;

            if (tunnelRenderer != null && zone.TunnelMaterial != null)
                tunnelRenderer.sharedMaterial = zone.TunnelMaterial;
            RenderSettings.ambientLight = zone.AmbientColor;
            RenderSettings.fog = true;
            RenderSettings.fogColor = zone.FogColor;
            RenderSettings.fogDensity = zone.FogDensity;
        }
    }
}
