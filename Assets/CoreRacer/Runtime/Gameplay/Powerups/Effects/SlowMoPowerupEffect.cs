using UnityEngine;

namespace CoreRacer.Gameplay.Powerups.Effects
{
    public sealed class SlowMoPowerupEffect : IPowerupEffect
    {
        public PowerupType Type => PowerupType.SlowMo;
        public void Activate(PowerupContext context, PowerupTuning tuning) { UnityEngine.Time.timeScale = Mathf.Clamp(tuning.Strength, 0.2f, 1f); }
        public void Tick(PowerupContext context, float deltaTime) { }
        public void Deactivate(PowerupContext context) { UnityEngine.Time.timeScale = 1f; }
    }
}
