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
}

public class RunZoneManager : MonoBehaviour
{
    [Header("Zones (sorted by startTime ascending)")]
    [SerializeField] private RunZone[] zones;

    [Header("References")]
    [SerializeField] private TunnelWallGenerator tunnelWalls;
    [SerializeField] private ObstacleRingGenerator obstacleRings;
    [SerializeField] private AudioManager audioManager;

    [Tooltip("Optional. If set, we won't override fog density directly, but you can still drive fogColor etc.")]
    [SerializeField] private TunnelAtmosphereController atmosphereController;

    private bool _runActive;
    private float _runTime;
    private int _currentZoneIndex = -1;

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

        if (tunnelWalls != null && zone.tunnelGradient != null)
            tunnelWalls.SetColorGradient(zone.tunnelGradient);

        if (obstacleRings != null && zone.obstacleGradient != null)
            obstacleRings.SetColorGradient(zone.obstacleGradient);

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

    public void StartRun()
    {
        _runActive = true;
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
    }

    public string CurrentZoneName =>
    (_currentZoneIndex >= 0 && _currentZoneIndex < zones.Length)
        ? zones[_currentZoneIndex].name
        : "None";
}
