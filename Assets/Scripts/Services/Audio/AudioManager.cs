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

    [Header("Music Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip[] gameplayMusicTracks;

    [Header("Gameplay Track BPMs (for debug)")]
    [SerializeField] private int[] gameplayTrackBPMs = { 118, 128, 142 };

    [Header("Crossfade")]
    [SerializeField] private float musicCrossfadeTime = 1.0f;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip pickupSfx;
    [SerializeField] private AudioClip hitSfx;
    [SerializeField] private AudioClip buttonClickSfx;
    [SerializeField] private AudioClip speedUpSfx;

    // State
    private bool _isInMenu = true;
    private int _currentGameplayIndex = -1;
    private bool _useA = true; // which music source is "active"
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
            if (gameplayMusicTracks == null || gameplayMusicTracks.Length == 0)
                return "Gameplay: None";
            var clip = gameplayMusicTracks[Mathf.Clamp(_currentGameplayIndex, 0, gameplayMusicTracks.Length - 1)];
            return clip != null ? clip.name : "Gameplay: None";
        }
    }

    public int CurrentTrackBPM
    {
        get
        {
            if (_isInMenu) return 95; // menu BPM from design
            if (gameplayTrackBPMs == null || gameplayTrackBPMs.Length == 0)
                return 0;
            int idx = Mathf.Clamp(_currentGameplayIndex, 0, gameplayTrackBPMs.Length - 1);
            return gameplayTrackBPMs[idx];
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

        if (musicSourceA != null)
        {
            musicSourceA.loop = false;
            musicSourceA.volume = 1f;
        }

        if (musicSourceB != null)
        {
            musicSourceB.loop = false;
            musicSourceB.volume = 0f;
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
    }

    public void PlayGameplayMusic()
    {
        _isInMenu = false;
        if (gameplayMusicTracks == null || gameplayMusicTracks.Length == 0) return;

        _currentGameplayIndex = 0;
        AudioClip clip = gameplayMusicTracks[0];
        CrossfadeToClip(clip, true);
    }

    public void PlayGameplayTrackIndex(int index)
    {
        _isInMenu = false;
        if (gameplayMusicTracks == null || gameplayMusicTracks.Length == 0) return;

        index = Mathf.Clamp(index, 0, gameplayMusicTracks.Length - 1);
        _currentGameplayIndex = index;

        AudioClip clip = gameplayMusicTracks[index];
        CrossfadeToClip(clip, true);
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

        mainMixer.SetFloat("MusicVolume", db);
    }

    // Core crossfade logic
    private void CrossfadeToClip(AudioClip newClip, bool loop)
    {
        if (newClip == null) return;
        if (musicSourceA == null || musicSourceB == null) return;

        if (_crossfadeRoutine != null)
            StopCoroutine(_crossfadeRoutine);

        _crossfadeRoutine = StartCoroutine(CrossfadeRoutine(newClip, loop));
    }

    private System.Collections.IEnumerator CrossfadeRoutine(AudioClip newClip, bool loop)
    {
        AudioSource from = ActiveMusicSource;
        AudioSource to = InactiveMusicSource;

        if (to == null || from == null) yield break;

        // Setup the "to" source
        to.clip = newClip;
        to.loop = loop;
        to.volume = 0f;
        to.Play();

        float t = 0f;
        float duration = Mathf.Max(0.01f, musicCrossfadeTime);

        float fromStartVolume = from.isPlaying ? from.volume : 0f;
        float toStartVolume = to.volume;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = t / duration;

            float fromVol = Mathf.Lerp(fromStartVolume, 0f, lerp);
            float toVol = Mathf.Lerp(toStartVolume, 1f, lerp);

            from.volume = fromVol;
            to.volume = toVol;

            yield return null;
        }

        from.volume = 0f;
        from.Stop();

        to.volume = 1f;
        _useA = !_useA; // swap active source
        _crossfadeRoutine = null;
    }

    // ---------------- SFX PUBLIC API ----------------

    public void PlayPickup()   => PlaySfx(pickupSfx);
    public void PlayHit()      => PlaySfx(hitSfx);
    public void PlayButtonClick() => PlaySfx(buttonClickSfx);
    public void PlaySpeedUp()  => PlaySfx(speedUpSfx);

    public void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    // ---------------- LOWPASS / MUFFLE API ----------------

    [Header("Lowpass Settings")]
    [SerializeField] private float lowpassNormalCutoff = 22000f;
    [SerializeField] private float lowpassMuffledCutoff = 800f;
    [SerializeField] private float lowpassTransitionTime = 0.12f;
    [SerializeField] private float lowpassHoldTime = 0.25f;

    /// <summary>
    /// Briefly muffles the music (death, big hit, slow-mo).
    /// </summary>
    public void TriggerDeathMuffle()
    {
        if (mainMixer == null || string.IsNullOrEmpty(musicLowpassParam))
            return;

        if (_lowpassRoutine != null)
            StopCoroutine(_lowpassRoutine);

        _lowpassRoutine = StartCoroutine(LowpassPulseRoutine());
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
