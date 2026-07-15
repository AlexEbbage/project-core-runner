using System;
using System.Collections.Generic;
using CoreRacer.Common.Time;
using CoreRacer.Config.Run;
using CoreRacer.Gameplay.Run;
using CoreRacer.Meta.Achievements;
using CoreRacer.Meta.DailyRewards;
using CoreRacer.Meta.Economy;
using CoreRacer.Meta.Profile;
using CoreRacer.Meta.Progression;
using CoreRacer.Services.Save;
using NUnit.Framework;
using UnityEngine;

namespace CoreRacer.Tests.EditMode
{
    public sealed class Vertical6ProgressionEconomyTests
    {
        [Test]
        public void RunRewardGrant_UpdatesCoinsXpAndRunTotals()
        {
            var profile = CreateProfile();
            var rewards = new RunRewardService(profile, new RunRewardConfig
            {
                XpPerScorePoint = 0.5f,
                PremiumCurrencyPerCoins = 100
            });

            var result = rewards.BuildResult(1000, 250, 123f, 45f, 2, RunEndReason.PlayerDeath, false);
            Assert.IsTrue(ProgressionEconomyRules.IsValidRunReward(result));

            rewards.Grant(result);

            Assert.AreEqual(250, profile.State.Wallet.Soft);
            Assert.AreEqual(2, profile.State.Wallet.Premium);
            Assert.AreEqual(1, profile.State.TotalRuns);
            Assert.AreEqual(250, profile.State.TotalCoinsCollected);
            Assert.AreEqual(2, profile.State.TotalPowerupsCollected);
            Assert.AreEqual(1000, profile.State.BestScore);
            Assert.AreEqual(123f, profile.State.BestDistance);
        }

        [Test]
        public void AchievementClaim_GrantsRewardOnceAndNotifiesProfileListeners()
        {
            var profile = CreateProfile();
            var rewards = new RewardGrantService(profile);
            var achievement = ScriptableObject.CreateInstance<AchievementDefinition>();
            try
            {
                achievement.Id = "first_run";
                achievement.DisplayName = "First Run";
                achievement.Metric = AchievementMetricType.TotalRuns;
                achievement.RequiredValue = 1;
                achievement.Rewards.Add(RewardGrant.Soft(100));

                var changed = 0;
                profile.Changed += () => changed++;
                profile.AddCurrency(CurrencyType.Soft, 10);
                profile.RecordRun(500, 10, 30f, 0);

                var service = new AchievementService(profile, rewards, new List<AchievementDefinition> { achievement });
                Assert.IsTrue(service.TryClaim("first_run"));
                Assert.IsFalse(service.TryClaim("first_run"));

                Assert.AreEqual(110, profile.State.Wallet.Soft);
                Assert.AreEqual(1, profile.State.ClaimedAchievements.Count);
                Assert.GreaterOrEqual(changed, 2);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(achievement);
            }
        }

        [Test]
        public void DailyRewardClaim_AdvancesStreakAndNotifiesProfileListeners()
        {
            var profile = CreateProfile();
            var rewards = new RewardGrantService(profile);
            var calendar = ScriptableObject.CreateInstance<DailyRewardCalendarConfig>();
            try
            {
                calendar.Days.Add(new DailyRewardDay
                {
                    DisplayName = "Day 1",
                    Rewards = new List<RewardGrant> { RewardGrant.Soft(75), RewardGrant.Experience(25) }
                });

                var changed = 0;
                profile.Changed += () => changed++;

                var service = new DailyRewardCalendarService(profile, rewards, new FixedClock(new DateTimeOffset(2026, 6, 4, 10, 0, 0, TimeSpan.Zero)), calendar);
                Assert.IsTrue(service.CanClaimToday());
                Assert.IsTrue(service.TryClaim(false));
                Assert.IsFalse(service.CanClaimToday());
                Assert.AreEqual(1, profile.State.DailyLoginStreak);
                Assert.AreEqual("2026-06-04", profile.State.LastDailyRewardDateUtc);
                Assert.AreEqual(75, profile.State.Wallet.Soft);
                Assert.GreaterOrEqual(changed, 1);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(calendar);
            }
        }

        [Test]
        public void ProgressionSnapshot_ReflectsCurrentProfileAndTaskSummary()
        {
            var profile = CreateProfile();
            var rewards = new RewardGrantService(profile);
            profile.AddCurrency(CurrencyType.Soft, 500);
            profile.RecordRun(2500, 300, 80f, 3);

            var task = ScriptableObject.CreateInstance<ProgressionTaskDefinition>();
            try
            {
                task.Id = "score_1000";
                task.DisplayName = "Score 1000";
                task.Metric = ProgressionTaskMetric.BestScore;
                task.TargetValue = 1000;
                task.Rewards.Add(RewardGrant.Soft(50));

                var tasks = new ProgressionTaskService(profile, rewards, new List<ProgressionTaskDefinition> { task });
                var snapshot = new ProgressionSnapshotService(profile, tasks).Build();

                Assert.AreEqual(500, snapshot.SoftCurrency);
                Assert.AreEqual(1, snapshot.TotalRuns);
                Assert.AreEqual(2500, snapshot.BestScore);
                Assert.AreEqual(1, snapshot.ReadyTasks);
                Assert.IsTrue(snapshot.HasClaimableTasks);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(task);
            }
        }

        [Test]
        public void ExperienceGrant_RollsAcrossLevelsAndKeepsRemainder()
        {
            var profile = CreateProfile();

            profile.AddExperience(1250);

            Assert.AreEqual(3, profile.State.Level);
            Assert.AreEqual(0, profile.State.Experience);
            Assert.AreEqual(1000, profile.ExperienceForNextLevel(profile.State.Level));

            profile.AddExperience(1001);

            Assert.AreEqual(4, profile.State.Level);
            Assert.AreEqual(1, profile.State.Experience);
        }

        private static PlayerProfileService CreateProfile()
        {
            var storage = new MemorySaveStorage();
            var repository = new PlayerProfileRepository(storage, new JsonSaveSerializer(), new ProfileMigrationService());
            return new PlayerProfileService(repository);
        }

        private sealed class FixedClock : IGameClock
        {
            public FixedClock(DateTimeOffset utcNow)
            {
                UtcNow = utcNow;
            }

            public float DeltaTime => 0.016f;
            public float UnscaledDeltaTime => 0.016f;
            public float TimeScale { get; set; } = 1f;
            public DateTimeOffset UtcNow { get; }
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
