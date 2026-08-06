using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public sealed class LabScreenView
    {
        public LabScreenView(VisualElement root)
        {
            Root = root;
            Status = root.Require<Label>("LabStatus");
            BoosterList = root.Require<VisualElement>("LabList");
            PassiveList = root.Require<VisualElement>("PassiveUpgradeList");
            ExperimentList = root.Require<VisualElement>("CoreExperimentList");
        }

        public VisualElement Root { get; }
        public Label Status { get; }
        public VisualElement BoosterList { get; }
        public VisualElement PassiveList { get; }
        public VisualElement ExperimentList { get; }
    }
}
