using UnityEngine;

namespace CoreRacer.Services.Audio
{
    [System.Serializable]
    public sealed class AudioEventDefinition
    {
        public AudioEventId Id;
        public AudioClip Clip;
        [Range(0f, 1f)] public float Volume = 1f;
        [Range(0.1f, 3f)] public float Pitch = 1f;
        public bool Loop;
        public bool UseMusicSource;
    }
}
