using System;
using CoreRacer.FTUE;
using CoreRacer.Gameplay.Run;
using UnityEngine;

namespace CoreRacer.UI.Toolkit
{
    public sealed class RunOverlayPresenter : IDisposable
    {
        private readonly RunOverlayView _view;
        private readonly CoreRacerUiContext _context;
        private readonly IUiAnimationService _animations;
        private bool _initialized;

        public RunOverlayPresenter(RunOverlayView view, CoreRacerUiContext context, IUiAnimationService animations)
        {
            _view = view;
            _context = context;
            _animations = animations;
        }

        public void Initialize()
        {
            if (_initialized)
                return;
            _initialized = true;
            _view.ResumeButton.clicked += Resume;
            _view.PauseHomeButton.clicked += Home;
            _view.ContinueButton.clicked += Continue;
            _view.EndRunButton.clicked += EndRun;
            _view.DoubleRewardsButton.clicked += DoubleRewards;
            _view.RetryButton.clicked += Retry;
            _view.HomeButton.clicked += Home;
            _view.TutorialContinue.clicked += AdvanceTutorial;
            UiModalInputBlocker.Attach(_view.PauseRoot);
            UiModalInputBlocker.Attach(_view.GameOverRoot);
            UiModalInputBlocker.Attach(_view.TutorialRoot);
            if (_context.Tutorial != null)
            {
                _context.Tutorial.StepChanged += ShowTutorial;
                _context.Tutorial.Completed += HideTutorial;
            }
        }

        public void ShowPause()
        {
            UiVisibility.SetVisible(_view.PauseRoot, true);
            _animations.ShowPopup(_view.PauseRoot);
        }

        public void HidePause() => _animations.HidePopup(_view.PauseRoot);

        public void ShowContinueOffer()
        {
            _view.GameOverTitle.text = "ALMOST THERE, RACER!";
            _view.GameOverMessage.text = "The Core is getting closer. Continue this run or bank your result.";
            UiVisibility.SetVisible(_view.ContinueActions, true);
            UiVisibility.SetVisible(_view.FinalActions, false);
            UiVisibility.SetVisible(_view.GameOverRoot, true);
            _animations.ShowBottomSheet(_view.GameOverRoot);
        }

        public void ShowContinueUnavailable()
        {
            _view.GameOverMessage.text = "Continue is unavailable. Finalising this run.";
            _animations.PlayInvalidAction(_view.GameOverRoot);
        }

        public void SetContinuePending(bool pending)
        {
            _view.ContinueButton.SetEnabled(!pending);
            _view.ContinueButton.text = pending ? "PLEASE WAIT..." : "CONTINUE";
            _view.ContinueButton.EnableInClassList(UiClassNames.Pending, pending);
        }

        public void ShowGameOver(RunResult result)
        {
            _view.GameOverTitle.text = "RUN COMPLETE";
            _view.ResultScore.text = result.Score.ToString("N0");
            _view.ResultCoins.text = result.Coins.ToString("N0");
            _view.ResultXp.text = result.Experience.ToString("N0");
            _view.ResultPremium.text = result.PremiumCurrency.ToString("N0");
            _view.GameOverMessage.text = $"Distance {result.Distance:0}m · {result.PowerupsCollected} powerups";
            UiVisibility.SetVisible(_view.ContinueActions, false);
            UiVisibility.SetVisible(_view.FinalActions, true);
            UiVisibility.SetVisible(_view.GameOverRoot, true);
            _animations.ShowBottomSheet(_view.GameOverRoot);
        }

        public void HideGameOver() => _animations.HideBottomSheet(_view.GameOverRoot);

        public void SetDoubleRewardPending(bool pending)
        {
            _view.DoubleRewardsButton.SetEnabled(!pending);
            _view.DoubleRewardsButton.text = pending ? "PLEASE WAIT..." : "DOUBLE REWARDS";
            _view.DoubleRewardsButton.EnableInClassList(UiClassNames.Pending, pending);
        }

        public void ShowDoubleRewardUnavailable()
        {
            _view.GameOverMessage.text = "Double rewards are unavailable right now.";
            _animations.PlayInvalidAction(_view.DoubleRewardsButton);
        }

        public void ShowDoubleRewardGranted(RunResult bonus)
        {
            _view.GameOverMessage.text = $"Bonus granted: +{bonus.Coins:N0} credits and +{bonus.Experience:N0} XP.";
            _view.DoubleRewardsButton.SetEnabled(false);
            _view.DoubleRewardsButton.text = "REWARDS DOUBLED";
            _animations.PlaySuccess(_view.GameOverRoot);
        }

        public void Dispose()
        {
            if (!_initialized)
                return;
            _initialized = false;
            UiModalInputBlocker.Detach(_view.PauseRoot);
            UiModalInputBlocker.Detach(_view.GameOverRoot);
            UiModalInputBlocker.Detach(_view.TutorialRoot);
            _view.ResumeButton.clicked -= Resume;
            _view.PauseHomeButton.clicked -= Home;
            _view.ContinueButton.clicked -= Continue;
            _view.EndRunButton.clicked -= EndRun;
            _view.DoubleRewardsButton.clicked -= DoubleRewards;
            _view.RetryButton.clicked -= Retry;
            _view.HomeButton.clicked -= Home;
            _view.TutorialContinue.clicked -= AdvanceTutorial;
            if (_context.Tutorial != null)
            {
                _context.Tutorial.StepChanged -= ShowTutorial;
                _context.Tutorial.Completed -= HideTutorial;
            }
        }

        private void Resume()
        {
            Time.timeScale = 1f;
            _context.RunController?.ResumeRun();
            HidePause();
        }

        private void Home()
        {
            Time.timeScale = 1f;
            _context.RunController?.ReturnToMenu();
        }

        private void Continue() => _context.RunController?.ContinueRun();
        private void EndRun() => _context.RunController?.DeclineContinue();
        private void DoubleRewards() => _context.RunController?.DoubleRunRewards();
        private void Retry() => _context.RunController?.RetryRun();
        private void AdvanceTutorial() => _context.Tutorial?.Advance();

        private void ShowTutorial(TutorialStepDefinition step)
        {
            if (step == null)
            {
                HideTutorial();
                return;
            }

            if (_context.RunController != null && _context.RunController.State == RunState.MainMenu && step.Kind != TutorialStepKind.Message && step.Kind != TutorialStepKind.WaitForRunStarted)
            {
                HideTutorial();
                return;
            }

            _view.TutorialTitle.text = string.IsNullOrWhiteSpace(step.TitleKey)
                ? "CORE TRAINING"
                : _context.Localization != null ? _context.Localization.Get(step.TitleKey) : step.TitleKey;
            _view.TutorialBody.text = string.IsNullOrWhiteSpace(step.BodyKey)
                ? "Follow the highlighted action."
                : _context.Localization != null ? _context.Localization.Get(step.BodyKey) : step.BodyKey;
            UiVisibility.SetVisible(_view.TutorialRoot, true);
            _animations.ShowPopup(_view.TutorialRoot);
        }

        private void HideTutorial() => _animations.HidePopup(_view.TutorialRoot);
    }
}
