# EditMode/ClockTamperDetectorTests.cs

```csharp
using System;
using CoreRacer.Services.Save;
using CoreRacer.Services.Time;
using NUnit.Framework;

namespace CoreRacer.Tests.EditMode
{
    public sealed class ClockTamperDetectorTests
    {
        [Test]
        public void Detects_Backwards_Time_Beyond_Tolerance()
        {
            var storage = new PlayerPrefsSaveStorage();
            var detector = new ClockTamperDetector(storage, TimeSpan.FromMinutes(1));
            detector.CheckAndRecord(new DateTimeOffset(2026, 5, 20, 12, 0, 0, TimeSpan.Zero));
            detector.CheckAndRecord(new DateTimeOffset(2026, 5, 20, 11, 0, 0, TimeSpan.Zero));
            Assert.IsTrue(detector.SuspiciousBackwardsClockDetected);
        }
    }
}

```
