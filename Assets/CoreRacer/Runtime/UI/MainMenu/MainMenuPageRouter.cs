using System.Collections.Generic;
using CoreRacer.UI.Shared;
using UnityEngine;

namespace CoreRacer.UI.MainMenu
{
    public enum MainMenuPage
    {
        Play,
        Shop,
        Hangar,
        Lab,
        Progression,
        Settings
    }

    public sealed class MainMenuPageRouter : MonoBehaviour
    {
        [System.Serializable]
        public sealed class PageBinding
        {
            public MainMenuPage Page;
            public UiView View;
        }

        [SerializeField] private MainMenuPage defaultPage = MainMenuPage.Play;
        [SerializeField] private List<PageBinding> pages = new List<PageBinding>();

        public void ShowDefault() => Show(defaultPage);

        public void Show(MainMenuPage page)
        {
            for (int i = 0; i < pages.Count; i++)
            {
                if (pages[i].View == null) continue;
                if (pages[i].Page == page) pages[i].View.Show();
                else pages[i].View.Hide();
            }
        }
    }
}
