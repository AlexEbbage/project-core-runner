using CoreRacer.Bootstrap;
using CoreRacer.Services.Settings;
using UnityEngine;
using UnityEngine.Audio;

namespace CoreRacer.Services.Audio
{
    public sealed class AudioMixerController : MonoBehaviour
    {
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private string masterVolumeParam = "MasterVolume";
        [SerializeField] private string musicVolumeParam = "MusicVolume";
        [SerializeField] private string sfxVolumeParam = "SfxVolume";

        private SettingsService _settings;

        private void Start()
        {
            GameServices.TryGet(out _settings);
            Apply();
        }

        public void Apply()
        {
            if (mixer == null || _settings == null)
                return;

            SetVolume(masterVolumeParam, _settings.State.MasterVolume);
            SetVolume(musicVolumeParam, _settings.State.MusicVolume);
            SetVolume(sfxVolumeParam, _settings.State.SfxVolume);
        }

        private void SetVolume(string parameter, float linear)
        {
            if (string.IsNullOrEmpty(parameter))
                return;
            var db = linear <= 0.0001f ? -80f : Mathf.Log10(Mathf.Clamp01(linear)) * 20f;
            mixer.SetFloat(parameter, db);
        }
    }
}
