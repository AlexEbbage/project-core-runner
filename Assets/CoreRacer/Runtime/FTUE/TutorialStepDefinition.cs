using System;

namespace CoreRacer.FTUE
{
    [Serializable]
    public sealed class TutorialStepDefinition
    {
        public string Id;
        public TutorialStepKind Kind = TutorialStepKind.Message;
        public string TitleKey;
        public string BodyKey;
        public string HighlightTargetId;
        public bool PauseGame;
        public bool RequiresExplicitContinue = true;
        public float MinimumDisplaySeconds = 0.5f;
    }
}
