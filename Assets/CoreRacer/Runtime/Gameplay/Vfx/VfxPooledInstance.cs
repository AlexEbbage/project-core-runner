using UnityEngine;

namespace CoreRacer.Gameplay.Vfx
{
    public sealed class VfxPooledInstance : MonoBehaviour
    {
        [SerializeField] private ParticleSystem[] particles;
        [SerializeField] private float fallbackLifetime = 2f;

        public float EstimatedLifetime
        {
            get
            {
                EnsureParticles();
                var maxLifetime = fallbackLifetime;
                for (int i = 0; i < particles.Length; i++)
                {
                    if (particles[i] == null) continue;
                    var main = particles[i].main;
                    maxLifetime = Mathf.Max(maxLifetime, main.duration + main.startLifetime.constantMax);
                }
                return maxLifetime;
            }
        }

        public void Play()
        {
            EnsureParticles();
            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i] == null) continue;
                particles[i].Clear(true);
                particles[i].Play(true);
            }
        }

        public void Stop()
        {
            EnsureParticles();
            for (int i = 0; i < particles.Length; i++)
                if (particles[i] != null)
                    particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void SetTint(Color tint)
        {
            EnsureParticles();
            var block = new MaterialPropertyBlock();
            block.SetColor("_Color", tint);
            for (var i = 0; i < particles.Length; i++)
            {
                if (particles[i] == null)
                    continue;
                var renderer = particles[i].GetComponent<ParticleSystemRenderer>();
                renderer?.SetPropertyBlock(block);
            }
        }

        private void EnsureParticles()
        {
            if (particles == null || particles.Length == 0)
                particles = GetComponentsInChildren<ParticleSystem>(true);
        }
    }
}
