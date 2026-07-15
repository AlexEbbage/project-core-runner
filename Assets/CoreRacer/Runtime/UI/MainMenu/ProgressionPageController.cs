using CoreRacer.Bootstrap;
using CoreRacer.Meta.Profile;
using CoreRacer.UI.Shared;

namespace CoreRacer.UI.MainMenu
{
    public sealed class ProgressionPageController : UiView
    {
        private PlayerProfileService _profile;
        private ProgressionHubController _hub;

        private void Awake()
        {
            GameServices.TryGet(out _profile);
            _hub = GetComponentInChildren<ProgressionHubController>(true);
        }

        private void OnEnable()
        {
            if (_profile == null)
                GameServices.TryGet(out _profile);
            if (_profile != null)
                _profile.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (_profile != null)
                _profile.Changed -= Refresh;
        }

        public override void Show()
        {
            base.Show();
            Refresh();
        }

        public void Refresh()
        {
            if (_hub == null)
                _hub = GetComponentInChildren<ProgressionHubController>(true);
            _hub?.RefreshVisiblePanel();
        }
    }
}
