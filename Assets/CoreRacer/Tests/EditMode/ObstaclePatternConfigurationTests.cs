using System.Linq;
using CoreRacer.Gameplay.Obstacles;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

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
            Assert.That(config.Patterns.All(pattern => Mathf.Abs(pattern.ObstacleScale - (4f / 7f)) < 0.0001f), Is.True,
                "Recovered obstacles must use the authored tunnel's seven-unit inner radius to fit the radius-four MVP tunnel exactly.");
            Assert.That(config.Patterns.All(pattern => Mathf.Abs(pattern.RotationOffsetDegrees + 30f) < 0.0001f), Is.True,
                "Recovered obstacles must correct the thirty-degree cross-section offset between the authored and procedural tunnels.");
            Assert.That(config.Patterns.All(pattern => pattern.MinIterations > 0 && pattern.MaxIterations >= pattern.MinIterations), Is.True);
            Assert.That(config.Patterns.Any(pattern => pattern.Type == ObstacleType.Fan && pattern.MinimumDifficulty > 0f), Is.True);
            Assert.That(config.Patterns.Any(pattern => pattern.Type == ObstacleType.Doors && pattern.MinimumDifficulty > 0f), Is.True);

            foreach (var pattern in config.Patterns)
            {
                var bodies = pattern.ObstaclePrefab.GetComponentsInChildren<Rigidbody>(true);
                Assert.That(bodies, Is.Not.Empty, $"{pattern.Id} must provide a Rigidbody for reliable trigger interaction.");
                Assert.That(bodies.All(body => body.isKinematic), Is.True,
                    $"{pattern.Id} obstacle bodies must remain kinematic.");
                Assert.That(bodies.All(body => body.collisionDetectionMode != CollisionDetectionMode.Discrete), Is.True,
                    $"{pattern.Id} obstacle bodies must not tunnel through the transform-driven player.");

                var colliders = pattern.ObstaclePrefab.GetComponentsInChildren<Collider>(true)
                    .Where(collider => collider.GetComponent<Renderer>() != null)
                    .ToArray();
                Assert.That(colliders, Is.Not.Empty, $"{pattern.Id} must provide collision geometry.");
                Assert.That(colliders.All(collider => collider.enabled && collider.isTrigger && collider.CompareTag("Obstacle")), Is.True,
                    $"Every {pattern.Id} collider must route through PlayerCollisionHandler.");
            }
        }
    }
}
