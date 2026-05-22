# Production Wiring Checklist

## Bootstrapper

On the scene `GameBootstrapper`, wire:

- Shop catalog
- String table
- Rotating task pool
- Daily reward calendar
- Remote config defaults
- Privacy links config
- Rewarded ad service behaviour
- Interstitial ad service behaviour
- Analytics service behaviour
- Push notification service behaviour

## Consent

Create a modal/panel with:

- Accept all button
- Reject personalised ads button
- Privacy policy button
- Terms button

Attach `ConsentPromptController` and wire the buttons. Replace placeholder URLs in `PrivacyLinks.asset` before release.

## Daily/weekly/monthly tasks

Use the generated `RotatingTaskPool.asset` as a starter. Add more tasks before release so the rotation does not feel repetitive.

Suggested minimum content depth:

- 8–12 daily tasks
- 8–12 weekly tasks
- 6–10 monthly tasks

Wire `RotatingTaskListView` to a scroll view and create a row prefab with `RotatingTaskRowView`.

## Daily login rewards

Use `DailyRewardCalendar.asset` as a starter. Review economy values before launch. Decide whether missed days reset the streak or whether players get a grace window.

The double-reward button should still call the rewarded ad path for premium users if the placement is `DailyLoginDoubleReward`.

## IAP

Unity IAP is installed and the Phase 7 adapter is wired under `CoreRacer_Bootstrapper/SdkAdapters`. Before release, confirm:

- Google Play product ID exactly matches `premium_user` or update `IapProductIds.PremiumUser` everywhere.
- Purchase success calls `IapPurchaseService.CompletePurchase(productId)`.
- Restore success calls `IapPurchaseService.RestoreOwnedProduct(productId)`.
- Failed restore never revokes premium.
- Product is set as non-consumable.

## Ads

LevelPlay dependency files are present, but no supported LevelPlay/IronSource C# runtime API was reflected during Phase 7. Keep rewarded/interstitial scene fields unassigned until the SDK API is verified. Then wire:

- Load callbacks
- Availability checks
- Show callbacks
- Rewarded completion
- Failure callbacks
- Placement names

Do not bypass the `AdPolicyService`; all ad decisions should go through it.

## Analytics and logging

Firebase Analytics is installed and wired through `FirebaseAnalyticsServiceAdapter`. Keep gameplay/UI logging through `IAnalyticsService` and `GameAnalytics`; do not call Firebase directly from feature code.

Firebase Crashlytics is not installed. Keep `FirebaseCrashlyticsAdapter` unassigned until the Crashlytics package is installed and `Firebase.Crashlytics.Crashlytics` is reflected.

Use `IGameLogger` for important production breadcrumbs:

- App start
- Run start/end
- Save load failure
- Reward grant failure
- Ad requested/completed/failed
- IAP started/completed/failed
- Profile migration
- Scene wiring errors

## Notifications and Addressables

Unity Mobile Notifications is installed and `MobilePushNotificationService` is wired for daily local reminders. Confirm platform permission UX and device behavior on Android/iOS release builds.

Addressables is not installed. Keep using the Resources fallback until `com.unity.addressables` is installed, reflected, and `CORE_RACER_ADDRESSABLES` is enabled.

Run `Tools/Core Racer/Validate SDK Status` before every release candidate.
