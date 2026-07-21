using CoreRacer.Gameplay.Player;
using NUnit.Framework;

namespace CoreRacer.Tests.EditMode
{
    public sealed class TouchSteeringInterpreterTests
    {
        [TestCase(100f, -1f)]
        [TestCase(980f, 1f)]
        [TestCase(540f, 0f)]
        [TestCase(565f, 0f)]
        public void ScreenSide_RespondsImmediatelyOutsideCentreDeadZone(float positionX, float expected)
        {
            Assert.AreEqual(expected, TouchSteeringInterpreter.EvaluateScreenSide(positionX, 1080f, 30f));
        }

        [TestCase(500f, 500f, 0f)]
        [TestCase(520f, 500f, 0f)]
        [TestCase(560f, 500f, 0.6667f)]
        [TestCase(400f, 500f, -1f)]
        public void Drag_UsesContinuousSignedInputAfterDeadZone(float positionX, float startX, float expected)
        {
            Assert.That(
                TouchSteeringInterpreter.EvaluateDrag(positionX, startX, 30f),
                Is.EqualTo(expected).Within(0.001f));
        }
    }
}
