using CoreRacer.Gameplay.Player;
using CoreRacer.Gameplay.Run;

namespace CoreRacer.Gameplay.Powerups
{
    public sealed class PowerupContext
    {
        public PlayerController Player;
        public PlayerHealth Health;
        public RunScoreTracker ScoreTracker;
        public RunCurrencyTracker CurrencyTracker;
    }
}
