using CoreRacer.UI.Shared;
using NUnit.Framework;
using UnityEngine;

namespace CoreRacer.Tests.EditMode
{
    public sealed class PortraitGameplayClarityTests
    {
        [Test]
        public void SafeArea_NormalizesPortraitInsets()
        {
            var normalized = SafeAreaRectTransform.Normalize(
                new Rect(0f, 100f, 1080f, 1720f),
                new Vector2(1080f, 1920f));

            Assert.That(normalized.xMin, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(normalized.yMin, Is.EqualTo(100f / 1920f).Within(0.0001f));
            Assert.That(normalized.xMax, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(normalized.yMax, Is.EqualTo(1820f / 1920f).Within(0.0001f));
        }

        [Test]
        public void SafeArea_InvalidScreenSizeFallsBackToFullRect()
        {
            Assert.That(SafeAreaRectTransform.Normalize(new Rect(), Vector2.zero), Is.EqualTo(new Rect(0f, 0f, 1f, 1f)));
        }
    }
}
