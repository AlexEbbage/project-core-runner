using System;
using System.Collections.Generic;
using CoreRacer.UI.Toolkit;
using NUnit.Framework;
using UnityEditor;
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
        public void RootUxml_SatisfiesEveryModularViewContract()
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/CoreRacer/Runtime/UI/Toolkit/CoreRacerUiRoot.uxml");
            Assert.NotNull(tree);
            var root = tree.CloneTree();

            Assert.DoesNotThrow(() => new MenuShellView(root.Require<VisualElement>("MainMenuScreen")));
            Assert.DoesNotThrow(() => new PlayScreenView(root.Require<VisualElement>("PlayScreen")));
            Assert.DoesNotThrow(() => new ShopScreenView(root.Require<VisualElement>("ShopScreen")));
            Assert.DoesNotThrow(() => new HangarScreenView(root.Require<VisualElement>("HangarScreen")));
            Assert.DoesNotThrow(() => new LabScreenView(root.Require<VisualElement>("LabScreen")));
            Assert.DoesNotThrow(() => new ProgressionScreenView(root.Require<VisualElement>("ProgressionScreen")));
            Assert.DoesNotThrow(() => new SettingsScreenView(root.Require<VisualElement>("SettingsScreen")));
            Assert.DoesNotThrow(() => new GameplayHudView(root.Require<VisualElement>("HudLayer")));
            Assert.DoesNotThrow(() => new RunOverlayView(root));
        }

        [Test]
        public void Router_ShowsExactlyOneScreenAndUpdatesNavigationState()
        {
            var play = new VisualElement();
            var shop = new VisualElement();
            var playNav = new Button();
            var shopNav = new Button();
            var animations = new ImmediateAnimations();
            var playPresenter = new FakePresenter(CoreRacerScreenId.Play, play, animations);
            var shopPresenter = new FakePresenter(CoreRacerScreenId.Shop, shop, animations);
            var router = new CoreRacerScreenRouter(
                new Dictionary<CoreRacerScreenId, IUiScreenPresenter>
                {
                    [CoreRacerScreenId.Play] = playPresenter,
                    [CoreRacerScreenId.Shop] = shopPresenter
                },
                new Dictionary<CoreRacerScreenId, Button>
                {
                    [CoreRacerScreenId.Play] = playNav,
                    [CoreRacerScreenId.Shop] = shopNav
                });

            router.Show(CoreRacerScreenId.Shop);

            Assert.AreEqual(CoreRacerScreenId.Shop, router.Current);
            Assert.IsTrue(play.ClassListContains("is-hidden"));
            Assert.IsFalse(shop.ClassListContains("is-hidden"));
            Assert.IsFalse(playNav.ClassListContains("is-selected"));
            Assert.IsTrue(shopNav.ClassListContains("is-selected"));
            Assert.AreSame(shop, animations.LastShown);
        }

        private sealed class FakePresenter : UiScreenPresenterBase
        {
            public FakePresenter(CoreRacerScreenId id, VisualElement root, IUiAnimationService animations)
                : base(id, root, animations) { }

            public override void Refresh() { }
            protected override void OnInitialize() { }
            protected override void OnDispose() { }
        }

        private sealed class ImmediateAnimations : IUiAnimationService
        {
            public bool ReducedMotion { get; set; }
            public VisualElement LastShown { get; private set; }
            public void ShowScreen(VisualElement element) { LastShown = element; element.RemoveFromClassList("is-hidden"); }
            public void ShowPopup(VisualElement element) { }
            public void HidePopup(VisualElement element) { element.AddToClassList("is-hidden"); }
            public void ShowBottomSheet(VisualElement element) { ShowPopup(element); }
            public void HideBottomSheet(VisualElement element) { HidePopup(element); }
            public void PlayInvalidAction(VisualElement element) { }
            public void PlaySuccess(VisualElement element) { }
            public void PlayAttention(VisualElement element) { }
            public void ShowToast(VisualElement element) { }
            public void Stop(VisualElement element) { }
            public void StopAll() { }
        }
    }
}
