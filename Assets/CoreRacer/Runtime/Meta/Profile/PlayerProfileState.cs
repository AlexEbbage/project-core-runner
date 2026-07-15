using System;
using System.Collections.Generic;
using CoreRacer.Meta.Economy;
using CoreRacer.Meta.Inventory;
using CoreRacer.Meta.Progression;

namespace CoreRacer.Meta.Profile
{
    [Serializable]
    public sealed class PlayerProfileState
    {
        public int Version = 3;
        public int Level = 1;
        public int Experience;
        public int BestScore;
        public float BestDistance;
        public int TotalRuns;
        public int TotalCoinsCollected;
        public int TotalPowerupsCollected;

        public CurrencyWallet Wallet = new CurrencyWallet();
        public UnlockableItemState Inventory = new UnlockableItemState();

        public string SelectedShipId = "starter_runner";
        public string SelectedSkinId = "classic_white";
        public string SelectedTrailId = "pulse_wake";
        public string SelectedCoreFxId = "starter_glow";
        public int SelectedLevelIndex;
        public List<string> EquippedBoosterIds = new List<string>();

        public List<SerializableIntById> ShipUpgradeLevels = new List<SerializableIntById>();
        public List<SerializableIntById> PowerupUpgradeLevels = new List<SerializableIntById>();
        public List<SerializableIntById> AchievementProgress = new List<SerializableIntById>();
        public List<SerializableBoolById> ClaimedAchievements = new List<SerializableBoolById>();
        public List<SerializableIntById> TaskProgress = new List<SerializableIntById>();
        public List<SerializableBoolById> ClaimedTasks = new List<SerializableBoolById>();

        public string LastDailyRewardDateUtc;
        public int DailyLoginStreak;
    }
}
