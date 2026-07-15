namespace CoreRacer.Meta.Progression
{
    public readonly struct ProgressionTaskSummary
    {
        public readonly int Total;
        public readonly int ReadyToClaim;
        public readonly int Claimed;

        public ProgressionTaskSummary(int total, int readyToClaim, int claimed)
        {
            Total = total;
            ReadyToClaim = readyToClaim;
            Claimed = claimed;
        }
    }
}
