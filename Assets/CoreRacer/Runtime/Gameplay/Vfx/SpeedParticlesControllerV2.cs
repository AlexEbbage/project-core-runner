using UnityEngine;

namespace CoreRacer.Gameplay.Vfx
{
    public sealed class SpeedParticlesControllerV2 : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particles;
        [SerializeField] private float minRate = 15f;
        [SerializeField] private float maxRate = 120f;
        [SerializeField] private float minSpeed = 1f;
        [SerializeField] private float maxSpeed = 8f;

        private ParticleSystem.EmissionModule _emission;
        private ParticleSystem.MainModule _main;

        private void Awake()
        {
            if (particles == null) particles = GetComponent<ParticleSystem>();
            if (particles == null) return;
            _emission = particles.emission;
            _main = particles.main;
        }

        public void SetIntensity(float normalizedIntensity)
        {
            if (particles == null) return;
            var t = Mathf.Clamp01(normalizedIntensity);
            _emission.rateOverTime = Mathf.Lerp(minRate, maxRate, t);
            _main.startSpeed = Mathf.Lerp(minSpeed, maxSpeed, t);
        }
    }
}
