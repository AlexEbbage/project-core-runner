using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A "zone" in the run, defined by time:
/// - Changes tunnel wall colors
/// - Changes obstacle colors
/// - Changes fog color/density
/// - Picks a gameplay music track
/// </summary>
[System.Serializable]
public class RunZone
{
    public string name;

    [Tooltip("Seconds since run start when this zone becomes active.")]
    public float startTime = 0f;

    [Header("Colors")]
    public Gradient tunnelGradient;
    public Gradient obstacleGradient;

    [Header("Fog")]
    public Color fogColor = Color.black;
    public float fogDensity = 0.008f;

    [Header("Music")]
    [Tooltip("Index into AudioManager gameplay tracks.")]
    public int musicTrackIndex = 0;

    [Header("Zone VFX + Intensity")]
    public ZoneVfxSettings vfxSettings = new ZoneVfxSettings();

    [System.Serializable]
    public class ZoneVfxSettings
    {
        [Tooltip("Optional VFX objects to toggle when this zone becomes active.")]
        public GameObject[] vfxObjects;
        [Range(0.25f, 2f)]
        [Tooltip("Multiplier for obstacle dark/light contrast.")]
        public float obstacleContrastMultiplier = 1f;
        [Range(0.25f, 3f)]
        [Tooltip("Multiplier for obstacle color cycle speed.")]
        public float obstacleColorCycleSpeedMultiplier = 1f;
    }
}

public class RunZoneManager : MonoBehaviour
{
    [Header("Zones (sorted by startTime ascending)")]
    [SerializeField] private RunZone[] zones;

    [Header("Mode")]
    [Tooltip("If enabled, only the first zone is used and no zone changes occur during a run.")]
    [SerializeField] private bool lockToSingleZone = true;

    [Header("References")]
    [SerializeField] private TunnelWallGenerator tunnelWalls;
    [SerializeField] private ObstacleRingGenerator obstacleRings;
    [SerializeField] private AudioManager audioManager;

    [Tooltip("Optional. If set, we won't override fog density directly, but you can still drive fogColor etc.")]
    [SerializeField] private TunnelAtmosphereController atmosphereController;

    private bool _runActive;
    private float _runTime;
    private int _currentZoneIndex = -1;
    private readonly HashSet<GameObject> _activeVfx = new HashSet<GameObject>();

    private void Awake()
    {
        if (tunnelWalls == null)
            tunnelWalls = FindFirstObjectByType<TunnelWallGenerator>();

        if (obstacleRings == null)
            obstacleRings = FindFirstObjectByType<ObstacleRingGenerator>();

        if (audioManager == null)
            audioManager = FindFirstObjectByType<AudioManager>();
    }

    private void Update()
    {
        if (!_runActive || zones == null || zones.Length == 0)
            return;

        if (lockToSingleZone)
        {
            if (_currentZoneIndex != 0)
                ApplyZone(0);
            return;
        }

        _runTime += Time.deltaTime;

        int newZoneIndex = GetZoneIndexForTime(_runTime);
        if (newZoneIndex != _currentZoneIndex)
        {
            _currentZoneIndex = newZoneIndex;
            ApplyZone(_currentZoneIndex);
        }
    }

    private int GetZoneIndexForTime(float time)
    {
        int bestIndex = 0;
        float bestStartTime = float.MinValue;

        for (int i = 0; i < zones.Length; i++)
        {
            if (zones[i] == null) continue;
            if (zones[i].startTime <= time && zones[i].startTime > bestStartTime)
            {
                bestIndex = i;
                bestStartTime = zones[i].startTime;
            }
        }

        return bestIndex;
    }

    public void ApplyZone(int index)
    {
        if (index < 0 || index >= zones.Length) return;

        _currentZoneIndex = index;
        var zone = zones[index];
        if (zone == null) return;

        if (tunnelWalls != null && zone.tunnelGradient != null)
            tunnelWalls.SetColorGradient(zone.tunnelGradient);

        var vfxSettings = zone.vfxSettings;
        ApplyZoneVfx(zone);

        if (obstacleRings != null)
        {
            float contrastMultiplier = vfxSettings != null ? vfxSettings.obstacleContrastMultiplier : 1f;
            float colorCycleMultiplier = vfxSettings != null ? vfxSettings.obstacleColorCycleSpeedMultiplier : 1f;
            if (zone.obstacleGradient != null)
            {
                //obstacleRings.SetColorStyle(zone.obstacleGradient, contrastMultiplier, colorCycleMultiplier);
            }
            else
            {
                //obstacleRings.SetColorStyle(contrastMultiplier, colorCycleMultiplier);
            }
        }

        // Fog
        //RenderSettings.fogColor = zone.fogColor;

        if (atmosphereController == null) // direct control
        {
            //RenderSettings.fogDensity = zone.fogDensity;
        }
        else
        {
            // If you want, you can add API to TunnelAtmosphereController
            // to adjust its base fog density / color instead of raw RenderSettings.
        }

        // Music
        if (audioManager != null)
        {
            audioManager.PlayGameplayTrackIndex(zone.musicTrackIndex);
            audioManager.SetMusicLevelForZone(_currentZoneIndex);
        }

        Debug.Log($"Zone changed to: {zone.name}");
    }

    private void ApplyZoneVfx(RunZone zone)
    {
        var nextActive = CollectZoneVfx(zone);

        foreach (var vfx in _activeVfx)
        {
            if (vfx != null && !nextActive.Contains(vfx))
                vfx.SetActive(false);
        }

        foreach (var vfx in nextActive)
        {
            if (vfx != null)
                vfx.SetActive(true);
        }

        _activeVfx.Clear();
        foreach (var vfx in nextActive)
        {
            if (vfx != null)
                _activeVfx.Add(vfx);
        }
    }

    private static HashSet<GameObject> CollectZoneVfx(RunZone zone)
    {
        var collected = new HashSet<GameObject>();
        if (zone == null || zone.vfxSettings == null || zone.vfxSettings.vfxObjects == null)
            return collected;

        foreach (var vfx in zone.vfxSettings.vfxObjects)
        {
            if (vfx != null)
                collected.Add(vfx);
        }

        return collected;
    }

    public void StartRun()
    {
        _runActive = true;

        if (lockToSingleZone && zones != null && zones.Length > 0)
        {
            ApplyZone(0);
            _currentZoneIndex = 0;
        }
    }

    // Called by GameManager when a new run starts
    public void OnResetRun()
    {
        _runTime = 0f;
        _currentZoneIndex = -1;

        if (zones != null && zones.Length > 0)
        {
            ApplyZone(0);
            _currentZoneIndex = 0;
        }
    }

    // Called by GameManager when run ends
    public void OnRunEnded()
    {
        _runActive = false;
        ApplyZoneVfx(null);
    }

    public string CurrentZoneName =>
    (_currentZoneIndex >= 0 && _currentZoneIndex < zones.Length)
        ? zones[_currentZoneIndex].name
        : "None";
}
