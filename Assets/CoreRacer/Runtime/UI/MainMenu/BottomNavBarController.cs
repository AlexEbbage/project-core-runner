using System;
using UnityEngine;

namespace CoreRacer.UI.MainMenu
{
    public sealed class BottomNavBarController : MonoBehaviour
    {
        [SerializeField] private MainMenuPageRouter router;

        private void Awake()
        {
            if (router == null)
                router = GetComponentInParent<MainMenuPageRouter>();
        }

        public void ShowPlay() => ShowBottomNavigationPage(MainMenuPage.Play);
        public void ShowHangar() => ShowBottomNavigationPage(MainMenuPage.Hangar);
        public void ShowLab() => ShowBottomNavigationPage(MainMenuPage.Lab);
        public void ShowShop() => ShowBottomNavigationPage(MainMenuPage.Shop);
        public void ShowProgression() => ShowBottomNavigationPage(MainMenuPage.Progression);

        [Obsolete("Settings is not a first-release bottom-nav destination. Wire the settings button to TopBarController.OpenSettings instead.")]
        public void ShowSettings()
        {
            router?.Show(MainMenuPage.Settings);
        }

        public void ShowPage(MainMenuPage page)
        {
            if (FinalMenuSetRules.IsBottomNavigationPage(page))
                ShowBottomNavigationPage(page);
            else
                Debug.LogWarning($"{page} is not a final bottom navigation page. Use a top-bar or modal action instead.", this);
        }

        private void ShowBottomNavigationPage(MainMenuPage page)
        {
            if (!FinalMenuSetRules.IsBottomNavigationPage(page))
            {
                Debug.LogWarning($"{page} is not a final bottom navigation page.", this);
                return;
            }

            router?.Show(page);
        }
    }
}
