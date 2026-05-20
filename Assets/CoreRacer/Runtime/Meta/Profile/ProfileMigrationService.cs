namespace CoreRacer.Meta.Profile
{
    public sealed class ProfileMigrationService
    {
        public PlayerProfileState Migrate(PlayerProfileState state)
        {
            if (state == null)
                return PlayerProfileDefaults.CreateNew();

            if (state.Version < 2)
                state.Version = 2;

            if (state.Wallet == null) state.Wallet = new CoreRacer.Meta.Economy.CurrencyWallet();
            if (state.Inventory == null) state.Inventory = new CoreRacer.Meta.Inventory.UnlockableItemState();
            if (state.Level <= 0) state.Level = 1;
            if (string.IsNullOrWhiteSpace(state.SelectedShipId)) state.SelectedShipId = "starter_runner";
            if (string.IsNullOrWhiteSpace(state.SelectedSkinId)) state.SelectedSkinId = "classic_white";
            if (string.IsNullOrWhiteSpace(state.SelectedTrailId)) state.SelectedTrailId = "pulse_wake";
            if (string.IsNullOrWhiteSpace(state.SelectedCoreFxId)) state.SelectedCoreFxId = "starter_glow";

            state.Inventory.Unlock(state.SelectedShipId);
            state.Inventory.Unlock(state.SelectedSkinId);
            state.Inventory.Unlock(state.SelectedTrailId);
            state.Inventory.Unlock(state.SelectedCoreFxId);
            return state;
        }
    }
}
