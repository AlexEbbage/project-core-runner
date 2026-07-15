using System;
using System.Collections.Generic;
using CoreRacer.Meta.Economy;
using CoreRacer.Meta.Inventory;
using CoreRacer.Meta.Progression;

namespace CoreRacer.Meta.Profile
{
    public sealed class ProfileMigrationService
    {
        public PlayerProfileState Migrate(PlayerProfileState state)
        {
            if (state == null)
                state = PlayerProfileDefaults.CreateNew();

            state.Version = Math.Max(2, state.Version);
            state.Level = Math.Max(1, state.Level);
            state.Experience = Math.Max(0, state.Experience);
            state.BestScore = Math.Max(0, state.BestScore);
            state.BestDistance = Math.Max(0f, state.BestDistance);
            state.TotalRuns = Math.Max(0, state.TotalRuns);
            state.TotalCoinsCollected = Math.Max(0, state.TotalCoinsCollected);
            state.TotalPowerupsCollected = Math.Max(0, state.TotalPowerupsCollected);
            state.SelectedLevelIndex = Math.Max(0, state.SelectedLevelIndex);
            state.DailyLoginStreak = Math.Max(0, state.DailyLoginStreak);

            state.Wallet = state.Wallet ?? new CurrencyWallet();
            state.Wallet.Soft = Math.Max(0, state.Wallet.Soft);
            state.Wallet.Premium = Math.Max(0, state.Wallet.Premium);
            state.Inventory = state.Inventory ?? new UnlockableItemState();
            state.Inventory.UnlockedIds = state.Inventory.UnlockedIds ?? new List<string>();
            state.ShipUpgradeLevels = state.ShipUpgradeLevels ?? new List<SerializableIntById>();
            state.PowerupUpgradeLevels = state.PowerupUpgradeLevels ?? new List<SerializableIntById>();
            state.AchievementProgress = state.AchievementProgress ?? new List<SerializableIntById>();
            state.ClaimedAchievements = state.ClaimedAchievements ?? new List<SerializableBoolById>();
            state.TaskProgress = state.TaskProgress ?? new List<SerializableIntById>();
            state.ClaimedTasks = state.ClaimedTasks ?? new List<SerializableBoolById>();

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
