using CoreRacer.Bootstrap;
using CoreRacer.FTUE;
using CoreRacer.Gameplay.Powerups;
using CoreRacer.Gameplay.Run;
using CoreRacer.Localization;
using CoreRacer.Meta.Achievements;
using CoreRacer.Meta.Boosters;
using CoreRacer.Meta.DailyRewards;
using CoreRacer.Meta.Levels;
using CoreRacer.Meta.Profile;
using CoreRacer.Meta.Progression;
using CoreRacer.Meta.Ships;
using CoreRacer.Meta.Shop;
using CoreRacer.Meta.Tasks;
using CoreRacer.Monetisation.Ads;
using CoreRacer.Services.Accessibility;
using CoreRacer.Services.Settings;
using CoreRacer.Services.Support;

namespace CoreRacer.UI.Toolkit
{
    public sealed class CoreRacerUiContext
    {
        public RunController RunController { get; }
        public RunSceneReferences RunReferences { get; }
        public LevelRoadmapConfigV2 LevelRoadmap { get; }
        public BoosterCatalog BoosterCatalog { get; }
        public ShipDatabase ShipDatabase { get; }
        public ShopCatalog ShopCatalog { get; }
        public PowerupUpgradeConfigV2 PowerupUpgrades { get; }

        public PlayerProfileService Profile { get; private set; }
        public SettingsService Settings { get; private set; }
        public AccessibilitySettingsService Accessibility { get; private set; }
        public TutorialService Tutorial { get; private set; }
        public DailyRewardCalendarService DailyRewards { get; private set; }
        public AchievementService Achievements { get; private set; }
        public ProgressionTaskService ProgressionTasks { get; private set; }
        public RotatingTaskService RotatingTasks { get; private set; }
        public ShopService Shop { get; private set; }
        public RewardedAdController RewardedAds { get; private set; }
        public SupportBundleExporter Support { get; private set; }
        public LocalizationServiceV2 Localization { get; private set; }
        public BoosterLoadoutService BoosterLoadout { get; private set; }

        public CoreRacerUiContext(
            RunController runController,
            RunSceneReferences runReferences,
            LevelRoadmapConfigV2 levelRoadmap,
            BoosterCatalog boosterCatalog,
            ShipDatabase shipDatabase,
            ShopCatalog shopCatalog,
            PowerupUpgradeConfigV2 powerupUpgrades)
        {
            RunController = runController;
            RunReferences = runReferences;
            LevelRoadmap = levelRoadmap;
            BoosterCatalog = boosterCatalog;
            ShipDatabase = shipDatabase;
            ShopCatalog = shopCatalog;
            PowerupUpgrades = powerupUpgrades;
            ResolveServices();
        }

        public void ResolveServices()
        {
            GameServices.TryGet(out PlayerProfileService profile);
            GameServices.TryGet(out SettingsService settings);
            GameServices.TryGet(out AccessibilitySettingsService accessibility);
            GameServices.TryGet(out TutorialService tutorial);
            GameServices.TryGet(out DailyRewardCalendarService dailyRewards);
            GameServices.TryGet(out AchievementService achievements);
            GameServices.TryGet(out ProgressionTaskService progressionTasks);
            GameServices.TryGet(out RotatingTaskService rotatingTasks);
            GameServices.TryGet(out ShopService shop);
            GameServices.TryGet(out RewardedAdController rewardedAds);
            GameServices.TryGet(out SupportBundleExporter support);
            GameServices.TryGet(out LocalizationServiceV2 localization);

            Profile = profile;
            Settings = settings;
            Accessibility = accessibility;
            Tutorial = tutorial;
            DailyRewards = dailyRewards;
            Achievements = achievements;
            ProgressionTasks = progressionTasks;
            RotatingTasks = rotatingTasks;
            Shop = shop;
            RewardedAds = rewardedAds;
            Support = support;
            Localization = localization;
            BoosterLoadout = Profile != null && BoosterCatalog != null
                ? new BoosterLoadoutService(Profile, BoosterCatalog)
                : null;
        }
    }
}
