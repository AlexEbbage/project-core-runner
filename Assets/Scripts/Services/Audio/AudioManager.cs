using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] private AudioSource musicSourceA;
    [SerializeField] private AudioSource musicSourceB;
    [SerializeField] private AudioSource sfxSource;

    [Header("Mixer")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private string musicVolumeParam = "MusicVolume";
    [SerializeField] private string musicLowpassParam = "MusicLowpass";

    [Serializable]
    private struct GameplayTrack
    {
        public AudioClip clip;
        public int bpm;
    }

    [Header("Music Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private GameplayTrack[] gameplayTracks;

    [Header("Crossfade")]
    [SerializeField] private float musicCrossfadeTime = 1.0f;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip pickupSfx;
    [SerializeField] private AudioClip hitSfx;
    [SerializeField] private AudioClip buttonClickSfx;
    [SerializeField] private AudioClip speedUpSfx;
    [SerializeField] private AudioClip countdownTimerSfx;
    [Header("SFX Pitch Variance")]
    [SerializeField] private Vector2 pickupPitchRange = new Vector2(0.95f, 1.05f);

    internal void SetMusicVolume(float musicVolume)
    {
        _musicVolume = Mathf.Clamp01(musicVolume);

        if (musicSourceA != null)
        {
            musicSourceA.volume = _musicVolume; 
        }

        if (musicSourceB != null)
        {
            musicSourceB.volume = _musicVolume;
        }
    }

    internal void SetSfxVolume(float sfxVolume)
    {
        if (sfxSource == null) return;
        sfxSource.volume = sfxVolume;
    }

    [SerializeField] private AudioClip countdownGoSfx;

    // State
    private bool _isInMenu = true;
    private int _currentGameplayIndex = -1;
    private bool _useA = true; // which music source is "active"
    private float _musicVolume = 1f;
    private Coroutine _crossfadeRoutine;
    private Coroutine _lowpassRoutine;

    // For debug overlay
    public bool IsInMenu => _isInMenu;

    public string CurrentTrackName
    {
        get
        {
            if (_isInMenu)
                return menuMusic != null ? $"Menu: {menuMusic.name}" : "Menu: None";
            if (gameplayTracks == null || gameplayTracks.Length == 0)
                return "Gameplay: None";
            var track = gameplayTracks[Mathf.Clamp(_currentGameplayIndex, 0, gameplayTracks.Length - 1)];
            return track.clip != null ? track.clip.name : "Gameplay: None";
        }
    }

    public int CurrentTrackBPM
    {
        get
        {
            if (_isInMenu) return 95; // menu BPM from design
            if (gameplayTracks == null || gameplayTracks.Length == 0)
                return 0;
            int idx = Mathf.Clamp(_currentGameplayIndex, 0, gameplayTracks.Length - 1);
            return gameplayTracks[idx].bpm;
        }
    }

    private void Awake()
    {
        // Fallback: if only one music source in scene, duplicate logic
        if (musicSourceA == null || musicSourceB == null)
        {
            var sources = GetComponentsInChildren<AudioSource>();
            if (sources.Length >= 3)
            {
                musicSourceA = sources[0];
                musicSourceB = sources[1];
                sfxSource = sources[2];
            }
        }

        if (musicSourceA == null || musicSourceB == null)
        {
            Debug.LogWarning($"{nameof(AudioManager)} requires two music sources for crossfading. Falling back to single-source playback.", this);
        }

        if (sfxSource == null)
        {
            Debug.LogWarning($"{nameof(AudioManager)} is missing an SFX source.", this);
        }

        if (gameplayTracks != null)
        {
            for (int i = 0; i < gameplayTracks.Length; i++)
            {
                if (gameplayTracks[i].clip == null)
                {
                    Debug.LogWarning($"{nameof(AudioManager)} gameplay track at index {i} is missing a clip.", this);
                }
            }
        }

        _musicVolume = SettingsData.MusicVolume;

        if (musicSourceA != null)
        {
            musicSourceA.playOnAwake = false;
            if (musicSourceA.isPlaying)
                musicSourceA.Stop();
            musicSourceA.loop = false;
            musicSourceA.volume = _musicVolume;
        }

        if (musicSourceB != null)
        {
            musicSourceB.playOnAwake = false;
            if (musicSourceB.isPlaying)
                musicSourceB.Stop();
            musicSourceB.loop = false;
            musicSourceB.volume = 0f;
        }

        if(sfxSource != null)
        {
            sfxSource.playOnAwake = false;
            if (sfxSource.isPlaying)
                sfxSource.Stop();
            sfxSource.volume = SettingsData.SfxVolume;
        }
    }

    private AudioSource ActiveMusicSource => _useA ? musicSourceA : musicSourceB;
    private AudioSource InactiveMusicSource => _useA ? musicSourceB : musicSourceA;

    // ---------------- MUSIC PUBLIC API ----------------

    public void PlayMenuMusic()
    {
        _isInMenu = true;

        if (menuMusic == null) return;

        CrossfadeToClip(menuMusic, true);

        if (mainMixer == null) return;

        float db = -6f;
        mainMixer.SetFloat(musicVolumeParam, db);
    }

    public void PlayGameplayMusic()
    {
        _isInMenu = false;
        if (gameplayTracks == null || gameplayTracks.Length == 0) return;

        _currentGameplayIndex = 0;
        AudioClip clip = GetGameplayClip(_currentGameplayIndex);
        if (clip == null)
        {
            Debug.LogWarning($"{nameof(AudioManager)} gameplay track at index {_currentGameplayIndex} is missing a clip.", this);
            return;
        }
        CrossfadeToClip(clip, true);
    }

    public void PlayGameplayTrackIndex(int index)
    {
        _isInMenu = false;
        if (gameplayTracks == null || gameplayTracks.Length == 0) return;

        index = Mathf.Clamp(index, 0, gameplayTracks.Length - 1);
        _currentGameplayIndex = index;

        AudioClip clip = GetGameplayClip(_currentGameplayIndex);
        if (clip == null)
        {
            Debug.LogWarning($"{nameof(AudioManager)} gameplay track at index {_currentGameplayIndex} is missing a clip.", this);
            return;
        }
        CrossfadeToClip(clip, true);
    }

    private AudioClip GetGameplayClip(int index)
    {
        if (gameplayTracks == null || gameplayTracks.Length == 0)
            return null;

        int clampedIndex = Mathf.Clamp(index, 0, gameplayTracks.Length - 1);
        return gameplayTracks[clampedIndex].clip;
    }

    public void StopMusic()
    {
        if (musicSourceA != null) musicSourceA.Stop();
        if (musicSourceB != null) musicSourceB.Stop();
    }

    public void SetMusicLevelForZone(int zoneIndex)
    {
        if (mainMixer == null) return;

        float db = -6f;
        switch (zoneIndex)
        {
            case 0: db = -7f; break;
            case 1: db = -6f; break;
            case 2: db = -5f; break;
        }

        mainMixer.SetFloat(musicVolumeParam, db);
    }

    // Core crossfade logic
    private void CrossfadeToClip(AudioClip newClip, bool loop)
    {
        if (newClip == null) return;
        if (musicSourceA == null || musicSourceB == null)
        {
            PlayOnSingleSource(newClip, loop);
            return;
        }

        if (ActiveMusicSource != null && ActiveMusicSource.clip == newClip && ActiveMusicSource.isPlaying)
            return;

        if (_crossfadeRoutine != null)
            StopCoroutine(_crossfadeRoutine);

        _crossfadeRoutine = StartCoroutine(CrossfadeRoutine(newClip, loop));
    }

    private void PlayOnSingleSource(AudioClip newClip, bool loop)
    {
        if (_crossfadeRoutine != null)
        {
            StopCoroutine(_crossfadeRoutine);
            _crossfadeRoutine = null;
        }

        AudioSource singleSource = musicSourceA != null ? musicSourceA : musicSourceB;
        if (singleSource == null) return;

        if (singleSource.clip == newClip && singleSource.isPlaying)
            return;

        AudioSource otherSource = singleSource == musicSourceA ? musicSourceB : musicSourceA;
        if (otherSource != null)
        {
            otherSource.Stop();
            otherSource.volume = 0f;
        }

        singleSource.clip = newClip;
        singleSource.loop = loop;
        singleSource.volume = _musicVolume;
        singleSource.Play();

        _useA = singleSource == musicSourceA;
    }

    private System.Collections.IEnumerator CrossfadeRoutine(AudioClip newClip, bool loop)
    {
        AudioSource from = ActiveMusicSource;
        AudioSource to = InactiveMusicSource;

        if (to == null || from == null) yield break;

        if (from.clip == newClip && from.isPlaying)
        {
            _crossfadeRoutine = null;
            yield break;
        }

        // Setup the "to" source
        to.clip = newClip;
        to.loop = loop;
        to.volume = 0f;
        to.Play();

        float t = 0f;
        float duration = Mathf.Max(0.01f, musicCrossfadeTime);

        float fromStartVolume = from.isPlaying ? from.volume : 0f;
        float toStartVolume = to.volume;
        float targetVolume = _musicVolume;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = t / duration;

            float fromVol = Mathf.Lerp(fromStartVolume, 0f, lerp);
            float toVol = Mathf.Lerp(toStartVolume, targetVolume, lerp);

            from.volume = fromVol;
            to.volume = toVol;

            yield return null;
        }

        from.volume = 0f;
        from.Stop();

        to.volume = targetVolume;
        _useA = !_useA; // swap active source
        _crossfadeRoutine = null;
    }

    // ---------------- SFX PUBLIC API ----------------

    public void PlayPickup() => PlaySfx(pickupSfx, pickupPitchRange);
    public void PlayHit() => PlaySfx(hitSfx);
    public void PlayButtonClick() => PlaySfx(buttonClickSfx);
    public void PlaySpeedUp() => PlaySfx(speedUpSfx);
    public void PlayCountdownTimer() => PlaySfx(countdownTimerSfx);
    public void PlayCountdownGo() => PlaySfx(countdownGoSfx);

    public void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    private void PlaySfx(AudioClip clip, Vector2 pitchRange)
    {
        if (sfxSource == null || clip == null) return;

        float originalPitch = sfxSource.pitch;
        float minPitch = Mathf.Min(pitchRange.x, pitchRange.y);
        float maxPitch = Mathf.Max(pitchRange.x, pitchRange.y);
        if (!Mathf.Approximately(minPitch, 1f) || !Mathf.Approximately(maxPitch, 1f))
        {
            sfxSource.pitch = Mathf.Clamp(UnityEngine.Random.Range(minPitch, maxPitch), -3f, 3f);
        }

        sfxSource.PlayOneShot(clip);
        sfxSource.pitch = originalPitch;
    }

    // ---------------- LOWPASS / MUFFLE API ----------------

    [Header("Lowpass Settings")]
    [SerializeField] private float lowpassNormalCutoff = 22000f;
    [SerializeField] private float lowpassMuffledCutoff = 800f;
    [SerializeField] private float lowpassTransitionTime = 0.12f;
    [SerializeField] private float lowpassHoldTime = 0.25f;
    [SerializeField] private AudioMixerSnapshot lowpassNormalSnapshot;
    [SerializeField] private AudioMixerSnapshot lowpassMuffledSnapshot;

    /// <summary>
    /// Briefly muffles the music (death, big hit, slow-mo).
    /// </summary>
    public void TriggerDeathMuffle()
    {
        if (lowpassNormalSnapshot != null && lowpassMuffledSnapshot != null)
        {
            if (_lowpassRoutine != null)
                StopCoroutine(_lowpassRoutine);

            _lowpassRoutine = StartCoroutine(LowpassSnapshotPulseRoutine());
            return;
        }

        if (mainMixer == null || string.IsNullOrEmpty(musicLowpassParam))
            return;

        if (_lowpassRoutine != null)
            StopCoroutine(_lowpassRoutine);

        _lowpassRoutine = StartCoroutine(LowpassPulseRoutine());
    }

    private System.Collections.IEnumerator LowpassSnapshotPulseRoutine()
    {
        lowpassMuffledSnapshot.TransitionTo(lowpassTransitionTime);

        if (lowpassHoldTime > 0f)
            yield return new WaitForSeconds(lowpassHoldTime);

        lowpassNormalSnapshot.TransitionTo(lowpassTransitionTime);
        _lowpassRoutine = null;
    }

    private System.Collections.IEnumerator LowpassPulseRoutine()
    {
        // Fade down to muffled
        yield return StartCoroutine(SetLowpassCutoff(lowpassNormalCutoff, lowpassMuffledCutoff, lowpassTransitionTime));

        // Hold
        if (lowpassHoldTime > 0f)
            yield return new WaitForSeconds(lowpassHoldTime);

        // Fade back up
        yield return StartCoroutine(SetLowpassCutoff(lowpassMuffledCutoff, lowpassNormalCutoff, lowpassTransitionTime));

        _lowpassRoutine = null;
    }

    private System.Collections.IEnumerator SetLowpassCutoff(float from, float to, float time)
    {
        float t = 0f;
        float duration = Mathf.Max(0.01f, time);

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = t / duration;
            float cutoff = Mathf.Lerp(from, to, lerp);
            mainMixer.SetFloat(musicLowpassParam, cutoff);
            yield return null;
        }

        mainMixer.SetFloat(musicLowpassParam, to);
    }
}
