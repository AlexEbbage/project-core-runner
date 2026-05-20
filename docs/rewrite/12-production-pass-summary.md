# Core Racer Production Pass Summary

This pass adds the remaining production-hardening layer on top of the clean replacement package.

## Added runtime systems

- Structured logging with categories, levels, recent-entry buffer, and crash breadcrumb buffer.
- Backwards-compatible safe save storage with checksum and backup keys.
- Performance metrics sampling for average FPS, 1% low FPS, managed memory, pool misses, and active VFX count.
- Analytics event taxonomy expansion covering sessions, runs, economy, ads, IAP, retention, tasks, achievements, and consent.
- Local remote config fallback service for LiveOps-ready values.
- Live event calendar scaffolding.
- GDPR/UK-GDPR style consent state, privacy links config, and consent prompt controller.
- Daily/weekly/monthly rotating task system with task pools, expiry, claiming, and reward grants.
- Enhanced daily login reward calendar with streak policy and double-reward support.
- Audio mixer controller and audio event library scaffolding.
- VFX library, event IDs, and quality settings scaffolding.
- Unity IAP adapter scaffold guarded by `CORE_RACER_UNITY_IAP`.
- LevelPlay interstitial adapter scaffold guarded by `CORE_RACER_LEVELPLAY`.
- Production readiness validator.

## Added editor tooling

Run these after import:

```text
Tools/Core Racer/Generate Default Config Assets
Tools/Core Racer/Validate Project
Tools/Core Racer/Validate Production Readiness
Tools/Core Racer/Validate Open Scene Wiring
Tools/Core Racer/Report Missing Scripts In Open Scene
```

The default config generator now also creates:

- `PrivacyLinks.asset`
- `RemoteConfigDefaults.asset`
- `RotatingTaskPool.asset`
- individual default daily/weekly/monthly task assets
- `DailyRewardCalendar.asset`

## Important remaining manual work

These cannot be completed outside Unity because they depend on your installed SDK versions, package manifest, ProjectSettings and store console setup:

1. Wire the installed LevelPlay rewarded/interstitial SDK API into the guarded adapter sections.
2. Install and configure Unity IAP, then use `UnityPurchasingAdapter` or wire callbacks into `IapPurchaseService`.
3. Wire Firebase Analytics and Crashlytics into `FirebaseAnalyticsServiceAdapter` and optionally send logger breadcrumbs to Crashlytics.
4. Replace placeholder privacy, terms and data-deletion URLs in `PrivacyLinks.asset`.
5. Wire `GameBootstrapper` references to generated configs in your clean scene.
6. Add consent UI prefab using `ConsentPromptController`.
7. Wire rotating task UI using `RotatingTaskListView` and `RotatingTaskRowView`.
8. Configure Android ProjectSettings: bundle ID, version code/name, signing, target SDK, orientation, IL2CPP, graphics API, privacy declarations.

## Premium/ad rule preserved

Premium bypasses:

- Continue/respawn ads
- Double run reward ads
- Interstitial ads

Premium still watches:

- Daily login double reward ads
- Mid-run rewarded offers
- Other rewarded ads

