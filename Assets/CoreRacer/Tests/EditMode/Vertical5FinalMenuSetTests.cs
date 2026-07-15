using System.Collections.Generic;
using System.Reflection;
using CoreRacer.UI.MainMenu;
using CoreRacer.UI.Shared;
using NUnit.Framework;
using UnityEngine;

namespace CoreRacer.Tests.EditMode
{
    public sealed class Vertical5FinalMenuSetTests
    {
        [Test]
        public void BottomNavigationPages_MatchFirstReleaseMenuSetInOrder()
        {
            var pages = FinalMenuSetRules.BottomNavigationPages;

            CollectionAssert.AreEqual(
                new[]
                {
                    MainMenuPage.Play,
                    MainMenuPage.Hangar,
                    MainMenuPage.Lab,
                    MainMenuPage.Shop,
                    MainMenuPage.Progression
                },
                pages);
        }

        [Test]
        public void Settings_IsTopLevelButNotBottomNavigation()
        {
            Assert.IsTrue(FinalMenuSetRules.IsTopLevelPage(MainMenuPage.Settings));
            Assert.IsFalse(FinalMenuSetRules.IsBottomNavigationPage(MainMenuPage.Settings));
            Assert.AreEqual(-1, FinalMenuSetRules.GetBottomNavigationIndex(MainMenuPage.Settings));
        }

        [Test]
        public void Router_ShowsOneBoundPageAndHidesTheOthers()
        {
            var routerGo = new GameObject("router");
            var playGo = new GameObject("play");
            var labGo = new GameObject("lab");
            var shopGo = new GameObject("shop");

            try
            {
                var router = routerGo.AddComponent<MainMenuPageRouter>();
                var play = playGo.AddComponent<UiView>();
                var lab = labGo.AddComponent<UiView>();
                var shop = shopGo.AddComponent<UiView>();
                SetPages(router, new List<MainMenuPageRouter.PageBinding>
                {
                    new MainMenuPageRouter.PageBinding { Page = MainMenuPage.Play, View = play },
                    new MainMenuPageRouter.PageBinding { Page = MainMenuPage.Lab, View = lab },
                    new MainMenuPageRouter.PageBinding { Page = MainMenuPage.Shop, View = shop }
                });

                Assert.IsTrue(router.TryShow(MainMenuPage.Lab));
                Assert.IsFalse(playGo.activeSelf);
                Assert.IsTrue(labGo.activeSelf);
                Assert.IsFalse(shopGo.activeSelf);
                Assert.AreEqual(MainMenuPage.Lab, router.CurrentPage);
            }
            finally
            {
                Object.DestroyImmediate(routerGo);
                Object.DestroyImmediate(playGo);
                Object.DestroyImmediate(labGo);
                Object.DestroyImmediate(shopGo);
            }
        }

        [Test]
        public void Router_ReportsMissingRequiredBottomPages()
        {
            var routerGo = new GameObject("router");
            var playGo = new GameObject("play");

            try
            {
                var router = routerGo.AddComponent<MainMenuPageRouter>();
                var play = playGo.AddComponent<UiView>();
                SetPages(router, new List<MainMenuPageRouter.PageBinding>
                {
                    new MainMenuPageRouter.PageBinding { Page = MainMenuPage.Play, View = play }
                });

                Assert.IsFalse(router.HasRequiredBottomNavigationPages());
            }
            finally
            {
                Object.DestroyImmediate(routerGo);
                Object.DestroyImmediate(playGo);
            }
        }

        private static void SetPages(MainMenuPageRouter router, List<MainMenuPageRouter.PageBinding> pages)
        {
            var field = typeof(MainMenuPageRouter).GetField("pages", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            field.SetValue(router, pages);
        }
    }
}
