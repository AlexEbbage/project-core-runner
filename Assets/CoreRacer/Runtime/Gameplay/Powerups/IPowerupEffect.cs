namespace CoreRacer.Gameplay.Powerups
{
    public interface IPowerupEffect
    {
        PowerupType Type { get; }
        void Activate(PowerupContext context, PowerupTuning tuning);
        void Tick(PowerupContext context, float deltaTime);
        void Deactivate(PowerupContext context);
    }
}
