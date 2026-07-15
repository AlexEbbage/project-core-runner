using System.Collections;
using CoreRacer.Bootstrap;
using CoreRacer.Gameplay.Environment;
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
            var tunnel = Object.FindObjectOfType<TunnelWallGeneratorV2>(true);
            Assert.NotNull(play, "The visible Play controller is missing.");
            Assert.NotNull(run, "The run controller is missing.");
            Assert.NotNull(references, "The run references are missing.");
            Assert.NotNull(cameraFollow, "The player camera follow component is missing.");
            Assert.NotNull(tunnel, "The procedural tunnel generator is missing.");

            var trail = references.Player.GetComponentInChildren<TrailRenderer>(true);
            Assert.NotNull(trail, "The player thruster trail is missing.");
            trail.Clear();
            trail.AddPosition(new Vector3(1000f, 1000f, 1000f));
            trail.AddPosition(new Vector3(1001f, 1001f, 1001f));

            play.StartCoreRun();
            yield return null;

            Assert.AreEqual(RunState.Running, run.State);
            Assert.IsNotEmpty(run.CurrentRunId, "Starting gameplay must create an active run session.");
            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsTrue(references.Player.gameObject.activeInHierarchy);
            Assert.IsFalse(references.MainMenu.gameObject.activeInHierarchy);
            Assert.IsTrue(references.Hud.gameObject.activeInHierarchy);
            Assert.IsTrue(cameraFollow.gameObject.activeInHierarchy);
            Assert.IsTrue(cameraFollow.FollowsTargetRoll, "The gameplay camera must roll with the player so the tunnel appears to rotate instead of the ship.");
            Assert.AreEqual(270f, references.Player.Motor.AngleDegrees, 0.01f, "A run must start on the bottom rail of the tunnel.");
            Assert.Less(references.Player.transform.position.y, 0f, "The player must begin below the tunnel centre.");
            Assert.Less(cameraFollow.transform.position.y, 0f, "The follow camera must begin behind the player on the bottom rail.");
            Assert.Less(Mathf.Abs(Mathf.DeltaAngle(cameraFollow.transform.eulerAngles.z, 0f)), 0.1f, "The bottom-rail start must keep the camera visually upright.");
            var gameplayCamera = cameraFollow.GetComponent<Camera>();
            Assert.NotNull(gameplayCamera, "The follow object must own the gameplay camera.");
            var playerViewportPosition = gameplayCamera.WorldToViewportPoint(references.Player.transform.position);
            Assert.That(playerViewportPosition.y, Is.InRange(0.3f, 0.45f), "The player must be framed in the lower portion of the portrait viewport.");
            Assert.AreEqual(140f, references.Player.Motor.AngularSpeedDegrees, 0.01f, "The core route must use the initial comfort steering speed.");
            Assert.AreEqual(6, tunnel.Sides, "The default roadmap route must configure a six-sided tunnel.");
            Assert.AreEqual(48, tunnel.SectionCount, "The core run must generate the authored tunnel section count.");
            Assert.NotNull(tunnel.GeneratedMesh, "Starting a run must produce a procedural tunnel mesh.");
            Assert.Greater(tunnel.GeneratedMesh.vertexCount, 0);
            Assert.Less(tunnel.WallTint.grayscale, 0.4f, "The tunnel tint must remain dark enough for hazards and pickups to read clearly.");
            var tunnelProperties = new MaterialPropertyBlock();
            tunnel.GetComponent<MeshRenderer>().GetPropertyBlock(tunnelProperties);
            Assert.Less(Vector4.Distance(tunnel.WallTint, tunnelProperties.GetColor("_Color")), 0.001f, "The readability tint must be applied to the runtime renderer without changing its shared material.");
            var trailPositions = new Vector3[trail.positionCount];
            trail.GetPositions(trailPositions);
            for (var i = 0; i < trailPositions.Length; i++)
                Assert.Less(Vector3.Distance(trailPositions[i], references.Player.transform.position), 20f, "Run start must clear stale trail positions left by the player teleport.");

            var firstRunId = run.CurrentRunId;
            Assert.IsFalse(run.TryStartRun(), "A duplicate Play request must be rejected while running.");
            Assert.AreEqual(firstRunId, run.CurrentRunId, "A duplicate Play request must not reset the active session.");

            var startZ = references.Player.transform.position.z;
            var cameraStartZ = cameraFollow.transform.position.z;
            references.Player.Motor.Move(1f, 0.25f);
            yield return null;
            yield return null;
            Assert.Greater(references.Player.transform.position.z, startZ, "The player must move forward during a run.");
            Assert.Greater(cameraFollow.transform.position.z, cameraStartZ, "The follow camera must advance with the player.");
            var movedViewportPosition = gameplayCamera.WorldToViewportPoint(references.Player.transform.position);
            Assert.Less(Mathf.Abs(movedViewportPosition.x - 0.5f), 0.02f, "The player must remain horizontally centred while the camera rolls.");
            Assert.That(movedViewportPosition.y, Is.InRange(0.3f, 0.45f), "The player must remain in the lower viewport while steering.");
            Assert.Less(Mathf.Abs(Mathf.DeltaAngle(cameraFollow.transform.eulerAngles.z, references.Player.transform.eulerAngles.z)), 0.1f, "The camera roll must match the player roll.");

            var firstTunnelStart = tunnel.StartZ;
            tunnel.AdvanceTo(firstTunnelStart + tunnel.TrailingDistance + tunnel.RecenterDistance + 1f);
            Assert.Greater(tunnel.StartZ, firstTunnelStart, "The generated tunnel must recycle forward as the run advances.");
            Assert.Greater(tunnel.EndZ, references.Player.transform.position.z, "Generated tunnel sections must remain ahead of the player.");

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
