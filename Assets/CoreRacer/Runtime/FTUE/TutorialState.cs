using System;

namespace CoreRacer.FTUE
{
    [Serializable]
    public sealed class TutorialState
    {
        public string TutorialId;
        public int CurrentStepIndex;
        public bool Started;
        public bool Completed;
        public string LastUpdatedUtcIso;
    }
}
