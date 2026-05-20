using CoreRacer.Services.Settings;
using UnityEngine;

namespace CoreRacer.Services.Audio
{
    public sealed class AudioService
    {
        private readonly SettingsService _settings;
        private AudioSource _musicSource;
        private AudioSource _sfxSource;

        public float MusicVolume => _settings != null ? _settings.State.MusicVolume : (_musicSource != null ? _musicSource.volume : 1f);
        public float SfxVolume => _settings != null ? _settings.State.SfxVolume : (_sfxSource != null ? _sfxSource.volume : 1f);

        public AudioService(SettingsService settings = null)
        {
            _settings = settings;
        }

        public void Bind(AudioSource musicSource, AudioSource sfxSource)
        {
            _musicSource = musicSource;
            _sfxSource = sfxSource;
            RefreshVolumes();
        }

        public void RefreshVolumes()
        {
            if (_settings == null) return;
            if (_musicSource != null) _musicSource.volume = _settings.State.MusicVolume;
            if (_sfxSource != null) _sfxSource.volume = _settings.State.SfxVolume;
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (_musicSource == null || clip == null) return;
            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            if (_musicSource != null) _musicSource.Stop();
        }

        public void PlaySfx(AudioClip clip)
        {
            if (_sfxSource == null || clip == null) return;
            _sfxSource.PlayOneShot(clip);
        }
    }
}
