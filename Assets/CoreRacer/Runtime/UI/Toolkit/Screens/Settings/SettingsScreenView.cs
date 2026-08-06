using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public sealed class SettingsScreenView
    {
        public SettingsScreenView(VisualElement root)
        {
            Root = root;
            Music = root.Require<Slider>("MusicSlider");
            Sfx = root.Require<Slider>("SfxSlider");
            Haptics = root.Require<Toggle>("HapticsToggle");
            DragControls = root.Require<Toggle>("DragControlsToggle");
            ReducedMotion = root.Require<Toggle>("ReducedMotionToggle");
            HighContrast = root.Require<Toggle>("HighContrastToggle");
            Privacy = root.Require<Button>("PrivacyButton");
            Support = root.Require<Button>("SupportButton");
            ResetTutorial = root.Require<Button>("ResetTutorialButton");
            Gallery = root.Require<Button>("GalleryButton");
            Status = root.Require<Label>("SettingsStatus");
        }

        public VisualElement Root { get; }
        public Slider Music { get; }
        public Slider Sfx { get; }
        public Toggle Haptics { get; }
        public Toggle DragControls { get; }
        public Toggle ReducedMotion { get; }
        public Toggle HighContrast { get; }
        public Button Privacy { get; }
        public Button Support { get; }
        public Button ResetTutorial { get; }
        public Button Gallery { get; }
        public Label Status { get; }
    }
}
