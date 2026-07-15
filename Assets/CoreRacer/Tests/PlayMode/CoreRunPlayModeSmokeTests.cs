using System.Collections;
using CoreRacer.Bootstrap;
using CoreRacer.Gameplay.Player;
using CoreRacer.Gameplay.Powerups;
using CoreRacer.Gameplay.Run;
using CoreRacer.UI.MainMenu;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace CoreRacer.Tests.PlayMode
{
    public sealed class CoreRunPlayModeSmokeTests
    {
        [UnityTest]
        public IEnumerator MainScene_HasOneEventSystemBootstrapperAndValidVisiblePlayListener()
        {
            yield return LoadMainScene();

            Assert.AreEqual(1, Object.FindObjectsOfType<EventSystem>().Length, "Exactly one active EventSystem is required.");
            Assert.AreEqual(1, Object.FindObjectsOfType<GameBootstrapper>().Length, "Exactly one active GameBootstrapper is required.");

            var playObject = GameObject.Find("Canvas/MainMenu/BottomNav/PlayButton");
            Assert.NotNull(playObject, "The visible bottom Play button is missing from the active menu.");
            var button = playObject.GetComponent<Button>();
            Assert.NotNull(button, "The visible Play object has no Button component.");
            Assert.IsTrue(button.IsActive() && button.IsInteractable(), "The visible Play button must be active and interactable.");
            Assert.Greater(button.onClick.GetPersistentEventCount(), 0, "The visible Play button has no persistent listener.");
            Assert.AreEqual("StartCoreRun", button.onClick.GetPersistentMethodName(0));
        }

        [UnityTest]
        public IEnumerator VisiblePlay_StartsCoreGameplay()
        {
            yield return LoadMainScene();

            var play = Object.FindObjectOfType<BottomNavBarController>(true);
            var run = Object.FindObjectOfType<RunController>(true);
            var references = Object.FindObjectOfType<RunSceneReferences>(true);
            var cameraFollow = Object.FindObjectOfType<PlayerCameraFollow>(true);
            Assert.NotNull(play, "The visible Play controller is missing.");
            Assert.NotNull(run, "The run controller is missing.");
            Assert.NotNull(references, "The run references are missing.");
            Assert.NotNull(cameraFollow, "The player camera follow component is missing.");

            play.StartCoreRun();
            yield return null;

            Assert.AreEqual(RunState.Running, run.State);
            Assert.IsNotEmpty(run.CurrentRunId, "Starting gameplay must create an active run session.");
            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsTrue(references.Player.gameObject.activeInHierarchy);
            Assert.IsFalse(references.MainMenu.gameObject.activeInHierarchy);
            Assert.IsTrue(references.Hud.gameObject.activeInHierarchy);
            Assert.IsTrue(cameraFollow.gameObject.activeInHierarchy);

            var firstRunId = run.CurrentRunId;
            Assert.IsFalse(run.TryStartRun(), "A duplicate Play request must be rejected while running.");
            Assert.AreEqual(firstRunId, run.CurrentRunId, "A duplicate Play request must not reset the active session.");

            var startZ = references.Player.transform.position.z;
            var cameraStartZ = cameraFollow.transform.position.z;
            yield return null;
            yield return null;
            Assert.Greater(references.Player.transform.position.z, startZ, "The player must move forward during a run.");
            Assert.Greater(cameraFollow.transform.position.z, cameraStartZ, "The follow camera must advance with the player.");

            references.CurrencyTracker.AddCoinPickup();
            Assert.Greater(references.CurrencyTracker.Coins, 0, "Collected coins must be reflected in run state.");
            var powerupActivated = false;
            references.Powerups.PowerupActivated += (_, __) => powerupActivated = true;
            references.Powerups.Activate(PowerupType.ScoreMultiplier);
            Assert.IsTrue(powerupActivated, "A collected powerup must activate its runtime effect.");

            run.ReturnToMenu();
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator RunLifecycle_GameOverRetryHomeAndPlayAgainAreCleanAndIdempotent()
        {
            yield return LoadMainScene();

            var play = Object.FindObjectOfType<BottomNavBarController>(true);
            var run = Object.FindObjectOfType<RunController>(true);
            var references = Object.FindObjectOfType<RunSceneReferences>(true);
            Assert.NotNull(play);
            Assert.NotNull(run);
            Assert.NotNull(references);

            play.StartCoreRun();
            yield return null;
            var firstRunId = run.CurrentRunId;

            EndCurrentRun(run);
            yield return null;
            Assert.AreEqual(RunState.GameOver, run.State);
            Assert.IsTrue(references.GameOver.gameObject.activeInHierarchy, "Ending a run must show Game Over.");

            var retryButton = FindButton(references.GameOver.transform, "RetryButton");
            var doubleRewardsButton = FindButton(references.GameOver.transform, "DoubleRewardsButton");
            var menuButton = FindButton(references.GameOver.transform, "MenuButton");
            Assert.NotNull(retryButton);
            Assert.NotNull(doubleRewardsButton);
            Assert.NotNull(menuButton);
            Assert.IsFalse(GetWorldRect(retryButton.transform as RectTransform).Overlaps(GetWorldRect(doubleRewardsButton.transform as RectTransform)), "Retry and Double Rewards must not overlap.");
            Assert.IsFalse(GetWorldRect(doubleRewardsButton.transform as RectTransform).Overlaps(GetWorldRect(menuButton.transform as RectTransform)), "Double Rewards and Menu must not overlap.");
            Assert.IsFalse(GetWorldRect(retryButton.transform as RectTransform).Overlaps(GetWorldRect(menuButton.transform as RectTransform)), "Retry and Menu must not overlap.");

            EndCurrentRun(run);
            Assert.AreEqual(RunState.GameOver, run.State, "Ending an already-ended run must be idempotent.");
            Assert.AreEqual(firstRunId, run.CurrentRunId);

            retryButton.onClick.Invoke();
            yield return null;
            var secondRunId = run.CurrentRunId;
            Assert.AreEqual(RunState.Running, run.State);
            Assert.AreNotEqual(firstRunId, secondRunId, "Retry must create a distinct run session.");
            Assert.AreEqual(0, references.ScoreTracker.CurrentScore);
            Assert.AreEqual(0, references.CurrencyTracker.Coins);
            Assert.Less(references.StatsTracker.Distance, 1f, "Retry must begin near zero rather than retain the previous run distance.");
            Assert.Less(references.StatsTracker.Duration, 1f, "Retry must reset the run duration.");
            Assert.IsFalse(references.GameOver.gameObject.activeInHierarchy);

            EndCurrentRun(run);
            yield return null;
            Assert.AreEqual(RunState.GameOver, run.State);
            menuButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(RunState.MainMenu, run.State);
            Assert.IsTrue(references.MainMenu.gameObject.activeInHierarchy);
            Assert.IsFalse(references.Hud.gameObject.activeInHierarchy);

            play.StartCoreRun();
            yield return null;
            Assert.AreEqual(RunState.Running, run.State, "Play must work again after returning Home.");
            Assert.AreNotEqual(secondRunId, run.CurrentRunId);

            run.ReturnToMenu();
            Time.timeScale = 1f;
        }

        private static IEnumerator LoadMainScene()
        {
            Time.timeScale = 1f;
            var load = SceneManager.LoadSceneAsync("CoreRacer_Main", LoadSceneMode.Single);
            Assert.NotNull(load, "CoreRacer_Main must be present in build settings for the PlayMode smoke test.");
            while (!load.isDone)
                yield return null;
            yield return null;
        }

        private static void EndCurrentRun(RunController run)
        {
            run.HandlePlayerDeath();
            if (run.State == RunState.ContinueOffered || run.State == RunState.Crashed)
                run.DeclineContinue();
        }

        private static Button FindButton(Transform root, string name)
        {
            var buttons = root.GetComponentsInChildren<Button>(true);
            for (var i = 0; i < buttons.Length; i++)
                if (buttons[i].name == name)
                    return buttons[i];
            return null;
        }

        private static Rect GetWorldRect(RectTransform transform)
        {
            var corners = new Vector3[4];
            transform.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }
    }
}
