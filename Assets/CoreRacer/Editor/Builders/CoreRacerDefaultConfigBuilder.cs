using System.IO;
using CoreRacer.Config.Gameplay;
using CoreRacer.Config.Monetisation;
using CoreRacer.Config.Run;
using CoreRacer.FTUE;
using CoreRacer.Gameplay.Obstacles;
using CoreRacer.Gameplay.Pickups;
using CoreRacer.Gameplay.Powerups;
using CoreRacer.Meta.Boosters;
using CoreRacer.Meta.DailyRewards;
using CoreRacer.Meta.Economy;
using CoreRacer.Meta.Tasks;
using CoreRacer.Services.Assets;
using CoreRacer.Services.Compliance;
using CoreRacer.Services.LiveOps;
using CoreRacer.Services.Notifications;
using CoreRacer.Meta.Levels;
using CoreRacer.Meta.Progression;
using CoreRacer.Meta.Shop;
using UnityEditor;
using UnityEngine;

namespace CoreRacer.Editor.Builders
{
    public static class CoreRacerDefaultConfigBuilder
    {
        private const string Root = "Assets/CoreRacer/Generated/Configs";

        [MenuItem("Tools/Core Racer/Generate Default Config Assets")]
        public static void Generate()
        {
            Directory.CreateDirectory(Root);
            CreateAsset<GameBalanceConfigV2>("GameBalance.asset");
            CreateAsset<SpeedScalingConfigV2>("SpeedScaling.asset");
            CreateAsset<RunConfig>("RunConfig.asset");
            CreateObstacleConfig();
            CreatePickupConfig();
            CreatePowerups();
            CreateShop();
            CreateLevels();
            CreateBoosters();
            CreateRewardedOffers();
            CreateProductionConfigs();
            CreateLaunchReadinessConfigs();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Core Racer default config assets generated.");
        }

        private static T CreateAsset<T>(string fileName) where T : ScriptableObject
        {
            var path = $"{Root}/{fileName}";
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void CreateObstacleConfig()
        {
            var config = CreateAsset<ObstacleGenerationConfig>("ObstacleGeneration.asset");
            if (config.Patterns.Count == 0)
            {
                var pattern = ScriptableObject.CreateInstance<ObstaclePatternDefinition>();
                pattern.Id = "starter_gap";
                // DisplayName assignment intentionally omitted for compatibility with older cached ObstaclePatternDefinition versions.
                pattern.MinimumDifficulty = 0f;
                pattern.Weight = 10f;
                pattern.Segments.Add(new ObstacleSegmentRule { SideIndex = 0, Blocked = true });
                pattern.Segments.Add(new ObstacleSegmentRule { SideIndex = 2, Blocked = true });
                AssetDatabase.CreateAsset(pattern, $"{Root}/ObstaclePattern_StarterGap.asset");
                config.Patterns.Add(pattern);
                EditorUtility.SetDirty(config);
            }
        }

        private static void CreatePickupConfig()
        {
            var config = CreateAsset<PickupGenerationConfig>("PickupGeneration.asset");
            if (config.PowerupLootTable.Count == 0)
            {
                config.PowerupLootTable.Add(new WeightedPowerupEntry { Type = PowerupType.Shield, Weight = 3f });
                config.PowerupLootTable.Add(new WeightedPowerupEntry { Type = PowerupType.Magnet, Weight = 3f });
                config.PowerupLootTable.Add(new WeightedPowerupEntry { Type = PowerupType.ScoreMultiplier, Weight = 2f });
                config.PowerupLootTable.Add(new WeightedPowerupEntry { Type = PowerupType.CoinMultiplier, Weight = 2f });
                config.PowerupLootTable.Add(new WeightedPowerupEntry { Type = PowerupType.AutoPilot, Weight = 1f });
                EditorUtility.SetDirty(config);
            }
        }

        private static void CreatePowerups()
        {
            var config = CreateAsset<PowerupUpgradeConfigV2>("PowerupUpgrades.asset");
            if (config.Upgrades.Count > 0) return;
            AddPowerup(config, PowerupType.Shield, "Shield", 120, 75);
            AddPowerup(config, PowerupType.Magnet, "Magnet", 100, 60);
            AddPowerup(config, PowerupType.ScoreMultiplier, "Score Multiplier", 150, 90);
            AddPowerup(config, PowerupType.CoinMultiplier, "Coin Multiplier", 150, 90);
            AddPowerup(config, PowerupType.AutoPilot, "Auto Pilot", 200, 125);
            EditorUtility.SetDirty(config);
        }

        private static void AddPowerup(PowerupUpgradeConfigV2 config, PowerupType type, string name, int baseCost, int increase)
        {
            var entry = new PowerupUpgradeEntry { Type = type, DisplayName = name, BaseCost = baseCost, CostIncrease = increase };
            entry.Levels.Add(new PowerupTuning(5f, 1f));
            entry.Levels.Add(new PowerupTuning(6f, 1.15f));
            entry.Levels.Add(new PowerupTuning(7f, 1.3f));
            entry.Levels.Add(new PowerupTuning(8f, 1.5f));
            config.Upgrades.Add(entry);
        }

        private static void CreateShop()
        {
            var catalog = CreateAsset<ShopCatalog>("ShopCatalog.asset");
            if (catalog.Items.Count > 0) return;
            catalog.Items.Add(new ShopItemDefinition { Id = "premium_user", DisplayName = "Premium User", Description = "No continue, double-reward or interstitial ads.", Kind = ShopItemKind.PremiumUser });
            catalog.Items.Add(new ShopItemDefinition { Id = "restore_purchases", DisplayName = "Restore Purchases", Kind = ShopItemKind.RestorePurchases });
            catalog.Items.Add(new ShopItemDefinition { Id = "nebula_speeder", DisplayName = "Nebula Speeder", Kind = ShopItemKind.Unlock, GrantItemId = "nebula_speeder", Price = new CurrencyAmount(CurrencyType.Soft, 2500) });
            catalog.Items.Add(new ShopItemDefinition { Id = "solar_flare_skin", DisplayName = "Solar Flare", Kind = ShopItemKind.Unlock, GrantItemId = "solar_flare_skin", Price = new CurrencyAmount(CurrencyType.Premium, 60) });
            EditorUtility.SetDirty(catalog);
        }

        private static void CreateLevels()
        {
            var roadmap = CreateAsset<LevelRoadmapConfigV2>("LevelRoadmap.asset");
            if (roadmap.Levels.Count > 0) return;
            roadmap.Levels.Add(new LevelDefinition { Id = "hex_sector_01", DisplayName = "FIRE", EnvironmentName = "Fire", Description = "A volatile furnace tunnel with ember-lit hazards.", RequiredPlayerLevel = 1, TunnelSides = 6, StartingSpeed = 16f, DifficultyMultiplier = 1f, ZoneId = "fire", ChallengeOne = "Reach 2,500 score", ChallengeTwo = "Travel 400m", ChallengeThree = "Collect 15 coins" });
            roadmap.Levels.Add(new LevelDefinition { Id = "hept_sector_02", DisplayName = "LIGHTNING", EnvironmentName = "Lightning", Description = "A storm-charged tunnel where electric arcs illuminate the route.", RequiredPlayerLevel = 2, TunnelSides = 6, StartingSpeed = 16f, DifficultyMultiplier = 1.1f, ZoneId = "lightning", ChallengeOne = "Reach 5,000 score", ChallengeTwo = "Travel 700m", ChallengeThree = "Collect 25 coins" });
            roadmap.Levels.Add(new LevelDefinition { Id = "oct_sector_03", DisplayName = "RADIATION", EnvironmentName = "Radiation", Description = "A contaminated run wrapped in toxic glow and warning colour.", RequiredPlayerLevel = 4, TunnelSides = 6, StartingSpeed = 16f, DifficultyMultiplier = 1.2f, ZoneId = "radiation", ChallengeOne = "Reach 8,000 score", ChallengeTwo = "Travel 1,000m", ChallengeThree = "Collect 40 coins" });
            roadmap.Levels.Add(new LevelDefinition { Id = "non_sector_04", DisplayName = "ICE", EnvironmentName = "Ice", Description = "A frozen corridor of cold light and crystalline hazards.", RequiredPlayerLevel = 6, TunnelSides = 6, StartingSpeed = 16f, DifficultyMultiplier = 1.3f, ZoneId = "ice", ChallengeOne = "Reach 12,000 score", ChallengeTwo = "Travel 1,400m", ChallengeThree = "Collect 55 coins" });
            roadmap.Levels.Add(new LevelDefinition { Id = "deca_sector_05", DisplayName = "FIRESTORM", EnvironmentName = "Firestorm", Description = "A late-game firestorm variant with denser hazard patterns.", RequiredPlayerLevel = 8, TunnelSides = 6, StartingSpeed = 16f, DifficultyMultiplier = 1.4f, ZoneId = "firestorm", ChallengeOne = "Reach 17,000 score", ChallengeTwo = "Travel 1,800m", ChallengeThree = "Collect 75 coins" });
            EditorUtility.SetDirty(roadmap);
        }

        private static void CreateBoosters()
        {
            var boosters = CreateAsset<BoosterCatalog>("BoosterCatalog.asset");
            if (boosters.Boosters.Count > 0) return;
            boosters.Boosters.Add(new BoosterDefinition { Id = "start_shield", DisplayName = "Start Shield", Family = BoosterFamily.Survival, EffectType = BoosterEffectType.StartShield, Value = 1f, Price = new CurrencyAmount(CurrencyType.Soft, 150) });
            boosters.Boosters.Add(new BoosterDefinition { Id = "coin_boost", DisplayName = "Coin Boost", Family = BoosterFamily.Economy, EffectType = BoosterEffectType.CoinMultiplier, Value = 2f, Price = new CurrencyAmount(CurrencyType.Soft, 250) });
            boosters.Boosters.Add(new BoosterDefinition { Id = "score_boost", DisplayName = "Score Boost", Family = BoosterFamily.Score, EffectType = BoosterEffectType.ScoreMultiplier, Value = 2f, Price = new CurrencyAmount(CurrencyType.Soft, 250) });
            EditorUtility.SetDirty(boosters);
        }


        private static void CreateProductionConfigs()
        {
            CreatePrivacyLinks();
            CreateRemoteConfigDefaults();
            CreateTaskPool();
            CreateDailyRewardCalendar();
        }

        private static void CreatePrivacyLinks()
        {
            var links = CreateAsset<PrivacyLinksConfig>("PrivacyLinks.asset");
            EditorUtility.SetDirty(links);
        }

        private static void CreateRemoteConfigDefaults()
        {
            var remote = CreateAsset<RemoteConfigDefaultsConfig>("RemoteConfigDefaults.asset");
            if (remote.Values.Count > 0) return;
            remote.Values.Add(new RemoteConfigValue { Key = RemoteConfigKeys.InterstitialCooldownSeconds, Value = "120" });
            remote.Values.Add(new RemoteConfigValue { Key = RemoteConfigKeys.ContinueRewardedAdEnabled, Value = "true" });
            remote.Values.Add(new RemoteConfigValue { Key = RemoteConfigKeys.DailyRewardDoubleAdEnabled, Value = "true" });
            remote.Values.Add(new RemoteConfigValue { Key = RemoteConfigKeys.FirstSessionCoinMultiplier, Value = "1" });
            remote.Values.Add(new RemoteConfigValue { Key = RemoteConfigKeys.ObstacleDifficultyMultiplier, Value = "1" });
            remote.Values.Add(new RemoteConfigValue { Key = RemoteConfigKeys.PowerupSpawnMultiplier, Value = "1" });
            remote.Values.Add(new RemoteConfigValue { Key = RemoteConfigKeys.PremiumOfferEnabled, Value = "true" });
            remote.Values.Add(new RemoteConfigValue { Key = "balance_obstacle_difficulty_multiplier", Value = "1" });
            remote.Values.Add(new RemoteConfigValue { Key = "balance_coin_reward_multiplier", Value = "1" });
            remote.Values.Add(new RemoteConfigValue { Key = "balance_upgrade_cost_multiplier", Value = "1" });
            remote.Values.Add(new RemoteConfigValue { Key = "balance_powerup_duration_multiplier", Value = "1" });
            remote.Values.Add(new RemoteConfigValue { Key = "rewarded_ad_bonus_soft_currency", Value = "100" });
            remote.Values.Add(new RemoteConfigValue { Key = "interstitial_cooldown_seconds", Value = "120" });
            EditorUtility.SetDirty(remote);
        }

        private static void CreateTaskPool()
        {
            var pool = CreateAsset<TaskPoolDefinition>("RotatingTaskPool.asset");
            if (pool.Tasks.Count > 0) return;
            pool.DailySlots = 3;
            pool.WeeklySlots = 5;
            pool.MonthlySlots = 4;

            AddRotatingTask(pool, "daily_3_runs", "Daily Runner", "Complete 3 runs today.", TaskCadence.Daily, ProgressionTaskMetric.RunsCompleted, 3, RewardGrant.Soft(150));
            AddRotatingTask(pool, "daily_500_coins", "Coin Sweep", "Collect 500 credits today.", TaskCadence.Daily, ProgressionTaskMetric.CoinsCollected, 500, RewardGrant.Soft(200));
            AddRotatingTask(pool, "daily_5_powerups", "Power Collector", "Collect 5 powerups today.", TaskCadence.Daily, ProgressionTaskMetric.PowerupsCollected, 5, RewardGrant.Experience(100));
            AddRotatingTask(pool, "daily_1500_score", "Clean Line", "Reach a score of 1,500.", TaskCadence.Daily, ProgressionTaskMetric.BestScore, 1500, RewardGrant.Soft(250));

            AddRotatingTask(pool, "weekly_30_runs", "Weekly Pilot", "Complete 30 runs this week.", TaskCadence.Weekly, ProgressionTaskMetric.RunsCompleted, 30, RewardGrant.Premium(10));
            AddRotatingTask(pool, "weekly_10000_coins", "Credit Surge", "Collect 10,000 credits this week.", TaskCadence.Weekly, ProgressionTaskMetric.CoinsCollected, 10000, RewardGrant.Premium(15));
            AddRotatingTask(pool, "weekly_40_powerups", "System Specialist", "Collect 40 powerups this week.", TaskCadence.Weekly, ProgressionTaskMetric.PowerupsCollected, 40, RewardGrant.Experience(750));
            AddRotatingTask(pool, "weekly_10000_score", "High-Speed Focus", "Reach a score of 10,000.", TaskCadence.Weekly, ProgressionTaskMetric.BestScore, 10000, RewardGrant.Premium(20));
            AddRotatingTask(pool, "weekly_50_runs", "Tunnel Veteran", "Complete 50 runs this week.", TaskCadence.Weekly, ProgressionTaskMetric.RunsCompleted, 50, RewardGrant.Soft(2500));

            AddRotatingTask(pool, "monthly_150_runs", "Monthly Racer", "Complete 150 runs this month.", TaskCadence.Monthly, ProgressionTaskMetric.RunsCompleted, 150, RewardGrant.Premium(50));
            AddRotatingTask(pool, "monthly_100000_coins", "Vault Breaker", "Collect 100,000 credits this month.", TaskCadence.Monthly, ProgressionTaskMetric.CoinsCollected, 100000, RewardGrant.Premium(75));
            AddRotatingTask(pool, "monthly_250_powerups", "Powerup Master", "Collect 250 powerups this month.", TaskCadence.Monthly, ProgressionTaskMetric.PowerupsCollected, 250, RewardGrant.Premium(60));
            AddRotatingTask(pool, "monthly_50000_score", "Sector Legend", "Reach a score of 50,000.", TaskCadence.Monthly, ProgressionTaskMetric.BestScore, 50000, RewardGrant.Premium(100));

            EditorUtility.SetDirty(pool);
        }

        private static void AddRotatingTask(TaskPoolDefinition pool, string id, string name, string description, TaskCadence cadence, ProgressionTaskMetric metric, int target, RewardGrant reward)
        {
            var path = $"{Root}/Task_{id}.asset";
            var task = AssetDatabase.LoadAssetAtPath<RotatingTaskDefinition>(path);
            if (task == null)
            {
                task = ScriptableObject.CreateInstance<RotatingTaskDefinition>();
                AssetDatabase.CreateAsset(task, path);
            }
            task.Id = id;
            task.DisplayName = name;
            task.Description = description;
            task.Cadence = cadence;
            task.Metric = metric;
            task.TargetValue = target;
            task.Weight = 100;
            task.Rewards.Clear();
            task.Rewards.Add(reward);
            EditorUtility.SetDirty(task);
            pool.Tasks.Add(task);
        }

        private static void CreateDailyRewardCalendar()
        {
            var calendar = CreateAsset<DailyRewardCalendarConfig>("DailyRewardCalendar.asset");
            if (calendar.Days.Count > 0) return;
            calendar.LoopAfterFinalDay = true;
            calendar.GraceDays = 1;
            calendar.ResetStreakOnMissedDay = false;
            AddDailyReward(calendar, "Day 1", RewardGrant.Soft(100));
            AddDailyReward(calendar, "Day 2", RewardGrant.Soft(150));
            AddDailyReward(calendar, "Day 3", RewardGrant.Experience(100));
            AddDailyReward(calendar, "Day 4", RewardGrant.Soft(250));
            AddDailyReward(calendar, "Day 5", RewardGrant.Premium(5));
            AddDailyReward(calendar, "Day 6", RewardGrant.Soft(500));
            AddDailyReward(calendar, "Day 7 Bonus", RewardGrant.Premium(15), true);
            EditorUtility.SetDirty(calendar);
        }

        private static void AddDailyReward(DailyRewardCalendarConfig calendar, string name, RewardGrant reward, bool bonus = false)
        {
            var day = new DailyRewardDay { DisplayName = name, IsBonusDay = bonus };
            day.Rewards.Add(reward);
            calendar.Days.Add(day);
        }



        private static void CreateLaunchReadinessConfigs()
        {
            CreateTutorialConfig();
            CreateNotificationTemplates();
            CreateAssetPreloadPlan();
        }

        private static void CreateTutorialConfig()
        {
            var tutorial = CreateAsset<TutorialConfig>("TutorialConfig.asset");
            tutorial.TutorialId = "core_racer_ftue_v2";
            tutorial.RunOnFreshInstall = true;
            tutorial.Steps.Clear();
            tutorial.Steps.Add(new TutorialStepDefinition { Id = "welcome", Kind = TutorialStepKind.WaitForRunStarted, TitleKey = "ftue.welcome.title", BodyKey = "ftue.welcome.body", HighlightTargetId = "play", RequiresExplicitContinue = false });
            tutorial.Steps.Add(new TutorialStepDefinition { Id = "move", Kind = TutorialStepKind.WaitForInput, TitleKey = "ftue.move.title", BodyKey = "ftue.move.body", HighlightTargetId = "player", RequiresExplicitContinue = false });
            tutorial.Steps.Add(new TutorialStepDefinition { Id = "dodge_first_obstacle", Kind = TutorialStepKind.WaitForObstacleAvoided, TitleKey = "ftue.dodge.title", BodyKey = "ftue.dodge.body", HighlightTargetId = "obstacle", RequiresExplicitContinue = false });
            tutorial.Steps.Add(new TutorialStepDefinition { Id = "collect_currency", Kind = TutorialStepKind.WaitForPickup, TitleKey = "ftue.currency.title", BodyKey = "ftue.currency.body", HighlightTargetId = "coin", RequiresExplicitContinue = false });
            tutorial.Steps.Add(new TutorialStepDefinition { Id = "collect_powerup", Kind = TutorialStepKind.WaitForPowerup, TitleKey = "ftue.powerup.title", BodyKey = "ftue.powerup.body", HighlightTargetId = "powerup", RequiresExplicitContinue = false });
            tutorial.Steps.Add(new TutorialStepDefinition { Id = "crash_continue_explanation", Kind = TutorialStepKind.Message, TitleKey = "ftue.crash.title", BodyKey = "ftue.crash.body", HighlightTargetId = "continue", RequiresExplicitContinue = true });
            tutorial.Steps.Add(new TutorialStepDefinition { Id = "first_upgrade_prompt", Kind = TutorialStepKind.WaitForUpgradePromptOpened, TitleKey = "ftue.upgrade.title", BodyKey = "ftue.upgrade.body", HighlightTargetId = "lab", RequiresExplicitContinue = false });
            tutorial.Steps.Add(new TutorialStepDefinition { Id = "daily_task_reward_prompt", Kind = TutorialStepKind.WaitForDailyTaskRewardPromptOpened, TitleKey = "ftue.tasks.title", BodyKey = "ftue.tasks.body", HighlightTargetId = "progression", RequiresExplicitContinue = false });
            tutorial.Steps.Add(new TutorialStepDefinition { Id = "complete", Kind = TutorialStepKind.Complete, TitleKey = "ftue.complete.title", BodyKey = "ftue.complete.body", RequiresExplicitContinue = true });
            EditorUtility.SetDirty(tutorial);
        }

        private static void CreateNotificationTemplates()
        {
            var templates = CreateAsset<NotificationTemplateConfig>("NotificationTemplates.asset");
            if (templates.Templates.Count > 0) return;
            templates.Templates.Add(new NotificationTemplate { Id = "daily_reward", Title = "Daily reward ready", Body = "Your Core Racer daily reward is ready to claim.", Hour = 18, Minute = 0 });
            templates.Templates.Add(new NotificationTemplate { Id = "weekly_reset", Title = "New weekly tasks", Body = "Fresh tunnel challenges are waiting for you.", Hour = 18, Minute = 30 });
            templates.Templates.Add(new NotificationTemplate { Id = "event_started", Title = "New event live", Body = "A limited-time Core Racer event has started.", Hour = 19, Minute = 0 });
            EditorUtility.SetDirty(templates);
        }

        private static void CreateAssetPreloadPlan()
        {
            CreateAsset<AssetPreloadPlan>("AssetPreloadPlan.asset");
        }

        private static void CreateRewardedOffers()
        {
            var offers = CreateAsset<RewardedOfferConfigV2>("RewardedOffers.asset");
            if (offers.Offers.Count > 0) return;
            offers.Offers.Add(new RewardedOfferDefinition { Id = "mid_run_coin_cache", Placement = RewardedOfferPlacement.MidRun, DisplayName = "Coin Cache", Description = "Watch to claim a coin cache.", Reward = new CurrencyAmount(CurrencyType.Soft, 100) });
            offers.Offers.Add(new RewardedOfferDefinition { Id = "daily_double", Placement = RewardedOfferPlacement.DailyLogin, DisplayName = "Double Daily", Description = "Watch to double today's daily reward.", Reward = new CurrencyAmount(CurrencyType.Soft, 150) });
            EditorUtility.SetDirty(offers);
        }
    }
}
