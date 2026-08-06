using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public sealed class GameplayHudView
    {
        public GameplayHudView(VisualElement root)
        {
            Root = root;
            Distance = root.Require<Label>("HudDistance");
            Score = root.Require<Label>("HudScore");
            Coins = root.Require<Label>("HudCoins");
            Health = root.Require<Label>("HudHealth");
            Zone = root.Require<Label>("HudZone");
            Progress = root.Require<ProgressBar>("HudProgress");
            Powerups = root.Require<VisualElement>("PowerupStrip");
            Pause = root.Require<Button>("PauseButton");
        }

        public VisualElement Root { get; }
        public Label Distance { get; }
        public Label Score { get; }
        public Label Coins { get; }
        public Label Health { get; }
        public Label Zone { get; }
        public ProgressBar Progress { get; }
        public VisualElement Powerups { get; }
        public Button Pause { get; }
    }
}
