using UnityEngine;

namespace CoreRacer.UI.MainMenu
{
    public sealed class BottomNavBarController : MonoBehaviour
    {
        [SerializeField] private MainMenuPageRouter router;

        public void ShowPlay() => router?.Show(MainMenuPage.Play);
        public void ShowShop() => router?.Show(MainMenuPage.Shop);
        public void ShowHangar() => router?.Show(MainMenuPage.Hangar);
        public void ShowLab() => router?.Show(MainMenuPage.Lab);
        public void ShowProgression() => router?.Show(MainMenuPage.Progression);
        public void ShowSettings() => router?.Show(MainMenuPage.Settings);
    }
}
