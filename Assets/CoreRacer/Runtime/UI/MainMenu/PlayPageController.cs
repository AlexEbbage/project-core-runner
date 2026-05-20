using CoreRacer.Gameplay.Run;
using CoreRacer.UI.Shared;
using UnityEngine;

namespace CoreRacer.UI.MainMenu
{
    public sealed class PlayPageController : UiView
    {
        [SerializeField] private RunController runController;
        public void Play() => runController?.StartRun();
    }
}
