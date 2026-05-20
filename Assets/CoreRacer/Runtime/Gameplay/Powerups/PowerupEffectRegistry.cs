using System.Collections.Generic;
using CoreRacer.Gameplay.Powerups.Effects;

namespace CoreRacer.Gameplay.Powerups
{
    public sealed class PowerupEffectRegistry
    {
        private readonly Dictionary<PowerupType, IPowerupEffect> _effects = new Dictionary<PowerupType, IPowerupEffect>();

        public PowerupEffectRegistry()
        {
            Register(new ShieldPowerupEffect());
            Register(new MagnetPowerupEffect());
            Register(new ScoreMultiplierPowerupEffect());
            Register(new CoinMultiplierPowerupEffect());
            Register(new AutoPilotPowerupEffect());
            Register(new SlowMoPowerupEffect());
        }

        public void Register(IPowerupEffect effect)
        {
            if (effect != null)
                _effects[effect.Type] = effect;
        }

        public bool TryGet(PowerupType type, out IPowerupEffect effect)
        {
            return _effects.TryGetValue(type, out effect);
        }
    }
}
