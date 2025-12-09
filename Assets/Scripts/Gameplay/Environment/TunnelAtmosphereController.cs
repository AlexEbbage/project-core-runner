using UnityEngine;

/// <summary>
/// Controls simple distance-based fog/atmosphere in the tunnel.
/// Fog gets denser and changes color as the player moves down +Z.
/// </summary>
public class TunnelAtmosphereController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Distance Range")]
    [Tooltip("Player Z at which we start blending fog effects.")]
    [SerializeField] private float minZ = 0f;

    [Tooltip("Player Z at which we reach max fog color/density.")]
    [SerializeField] private float maxZ = 300f;

    [Header("Fog Color")]
    [Tooltip("Fog color will be taken from this gradient based on distance.")]
    [SerializeField] private Gradient fogColorOverDistance;

    [Header("Fog Density")]
    [SerializeField] private float minFogDensity = 0.003f;
    [SerializeField] private float maxFogDensity = 0.03f;

    [Header("Mode")]
    [SerializeField] private FogMode fogMode = FogMode.Exponential;

    private void Awake()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = fogMode;
    }

    private void LateUpdate()
    {
        //if (player == null)
        //    return;

        //float z = player.position.z;

        //// Normalize distance 0..1 between minZ and maxZ
        //float t = Mathf.InverseLerp(minZ, maxZ, z);

        //// Color
        //if (fogColorOverDistance != null)
        //{
        //    RenderSettings.fogColor = fogColorOverDistance.Evaluate(Mathf.Clamp01(t));
        //}

        //// Density
        //RenderSettings.fogDensity = Mathf.Lerp(minFogDensity, maxFogDensity, Mathf.Clamp01(t));
    }
}
