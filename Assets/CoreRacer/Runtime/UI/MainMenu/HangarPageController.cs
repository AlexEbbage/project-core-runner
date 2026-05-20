using CoreRacer.Meta.Profile;
using CoreRacer.UI.Shared;
using UnityEngine;

namespace CoreRacer.UI.MainMenu
{
    public sealed class HangarPageController : UiView
    {
        private PlayerProfileService _profile;
        private void Awake() => CoreRacer.Bootstrap.GameServices.TryGet(out _profile);

        public void EquipShip(string shipId)
        {
            if (_profile == null || !_profile.State.Inventory.IsUnlocked(shipId)) return;
            _profile.State.SelectedShipId = shipId;
            _profile.Save();
        }
    }
}
