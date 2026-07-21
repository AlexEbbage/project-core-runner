namespace CoreRacer.Gameplay.Run
{
    public interface IRunUiPresenter
    {
        void ShowMainMenu();
        void HideMainMenu();
        void ShowHud();
        void HideHud();
        void ShowPause();
        void HidePause();
        void ShowContinueOffer();
        void HideGameOver();
        void ShowContinueUnavailable();
        void SetContinuePending(bool pending);
        void ShowGameOver(RunResult result);
        void SetDoubleRewardPending(bool pending);
        void ShowDoubleRewardUnavailable();
        void ShowDoubleRewardGranted(RunResult bonus);
    }
}
