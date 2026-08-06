using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public sealed class ProgressionScreenView
    {
        public ProgressionScreenView(VisualElement root)
        {
            Root = root;
            Summary = root.Require<Label>("ProgressionSummary");
            LevelValue = root.Require<Label>("ProgressionLevelValue");
            XpValue = root.Require<Label>("ProgressionXpValue");
            XpBar = root.Require<ProgressBar>("ProgressionXpBar");
            StarsValue = root.Require<Label>("ProgressionStarsValue");
            DailyTab = root.Require<Button>("DailyTab");
            TasksTab = root.Require<Button>("TasksTab");
            AchievementsTab = root.Require<Button>("AchievementsTab");
            DailyPanel = root.Require<VisualElement>("DailyPanel");
            TasksPanel = root.Require<VisualElement>("TasksPanel");
            AchievementsPanel = root.Require<VisualElement>("AchievementsPanel");
            DailyList = root.Require<VisualElement>("DailyList");
            TaskList = root.Require<VisualElement>("TaskList");
            AchievementList = root.Require<VisualElement>("AchievementList");
            DailyStatus = root.Require<Label>("DailyStatus");
            ClaimDaily = root.Require<Button>("ClaimDailyButton");
            ClaimDailyDouble = root.Require<Button>("ClaimDailyDoubleButton");
        }

        public VisualElement Root { get; }
        public Label Summary { get; }
        public Label LevelValue { get; }
        public Label XpValue { get; }
        public ProgressBar XpBar { get; }
        public Label StarsValue { get; }
        public Button DailyTab { get; }
        public Button TasksTab { get; }
        public Button AchievementsTab { get; }
        public VisualElement DailyPanel { get; }
        public VisualElement TasksPanel { get; }
        public VisualElement AchievementsPanel { get; }
        public VisualElement DailyList { get; }
        public VisualElement TaskList { get; }
        public VisualElement AchievementList { get; }
        public Label DailyStatus { get; }
        public Button ClaimDaily { get; }
        public Button ClaimDailyDouble { get; }

        public void ShowPanel(string panel)
        {
            UiVisibility.SetVisible(DailyPanel, panel == "daily");
            UiVisibility.SetVisible(TasksPanel, panel == "tasks");
            UiVisibility.SetVisible(AchievementsPanel, panel == "achievements");
            DailyTab.EnableInClassList(UiClassNames.Selected, panel == "daily");
            TasksTab.EnableInClassList(UiClassNames.Selected, panel == "tasks");
            AchievementsTab.EnableInClassList(UiClassNames.Selected, panel == "achievements");
        }
    }
}
