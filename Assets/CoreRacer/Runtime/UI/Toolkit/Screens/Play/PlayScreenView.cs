using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public sealed class PlayScreenView
    {
        public PlayScreenView(VisualElement root)
        {
            Root = root;
            PreviousButton = root.Require<Button>("PreviousZoneButton");
            NextButton = root.Require<Button>("NextZoneButton");
            StartButton = root.Require<Button>("PlayButton");
            LevelSurface = root.Require<VisualElement>("SelectedRunCard");
            LevelTitle = root.Require<Label>("SelectedLevelTitle");
            LevelDescription = root.Require<Label>("SelectedLevelDescription");
            LevelStatus = root.Require<Label>("SelectedLevelStatus");
            HighScore = root.Require<Label>("SelectedLevelHighScore");
            StarOne = root.Require<Label>("LevelStarOne");
            StarTwo = root.Require<Label>("LevelStarTwo");
            StarThree = root.Require<Label>("LevelStarThree");
            RewardOne = root.Require<VisualElement>("LevelRewardOne");
            RewardTwo = root.Require<VisualElement>("LevelRewardTwo");
            RewardThree = root.Require<VisualElement>("LevelRewardThree");
            RewardOneState = root.Require<Label>("LevelRewardOneState");
            RewardTwoState = root.Require<Label>("LevelRewardTwoState");
            RewardThreeState = root.Require<Label>("LevelRewardThreeState");
            BoosterSummary = root.Require<Label>("BoosterSummary");
            BoosterList = root.Require<VisualElement>("BoosterList");
        }

        public VisualElement Root { get; }
        public Button PreviousButton { get; }
        public Button NextButton { get; }
        public Button StartButton { get; }
        public VisualElement LevelSurface { get; }
        public Label LevelTitle { get; }
        public Label LevelDescription { get; }
        public Label LevelStatus { get; }
        public Label HighScore { get; }
        public Label StarOne { get; }
        public Label StarTwo { get; }
        public Label StarThree { get; }
        public VisualElement RewardOne { get; }
        public VisualElement RewardTwo { get; }
        public VisualElement RewardThree { get; }
        public Label RewardOneState { get; }
        public Label RewardTwoState { get; }
        public Label RewardThreeState { get; }
        public Label BoosterSummary { get; }
        public VisualElement BoosterList { get; }

        public void SetStars(int stars)
        {
            StarOne.EnableInClassList(UiClassNames.Earned, stars >= 1);
            StarTwo.EnableInClassList(UiClassNames.Earned, stars >= 2);
            StarThree.EnableInClassList(UiClassNames.Earned, stars >= 3);
        }

        public void SetReward(int index, bool earned, bool next, string state)
        {
            var element = index == 1 ? RewardOne : index == 2 ? RewardTwo : RewardThree;
            var label = index == 1 ? RewardOneState : index == 2 ? RewardTwoState : RewardThreeState;
            element.EnableInClassList(UiClassNames.Claimed, earned);
            element.EnableInClassList(UiClassNames.Attention, next);
            element.EnableInClassList(UiClassNames.Locked, !earned && !next);
            label.text = state ?? string.Empty;
        }
    }
}
