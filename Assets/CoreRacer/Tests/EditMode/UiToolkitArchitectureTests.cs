using System;
using System.Collections.Generic;
using CoreRacer.UI.Toolkit;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace CoreRacer.Tests.EditMode
{
    public sealed class UiToolkitArchitectureTests
    {
        [Test]
        public void Require_ThrowsClearErrorForBrokenViewContract()
        {
            var root = new VisualElement();
            var error = Assert.Throws<InvalidOperationException>(() => root.Require<Button>("MissingAction"));
            StringAssert.Contains("MissingAction", error.Message);
        }

        [Test]
        public void Router_ShowsExactlyOneScreenAndUpdatesNavigationState()
        {
            var play = new VisualElement();
            var shop = new VisualElement();
            var playNav = new Button();
            var shopNav = new Button();
            var animations = new ImmediateAnimations();
            var router = new CoreRacerScreenRouter(
                new Dictionary<CoreRacerScreenId, VisualElement>
                {
                    [CoreRacerScreenId.Play] = play,
                    [CoreRacerScreenId.Shop] = shop
                },
                new Dictionary<CoreRacerScreenId, Button>
                {
                    [CoreRacerScreenId.Play] = playNav,
                    [CoreRacerScreenId.Shop] = shopNav
                },
                animations);

            router.Show(CoreRacerScreenId.Shop);

            Assert.AreEqual(CoreRacerScreenId.Shop, router.Current);
            Assert.IsTrue(play.ClassListContains("is-hidden"));
            Assert.IsFalse(shop.ClassListContains("is-hidden"));
            Assert.IsFalse(playNav.ClassListContains("is-selected"));
            Assert.IsTrue(shopNav.ClassListContains("is-selected"));
            Assert.AreSame(shop, animations.LastShown);
        }

        private sealed class ImmediateAnimations : IUiAnimationService
        {
            public bool ReducedMotion { get; set; }
            public VisualElement LastShown { get; private set; }
            public void ShowScreen(VisualElement element) { LastShown = element; element.RemoveFromClassList("is-hidden"); }
            public void ShowPopup(VisualElement element) { }
            public void HidePopup(VisualElement element) { }
            public void PlayInvalidAction(VisualElement element) { }
            public void PlaySuccess(VisualElement element) { }
            public void ShowToast(VisualElement element) { }
            public void Stop(VisualElement element) { }
            public void StopAll() { }
        }
    }
}
