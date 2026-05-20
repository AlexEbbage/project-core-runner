using CoreRacer.Meta.Economy;

namespace CoreRacer.Meta.Profile
{
    public static class PlayerProfileDefaults
    {
        public static PlayerProfileState CreateNew()
        {
            var state = new PlayerProfileState();
            state.Wallet.Add(CurrencyType.Soft, 0);
            state.Wallet.Add(CurrencyType.Premium, 0);
            state.Inventory.Unlock(state.SelectedShipId);
            state.Inventory.Unlock(state.SelectedSkinId);
            state.Inventory.Unlock(state.SelectedTrailId);
            state.Inventory.Unlock(state.SelectedCoreFxId);
            return state;
        }
    }
}
