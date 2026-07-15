using System.Collections;
using CoreRacer.Gameplay.Run;
using CoreRacer.UI.MainMenu;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CoreRacer.Tests.PlayMode
{
    public sealed class CoreRunPlayModeSmokeTests
    {
        [UnityTest]
        public IEnumerator VisiblePlay_StartsCoreGameplay()
        {
            Time.timeScale = 1f;
            var load = SceneManager.LoadSceneAsync("CoreRacer_Main", LoadSceneMode.Single);
            Assert.NotNull(load, "CoreRacer_Main must be present in build settings for the PlayMode smoke test.");
            while (!load.isDone)
                yield return null;
            yield return null;

            var play = Object.FindObjectOfType<BottomNavBarController>(true);
            var run = Object.FindObjectOfType<RunController>(true);
            var references = Object.FindObjectOfType<RunSceneReferences>(true);
            Assert.NotNull(play, "The visible Play controller is missing.");
            Assert.NotNull(run, "The run controller is missing.");
            Assert.NotNull(references, "The run references are missing.");

            play.StartCoreRun();
            yield return null;

            Assert.AreEqual(RunState.Running, run.State);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsTrue(references.Player.gameObject.activeInHierarchy);
            Assert.IsFalse(references.MainMenu.gameObject.activeInHierarchy);
            Assert.IsTrue(references.Hud.gameObject.activeInHierarchy);

            run.ReturnToMenu();
            Time.timeScale = 1f;
        }
    }
}
