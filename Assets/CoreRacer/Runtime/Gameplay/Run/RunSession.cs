using System;

namespace CoreRacer.Gameplay.Run
{
    public sealed class RunSession
    {
        public string RunId;
        public string LevelId;
        public string ShipId;
        public float StartedAtSeconds;
        public int ContinuesUsed;
        public bool RewardsGranted;
        public bool DoubleRewardsGranted;

        public void Reset(string levelId, string shipId, float startedAtSeconds)
        {
            RunId = Guid.NewGuid().ToString("N");
            LevelId = levelId;
            ShipId = shipId;
            StartedAtSeconds = startedAtSeconds;
            ContinuesUsed = 0;
            RewardsGranted = false;
            DoubleRewardsGranted = false;
        }
    }
}
