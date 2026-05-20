using CoreRacer.Meta.Profile;
using CoreRacer.Meta.Ships;
using UnityEngine;

namespace CoreRacer.Gameplay.Player
{
    public sealed class PlayerCosmeticsController : MonoBehaviour
    {
        [SerializeField] private Transform shipRoot;
        [SerializeField] private ShipDatabase shipDatabase;
        private GameObject _currentShip;

        public void Apply(PlayerProfileState profile)
        {
            if (profile == null || shipDatabase == null || shipRoot == null)
                return;

            var definition = shipDatabase.GetShip(profile.SelectedShipId);
            if (definition == null || definition.Prefab == null)
                return;

            if (_currentShip != null)
                Destroy(_currentShip);

            _currentShip = Instantiate(definition.Prefab, shipRoot);
        }
    }
}
