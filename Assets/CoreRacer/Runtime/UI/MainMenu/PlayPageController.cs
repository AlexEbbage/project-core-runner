using CoreRacer.Gameplay.Run;
using CoreRacer.UI.Shared;
using UnityEngine;

namespace CoreRacer.UI.MainMenu
{
    public sealed class PlayPageController : UiView
    {
        [SerializeField] private RunController runController;
        [SerializeField] private LevelSelectPageController levelSelect;

        private void Awake()
        {
            if (levelSelect == null)
                levelSelect = GetComponentInChildren<LevelSelectPageController>(true);
        }

        public override void Show()
        {
            base.Show();
            levelSelect?.Refresh();
        }

        public void Play() => runController?.StartRun();
    }
}
