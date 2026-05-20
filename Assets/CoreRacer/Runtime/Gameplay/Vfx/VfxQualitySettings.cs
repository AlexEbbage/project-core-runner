using UnityEngine;

namespace CoreRacer.Gameplay.Vfx
{
    [CreateAssetMenu(menuName = "Core Racer/VFX/VFX Quality Settings")]
    public sealed class VfxQualitySettings : ScriptableObject
    {
        public bool LowQualityMode;
        public int MaxActivePooledEffects = 32;
        public float ParticleIntensityMultiplier = 1f;
        public bool EnableScreenFlashes = true;
        public bool EnableCameraShake = true;
    }
}
