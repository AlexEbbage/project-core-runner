using CoreRacer.Gameplay.Player;
using CoreRacer.Gameplay.Run;
using UnityEngine;

namespace CoreRacer.Gameplay.Powerups
{
    public sealed class PowerupContextBuilder : MonoBehaviour
    {
        public PlayerController Player;
        public PlayerHealth Health;
        public RunScoreTracker ScoreTracker;
        public RunCurrencyTracker CurrencyTracker;

        public PowerupContext Build()
        {
            return new PowerupContext
            {
                Player = Player,
                Health = Health,
                ScoreTracker = ScoreTracker,
                CurrencyTracker = CurrencyTracker
            };
        }
    }
}
