namespace CoreRacer.Gameplay.Run
{
    public sealed class RunSession
    {
        public string LevelId;
        public string ShipId;
        public float StartedAtSeconds;
        public int ContinuesUsed;
        public bool RewardsGranted;

        public void Reset(string levelId, string shipId, float startedAtSeconds)
        {
            LevelId = levelId;
            ShipId = shipId;
            StartedAtSeconds = startedAtSeconds;
            ContinuesUsed = 0;
            RewardsGranted = false;
        }
    }
}
