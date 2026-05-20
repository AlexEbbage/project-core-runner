# EditMode/AdPolicyLaunchTests.cs

```csharp
using CoreRacer.Monetisation.Ads;
using CoreRacer.Monetisation.Premium;
using CoreRacer.Services.Save;
using NUnit.Framework;

namespace CoreRacer.Tests.EditMode
{
    public sealed class AdPolicyLaunchTests
    {
        [Test]
        public void Premium_Bypasses_Continue_DoubleReward_And_Interstitial()
        {
            var storage = new PlayerPrefsSaveStorage();
            var premium = new PremiumEntitlementService(storage);
            premium.GrantPremium();
            var policy = new AdPolicyService(premium);
            Assert.IsFalse(policy.RequiresAd(AdPlacement.ContinueRun));
            Assert.IsFalse(policy.RequiresAd(AdPlacement.DoubleRunRewards));
            Assert.IsFalse(policy.RequiresAd(AdPlacement.Interstitial));
        }
    }
}

```
