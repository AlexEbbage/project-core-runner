namespace CoreRacer.Gameplay.Powerups.Effects
{
    public sealed class AutoPilotPowerupEffect : IPowerupEffect
    {
        public PowerupType Type => PowerupType.AutoPilot;
        public void Activate(PowerupContext context, PowerupTuning tuning) { context.Player?.SetAutoPilot(true, 0f); }
        public void Tick(PowerupContext context, float deltaTime) { }
        public void Deactivate(PowerupContext context) { context.Player?.SetAutoPilot(false); }
    }
}
