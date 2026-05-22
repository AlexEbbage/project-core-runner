# Core Racer Final Handoff

Generated for Phase 8 final validation and handoff.

Status labels:

- `Verified`: confirmed by validator, Console, test runner, or direct project inspection.
- `Partial`: implemented or wired, but release-grade proof is incomplete.
- `Blocked`: a known issue prevents release or closed-testing readiness.
- `Not verified`: not exercised in this pass.

## 1. Compile status

Status: `Verified`

- Unity Editor: `2022.3.62f3`.
- Active scene during validation: `Assets/CoreRacer/Scenes/CoreRacer_Main.unity`.
- Editor state before final checks: idle, not compiling, no domain reload pending.
- Pre-validation Console: no project errors or warnings; only MCP test-run throttle logs from prior test execution.
- Final compile-sensitive SDK symbols are active for Android and Standalone:
  - `CORE_RACER_UNITY_IAP`
  - `CORE_RACER_FIREBASE`
  - `CORE_RACER_MOBILE_NOTIFICATIONS`
- No compile errors were present after the final validation pass.

## 2. Main scene status

Status: `Verified`

- `Tools/Core Racer/Validate Open Scene Wiring`: passed.
- `Tools/Core Racer/Report Missing Scripts In Open Scene`: no missing scripts found.
- `Tools/Core Racer/Validate FTUE Tutorial`: passed.
- `CoreRacer_Main.unity` contains the clean scene runtime shell and Phase 7 `SdkAdapters` child under `CoreRacer_Bootstrapper`.
- Verified assigned SDK scene adapters:
  - `FirebaseAnalyticsServiceAdapter`
  - `MobilePushNotificationService`
  - `UnityPurchasingAdapter` present under `SdkAdapters`

## 3. Build scenes status

Status: `Blocked`

- Active build target: `Android`.
- Build scenes count: `1`.
- Enabled build scene: `Assets/Scenes/GameScene.unity`.
- `Assets/CoreRacer/Scenes/CoreRacer_Main.unity` is not enabled in build settings.
- This is a closed-testing blocker unless the team intentionally ships the legacy `GameScene.unity`.

## 4. Core run loop status

Status: `Partial`

- Clean scene wiring validator passed.
- Phase 3 through Phase 6 smoke work previously verified the clean run loop, camera follow, pickups, obstacles, tunnel presentation, UI flow, and FTUE progression.
- Current automated EditMode and PlayMode runner smoke checks succeeded, but both suites reported `0` actual tests.
- No full manual run-loop smoke was repeated during Phase 8.

## 5. UI status

Status: `Partial`

- Phase 5 authored hub flow is present in `CoreRacer_Main.unity` for Play, Shop, Hangar, Lab, Progression, and Settings.
- Localization validation passed with `Issues: 0`.
- Product catalogue validation passed with `Issues: 0`.
- Remaining UI work is manual UX/device review: navigation polish, small-screen layout checks, modal/button state QA, and final art/content review.

## 6. FTUE status

Status: `Verified`

- `Tools/Core Racer/Validate FTUE Tutorial`: passed.
- FTUE config, tutorial overlay, tutorial director, deterministic pickup/powerup assists, prompt routing, reset support, and analytics event names were previously wired.
- Prior Phase 6 smoke verified tutorial reset/start/step completion/completion analytics in debug analytics.
- Device-level first-session pacing still needs player QA, but the scene/config wiring is verified.

## 7. Ads/IAP status

Status: `Partial`

- Unity IAP / Unity Purchasing:
  - Package installed: yes, `com.unity.purchasing@4.12.2`.
  - Reflected API present: `UnityPurchasing`, `IDetailedStoreListener`.
  - Symbol enabled: `CORE_RACER_UNITY_IAP`.
  - Adapter present: `UnityPurchasingAdapter`.
- LevelPlay / IronSource:
  - LevelPlay dependency files are present.
  - No supported LevelPlay/IronSource C# runtime API type was reflected.
  - `CORE_RACER_LEVELPLAY` remains disabled.
  - Rewarded and interstitial scene fields remain unassigned.
- Store-sandbox purchase and restore have not been verified on device.

## 8. Premium policy verification

Status: `Partial`

- Static/service inspection confirms premium grants route through `IapPurchaseService.CompletePurchase` and `PremiumEntitlementService.GrantPremium`.
- Restore path only grants positive ownership and does not revoke existing premium on failed restore.
- `AdPolicyService.RequiresAd` bypasses premium-bypass placements when premium is active.
- Rewarded and interstitial controllers route through `AdPolicyService`; direct gameplay/UI SDK ad decisions were not found in the clean runtime path inspected.
- Runtime premium entitlement with Google Play sandbox is not verified.

## 9. Analytics/logging/crash support status

Status: `Partial`

- Firebase Analytics:
  - Assets installed: Firebase Analytics 13.6.0.
  - Reflected API present: `FirebaseApp`, `FirebaseAnalytics`.
  - Symbol enabled: `CORE_RACER_FIREBASE`.
  - Adapter assigned: `FirebaseAnalyticsServiceAdapter`.
- Logging:
  - `GameBootstrapper` registers `GameLogger` and optional Unity log forwarding.
  - Debug analytics path remains available.
- Crash support:
  - `ICrashReportingService` and `DebugCrashReportingService` exist.
  - Firebase Crashlytics is not installed.
  - `CORE_RACER_FIREBASE_CRASHLYTICS` remains disabled.
  - Crashlytics production reporting is manual setup.

## 10. Daily rewards/tasks/achievements status

Status: `Verified`

- `GameBootstrapper` registers:
  - `DailyRewardCalendarService`
  - `RotatingTaskService`
  - `AchievementService`
- `Tools/Core Racer/Validate Production Readiness` did not report task-pool or daily-reward content blockers.
- `Tools/Core Racer/Validate Product Catalogues`: `Issues: 0`.
- UI routing for daily login, rotating tasks, and achievements is present under the Phase 5 Progression page.

## 11. Economy simulation summary

Status: `Verified`

`Tools/Core Racer/Run Default Economy Simulation` was run and updated `docs/reports/economy-simulation-report.md`.

Default report:

- Runs simulated: `100`
- Soft currency earned: `18,500`
- Premium currency earned: `50`
- Rewarded ads watched estimate: `25`
- Runs to first upgrade: `3`
- Runs to second ship: `25`

Tuning note from the generated report: first upgrade should usually be reachable in the first `2-5` runs.

## 12. Accessibility/comfort status

Status: `Partial`

- `AccessibilitySettingsService` is registered by `GameBootstrapper`.
- `ComfortSettingsController` exists and is routed through the Settings hub.
- Haptics/settings services are registered in the clean bootstrap path.
- Device-level accessibility, comfort, haptic behavior, and small-screen usability are not verified in this pass.

## 13. Offline/app lifecycle status

Status: `Partial`

- Save path uses `PlayerPrefsSaveStorage` wrapped by `SafeSaveStorage`.
- Data/privacy controls use `DataControlsService`.
- Time integrity uses `LocalDeviceTimeAuthority` and `ClockTamperDetector`.
- Offline support types exist: `NetworkStatusService`, `OfflineModeService`, and `QueuedAnalyticsService`.
- App lifecycle type exists with pause/focus handling: `AppLifecycleService`.
- Runtime lifecycle behavior was not exercised on device during this pass.

## 14. Store checklist status

Status: `Blocked`

- Google Play listing checklist exists at `docs/store/01-google-play-listing-checklist.md`, but all listing/declaration/release-operation items remain unchecked.
- Ads/IAP declaration doc exists and lists premium/ad policy expectations.
- `Tools/Core Racer/Validate Production Readiness`: blocked by placeholder privacy policy URL.
- `Tools/Core Racer/Validate Launch Readiness`: found `1` issue from placeholder `example.com` URL content.
- Privacy links asset still contains:
  - `https://example.com/privacy`
  - `https://example.com/terms`
  - `https://example.com/data-deletion`

## 15. Remaining manual work

Status: `Blocked`

1. Replace placeholder privacy, terms, and data deletion URLs in `PrivacyLinks.asset`.
2. Decide and fix build scenes: add `CoreRacer_Main.unity` or explicitly approve `GameScene.unity`.
3. Install/verify callable LevelPlay/IronSource C# SDK APIs, then wire rewarded/interstitial adapters.
4. Install and wire Firebase Crashlytics, or explicitly remove it from launch requirements.
5. Install Addressables only if remote/content-addressable loading is required for the release.
6. Verify Google Play Console product `premium_user`.
7. Run Google Play sandbox purchase and restore tests.
8. Complete store listing, data safety, ads, IAP, target audience, and content rating declarations.
9. Run Android device QA for first session, run loop, UI, FTUE, notifications, haptics, and performance.
10. Add real EditMode/PlayMode tests; current test suites contain zero actual tests.

## 16. Known risks

Status: `Blocked`

- Build settings point at legacy `Assets/Scenes/GameScene.unity`, not the clean `CoreRacer_Main.unity`.
- Privacy/store URLs are placeholders.
- Production readiness has one blocking issue.
- Launch readiness has one issue.
- LevelPlay C# runtime API is not reflected, so production ads are not wired.
- Firebase Crashlytics is not installed.
- Addressables is not installed.
- EditMode and PlayMode smoke checks pass with zero actual tests.
- Full Android build was not run because the build-scene mismatch is a blocker.
- Device-level QA has not been completed for store-sandbox IAP, notifications, lifecycle, accessibility, haptics, or performance.

## 17. Recommended next 10 tasks before closed testing

Status: `Blocked`

1. Replace `PrivacyLinks.asset` placeholder URLs with production URLs.
2. Update Build Settings to include the intended launch scene, most likely `Assets/CoreRacer/Scenes/CoreRacer_Main.unity`.
3. Re-run production and launch readiness validators until both have zero blocking issues.
4. Add minimum EditMode tests for `AdPolicyService`, `IapPurchaseService`, `SafeSaveStorage`, tutorial progression, and task/daily reward claim rules.
5. Add a PlayMode smoke test that loads `CoreRacer_Main.unity`, starts a run, collects a pickup, crashes/continues or ends, and returns to menu.
6. Verify Unity IAP `premium_user` in Google Play sandbox, including restore behavior.
7. Install/verify LevelPlay C# APIs and wire rewarded/interstitial adapters without bypassing `AdPolicyService`.
8. Complete Google Play listing, Data Safety, ads/IAP declarations, target audience, and content rating.
9. Run Android device QA for FTUE, hub UI, run loop, notifications, comfort settings, privacy/data controls, and support/debug reset flows.
10. Produce a release-candidate Android App Bundle from the corrected build scenes and archive Console/build logs.

## Validation log summary

- Console before validation: no project errors or warnings.
- `Validate Project`: passed.
- `Validate Production Readiness`: blocked, `1` blocking issue.
- `Validate Launch Readiness`: `1` issue.
- `Validate Product Catalogues`: `Issues: 0`.
- `Validate Localization Tables`: `Issues: 0`.
- `Validate Open Scene Wiring`: passed.
- `Report Missing Scripts In Open Scene`: no missing scripts found.
- `Validate FTUE Tutorial`: passed.
- `Validate SDK Status`: report generated; three manual SDK warnings: LevelPlay C# API absent, Crashlytics not installed, Addressables not installed.
- EditMode test runner: succeeded, `0` tests.
- PlayMode test runner: succeeded, `0` tests.
- Full Android build: not run; blocked by build-scene mismatch.
