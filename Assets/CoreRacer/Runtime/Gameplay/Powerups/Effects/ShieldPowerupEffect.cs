namespace CoreRacer.Gameplay.Powerups.Effects
{
    public sealed class ShieldPowerupEffect : IPowerupEffect
    {
        public PowerupType Type => PowerupType.Shield;
        public void Activate(PowerupContext context, PowerupTuning tuning) { if (context.Health != null) context.Health.Revive(tuning.Duration); }
        public void Tick(PowerupContext context, float deltaTime) { }
        public void Deactivate(PowerupContext context) { }
    }
}
