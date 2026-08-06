using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public sealed class RunOverlayView
    {
        public RunOverlayView(VisualElement root)
        {
            PauseRoot = root.Require<VisualElement>("PauseOverlay");
            ResumeButton = root.Require<Button>("ResumeButton");
            PauseHomeButton = root.Require<Button>("PauseHomeButton");
            GameOverRoot = root.Require<VisualElement>("GameOverPopup");
            GameOverTitle = root.Require<Label>("GameOverTitle");
            GameOverMessage = root.Require<Label>("GameOverMessage");
            ResultScore = root.Require<Label>("ResultScore");
            ResultCoins = root.Require<Label>("ResultCoins");
            ResultXp = root.Require<Label>("ResultXp");
            ResultPremium = root.Require<Label>("ResultPremium");
            ContinueActions = root.Require<VisualElement>("ContinueActions");
            FinalActions = root.Require<VisualElement>("FinalActions");
            ContinueButton = root.Require<Button>("ContinueRunButton");
            EndRunButton = root.Require<Button>("EndRunButton");
            DoubleRewardsButton = root.Require<Button>("DoubleRewardsButton");
            RetryButton = root.Require<Button>("RetryButton");
            HomeButton = root.Require<Button>("HomeButton");
            TutorialRoot = root.Require<VisualElement>("TutorialOverlay");
            TutorialTitle = root.Require<Label>("TutorialTitle");
            TutorialBody = root.Require<Label>("TutorialBody");
            TutorialContinue = root.Require<Button>("TutorialContinueButton");
        }

        public VisualElement PauseRoot { get; }
        public Button ResumeButton { get; }
        public Button PauseHomeButton { get; }
        public VisualElement GameOverRoot { get; }
        public Label GameOverTitle { get; }
        public Label GameOverMessage { get; }
        public Label ResultScore { get; }
        public Label ResultCoins { get; }
        public Label ResultXp { get; }
        public Label ResultPremium { get; }
        public VisualElement ContinueActions { get; }
        public VisualElement FinalActions { get; }
        public Button ContinueButton { get; }
        public Button EndRunButton { get; }
        public Button DoubleRewardsButton { get; }
        public Button RetryButton { get; }
        public Button HomeButton { get; }
        public VisualElement TutorialRoot { get; }
        public Label TutorialTitle { get; }
        public Label TutorialBody { get; }
        public Button TutorialContinue { get; }
    }
}
