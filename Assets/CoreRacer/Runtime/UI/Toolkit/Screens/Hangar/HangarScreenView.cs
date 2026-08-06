using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public sealed class HangarScreenView
    {
        public HangarScreenView(VisualElement root)
        {
            Root = root;
            PreviousButton = root.Require<Button>("HangarPreviousButton");
            NextButton = root.Require<Button>("HangarNextButton");
            EquipButton = root.Require<Button>("HangarEquipButton");
            UpgradeButton = root.Require<Button>("HangarUpgradeButton");
            SelectionTitle = root.Require<Label>("HangarSelectionTitle");
            SelectionStatus = root.Require<Label>("HangarSelectionStatus");
            Status = root.Require<Label>("HangarStatus");
            Preview = root.Require<VisualElement>("HangarShipPreview");
            List = root.Require<VisualElement>("HangarList");
            Speed = root.Require<ProgressBar>("HangarSpeedStat");
            Handling = root.Require<ProgressBar>("HangarHandlingStat");
            Shield = root.Require<ProgressBar>("HangarShieldStat");
            Boost = root.Require<ProgressBar>("HangarBoostStat");
            ShipsTab = root.Require<Button>("HangarShipsTab");
            SkinsTab = root.Require<Button>("HangarSkinsTab");
            TrailsTab = root.Require<Button>("HangarTrailsTab");
            CoreFxTab = root.Require<Button>("HangarCoreFxTab");
        }

        public VisualElement Root { get; }
        public Button PreviousButton { get; }
        public Button NextButton { get; }
        public Button EquipButton { get; }
        public Button UpgradeButton { get; }
        public Label SelectionTitle { get; }
        public Label SelectionStatus { get; }
        public Label Status { get; }
        public VisualElement Preview { get; }
        public VisualElement List { get; }
        public ProgressBar Speed { get; }
        public ProgressBar Handling { get; }
        public ProgressBar Shield { get; }
        public ProgressBar Boost { get; }
        public Button ShipsTab { get; }
        public Button SkinsTab { get; }
        public Button TrailsTab { get; }
        public Button CoreFxTab { get; }

        public void SetSection(string section)
        {
            ShipsTab.EnableInClassList(UiClassNames.Selected, section == "ships");
            SkinsTab.EnableInClassList(UiClassNames.Selected, section == "skins");
            TrailsTab.EnableInClassList(UiClassNames.Selected, section == "trails");
            CoreFxTab.EnableInClassList(UiClassNames.Selected, section == "corefx");
        }
    }
}
