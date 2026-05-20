using CoreRacer.Meta.Profile;
using CoreRacer.UI.Shared;
using UnityEngine;

namespace CoreRacer.UI.MainMenu
{
    public sealed class MainMenuShell : UiView
    {
        [SerializeField] private TopBarController topBar;
        [SerializeField] private MainMenuPageRouter router;

        private PlayerProfileService _profile;

        private void Awake()
        {
            CoreRacer.Bootstrap.GameServices.TryGet(out _profile);
        }

        public override void Show()
        {
            base.Show();
            topBar?.Refresh();
            router?.ShowDefault();
        }
    }
}
