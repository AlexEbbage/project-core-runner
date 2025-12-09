using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows current music + zone information on screen.
/// Activate only in Development Build or via toggle.
/// </summary>
public class MusicDebugOverlay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Text overlayText;
    [SerializeField] private RunZoneManager zoneManager;
    [SerializeField] private AudioManager audioManager;

    [Header("Settings")]
    [SerializeField] private bool showInReleaseBuild = false;

    private bool _visible = true;

    private void Awake()
    {
        if (!showInReleaseBuild && !Debug.isDebugBuild)
        {
            gameObject.SetActive(false);
            return;
        }

        if (overlayText == null)
        {
            Debug.LogError("MusicDebugOverlay: overlayText is null!");
            enabled = false;
            return;
        }

        if (zoneManager == null) zoneManager = FindFirstObjectByType<RunZoneManager>();
        if (audioManager == null) audioManager = FindFirstObjectByType<AudioManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3)) // toggle visibility
        {
            _visible = !_visible;
            overlayText.enabled = _visible;
        }

        if (!_visible) return;

        string zoneName = zoneManager != null
            ? zoneManager.CurrentZoneName
            : "Unknown";

        string trackName = audioManager != null
            ? audioManager.CurrentTrackName
            : "None";

        int bpm = audioManager != null
            ? audioManager.CurrentTrackBPM
            : 0;

        overlayText.text =
            $"<b>[Music Debug]</b>\n" +
            $"Zone: <color=#00D0FF>{zoneName}</color>\n" +
            $"Track: <color=#FFD000>{trackName}</color>\n" +
            $"BPM: <color=#FF00AA>{bpm}</color>";
    }
}
