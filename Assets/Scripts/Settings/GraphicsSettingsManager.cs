using UnityEngine;

public enum GraphicsQuality
{
    Low = 0,
    High = 1
}

/// <summary>
/// Central place to control graphics quality:
/// - Toggles post-processing
/// - Tweaks fog density
/// - Adjusts shadow settings
/// Stores choice in PlayerPrefs so it persists.
/// </summary>
public class GraphicsSettingsManager : MonoBehaviour
{
    private const string QualityKey = "GraphicsQuality";

    [Header("References")]
    [Tooltip("Root object that holds your post-processing Volume / stack.")]
    [SerializeField] private GameObject postProcessingRoot;

    [Header("Fog Settings (Low / High)")]
    [SerializeField] private float lowFogDensity = 0.003f;
    [SerializeField] private float highFogDensity = 0.01f;

    [Header("Shadows")]
    [SerializeField] private Light mainDirectionalLight;
    [SerializeField] private LightShadows lowShadowMode = LightShadows.None;
    [SerializeField] private LightShadows highShadowMode = LightShadows.Soft;

    public static GraphicsQuality CurrentQuality { get; private set; } = GraphicsQuality.High;

    private void Awake()
    {
        int saved = PlayerPrefs.GetInt(QualityKey, (int)GraphicsQuality.High);
        GraphicsQuality quality = (GraphicsQuality)Mathf.Clamp(saved, 0, 1);
        ApplyQuality(quality, false);
    }

    public void SetLowQuality()
    {
        ApplyQuality(GraphicsQuality.Low, true);
    }

    public void SetHighQuality()
    {
        ApplyQuality(GraphicsQuality.High, true);
    }

    private void ApplyQuality(GraphicsQuality quality, bool save)
    {
        CurrentQuality = quality;

        // Post-processing
        if (postProcessingRoot != null)
        {
            postProcessingRoot.SetActive(quality == GraphicsQuality.High);
        }

        // Fog density
        if (RenderSettings.fog)
        {
            RenderSettings.fogDensity = (quality == GraphicsQuality.High)
                ? highFogDensity
                : lowFogDensity;
        }

        // Shadows
        if (mainDirectionalLight != null)
        {
            mainDirectionalLight.shadows = (quality == GraphicsQuality.High)
                ? highShadowMode
                : lowShadowMode;
        }

        if (save)
        {
            PlayerPrefs.SetInt(QualityKey, (int)quality);
            PlayerPrefs.Save();
        }
    }
}
