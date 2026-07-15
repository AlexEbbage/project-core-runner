using CoreRacer.UI.Shared;
using UnityEngine;

namespace CoreRacer.UI.MainMenu
{
    public sealed class MainMenuShell : UiView
    {
        [SerializeField] private TopBarController topBar;
        [SerializeField] private MainMenuPageRouter router;
        [SerializeField] private bool showDefaultPageOnShow = true;

        public MainMenuPageRouter Router => router;

        private void Awake()
        {
            if (router == null)
                router = GetComponentInChildren<MainMenuPageRouter>(true);
            if (topBar == null)
                topBar = GetComponentInChildren<TopBarController>(true);
        }

        private void OnEnable()
        {
            if (router != null)
                router.PageChanged += HandlePageChanged;
        }

        private void OnDisable()
        {
            if (router != null)
                router.PageChanged -= HandlePageChanged;
        }

        public override void Show()
        {
            base.Show();
            topBar?.Refresh();
            if (showDefaultPageOnShow)
                router?.ShowDefault();
            else if (router != null && router.HasCurrentPage)
                router.Show(router.CurrentPage);
        }

        public void ShowPlay() => router?.Show(MainMenuPage.Play);
        public void ShowHangar() => router?.Show(MainMenuPage.Hangar);
        public void ShowLab() => router?.Show(MainMenuPage.Lab);
        public void ShowShop() => router?.Show(MainMenuPage.Shop);
        public void ShowProgression() => router?.Show(MainMenuPage.Progression);
        public void ShowSettings() => router?.Show(MainMenuPage.Settings);

        private void HandlePageChanged(MainMenuPage page)
        {
            topBar?.Refresh();
        }
    }
}
