using UnityEngine;

namespace CoreRacer.Gameplay.Powerups.Effects
{
    public sealed class SlowMoPowerupEffect : IPowerupEffect
    {
        public PowerupType Type => PowerupType.SlowMo;
        public void Activate(PowerupContext context, PowerupTuning tuning)
        {
            // Avoid owning global Time.timeScale; slowing the runner gives reaction time without breaking pause/continue.
            context.Player?.SetSpeedMultiplier(Mathf.Clamp(tuning.Strength, 0.25f, 1f));
        }
        public void Tick(PowerupContext context, float deltaTime) { }
        public void Deactivate(PowerupContext context) { context.Player?.SetSpeedMultiplier(1f); }
    }
}
