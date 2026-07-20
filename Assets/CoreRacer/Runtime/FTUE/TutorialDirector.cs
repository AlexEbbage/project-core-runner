using CoreRacer.Bootstrap;
using CoreRacer.Gameplay.Obstacles;
using CoreRacer.Gameplay.Pickups;
using CoreRacer.Gameplay.Run;
using CoreRacer.UI.FTUE;
using CoreRacer.UI.MainMenu;
using UnityEngine;

namespace CoreRacer.FTUE
{
    public sealed class TutorialDirector : MonoBehaviour
    {
        [SerializeField] private RunController runController;
        [SerializeField] private ObstacleWorldController obstacleWorld;
        [SerializeField] private PickupWorldController pickupWorld;
        [SerializeField] private MainMenuPageRouter router;
        [SerializeField] private TutorialOverlayController overlay;

        private TutorialService _tutorial;

        private void OnEnable()
        {
            if (GameServices.TryGet(out _tutorial))
                _tutorial.StepChanged += HandleStepChanged;
        }

        private void Start()
        {
            if (_tutorial == null && GameServices.TryGet(out _tutorial))
                _tutorial.StepChanged += HandleStepChanged;

            overlay?.BeginIfNeeded();
        }

        private void OnDisable()
        {
            if (_tutorial != null)
                _tutorial.StepChanged -= HandleStepChanged;
        }

        private void HandleStepChanged(TutorialStepDefinition step)
        {
            if (step == null)
                return;

            switch (step.Kind)
            {
                case TutorialStepKind.WaitForPickup:
                    pickupWorld?.QueueTutorialCoin();
                    break;
                case TutorialStepKind.WaitForPowerup:
                    pickupWorld?.QueueTutorialPowerup();
                    break;
                case TutorialStepKind.WaitForUpgradePromptOpened:
                    runController?.ReturnToMenu();
                    router?.Show(MainMenuPage.Lab);
                    break;
                case TutorialStepKind.WaitForDailyTaskRewardPromptOpened:
                    runController?.ReturnToMenu();
                    router?.Show(MainMenuPage.Progression);
                    break;
            }
        }
    }
}
