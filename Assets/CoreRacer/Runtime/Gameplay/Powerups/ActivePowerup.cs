namespace CoreRacer.Gameplay.Powerups
{
    public sealed class ActivePowerup
    {
        public PowerupType Type;
        public IPowerupEffect Effect;
        public PowerupTuning Tuning;
        public float RemainingSeconds;

        public ActivePowerup(PowerupType type, IPowerupEffect effect, PowerupTuning tuning)
        {
            Type = type;
            Effect = effect;
            Tuning = tuning;
            RemainingSeconds = tuning.Duration;
        }

        public void Refresh(PowerupTuning tuning)
        {
            Tuning = tuning;
            RemainingSeconds = tuning.Duration;
        }
    }
}
