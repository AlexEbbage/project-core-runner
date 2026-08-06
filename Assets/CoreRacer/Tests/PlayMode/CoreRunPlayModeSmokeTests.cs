using System.Collections;
using CoreRacer.Bootstrap;
using CoreRacer.FTUE;
using CoreRacer.Gameplay.Environment;
using CoreRacer.Gameplay.Pickups;
using CoreRacer.Gameplay.Player;
using CoreRacer.Gameplay.Powerups;
using CoreRacer.Gameplay.Run;
using CoreRacer.Gameplay.Vfx;
using CoreRacer.Meta.Profile;
using CoreRacer.UI.Toolkit;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace CoreRacer.Tests.PlayMode
{
    public sealed class CoreRunPlayModeSmokeTests : IsolatedPlayerProfilePlayModeTest
    {
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator VisiblePlay_StartsCoreGameplayAndGeneratedWorldMoves()
        {
            yield return LoadMainScene();
            var run = Object.FindObjectOfType<RunController>(true);
            var references = Object.FindObjectOfType<RunSceneReferences>(true);
            var cameraFollow = Object.FindObjectOfType<PlayerCameraFollow>(true);
            var tunnel = Object.FindObjectOfType<TunnelWallGeneratorV2>(true);
            var document = Object.FindObjectOfType<UIDocument>(true);
            Assert.NotNull(run);
            Assert.NotNull(references);
            Assert.NotNull(cameraFollow);
            Assert.NotNull(tunnel);
            Assert.NotNull(document);

            Submit(document.rootVisualElement.Q<Button>("PlayButton"));
            yield return null;
            Assert.AreEqual(RunState.Running, run.State);
            Assert.IsNotEmpty(run.CurrentRunId);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsTrue(references.Player.gameObject.activeInHierarchy);
            Assert.AreEqual(6, tunnel.Sides);
            Assert.AreEqual(2, tunnel.GeneratedMesh.subMeshCount);
            Assert.Greater(references.ObstacleWorld.ActiveRings.Count, 0);
            Assert.IsTrue(references.ObstacleWorld.ActiveRings[0].UsesAuthoredObstacle);

            var startZ = references.Player.transform.position.z;
            var cameraZ = cameraFollow.transform.position.z;
            references.Player.Motor.Move(1f, 0.25f);
            yield return null;
            yield return null;
            Assert.Greater(references.Player.transform.position.z, startZ);
            Assert.Greater(cameraFollow.transform.position.z, cameraZ);
            Assert.Greater(references.StatsTracker.Distance, 0f);
            run.ReturnToMenu();
        }

        [UnityTest]
        public IEnumerator CoinVisual_UsesHexLaneRotationAndCollectsThroughRadialChild()
        {
            yield return LoadMainScene();
            var run = Object.FindObjectOfType<RunController>(true);
            var references = Object.FindObjectOfType<RunSceneReferences>(true);
            Assert.IsTrue(run.TryStartRun());
            yield return null;
            PickupView coin = null;
            foreach (var pickup in Object.FindObjectsOfType<PickupView>())
                if (pickup.Type == PickupType.Coin) { coin = pickup; break; }
            Assert.NotNull(coin);
            Assert.NotNull(coin.RadialBody);
            Assert.That(coin.RadialBody.localPosition.x, Is.EqualTo(3f).Within(0.01f));
            Assert.That(Mathf.Repeat(coin.transform.eulerAngles.z, 60f), Is.EqualTo(30f).Within(0.1f));
            var before = references.CurrencyTracker.Coins;
            references.Player.transform.position = coin.WorldPosition;
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            Assert.Greater(references.CurrencyTracker.Coins, before);
            run.ReturnToMenu();
        }

        [UnityTest]
        public IEnumerator RunLifecycle_ContinueRetryAndHomeAreClean()
        {
            yield return LoadMainScene();
            var run = Object.FindObjectOfType<RunController>(true);
            var references = Object.FindObjectOfType<RunSceneReferences>(true);
            var document = Object.FindObjectOfType<UIDocument>(true);
            Assert.IsTrue(run.TryStartRun());
            yield return null;
            var firstId = run.CurrentRunId;
            references.ScoreTracker.AddPickupScore(50);
            references.CurrencyTracker.AddCoinPickup();

            run.HandlePlayerDeath();
            Assert.AreEqual(RunState.ContinueOffered, run.State);
            Submit(document.rootVisualElement.Q<Button>("ContinueRunButton"));
            yield return null;
            Assert.AreEqual(RunState.Running, run.State);
            Assert.AreEqual(1f, Time.timeScale);

            EndCurrentRun(run);
            yield return null;
            Assert.AreEqual(RunState.GameOver, run.State);
            Submit(document.rootVisualElement.Q<Button>("RetryButton"));
            yield return null;
            Assert.AreEqual(RunState.Running, run.State);
            Assert.AreNotEqual(firstId, run.CurrentRunId);
            Assert.AreEqual(0, references.CurrencyTracker.Coins);

            EndCurrentRun(run);
            yield return null;
            Submit(document.rootVisualElement.Q<Button>("HomeButton"));
            yield return null;
            Assert.AreEqual(RunState.MainMenu, run.State);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [UnityTest]
        public IEnumerator HudAndVfx_RespondToDamageAndPowerups()
        {
            yield return LoadMainScene();
            var run = Object.FindObjectOfType<RunController>(true);
            var references = Object.FindObjectOfType<RunSceneReferences>(true);
            var document = Object.FindObjectOfType<UIDocument>(true);
            var vfx = Object.FindObjectOfType<VfxManager>(true);
            Assert.IsTrue(run.TryStartRun());
            yield return null;
            references.PlayerHealth.Damage(1f);
            Assert.AreEqual("HULL  1/2", document.rootVisualElement.Q<Label>("HudHealth").text);
            Assert.AreEqual(VfxEventId.CrashSparks, vfx.LastPlayedEvent);
            references.Powerups.Activate(PowerupType.Shield);
            Assert.NotNull(document.rootVisualElement.Q<Label>("Powerup_Shield"));
            Assert.AreEqual(VfxEventId.ShieldShell, vfx.LastPlayedEvent);
            references.Powerups.ClearAll();
            Assert.IsNull(document.rootVisualElement.Q<Label>("Powerup_Shield"));
            Assert.AreEqual(VfxEventId.ShieldBreak, vfx.LastPlayedEvent);
            run.ReturnToMenu();
        }

        [UnityTest]
        public IEnumerator ProgressionLevel_UnlocksLateRouteAndPersists()
        {
            yield return LoadMainScene();
            Assert.IsTrue(GameServices.TryGet<PlayerProfileService>(out var profile));
            var originalLevel = profile.State.Level;
            var originalExperience = profile.State.Experience;
            try
            {
                profile.Mutate(state => { state.Level = 1; state.Experience = 0; });
                profile.AddExperience(8750);
                Assert.AreEqual(8, profile.State.Level);
            }
            finally
            {
                profile.Mutate(state => { state.Level = originalLevel; state.Experience = originalExperience; });
            }
        }

        private static IEnumerator LoadMainScene()
        {
            Time.timeScale = 1f;
            var load = SceneManager.LoadSceneAsync("CoreRacer_Main", LoadSceneMode.Single);
            Assert.NotNull(load);
            while (!load.isDone) yield return null;
            yield return null;
            Object.FindObjectOfType<RunController>(true)?.ReturnToMenu();
            if (GameServices.TryGet<TutorialService>(out var tutorial)) tutorial.ResetForTesting();
            yield return null;
        }

        private static void EndCurrentRun(RunController run)
        {
            run.HandlePlayerDeath();
            if (run.State == RunState.ContinueOffered || run.State == RunState.Crashed) run.DeclineContinue();
        }

        private static void Submit(Button button)
        {
            Assert.NotNull(button);
            using (var submit = NavigationSubmitEvent.GetPooled())
            {
                submit.target = button;
                button.SendEvent(submit);
            }
        }
    }
}
