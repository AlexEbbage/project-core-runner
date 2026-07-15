namespace CoreRacer.Gameplay.Powerups.Effects
{
    public sealed class MagnetPowerupEffect : IPowerupEffect
    {
        public PowerupType Type => PowerupType.Magnet;
        public void Activate(PowerupContext context, PowerupTuning tuning) { context.Magnet?.SetRadiusMultiplier(UnityEngine.Mathf.Max(1f, tuning.Strength)); }
        public void Tick(PowerupContext context, float deltaTime) { }
        public void Deactivate(PowerupContext context) { context.Magnet?.SetRadiusMultiplier(0f); }
    }
}
