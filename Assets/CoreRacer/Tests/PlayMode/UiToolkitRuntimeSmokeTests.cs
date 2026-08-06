using System.Collections;
using CoreRacer.Gameplay.Run;
using CoreRacer.UI.Toolkit;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace CoreRacer.Tests.PlayMode
{
    public sealed class UiToolkitRuntimeSmokeTests : IsolatedPlayerProfilePlayModeTest
    {
        [UnityTest]
        public IEnumerator VisiblePlayButton_StartsRunAndRunLifecycleReturnsToMenu()
        {
            SceneManager.LoadScene("CoreRacer_Main");
            yield return null;
            yield return null;

            var ui = Object.FindObjectOfType<CoreRacerUiController>(true);
            var document = Object.FindObjectOfType<UIDocument>(true);
            var run = Object.FindObjectOfType<RunController>(true);
            Assert.NotNull(ui);
            Assert.NotNull(document);
            Assert.NotNull(run);
            Assert.IsTrue(ui.IsInitialized);
            Assert.Zero(Object.FindObjectsOfType<Canvas>(true).Length, "The final runtime UI must not retain a competing uGUI Canvas.");

            Submit(document.rootVisualElement.Q<Button>("PlayButton"));
            yield return null;
            Assert.AreEqual(RunState.Running, run.State);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsTrue(document.rootVisualElement.Q<VisualElement>("ScreenLayer").ClassListContains("is-hidden"));
            Assert.IsFalse(document.rootVisualElement.Q<VisualElement>("HudLayer").ClassListContains("is-hidden"));

            run.HandlePlayerDeath();
            yield return null;
            Assert.AreEqual(RunState.ContinueOffered, run.State);
            Assert.IsFalse(document.rootVisualElement.Q<VisualElement>("GameOverPopup").ClassListContains("is-hidden"));

            Submit(document.rootVisualElement.Q<Button>("EndRunButton"));
            yield return null;
            Assert.AreEqual(RunState.GameOver, run.State);
            Submit(document.rootVisualElement.Q<Button>("RetryButton"));
            yield return null;
            Assert.AreEqual(RunState.Running, run.State);

            run.HandlePlayerDeath();
            run.DeclineContinue();
            yield return null;
            Submit(document.rootVisualElement.Q<Button>("HomeButton"));
            yield return null;
            Assert.AreEqual(RunState.MainMenu, run.State);
            Assert.IsFalse(document.rootVisualElement.Q<VisualElement>("ScreenLayer").ClassListContains("is-hidden"));
        }

        [UnityTest]
        public IEnumerator NavigationAndModal_HaveDeterministicStateAndBlockUnderlyingInput()
        {
            SceneManager.LoadScene("CoreRacer_Main");
            yield return null;
            yield return null;
            var ui = Object.FindObjectOfType<CoreRacerUiController>(true);
            var document = Object.FindObjectOfType<UIDocument>(true);
            var root = document.rootVisualElement;
            var play = root.Q<Button>("PlayButton");
            var picked = root.panel.Pick(play.worldBound.center);

            Assert.IsTrue(picked == play || play.Contains(picked), $"Desktop pointer hit '{picked?.name ?? "null"}' instead of PlayButton.");

            Submit(root.Q<Button>("NavShop"));
            Assert.AreEqual(CoreRacerScreenId.Shop, ui.CurrentScreen);
            Assert.IsFalse(root.Q<ScrollView>("ShopScreen").ClassListContains("is-hidden"));
            Assert.IsTrue(root.Q<ScrollView>("PlayScreen").ClassListContains("is-hidden"));

            Submit(root.Q<Button>("SettingsShortcutButton"));
            Submit(root.Q<Button>("PrivacyButton"));
            Assert.AreEqual(CoreRacerScreenId.Settings, ui.CurrentScreen);
            Assert.IsTrue(ui.IsModalOpen);
            Assert.AreEqual(PickingMode.Position, root.Q<VisualElement>("GenericModal").pickingMode);
            Submit(root.Q<Button>("ModalCloseButton"));
            Assert.IsFalse(ui.IsModalOpen);
        }

        [UnityTest]
        public IEnumerator LevelCarousel_ExposesOnePlayableMvpRouteAndOneLockedPreview()
        {
            SceneManager.LoadScene("CoreRacer_Main");
            yield return null;
            yield return null;
            var document = Object.FindObjectOfType<UIDocument>(true);
            var root = document.rootVisualElement;
            var previous = root.Q<Button>("PreviousZoneButton");
            var next = root.Q<Button>("NextZoneButton");
            var play = root.Q<Button>("PlayButton");

            Assert.IsTrue(previous.ClassListContains("is-invisible"));
            Assert.IsFalse(next.ClassListContains("is-invisible"));
            Assert.IsTrue(play.enabledSelf);
            Assert.AreEqual("CORE RUN", root.Q<Label>("SelectedLevelTitle").text);
            Assert.IsNull(root.Q<VisualElement>("LevelList"), "The old multi-route list must not be present in the MVP Play screen.");
            Assert.That(root.Q<VisualElement>("SelectedRunCard").worldBound.center.x, Is.EqualTo(root.worldBound.center.x).Within(1f));
            Assert.AreEqual("0", root.Q<Label>("SoftCurrencyLabel").text);
            Assert.AreEqual("0", root.Q<Label>("PremiumCurrencyLabel").text);
            Assert.AreEqual("0", root.Q<Label>("SelectedLevelHighScore").text);
            Assert.IsFalse(root.Q<Label>("LevelStarOne").ClassListContains("is-earned"));
            Assert.AreEqual(3, root.Q<VisualElement>("BoosterList").childCount);
            Assert.Less(root.Q<VisualElement>("BoosterList").worldBound.yMax, root.Q<VisualElement>("BottomNav").worldBound.yMin, "Boosters should be visible above the fixed navigation without scrolling.");

            Submit(next);
            yield return null;
            Assert.IsFalse(previous.ClassListContains("is-invisible"));
            Assert.IsTrue(next.ClassListContains("is-invisible"));
            Assert.IsFalse(play.enabledSelf);
            Assert.AreEqual("NEXT ZONE", root.Q<Label>("SelectedLevelTitle").text);
            StringAssert.Contains("Coming soon", root.Q<Label>("SelectedLevelDescription").text);

            Submit(previous);
            yield return null;
            Assert.IsTrue(play.enabledSelf);
            Assert.AreEqual("CORE RUN", root.Q<Label>("SelectedLevelTitle").text);
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
