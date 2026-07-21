using CoreRacer.Bootstrap;
using CoreRacer.Gameplay.Obstacles;
using CoreRacer.Gameplay.Pickups;
using CoreRacer.Gameplay.Run;
using UnityEngine;

namespace CoreRacer.FTUE
{
    public sealed class TutorialDirector : MonoBehaviour
    {
        [SerializeField] private RunController runController;
        [SerializeField] private ObstacleWorldController obstacleWorld;
        [SerializeField] private PickupWorldController pickupWorld;

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

            if (_tutorial != null && _tutorial.ShouldRunForFreshInstall())
                _tutorial.Start();
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
                case TutorialStepKind.WaitForDailyTaskRewardPromptOpened:
                    runController?.ReturnToMenu();
                    break;
            }
        }
    }
}
