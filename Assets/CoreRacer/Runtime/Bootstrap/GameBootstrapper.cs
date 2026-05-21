using System.Collections.Generic;
using CoreRacer.Common;
using CoreRacer.Common.Events;
using CoreRacer.Common.Time;
using CoreRacer.FTUE;
using CoreRacer.Gameplay.Vfx;
using CoreRacer.Localization;
using CoreRacer.Meta.DailyRewards;
using CoreRacer.Meta.Economy;
using CoreRacer.Meta.Profile;
using CoreRacer.Meta.Shop;
using CoreRacer.Meta.Tasks;
using CoreRacer.Monetisation.Ads;
using CoreRacer.Monetisation.Iap;
using CoreRacer.Monetisation.Premium;
using CoreRacer.Services.Accessibility;
using CoreRacer.Services.Analytics;
using CoreRacer.Services.Crash;
using CoreRacer.Services.Audio;
using CoreRacer.Services.Compliance;
using CoreRacer.Services.Haptics;
using CoreRacer.Services.LiveOps;
using CoreRacer.Services.Support;
using CoreRacer.Services.Time;
using CoreRacer.Services.Logging;
using CoreRacer.Services.Metrics;
using CoreRacer.Services.Notifications;
using CoreRacer.Services.Save;
using CoreRacer.Services.Settings;
using UnityEngine;

namespace CoreRacer.Bootstrap
{
    public sealed class GameBootstrapper : MonoBehaviour
    {
        [Header("Optional scene services")]
        [SerializeField] private MonoBehaviour rewardedAdServiceBehaviour;
        [SerializeField] private MonoBehaviour interstitialAdServiceBehaviour;
        [SerializeField] private MonoBehaviour analyticsServiceBehaviour;
        [SerializeField] private MonoBehaviour pushNotificationServiceBehaviour;
        [SerializeField] private MonoBehaviour crashReportingServiceBehaviour;
        [SerializeField] private bool useDebugAnalyticsInEditor = true;
        [SerializeField] private bool addUnityLogForwarder = true;

        [Header("Catalogs")]
        [SerializeField] private ShopCatalog shopCatalog;
        [SerializeField] private StringTable stringTable;
        [SerializeField] private TaskPoolDefinition rotatingTaskPool;
        [SerializeField] private DailyRewardCalendarConfig dailyRewardCalendar;
        [SerializeField] private RemoteConfigDefaultsConfig remoteConfigDefaults;
        [SerializeField] private PrivacyLinksConfig privacyLinks;
        [SerializeField] private TutorialConfig tutorialConfig;
        [SerializeField] private NotificationTemplateConfig notificationTemplateConfig;
        [SerializeField] private List<LiveEventDefinition> liveEvents = new List<LiveEventDefinition>();

        [Header("Presentation Libraries")]
        [SerializeField] private AudioEventLibrary audioEventLibrary;
        [SerializeField] private VfxLibrary vfxLibrary;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            var registry = new ServiceRegistry();
            GameServices.SetRegistry(registry);

            var logger = new GameLogger(
#if UNITY_EDITOR
                LogLevel.Debug
#else
                LogLevel.Info
#endif
            );

            if (addUnityLogForwarder && GetComponent<UnityLogForwarder>() == null)
            {
                var forwarder = gameObject.AddComponent<UnityLogForwarder>();
                forwarder.Initialize(logger);
            }

            var events = new GameEventBus();
            var clock = new UnityGameClock();
            var rawStorage = new PlayerPrefsSaveStorage();
            var storage = new SafeSaveStorage(rawStorage);
            var serializer = new JsonSaveSerializer();
            var migration = new ProfileMigrationService();
            var profileRepository = new PlayerProfileRepository(storage, serializer, migration);
            var profile = new PlayerProfileService(profileRepository);
            var rewards = new RewardGrantService(profile);
            var premium = new PremiumEntitlementService(storage);
            var adPolicy = new AdPolicyService(premium);
            var settings = new SettingsService(storage, serializer);
            var audio = new AudioService(settings);
            var haptics = new HapticsService(settings.State.HapticsEnabled);
            var localization = new LocalizationServiceV2(stringTable);
            var remoteConfig = new LocalRemoteConfigService(remoteConfigDefaults);
            var eventCalendar = new EventCalendarService(clock, liveEvents);

            var analytics = analyticsServiceBehaviour as IAnalyticsService;
            if (analytics == null && useDebugAnalyticsInEditor)
                analytics = new DebugAnalyticsService();
            var analyticsService = analytics ?? new DebugAnalyticsService();
            var gameAnalytics = new GameAnalytics(analyticsService);
            var runFunnelAnalytics = new RunFunnelAnalytics(analyticsService);
            var economyAnalytics = new EconomyAnalytics(analyticsService);
            var adIapAnalytics = new AdIapAnalytics(analyticsService);
            var performanceMetrics = new PerformanceMetricsService();
            var consent = new ConsentService(storage, serializer, privacyLinks, analyticsService);
            var accessibility = new AccessibilitySettingsService(storage, serializer);
            var timeAuthority = new LocalDeviceTimeAuthority();
            var clockTamper = new ClockTamperDetector(storage);
            clockTamper.CheckAndRecord(timeAuthority.UtcNow);
            var dataControls = new DataControlsService(storage, serializer);
            var crashReporting = crashReportingServiceBehaviour as ICrashReportingService ?? new DebugCrashReportingService();
            var economyLedger = new EconomyLedger(storage, serializer);
            var economyAnomalies = new EconomyAnomalyDetector();
            var balanceOverrides = new BalanceOverrideService(remoteConfig);

            var rewarded = rewardedAdServiceBehaviour as IRewardedAdService;
            var interstitial = interstitialAdServiceBehaviour as IInterstitialAdService;
            var rewardedController = new RewardedAdController(rewarded, adPolicy, gameAnalytics);
            var interstitialController = new InterstitialAdController(interstitial, adPolicy, gameAnalytics);
            var iap = new IapPurchaseService(premium);
            var purchases = new PurchaseService(profile, rewards);
            var shop = new ShopService(shopCatalog, profile, rewards, premium, iap);
            var notifications = pushNotificationServiceBehaviour as IPushNotificationService ?? new NoOpPushNotificationService();
            var notificationPermission = new NotificationPermissionService(storage);
            var notificationScheduler = new LocalNotificationScheduler(notifications, notificationTemplateConfig);
            var rotatingTasks = new RotatingTaskService(profile, rewards, storage, serializer, clock, rotatingTaskPool, analyticsService, logger);
            var dailyRewards = new DailyRewardCalendarService(profile, rewards, clock, dailyRewardCalendar, analyticsService, logger);
            var tutorial = new TutorialService(storage, serializer, tutorialConfig, analyticsService, logger);
            var firstSessionFunnel = new FirstSessionFunnelTracker(storage, analyticsService);
            var supportExporter = new SupportBundleExporter(storage, null, economyLedger);

            registry.Register(events);
            registry.Register<IGameClock>(clock);
            registry.Register<ISaveStorage>(storage);
            registry.Register(serializer);
            registry.Register<IGameLogger>(logger);
            registry.Register(profile);
            registry.Register(rewards);
            registry.Register(purchases);
            registry.Register(shop);
            registry.Register(premium);
            registry.Register(adPolicy);
            registry.Register(rewardedController);
            registry.Register(interstitialController);
            registry.Register(iap);
            registry.Register<IAnalyticsService>(analyticsService);
            registry.Register(gameAnalytics);
            registry.Register(runFunnelAnalytics);
            registry.Register(economyAnalytics);
            registry.Register(adIapAnalytics);
            registry.Register(settings);
            registry.Register(audio);
            if (audioEventLibrary != null) registry.Register(audioEventLibrary);
            if (vfxLibrary != null) registry.Register(vfxLibrary);
            registry.Register(haptics);
            registry.Register<IPushNotificationService>(notifications);
            registry.Register(localization);
            registry.Register<IRemoteConfigService>(remoteConfig);
            registry.Register(eventCalendar);
            registry.Register(performanceMetrics);
            registry.Register(consent);
            registry.Register(accessibility);
            registry.Register<ITimeAuthority>(timeAuthority);
            registry.Register(clockTamper);
            registry.Register(dataControls);
            registry.Register<ICrashReportingService>(crashReporting);
            registry.Register(economyLedger);
            registry.Register(economyAnomalies);
            registry.Register(balanceOverrides);
            registry.Register(notificationPermission);
            registry.Register(notificationScheduler);
            registry.Register(rotatingTasks);
            registry.Register(dailyRewards);
            registry.Register(tutorial);
            registry.Register(firstSessionFunnel);
            registry.Register(supportExporter);

            logger.Info(LogCategory.Bootstrap, "Core Racer services bootstrapped.", this);
        }
    }
}
