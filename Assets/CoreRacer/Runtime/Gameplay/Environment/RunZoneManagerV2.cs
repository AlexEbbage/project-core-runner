using UnityEngine;
using CoreRacer.Gameplay.Vfx;

namespace CoreRacer.Gameplay.Environment
{
    public sealed class RunZoneManagerV2 : MonoBehaviour
    {
        private static readonly Color BaseWallTint = new Color(0.82f, 0.84f, 0.88f, 1f);
        private static readonly Color BaseAmbientColor = new Color(0.42f, 0.44f, 0.48f, 1f);
        private static readonly Color BaseFogColor = new Color(0.035f, 0.04f, 0.05f, 1f);
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
                tunnelGenerator.SetWallTint(BaseWallTint);
            vfxManager?.ClearEnvironmentTint();

            if (tunnelRenderer != null && zone.TunnelMaterial != null)
                tunnelRenderer.sharedMaterial = zone.TunnelMaterial;
            RenderSettings.ambientLight = BaseAmbientColor;
            RenderSettings.fog = true;
            RenderSettings.fogColor = BaseFogColor;
            RenderSettings.fogDensity = 0.008f;
        }
    }
}
