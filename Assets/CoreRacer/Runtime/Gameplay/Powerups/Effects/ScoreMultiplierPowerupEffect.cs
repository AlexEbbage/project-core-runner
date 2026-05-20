namespace CoreRacer.Gameplay.Powerups.Effects
{
    public sealed class ScoreMultiplierPowerupEffect : IPowerupEffect
    {
        public PowerupType Type => PowerupType.ScoreMultiplier;
        public void Activate(PowerupContext context, PowerupTuning tuning) { context.ScoreTracker?.SetScorePowerupMultiplier(tuning.Strength); }
        public void Tick(PowerupContext context, float deltaTime) { }
        public void Deactivate(PowerupContext context) { context.ScoreTracker?.SetScorePowerupMultiplier(1f); }
    }
}
