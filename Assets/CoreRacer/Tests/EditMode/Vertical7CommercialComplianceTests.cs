using System;
using System.Collections.Generic;
using CoreRacer.Monetisation.Ads;
using CoreRacer.Monetisation.Commercial;
using CoreRacer.Monetisation.Iap;
using CoreRacer.Monetisation.Premium;
using CoreRacer.Services.Compliance;
using CoreRacer.Services.Save;
using NUnit.Framework;
using UnityEngine;

namespace CoreRacer.Tests.EditMode
{
    public sealed class Vertical7CommercialComplianceTests
    {
        [Test]
        public void PremiumBypass_IsGrantableRewardedOutcome()
        {
            var premium = new PremiumEntitlementService(new MemorySaveStorage());
            premium.GrantPremium();
            var policy = new AdPolicyService(premium);
            var controller = new RewardedAdController(null, policy, null);

            RewardedAdResult result = RewardedAdResult.FailedToShow;
            controller.ShowOrBypass(AdPlacement.ContinueRun, completed => result = completed);

            Assert.AreEqual(RewardedAdResult.BypassedByPremium, result);
            Assert.IsTrue(controller.ShouldGrantReward(result));
        }

        [Test]
        public void MissingRewardedProvider_ReturnsNotReadyAndDoesNotGrantReward()
        {
            var premium = new PremiumEntitlementService(new MemorySaveStorage());
            var policy = new AdPolicyService(premium);
            var controller = new RewardedAdController(null, policy, null);

            RewardedAdResult result = RewardedAdResult.Rewarded;
            controller.ShowOrBypass(AdPlacement.ContinueRun, completed => result = completed);

            Assert.AreEqual(RewardedAdResult.NotReady, result);
            Assert.IsFalse(controller.ShouldGrantReward(result));
        }

        [Test]
        public void IapPurchaseService_GrantsPremiumOnlyForKnownPremiumProduct()
        {
            var premium = new PremiumEntitlementService(new MemorySaveStorage());
            var iap = new IapPurchaseService(premium);
            var completed = new List<IapPurchaseResult>();
            iap.PurchaseCompleted += (_, result) => completed.Add(result);

            iap.CompletePurchase("unknown_product");
            Assert.IsFalse(premium.HasPremium);
            Assert.AreEqual(IapPurchaseResult.UnknownProduct, completed[0]);

            iap.CompletePurchase(IapProductIds.PremiumUser);
            Assert.IsTrue(premium.HasPremium);
            Assert.AreEqual(IapPurchaseResult.Success, completed[1]);
        }

        [Test]
        public void ComplianceRules_RejectPlaceholderStoreLinks()
        {
            Assert.IsFalse(CommercialComplianceRules.HasProductionSafeUrl("https://example.com/privacy"));
            Assert.IsFalse(CommercialComplianceRules.HasProductionSafeUrl(""));
            Assert.IsTrue(CommercialComplianceRules.HasProductionSafeUrl("https://coreracer.example-studio.co.uk/privacy"));
        }

        [Test]
        public void ReadinessSnapshot_RequiresPolicyLinksAndResolvedConsent()
        {
            var storage = new MemorySaveStorage();
            var links = ScriptableObject.CreateInstance<PrivacyLinksConfig>();
            try
            {
                links.PrivacyPolicyUrl = "https://coreracer.example-studio.co.uk/privacy";
                links.TermsUrl = "https://coreracer.example-studio.co.uk/terms";
                links.DataDeletionUrl = "https://coreracer.example-studio.co.uk/data-deletion";

                var consent = new ConsentService(storage, new JsonSaveSerializer(), links);
                consent.SetAnalyticsConsent(TrackingConsentState.Granted);
                consent.SetAdsPersonalizationConsent(TrackingConsentState.Denied);
                consent.AcceptPolicies("1.0", "1.0");

                var premium = new PremiumEntitlementService(storage);
                var iap = new IapPurchaseService(premium);
                iap.PurchaseRequested += _ => { };
                iap.RestoreRequested += () => { };
                iap.SetStoreAdapterAvailability(true);
                var readiness = new CommercialReadinessService(consent, premium, null, null, iap);

                var snapshot = readiness.BuildSnapshot();
                Assert.IsTrue(snapshot.StoreLinksConfigured);
                Assert.IsTrue(snapshot.ConsentResolved);
                Assert.IsTrue(snapshot.IapConfigured);
                Assert.IsTrue(snapshot.IsCommerciallySafe);
                Assert.IsFalse(snapshot.CanUsePersonalizedAds);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(links);
            }
        }

        private sealed class MemorySaveStorage : ISaveStorage
        {
            private readonly Dictionary<string, string> _values = new Dictionary<string, string>();
            public bool Exists(string key) => _values.ContainsKey(key);
            public string Load(string key) => _values.TryGetValue(key, out var value) ? value : null;
            public void Save(string key, string value) => _values[key] = value;
            public void Delete(string key) => _values.Remove(key);
        }

        private sealed class FakeRewardedAds : IRewardedAdService
        {
            private readonly RewardedAdResult _result;
            public FakeRewardedAds(RewardedAdResult result)
            {
                _result = result;
            }

            public bool IsRewardedAdReady() => true;
            public void ShowRewardedAd(AdPlacement placement, Action<RewardedAdResult> onCompleted) => onCompleted?.Invoke(_result);
        }
    }
}
