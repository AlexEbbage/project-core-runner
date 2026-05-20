# EditMode/AdPolicyServiceTests.cs

```csharp
using CoreRacer.Monetisation.Ads;
using CoreRacer.Monetisation.Premium;
using CoreRacer.Services.Save;
using NUnit.Framework;

namespace CoreRacer.Tests.EditMode
{
    public sealed class AdPolicyServiceTests
    {
        private sealed class MemoryStorage : ISaveStorage
        {
            public string Value;
            public bool Exists(string key) => Value != null;
            public string Load(string key) => Value ?? string.Empty;
            public void Save(string key, string value) => Value = value;
            public void Delete(string key) => Value = null;
        }

        [Test]
        public void Premium_Bypasses_Continue_DoubleRewards_And_Interstitial()
        {
            var storage = new MemoryStorage();
            var premium = new PremiumEntitlementService(storage);
            var policy = new AdPolicyService(premium);
            premium.GrantPremium();

            Assert.False(policy.RequiresAd(AdPlacement.ContinueRun));
            Assert.False(policy.RequiresAd(AdPlacement.DoubleRunRewards));
            Assert.False(policy.RequiresAd(AdPlacement.Interstitial));
            Assert.True(policy.RequiresAd(AdPlacement.MidRunRewardedOffer));
            Assert.True(policy.RequiresAd(AdPlacement.DailyLoginDoubleReward));
        }
    }
}

```
