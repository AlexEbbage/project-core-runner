using System.Collections;
using UnityEngine;

namespace CoreRacer.Services.Audio
{
    public sealed class AudioDuckHelper : MonoBehaviour
    {
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private float duckVolume = 0.35f;
        [SerializeField] private float fadeSeconds = 0.15f;
        private Coroutine _routine;
        private float _baseVolume = 1f;

        private void Awake()
        {
            if (musicSource != null)
                _baseVolume = musicSource.volume;
        }

        public void Duck(float duration)
        {
            if (musicSource == null)
                return;

            if (_routine != null)
                StopCoroutine(_routine);
            _routine = StartCoroutine(DuckRoutine(duration));
        }

        private IEnumerator DuckRoutine(float duration)
        {
            yield return FadeTo(duckVolume, fadeSeconds);
            yield return new WaitForSecondsRealtime(Mathf.Max(0f, duration));
            yield return FadeTo(_baseVolume, fadeSeconds);
            _routine = null;
        }

        private IEnumerator FadeTo(float target, float seconds)
        {
            var start = musicSource.volume;
            var elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += UnityEngine.Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(start, target, seconds <= 0f ? 1f : elapsed / seconds);
                yield return null;
            }
            musicSource.volume = target;
        }
    }
}
