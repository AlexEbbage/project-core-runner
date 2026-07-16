using UnityEngine;

namespace CoreRacer.Services.Audio
{
    [DisallowMultipleComponent]
    public sealed class AudioRuntimeHost : MonoBehaviour
    {
        public AudioSource MusicSource { get; private set; }
        public AudioSource SfxSource { get; private set; }
        public bool IsBound { get; private set; }

        public void Initialize(AudioService audio)
        {
            if (audio == null)
            {
                Debug.LogError("[CoreRacer.Audio] Cannot initialize the runtime host without AudioService.", this);
                return;
            }

            var sources = GetComponents<AudioSource>();
            MusicSource = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();
            SfxSource = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();
            Configure(MusicSource, true);
            Configure(SfxSource, false);
            audio.Bind(MusicSource, SfxSource);
            IsBound = true;
        }

        private static void Configure(AudioSource source, bool loop)
        {
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
        }
    }
}
