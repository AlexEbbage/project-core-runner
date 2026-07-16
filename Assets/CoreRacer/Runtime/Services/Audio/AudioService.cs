using CoreRacer.Services.Settings;
using UnityEngine;

namespace CoreRacer.Services.Audio
{
    public sealed class AudioService
    {
        private readonly SettingsService _settings;
        private AudioEventLibrary _library;
        private AudioSource _musicSource;
        private AudioSource _sfxSource;
        private float _musicEventVolume = 1f;

        public float MusicVolume => _settings != null ? _settings.State.MusicVolume : (_musicSource != null ? _musicSource.volume : 1f);
        public float SfxVolume => _settings != null ? _settings.State.SfxVolume : (_sfxSource != null ? _sfxSource.volume : 1f);
        public bool IsBound => _musicSource != null && _sfxSource != null;
        public AudioEventId? LastPlayedEventId { get; private set; }
        public int PlayedEventCount { get; private set; }

        public AudioService(SettingsService settings = null, AudioEventLibrary library = null)
        {
            _settings = settings;
            _library = library;
        }

        public void ConfigureLibrary(AudioEventLibrary library) => _library = library;

        public void Bind(AudioSource musicSource, AudioSource sfxSource)
        {
            _musicSource = musicSource;
            _sfxSource = sfxSource;
            RefreshVolumes();
        }

        public void RefreshVolumes()
        {
            if (_settings == null) return;
            if (_musicSource != null) _musicSource.volume = _settings.State.MusicVolume * _musicEventVolume;
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

        public bool PlayEvent(AudioEventId id)
        {
            if (_library == null || !_library.TryGet(id, out var definition) || definition == null || definition.Clip == null)
                return false;

            var source = definition.UseMusicSource ? _musicSource : _sfxSource;
            if (source == null)
                return false;

            if (definition.UseMusicSource)
            {
                _musicEventVolume = Mathf.Clamp01(definition.Volume);
                source.volume = (_settings != null ? _settings.State.MusicVolume : 1f) * _musicEventVolume;
                source.pitch = Mathf.Clamp(definition.Pitch, 0.1f, 3f);
                source.loop = definition.Loop;
                if (source.clip != definition.Clip || !source.isPlaying)
                {
                    source.clip = definition.Clip;
                    source.Play();
                }
            }
            else
            {
                source.pitch = Mathf.Clamp(definition.Pitch, 0.1f, 3f);
                source.PlayOneShot(definition.Clip, Mathf.Clamp01(definition.Volume));
            }

            LastPlayedEventId = id;
            PlayedEventCount++;
            return true;
        }
    }
}
