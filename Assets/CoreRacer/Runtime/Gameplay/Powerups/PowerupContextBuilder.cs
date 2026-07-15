using CoreRacer.Gameplay.Pickups;
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
        public PickupMagnetController Magnet;
        public AutoPilotSteeringController AutoPilotSteering;

        public PowerupContext Build()
        {
            if (Player != null)
            {
                if (Magnet == null) Magnet = Player.GetComponent<PickupMagnetController>();
                if (AutoPilotSteering == null) AutoPilotSteering = Player.GetComponent<AutoPilotSteeringController>();
            }

            return new PowerupContext
            {
                Player = Player,
                Health = Health,
                ScoreTracker = ScoreTracker,
                CurrencyTracker = CurrencyTracker,
                Magnet = Magnet,
                AutoPilotSteering = AutoPilotSteering
            };
        }
    }
}
