using System.Collections.Generic;
using CoreRacer.Monetisation.Ads;
using CoreRacer.Monetisation.Premium;
using CoreRacer.Services.Save;
using NUnit.Framework;

namespace CoreRacer.Tests.EditMode
{
    public sealed class AdPolicyServiceTests
    {
        [Test]
        public void PremiumBypassesContinueAndGrantsReward()
        {
            var storage = new MemorySaveStorage();
            var premium = new PremiumEntitlementService(storage);
            var policy = new AdPolicyService(premium);

            Assert.IsTrue(policy.RequiresAd(AdPlacement.ContinueRun));

            premium.GrantPremium();

            Assert.IsFalse(policy.RequiresAd(AdPlacement.ContinueRun));
            Assert.IsTrue(policy.ShouldGrantRewardWhenBypassed(AdPlacement.ContinueRun));
        }

        [Test]
        public void PremiumDoesNotBypassTrueRewardedDailyLoginDoubleReward()
        {
            var storage = new MemorySaveStorage();
            var premium = new PremiumEntitlementService(storage);
            var policy = new AdPolicyService(premium);

            premium.GrantPremium();

            Assert.IsTrue(policy.RequiresAd(AdPlacement.DailyLoginDoubleReward));
            Assert.IsFalse(policy.ShouldGrantRewardWhenBypassed(AdPlacement.DailyLoginDoubleReward));
        }

        private sealed class MemorySaveStorage : ISaveStorage
        {
            private readonly Dictionary<string, string> _values = new Dictionary<string, string>();
            public bool Exists(string key) => _values.ContainsKey(key);
            public string Load(string key) => _values.TryGetValue(key, out var value) ? value : null;
            public void Save(string key, string value) => _values[key] = value;
            public void Delete(string key) => _values.Remove(key);
        }
    }
}
