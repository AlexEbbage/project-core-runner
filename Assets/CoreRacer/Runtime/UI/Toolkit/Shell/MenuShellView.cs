using System.Collections.Generic;
using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public sealed class MenuShellView
    {
        public MenuShellView(VisualElement root)
        {
            Root = root;
            ProfileButton = root.Require<Button>("ProfileButton");
            SettingsButton = root.Require<Button>("SettingsShortcutButton");
            LevelLabel = root.Require<Label>("LevelLabel");
            XpLabel = root.Require<Label>("ProfileXpLabel");
            XpBar = root.Require<ProgressBar>("ProfileXpBar");
            SoftCurrencyLabel = root.Require<Label>("SoftCurrencyLabel");
            PremiumCurrencyLabel = root.Require<Label>("PremiumCurrencyLabel");
            Navigation = new Dictionary<CoreRacerScreenId, Button>
            {
                [CoreRacerScreenId.Play] = root.Require<Button>("NavPlay"),
                [CoreRacerScreenId.Shop] = root.Require<Button>("NavShop"),
                [CoreRacerScreenId.Hangar] = root.Require<Button>("NavHangar"),
                [CoreRacerScreenId.Lab] = root.Require<Button>("NavLab"),
                [CoreRacerScreenId.Progression] = root.Require<Button>("NavProgression")
            };
        }

        public VisualElement Root { get; }
        public Button ProfileButton { get; }
        public Button SettingsButton { get; }
        public Label LevelLabel { get; }
        public Label XpLabel { get; }
        public ProgressBar XpBar { get; }
        public Label SoftCurrencyLabel { get; }
        public Label PremiumCurrencyLabel { get; }
        public Dictionary<CoreRacerScreenId, Button> Navigation { get; }
    }
}
