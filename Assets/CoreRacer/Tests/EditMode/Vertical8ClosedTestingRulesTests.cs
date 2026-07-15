using System.Collections.Generic;
using CoreRacer.Services.Diagnostics;
using NUnit.Framework;

namespace CoreRacer.Tests.EditMode
{
    public sealed class Vertical8ClosedTestingRulesTests
    {
        [Test]
        public void BuildSceneRules_AllowOnlyCoreRacerMainAsEnabledScene()
        {
            var valid = new List<BuildSceneReadinessInfo>
            {
                new BuildSceneReadinessInfo(ClosedTestingReadinessRules.ExpectedMainScenePath, true)
            };

            var wrongScene = new List<BuildSceneReadinessInfo>
            {
                new BuildSceneReadinessInfo("Assets/Scenes/GameScene.unity", true)
            };

            var extraEnabledScene = new List<BuildSceneReadinessInfo>
            {
                new BuildSceneReadinessInfo(ClosedTestingReadinessRules.ExpectedMainScenePath, true),
                new BuildSceneReadinessInfo("Assets/Scenes/GameScene.unity", true)
            };

            Assert.IsTrue(ClosedTestingReadinessRules.HasOnlyExpectedEnabledScene(valid));
            Assert.IsFalse(ClosedTestingReadinessRules.HasOnlyExpectedEnabledScene(wrongScene));
            Assert.IsFalse(ClosedTestingReadinessRules.HasOnlyExpectedEnabledScene(extraEnabledScene));
        }

        [Test]
        public void BundleIdentifierRules_RejectDefaultsAndAcceptProductionStyleIds()
        {
            Assert.IsFalse(ClosedTestingReadinessRules.IsProductionBundleIdentifier(""));
            Assert.IsFalse(ClosedTestingReadinessRules.IsProductionBundleIdentifier("com.DefaultCompany.CoreRacer"));
            Assert.IsFalse(ClosedTestingReadinessRules.IsProductionBundleIdentifier("com.company.product"));
            Assert.IsFalse(ClosedTestingReadinessRules.IsProductionBundleIdentifier("core-racer"));
            Assert.IsTrue(ClosedTestingReadinessRules.IsProductionBundleIdentifier("com.alexebb.coreracer"));
        }

        [Test]
        public void VersionRules_RejectPrototypeDefaults()
        {
            Assert.IsFalse(ClosedTestingReadinessRules.IsReleaseVersionReady(""));
            Assert.IsFalse(ClosedTestingReadinessRules.IsReleaseVersionReady("1.0"));
            Assert.IsFalse(ClosedTestingReadinessRules.IsReleaseVersionReady("0.0.1"));
            Assert.IsTrue(ClosedTestingReadinessRules.IsReleaseVersionReady("0.2.0"));
            Assert.IsTrue(ClosedTestingReadinessRules.IsReleaseVersionReady("1.0.1"));

            Assert.IsFalse(ClosedTestingReadinessRules.IsClosedTestingVersionCodeReady(0));
            Assert.IsFalse(ClosedTestingReadinessRules.IsClosedTestingVersionCodeReady(1));
            Assert.IsTrue(ClosedTestingReadinessRules.IsClosedTestingVersionCodeReady(2));
        }

        [Test]
        public void UrlRules_RequireHttpsAndRejectPlaceholderDomains()
        {
            Assert.IsFalse(ClosedTestingReadinessRules.IsProductionSafeUrl("http://coreracer.example-studio.co.uk/privacy"));
            Assert.IsFalse(ClosedTestingReadinessRules.IsProductionSafeUrl("https://example.com/privacy"));
            Assert.IsFalse(ClosedTestingReadinessRules.IsProductionSafeUrl("https://localhost/privacy"));
            Assert.IsFalse(ClosedTestingReadinessRules.IsProductionSafeUrl("https://your-domain/privacy"));
            Assert.IsTrue(ClosedTestingReadinessRules.IsProductionSafeUrl("https://coreracer.alexebb.co.uk/privacy"));
        }

        [Test]
        public void ReadinessSnapshot_IsReadyOnlyWhenAllGatesPass()
        {
            var snapshot = new ClosedTestingReadinessSnapshot
            {
                BuildSceneIsCorrect = true,
                AndroidBuildTargetSelected = true,
                BundleIdentifierReady = true,
                BundleVersionReady = true,
                BundleVersionCodeReady = true,
                StoreLinksReady = true,
                RequiredConfigsReady = true,
                SceneWiringReady = true,
                MissingScriptsClear = true,
                SmokeTestsPresent = true,
                VerticalDocsPresent = true
            };

            Assert.IsTrue(snapshot.IsReady);
            snapshot.StoreLinksReady = false;
            Assert.IsFalse(snapshot.IsReady);
        }
    }
}
