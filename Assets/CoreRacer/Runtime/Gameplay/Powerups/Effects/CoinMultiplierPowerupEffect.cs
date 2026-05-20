namespace CoreRacer.Gameplay.Powerups.Effects
{
    public sealed class CoinMultiplierPowerupEffect : IPowerupEffect
    {
        public PowerupType Type => PowerupType.CoinMultiplier;
        public void Activate(PowerupContext context, PowerupTuning tuning) { context.CurrencyTracker?.SetCoinMultiplier(tuning.Strength); }
        public void Tick(PowerupContext context, float deltaTime) { }
        public void Deactivate(PowerupContext context) { context.CurrencyTracker?.SetCoinMultiplier(1f); }
    }
}
