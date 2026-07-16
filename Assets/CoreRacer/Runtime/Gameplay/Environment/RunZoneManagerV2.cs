using UnityEngine;
using CoreRacer.Gameplay.Vfx;

namespace CoreRacer.Gameplay.Environment
{
    public sealed class RunZoneManagerV2 : MonoBehaviour
    {
        [SerializeField] private RunZoneCatalog catalog;
        [SerializeField] private MeshRenderer tunnelRenderer;
        [SerializeField] private TunnelWallGeneratorV2 tunnelGenerator;
        [SerializeField] private VfxManager vfxManager;

        public string CurrentZoneId { get; private set; }

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

            CurrentZoneId = zone.Id;

            if (tunnelGenerator == null)
                tunnelGenerator = FindObjectOfType<TunnelWallGeneratorV2>();
            if (vfxManager == null)
                vfxManager = FindObjectOfType<VfxManager>();

            if (tunnelGenerator != null)
                tunnelGenerator.SetWallTint(zone.WallTint);
            vfxManager?.SetEnvironmentTint(zone.AccentColor);

            if (tunnelRenderer != null && zone.TunnelMaterial != null)
                tunnelRenderer.sharedMaterial = zone.TunnelMaterial;
            RenderSettings.ambientLight = zone.AmbientColor;
            RenderSettings.fog = true;
            RenderSettings.fogColor = zone.FogColor;
            RenderSettings.fogDensity = zone.FogDensity;
        }
    }
}
