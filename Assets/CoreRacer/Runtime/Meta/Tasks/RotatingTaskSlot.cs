using System;

namespace CoreRacer.Meta.Tasks
{
    [Serializable]
    public sealed class RotatingTaskSlot
    {
        public string TaskId;
        public TaskCadence Cadence;
        public int ProgressStartValue;
        public bool Claimed;
        public string AssignedAtUtcIso;
        public string ExpiresAtUtcIso;
    }
}
