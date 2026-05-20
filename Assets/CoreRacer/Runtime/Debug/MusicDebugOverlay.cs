using CoreRacer.Bootstrap;
using CoreRacer.Services.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.Debugging
{
    /// <summary>
    /// Optional debug overlay for validating the audio service and scene music wiring.
    /// </summary>
    public sealed class MusicDebugOverlay : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField] private bool showInEditorOnly = true;
        private AudioService _audio;

        private void Awake()
        {
            GameServices.TryGet(out _audio);
        }

        private void Update()
        {
            if (showInEditorOnly && !Application.isEditor)
            {
                if (label != null) label.enabled = false;
                return;
            }

            if (label == null)
                return;

            label.enabled = true;
            label.text = _audio == null
                ? "Audio: service not registered"
                : $"Audio: music={_audio.MusicVolume:0.00} sfx={_audio.SfxVolume:0.00}";
        }
    }
}
