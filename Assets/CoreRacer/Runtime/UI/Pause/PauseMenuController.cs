using CoreRacer.Gameplay.Run;
using CoreRacer.UI.Shared;
using UnityEngine;

namespace CoreRacer.UI.Pause
{
    public sealed class PauseMenuController : UiView
    {
        [SerializeField] private RunController runController;

        public void TogglePause()
        {
            if (runController == null) return;
            if (runController.State == RunState.Running)
            {
                runController.PauseRun();
                Show();
            }
            else if (runController.State == RunState.Paused)
            {
                runController.ResumeRun();
                Hide();
            }
        }

        public void Resume()
        {
            runController?.ResumeRun();
            Hide();
        }

        public void QuitToMenu()
        {
            runController?.ReturnToMenu();
            Hide();
        }
    }
}
