using System.Linq;
using CoreRacer.Gameplay.Obstacles;
using NUnit.Framework;
using UnityEditor;

namespace CoreRacer.Tests.EditMode
{
    public sealed class ObstaclePatternConfigurationTests
    {
        [Test]
        public void MvpConfiguration_UsesAuthoredHexagonalDifficultyPatterns()
        {
            var config = AssetDatabase.LoadAssetAtPath<ObstacleGenerationConfig>(
                "Assets/CoreRacer/Generated/Configs/ObstacleGeneration.asset");

            Assert.That(config, Is.Not.Null);
            Assert.That(config.TunnelSides, Is.EqualTo(6));
            Assert.That(config.Patterns, Has.Count.EqualTo(5));
            Assert.That(config.Patterns.All(pattern => pattern != null && pattern.ObstaclePrefab != null), Is.True);
            Assert.That(config.Patterns.All(pattern => pattern.ObstacleScale > 0f && pattern.ObstacleScale < 0.5f), Is.True,
                "Recovered obstacles must be fitted inside the radius-four MVP tunnel instead of remaining at their original Blender scale.");
            Assert.That(config.Patterns.All(pattern => pattern.MinIterations > 0 && pattern.MaxIterations >= pattern.MinIterations), Is.True);
            Assert.That(config.Patterns.Any(pattern => pattern.Type == ObstacleType.Fan && pattern.MinimumDifficulty > 0f), Is.True);
            Assert.That(config.Patterns.Any(pattern => pattern.Type == ObstacleType.Doors && pattern.MinimumDifficulty > 0f), Is.True);
        }
    }
}
