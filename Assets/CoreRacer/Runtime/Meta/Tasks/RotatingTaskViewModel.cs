using System;
using System.Collections.Generic;
using CoreRacer.Meta.Economy;

namespace CoreRacer.Meta.Tasks
{
    public sealed class RotatingTaskViewModel
    {
        public string Id;
        public string DisplayName;
        public string Description;
        public TaskCadence Cadence;
        public RotatingTaskStatus Status;
        public int Progress;
        public int Target;
        public DateTimeOffset ExpiresAtUtc;
        public List<RewardGrant> Rewards;
    }
}
