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
using CoreRacer.Services.Audio;
using CoreRacer.UI.MainMenu;
using CoreRacer.UI.Shared;
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
        private static readonly string[] TutorialSaveKeys =
        {
            "core_racer_tutorial_state",
            "core_racer_tutorial_state.checksum",
            "core_racer_tutorial_state.backup",
            "core_racer_tutorial_state.backup.checksum"
        };

        private readonly bool[] _tutorialSaveExisted = new bool[TutorialSaveKeys.Length];
        private readonly string[] _tutorialSaveValues = new string[TutorialSaveKeys.Length];

        [UnitySetUp]
        public IEnumerator PreserveTutorialSave()
        {
            for (var i = 0; i < TutorialSaveKeys.Length; i++)
            {
                _tutorialSaveExisted[i] = PlayerPrefs.HasKey(TutorialSaveKeys[i]);
                _tutorialSaveValues[i] = PlayerPrefs.GetString(TutorialSaveKeys[i], string.Empty);
            }

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator RestoreTutorialSave()
        {
            for (var i = 0; i < TutorialSaveKeys.Length; i++)
            {
                if (_tutorialSaveExisted[i])
                    PlayerPrefs.SetString(TutorialSaveKeys[i], _tutorialSaveValues[i]);
                else
                    PlayerPrefs.DeleteKey(TutorialSaveKeys[i]);
            }

            PlayerPrefs.Save();
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator FirstRunTutorial_WaitsForCrashAndSuccessfulContinue()
        {
            yield return LoadMainScene();

            var run = Object.FindObjectOfType<RunController>(true);
            var references = Object.FindObjectOfType<RunSceneReferences>(true);
            Assert.NotNull(run);
            Assert.NotNull(references);
            Assert.IsTrue(GameServices.TryGet<TutorialService>(out var tutorial));

            tutorial.ResetForTesting();
            tutorial.Start();
            Assert.AreEqual("welcome", tutorial.CurrentStep.Id);

            run.StartRun();
            yield return null;
            Assert.AreEqual("move", tutorial.CurrentStep.Id);

            tutorial.Notify(TutorialStepKind.WaitForInput, "player");
            tutorial.Notify(TutorialStepKind.WaitForObstacleAvoided, "obstacle");
            tutorial.Notify(TutorialStepKind.WaitForPickup, "coin");
            tutorial.Notify(TutorialStepKind.WaitForPowerup, "powerup");
            Assert.AreEqual("crash_continue_explanation", tutorial.CurrentStep.Id);
            Assert.AreEqual(TutorialStepKind.WaitForCrash, tutorial.CurrentStep.Kind);

            run.HandlePlayerDeath();
            Assert.AreEqual(RunState.ContinueOffered, run.State);
            Assert.AreEqual("continue_first_run", tutorial.CurrentStep.Id);
            Assert.AreEqual(TutorialStepKind.WaitForContinue, tutorial.CurrentStep.Kind);

            var continueButton = FindButton(references.GameOver.transform, "ContinueButton");
            Assert.NotNull(continueButton);
            continueButton.onClick.Invoke();
            yield return null;

            Assert.AreEqual(RunState.Running, run.State);
            Assert.IsTrue(tutorial.State.Completed, "The gameplay tutorial should complete only after Continue restores the run.");

            run.ReturnToMenu();
            tutorial.ResetForTesting();
        }

        [UnityTest]
        public IEnumerator CoinVisual_UsesHexLaneRotationCorrection()
        {
            yield return LoadMainScene();

            var run = Object.FindObjectOfType<RunController>(true);
            var references = Object.FindObjectOfType<RunSceneReferences>(true);
            run.StartRun();
            yield return null;

            var pickups = Object.FindObjectsOfType<PickupView>();
            PickupView coin = null;
            for (var i = 0; i < pickups.Length; i++)
            {
                if (pickups[i].Type == PickupType.Coin)
                {
                    coin = pickups[i];
                    break;
                }
            }

            Assert.NotNull(coin, "The coin pool must contain the authored coin prefab.");
            Assert.NotNull(coin.RadialBody, "The coin needs a radial child so its lane placement is visible in the hierarchy.");
            Assert.NotNull(coin.RadialBody.GetComponent<SphereCollider>(), "The coin trigger must move with its radial body.");
            Assert.NotNull(coin.RadialBody.GetComponent<PickupTriggerRelay>(), "The radial trigger must relay collection to the pooled pickup root.");
            Assert.That(coin.transform.position.x, Is.EqualTo(0f).Within(0.01f));
            Assert.That(coin.transform.position.y, Is.EqualTo(0f).Within(0.01f));
            Assert.That(coin.RadialBody.localPosition.x, Is.EqualTo(3f).Within(0.01f));
            Assert.That(coin.RadialBody.localPosition.y, Is.EqualTo(0f).Within(0.01f));
            Assert.That(coin.RadialBody.localPosition.z, Is.EqualTo(0f).Within(0.01f));
            var normalizedAngle = Mathf.Repeat(coin.transform.eulerAngles.z, 60f);
            Assert.That(normalizedAngle, Is.EqualTo(30f).Within(0.1f), "Coins should occupy hex wall centres, not tunnel corners.");
            var visual = coin.RadialBody.Find("Visual");
            Assert.NotNull(visual, "The coin prefab must keep its Visual child.");
            Assert.That(Mathf.DeltaAngle(0f, visual.localEulerAngles.z), Is.EqualTo(0f).Within(0.1f));

            var coinsBefore = references.CurrencyTracker.Coins;
            references.Player.transform.position = coin.WorldPosition;
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            Assert.That(references.CurrencyTracker.Coins, Is.GreaterThan(coinsBefore), "The offset child trigger must still collect the coin.");

            run.ReturnToMenu();
            if (GameServices.TryGet<TutorialService>(out var tutorial))
                tutorial.ResetForTesting();
        }

        [UnityTest]
        public IEnumerator PortraitHud_ShowsRunMetricsPowerupsAndPauseSafeState()
        {
            yield return LoadMainScene();

            var run = Object.FindObjectOfType<RunController>(true);
            var references = Object.FindObjectOfType<RunSceneReferences>(true);
            var safeArea = Object.FindObjectOfType<SafeAreaRectTransform>(true);
            Assert.NotNull(safeArea, "The gameplay HUD must be contained by the authored safe-area root.");
            Assert.AreSame(safeArea.transform, references.Hud.transform.parent);
            Assert.NotNull(references.Hud.ScoreText);
            Assert.NotNull(references.Hud.DistanceText);
            Assert.NotNull(references.Hud.CoinsText);
            Assert.NotNull(references.Hud.HealthText);
            Assert.NotNull(references.Hud.PowerupStrip);

            run.StartRun();
            yield return null;

            Assert.That(references.Hud.ScoreText.text, Does.StartWith("SCORE"));
            Assert.That(references.Hud.DistanceText.text, Does.StartWith("DIST"));
            Assert.That(references.Hud.CoinsText.text, Does.StartWith("COINS"));
            Assert.That(references.Hud.HealthText.text, Is.Empty, "Full hull should not consume HUD space during normal play.");
            references.PlayerHealth.Damage(1f);
            Assert.That(references.Hud.HealthText.text, Is.EqualTo("HULL  1/2"), "Hull should appear when the ship is damaged.");

            references.Powerups.Activate(PowerupType.Shield);
            Assert.AreEqual(1, references.Hud.PowerupStrip.ActiveCount);
            Assert.That(references.Hud.PowerupStrip.DisplayText, Does.Contain("SHIELD"));
            Assert.IsTrue(references.Powerups.TryGetRemainingSeconds(PowerupType.Shield, out var beforePause));

            references.PauseMenu.TogglePause();
            Assert.AreEqual(RunState.Paused, run.State);
            Assert.AreEqual(0f, Time.timeScale);
            Assert.IsTrue(references.PauseMenu.gameObject.activeInHierarchy);
            yield return new WaitForSecondsRealtime(0.25f);

            Assert.IsTrue(references.Powerups.TryGetRemainingSeconds(PowerupType.Shield, out var duringPause));
            Assert.That(duringPause, Is.EqualTo(beforePause).Within(0.02f), "Powerups must not expire behind a paused HUD.");

            references.PauseMenu.Resume();
            yield return null;
            Assert.AreEqual(RunState.Running, run.State);
            Assert.AreEqual(1f, Time.timeScale);

            run.ReturnToMenu();
        }

        [UnityTest]
        public IEnumerator CoreAudio_RoutesMenuRunObstacleShieldAndDamageEvents()
        {
            yield return LoadMainScene();

            var host = Object.FindObjectOfType<AudioRuntimeHost>(true);
            var play = Object.FindObjectOfType<BottomNavBarController>(true);
            var run = Object.FindObjectOfType<RunController>(true);
            var references = Object.FindObjectOfType<RunSceneReferences>(true);
            Assert.NotNull(host, "The bootstrap audio host is missing.");
            Assert.IsTrue(host.IsBound, "The bootstrap audio host did not bind its music and SFX sources.");
            Assert.IsTrue(GameServices.TryGet<AudioService>(out var audio));
            Assert.AreEqual(AudioEventId.MenuMusic, audio.LastPlayedEventId);
            Assert.AreEqual("MenuTrack", host.MusicSource.clip.name);

            play.StartCoreRun();
            yield return null;

            Assert.AreEqual(AudioEventId.RunMusic, audio.LastPlayedEventId);
            Assert.AreEqual("Zone1Music", host.MusicSource.clip.name);
            var firstRing = references.ObstacleWorld.ActiveRings[0];
            var playerCollider = references.Player.GetComponent<Collider>();
            Assert.NotNull(playerCollider);
            playerCollider.enabled = false;
            var deltaTime = (firstRing.Z + 1f - references.Player.transform.position.z) /
                            Mathf.Max(0.01f, references.Player.Motor.EffectiveForwardSpeed);
            references.Player.Motor.Move(0f, deltaTime);
            yield return null;
            playerCollider.enabled = true;
            Assert.AreEqual(AudioEventId.ObstaclePassed, audio.LastPlayedEventId);

            references.PlayerHealth.Damage(0.1f);
            Assert.AreEqual(AudioEventId.PlayerHit, audio.LastPlayedEventId);

            references.Powerups.Activate(PowerupType.Shield);
            Assert.AreEqual(AudioEventId.ShieldActivated, audio.LastPlayedEventId);
            references.Powerups.ClearAll();
            Assert.AreEqual(AudioEventId.ShieldBroken, audio.LastPlayedEventId);

            run.ReturnToMenu();
            yield return null;
            Assert.AreEqual(AudioEventId.MenuMusic, audio.LastPlayedEventId);
            Assert.AreEqual("MenuTrack", host.MusicSource.clip.name);
        }

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
        public IEnumerator AuthoredObstacleCollider_OverlapsOrbitAndRoutesDamage()
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

            Assert.Greater(references.ObstacleWorld.ActiveRings.Count, 0);
            var firstRing = references.ObstacleWorld.ActiveRings[0];
            var obstacleCollider = System.Array.Find(
                firstRing.GetComponentsInChildren<Collider>(false),
                collider => collider.CompareTag("Obstacle"));
            var playerCollider = references.Player.GetComponent<Collider>();
            Assert.NotNull(obstacleCollider, "The authored obstacle has no active Obstacle-tagged collider.");
            Assert.NotNull(playerCollider, "The player has no collider for obstacle interaction.");
            Physics.SyncTransforms();

            var contactPosition = Vector3.zero;
            var contactRotation = Quaternion.identity;
            var foundContact = false;
            for (var angle = 0; angle < 360 && !foundContact; angle += 5)
            {
                var radians = angle * Mathf.Deg2Rad;
                var candidatePosition = new Vector3(
                    Mathf.Cos(radians) * 3f,
                    Mathf.Sin(radians) * 3f,
                    obstacleCollider.bounds.center.z);
                var candidateRotation = Quaternion.Euler(0f, 0f, angle + 90f);
                foundContact = Physics.ComputePenetration(
                    playerCollider,
                    candidatePosition,
                    candidateRotation,
                    obstacleCollider,
                    obstacleCollider.transform.position,
                    obstacleCollider.transform.rotation,
                    out _,
                    out _);
                if (foundContact)
                {
                    contactPosition = candidatePosition;
                    contactRotation = candidateRotation;
                }
            }

            Assert.IsTrue(foundContact, "The fitted obstacle must intersect the player's three-unit orbit.");
            references.Player.EndRun();
            references.PlayerHealth.ResetHealth();
            references.Player.transform.SetPositionAndRotation(contactPosition, contactRotation);
            Physics.SyncTransforms();
            references.Player.gameObject.SendMessage("OnTriggerEnter", obstacleCollider, SendMessageOptions.RequireReceiver);
            yield return null;

            Assert.IsFalse(references.PlayerHealth.IsAlive, "Entering an authored obstacle trigger must damage the player lethally.");
            Assert.AreNotEqual(RunState.Running, run.State, "A lethal authored-obstacle collision must leave the running state.");
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
            Assert.AreEqual(2, tunnel.GeneratedMesh.subMeshCount, "The MVP tunnel must alternate two wall shades.");
            Assert.Greater(tunnel.WallTint.grayscale, 0.7f, "The primary MVP tunnel shade must remain white-ish.");
            Assert.Greater(tunnel.AlternateWallTint.grayscale, 0.55f, "The alternate MVP tunnel shade must remain white-ish.");
            Assert.Greater(tunnel.WallTint.grayscale, tunnel.AlternateWallTint.grayscale, "The two MVP tunnel shades must remain visually distinct.");
            var tunnelProperties = new MaterialPropertyBlock();
            tunnel.GetComponent<MeshRenderer>().GetPropertyBlock(tunnelProperties, 0);
            Assert.Less(Vector4.Distance(tunnel.WallTint, tunnelProperties.GetColor("_Color")), 0.001f, "The readability tint must be applied to the runtime renderer without changing its shared material.");
            Assert.NotNull(references.ObstacleWorld, "The obstacle world is missing.");
            Assert.Greater(references.ObstacleWorld.ActiveRings.Count, 0, "Starting a run must generate obstacle groups ahead of the player.");
            Assert.IsTrue(references.ObstacleWorld.ActiveRings[0].UsesAuthoredObstacle, "The first obstacle group must use the recovered authored obstacle prefab instead of cube segments.");
            Assert.AreEqual(4f / 7f, references.ObstacleWorld.ActiveRings[0].AuthoredObstacleScale, 0.0001f,
                "Authored obstacle meshes must use the preserved tunnel mesh radius to touch the radius-four tunnel wall.");
            Assert.Less(Mathf.Abs(Mathf.DeltaAngle(references.ObstacleWorld.ActiveRings[0].transform.eulerAngles.z % 60f, 30f)), 0.01f,
                "Authored obstacles must correct the thirty-degree alignment difference between the old and procedural hex tunnels.");
            Assert.AreEqual("wedge_easy", references.ObstacleWorld.ActiveRings[0].PatternId, "The starter difficulty must begin with a readable wedge pattern.");
            Assert.GreaterOrEqual(references.ObstacleWorld.ActiveRings.Count, 2, "The starter group must contain enough rings to establish a readable lane.");
            Assert.Less(
                Mathf.Abs(Mathf.DeltaAngle(
                    references.ObstacleWorld.ActiveRings[0].transform.eulerAngles.z,
                    references.ObstacleWorld.ActiveRings[1].transform.eulerAngles.z)),
                0.01f,
                "Rings inside one obstacle group must preserve the same safe-lane orientation.");
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
        public IEnumerator RunVfx_RespondsToDamageShieldAndSpeed()
        {
            yield return LoadMainScene();

            var run = Object.FindObjectOfType<RunController>(true);
            var references = Object.FindObjectOfType<RunSceneReferences>(true);
            var vfx = Object.FindObjectOfType<VfxManager>(true);
            var speedParticles = Object.FindObjectOfType<SpeedParticlesControllerV2>(true);
            Assert.NotNull(run);
            Assert.NotNull(references);
            Assert.NotNull(vfx);
            Assert.NotNull(speedParticles);

            Assert.IsTrue(run.TryStartRun());
            yield return null;
            Assert.AreEqual(RunState.Running, run.State);

            references.PlayerHealth.Damage(0.1f);
            Assert.IsTrue(vfx.LastPlayedEvent.HasValue);
            Assert.AreEqual(VfxEventId.CrashSparks, vfx.LastPlayedEvent.Value);

            references.Powerups.Activate(PowerupType.Shield);
            Assert.AreEqual(VfxEventId.ShieldShell, vfx.LastPlayedEvent);
            references.Powerups.ClearAll();
            Assert.AreEqual(VfxEventId.ShieldBreak, vfx.LastPlayedEvent);
            Assert.GreaterOrEqual(speedParticles.Intensity, 0f);

            run.ReturnToMenu();
        }

        [UnityTest]
        public IEnumerator RunLifecycle_GameOverRetryHomeAndPlayAgainAreCleanAndIdempotent()
        {
            yield return LoadMainScene();

            var play = Object.FindObjectOfType<BottomNavBarController>(true);
            var run = Object.FindObjectOfType<RunController>(true);
            var references = Object.FindObjectOfType<RunSceneReferences>(true);
            var cameraFollow = Object.FindObjectOfType<PlayerCameraFollow>(true);
            Assert.NotNull(play);
            Assert.NotNull(run);
            Assert.NotNull(references);
            Assert.NotNull(cameraFollow);

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

        [UnityTest]
        public IEnumerator RunRewards_ContinueDoubleRewardRetryAndProfileSettlementAreClean()
        {
            yield return LoadMainScene();

            var play = Object.FindObjectOfType<BottomNavBarController>(true);
            var run = Object.FindObjectOfType<RunController>(true);
            var references = Object.FindObjectOfType<RunSceneReferences>(true);
            Assert.NotNull(play);
            Assert.NotNull(run);
            Assert.NotNull(references);
            Assert.IsTrue(GameServices.TryGet<PlayerProfileService>(out var profile));

            var originalLevel = profile.State.Level;
            var originalExperience = profile.State.Experience;
            var originalSoft = profile.State.Wallet.Soft;
            var originalPremium = profile.State.Wallet.Premium;
            var originalTotalRuns = profile.State.TotalRuns;
            var originalTotalCoins = profile.State.TotalCoinsCollected;
            var originalTotalPowerups = profile.State.TotalPowerupsCollected;
            var originalBestScore = profile.State.BestScore;
            var originalBestDistance = profile.State.BestDistance;
            var originalSelectedIndex = profile.State.SelectedLevelIndex;
            var originalBoosters = new System.Collections.Generic.List<string>(profile.State.EquippedBoosterIds);
            var cameraFollow = Object.FindObjectOfType<PlayerCameraFollow>(true);
            Assert.NotNull(cameraFollow, "The player camera follow component is missing.");

            try
            {
                play.StartCoreRun();
                yield return null;
                Assert.AreEqual(RunState.Running, run.State);

                references.ScoreTracker.AddPickupScore(100);
                references.CurrencyTracker.AddCoinPickup(10);
                run.HandlePlayerDeath();
                Assert.AreEqual(RunState.ContinueOffered, run.State, "A first death should offer a continue.");
                Assert.AreEqual(0.2f, Time.timeScale, 0.01f, "Crash presentation should use slow motion instead of freezing time.");
                Assert.IsFalse(cameraFollow.IsFollowing, "Crash presentation should stop camera follow movement.");
                var continueButton = FindButton(references.GameOver.transform, "ContinueButton");
                Assert.NotNull(continueButton);
                Assert.AreEqual(0, continueButton.onClick.GetPersistentEventCount(), "Continue must be routed by the runtime Game Over controller, not a stale serialized listener.");
                continueButton.onClick.Invoke();
                yield return null;
                Assert.AreEqual(RunState.Running, run.State);
                Assert.AreEqual(1f, Time.timeScale);
                Assert.IsTrue(cameraFollow.IsFollowing, "Continue should restore camera follow movement.");

                references.ScoreTracker.AddPickupScore(100);
                references.CurrencyTracker.AddCoinPickup(10);
                run.HandlePlayerDeath();
                Assert.AreEqual(RunState.ContinueOffered, run.State);
                run.DeclineContinue();
                yield return null;
                Assert.AreEqual(RunState.GameOver, run.State);

                var settled = run.LastResult;
                Assert.Greater(settled.Coins, 0);
                Assert.Greater(settled.Experience, 0);
                Assert.AreEqual(originalSoft + settled.Coins, profile.State.Wallet.Soft);
                Assert.AreEqual(originalPremium + settled.PremiumCurrency, profile.State.Wallet.Premium);
                Assert.AreEqual(originalTotalRuns + 1, profile.State.TotalRuns);
                Assert.AreEqual(originalTotalCoins + settled.Coins, profile.State.TotalCoinsCollected);
                Assert.AreEqual(originalTotalPowerups, profile.State.TotalPowerupsCollected);
                Assert.IsTrue(profile.State.Level != originalLevel || profile.State.Experience != originalExperience, "Base run XP must persist to the profile.");

                var afterBaseLevel = profile.State.Level;
                var afterBaseExperience = profile.State.Experience;
                Assert.IsTrue(run.DoubleRunRewards(), "The development rewarded provider should complete Double Rewards.");
                yield return null;
                Assert.AreEqual(originalSoft + settled.Coins * 2, profile.State.Wallet.Soft);
                Assert.AreEqual(originalPremium + settled.PremiumCurrency * 2, profile.State.Wallet.Premium);
                Assert.IsTrue(profile.State.Level != afterBaseLevel || profile.State.Experience != afterBaseExperience, "Double Rewards must persist its XP bonus.");
                Assert.IsFalse(run.DoubleRunRewards(), "Double Rewards must be one-shot per run.");

                var retryButton = FindButton(references.GameOver.transform, "RetryButton");
                Assert.NotNull(retryButton);
                retryButton.onClick.Invoke();
                yield return null;
                Assert.AreEqual(RunState.Running, run.State);
                Assert.AreEqual(originalTotalRuns + 1, profile.State.TotalRuns, "Retry must not settle a second run before it ends.");
                Assert.AreEqual(originalSoft + settled.Coins * 2, profile.State.Wallet.Soft);

                run.ReturnToMenu();
                yield return null;
                Assert.AreEqual(RunState.MainMenu, run.State);
                Assert.AreEqual(1f, Time.timeScale);
            }
            finally
            {
                run.ReturnToMenu();
                profile.Mutate(state =>
                {
                    state.Level = originalLevel;
                    state.Experience = originalExperience;
                    state.Wallet.Soft = originalSoft;
                    state.Wallet.Premium = originalPremium;
                    state.TotalRuns = originalTotalRuns;
                    state.TotalCoinsCollected = originalTotalCoins;
                    state.TotalPowerupsCollected = originalTotalPowerups;
                    state.BestScore = originalBestScore;
                    state.BestDistance = originalBestDistance;
                    state.SelectedLevelIndex = originalSelectedIndex;
                    state.EquippedBoosterIds.Clear();
                    state.EquippedBoosterIds.AddRange(originalBoosters);
                });
                Time.timeScale = 1f;
            }
        }

        [UnityTest]
        public IEnumerator RoadmapAndBoosters_PersistSelectionAndApplyOnlyToTheRun()
        {
            yield return LoadMainScene();

            var levelSelect = Object.FindObjectOfType<LevelSelectPageController>(true);
            var boosterLoadout = Object.FindObjectOfType<BoosterLoadoutController>(true);
            var run = Object.FindObjectOfType<RunController>(true);
            var references = Object.FindObjectOfType<RunSceneReferences>(true);
            var tunnel = Object.FindObjectOfType<TunnelWallGeneratorV2>(true);
            var zones = Object.FindObjectOfType<RunZoneManagerV2>(true);
            Assert.NotNull(levelSelect, "The authored level-select controller is missing.");
            Assert.NotNull(boosterLoadout, "The authored pre-run booster loadout is missing.");
            Assert.NotNull(run);
            Assert.NotNull(references);
            Assert.NotNull(tunnel);
            Assert.NotNull(zones);
            Assert.IsTrue(GameServices.TryGet<PlayerProfileService>(out var profile));

            var originalLevel = profile.State.Level;
            var originalSelectedIndex = profile.State.SelectedLevelIndex;
            var originalBoosters = new System.Collections.Generic.List<string>(profile.State.EquippedBoosterIds);

            profile.Mutate(state =>
            {
                state.Level = 8;
                state.SelectedLevelIndex = 0;
                state.EquippedBoosterIds.Clear();
                state.EquippedBoosterIds.Add("start_shield");
                state.EquippedBoosterIds.Add("coin_boost");
                state.EquippedBoosterIds.Add("score_boost");
            });

            levelSelect.Refresh();
            boosterLoadout.Refresh();
            Assert.AreEqual(5, levelSelect.RouteCount, "The active roadmap must expose all five authored environments.");
            Assert.AreEqual(3, boosterLoadout.VisibleOptionCount, "The pre-run surface must expose the three authored booster families.");
            Assert.IsTrue(levelSelect.TrySelectLevel("deca_sector_05"), "The unlocked FIRESTORM environment must be selectable.");
            Assert.AreEqual(4, profile.State.SelectedLevelIndex);
            Assert.AreEqual("deca_sector_05", levelSelect.SelectedLevelId);

            Assert.IsTrue(levelSelect.TryPlaySelected());
            yield return null;

            Assert.AreEqual(RunState.Running, run.State);
            Assert.AreEqual(6, tunnel.Sides, "Every MVP environment must preserve the single six-sided tunnel type.");
            Assert.AreEqual("firestorm", zones.CurrentZoneId, "The selected environment must be applied to the run.");
            Assert.AreEqual(2f, run.ActiveBoosterModifiers.ScoreMultiplier);
            Assert.AreEqual(2f, run.ActiveBoosterModifiers.CoinMultiplier);
            Assert.AreEqual(1f, run.ActiveBoosterModifiers.StartShieldSeconds);
            Assert.AreEqual(2f, references.ScoreTracker.RunScoreMultiplier);
            Assert.AreEqual(2f, references.CurrencyTracker.RunCoinMultiplier);
            Assert.IsTrue(references.PlayerHealth.IsInvulnerable, "Start Shield must protect the player when the run begins.");

            var scoreBeforePickup = references.ScoreTracker.CurrentScore;
            references.ScoreTracker.AddPickupScore(10);
            references.CurrencyTracker.AddCoinPickup();
            Assert.GreaterOrEqual(references.ScoreTracker.CurrentScore - scoreBeforePickup, 20);
            Assert.GreaterOrEqual(references.CurrencyTracker.Coins, 2);

            run.ReturnToMenu();
            levelSelect.Refresh();
            Assert.AreEqual(RunState.MainMenu, run.State);
            Assert.AreEqual(1f, references.ScoreTracker.RunScoreMultiplier, "Score booster state must clear after the run.");
            Assert.AreEqual(1f, references.CurrencyTracker.RunCoinMultiplier, "Coin booster state must clear after the run.");
            Assert.AreEqual("deca_sector_05", levelSelect.SelectedLevelId, "The selected route must remain persisted on return to the hub.");

            profile.Mutate(state =>
            {
                state.Level = originalLevel;
                state.SelectedLevelIndex = originalSelectedIndex;
                state.EquippedBoosterIds.Clear();
                state.EquippedBoosterIds.AddRange(originalBoosters);
            });
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator ProgressionLevelUp_UnlocksLateRouteAndPersistsSelection()
        {
            yield return LoadMainScene();

            var levelSelect = Object.FindObjectOfType<LevelSelectPageController>(true);
            Assert.NotNull(levelSelect);
            Assert.IsTrue(GameServices.TryGet<PlayerProfileService>(out var profile));

            var originalLevel = profile.State.Level;
            var originalExperience = profile.State.Experience;
            var originalSelectedIndex = profile.State.SelectedLevelIndex;
            try
            {
                profile.Mutate(state =>
                {
                    state.Level = 1;
                    state.Experience = 0;
                    state.SelectedLevelIndex = 0;
                });
                levelSelect.Refresh();

                Assert.IsFalse(levelSelect.TrySelectLevel("deca_sector_05"), "The late route must remain locked at level 1.");

                profile.AddExperience(8750);
                levelSelect.Refresh();

                Assert.AreEqual(8, profile.State.Level);
                Assert.IsTrue(levelSelect.TrySelectLevel("deca_sector_05"), "Leveling to the authored threshold must unlock the late route.");
                Assert.AreEqual("deca_sector_05", levelSelect.SelectedLevelId);
                Assert.AreEqual(4, profile.State.SelectedLevelIndex);
            }
            finally
            {
                profile.Mutate(state =>
                {
                    state.Level = originalLevel;
                    state.Experience = originalExperience;
                    state.SelectedLevelIndex = originalSelectedIndex;
                });
            }
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
