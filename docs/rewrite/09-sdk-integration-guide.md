# SDK Integration Guide

The replacement package uses dependency-safe adapters so Unity can compile before external packages are installed. Wire SDKs only after the clean scene compiles.

## Phase 7 verified status

Run `Tools/Core Racer/Validate SDK Status` after package changes. Current Phase 7 status:

| SDK | Package/assets | Reflected API | Core Racer symbol | Clean scene adapter |
| --- | --- | --- | --- | --- |
| Unity IAP / Unity Purchasing | `com.unity.purchasing@4.12.2` installed | `UnityPurchasing`, `IDetailedStoreListener` verified | `CORE_RACER_UNITY_IAP` enabled for Standalone/Android | `UnityPurchasingAdapter` present under `SdkAdapters` |
| Firebase Analytics | Firebase Analytics 13.6.0 assets installed | `FirebaseApp`, `FirebaseAnalytics` verified | `CORE_RACER_FIREBASE` enabled for Standalone/Android | `FirebaseAnalyticsServiceAdapter` assigned |
| Mobile Notifications | `com.unity.mobile.notifications@2.3.2` installed | Android and iOS notification centers verified | `CORE_RACER_MOBILE_NOTIFICATIONS` enabled for Standalone/Android | `MobilePushNotificationService` assigned |
| LevelPlay / IronSource | LevelPlay dependency files present | No supported C# API type reflected | Disabled | Unassigned |
| Firebase Crashlytics | Not installed | No `Firebase.Crashlytics.Crashlytics` type reflected | Disabled | Unassigned |
| Addressables | Not installed | No Addressables API reflected | Disabled | Resources fallback |

Do not treat `LEVELPLAY_DEPENDENCIES_INSTALLED` as an ad integration flag. It only indicates dependency resolver output, not the presence of a callable LevelPlay C# runtime API.

## LevelPlay rewarded ads

File:

`Assets/CoreRacer/Runtime/Monetisation/Ads/LevelPlayRewardedAdServiceAdapter.cs`

Add the real SDK implementation behind:

```csharp
#if CORE_RACER_LEVELPLAY
#endif
```

Expected behaviour:

- `IsRewardedAdReady()` returns true only when the LevelPlay rewarded placement is loaded.
- `ShowRewardedAd(placement, callback)` calls callback exactly once.
- Use `RewardedAdResult.Completed` only after the SDK reward callback.
- Use `RewardedAdResult.FailedToShow`, `Skipped`, or `NotReady` for all non-reward outcomes.
- Leave the clean scene rewarded/interstitial adapter fields unassigned until reflection confirms the exact LevelPlay/IronSource runtime types and callbacks for the installed SDK.

Recommended placement mapping:

| AdPlacement | LevelPlay placement |
|---|---|
| ContinueRun | `continue_run` |
| DoubleRunRewards | `double_run_rewards` |
| DailyLoginDoubleReward | `daily_login_double` |
| MidRunRewardedOffer | `mid_run_offer` |

## Interstitial ads

Create a real `MonoBehaviour` that implements `IInterstitialAdService`.

Use `InterstitialAdController` for policy decisions. Do not call the SDK directly from UI/gameplay.

## Firebase analytics

File:

`Assets/CoreRacer/Runtime/Services/Analytics/FirebaseAnalyticsServiceAdapter.cs`

The Phase 7 adapter is a scene-assignable `MonoBehaviour`. It initializes with `FirebaseApp.CheckAndFixDependenciesAsync`, converts primitive analytics parameters into `Firebase.Analytics.Parameter`, and sends events with `FirebaseAnalytics.LogEvent`.

Keep gameplay/UI logging through `GameAnalytics`; do not scatter raw Firebase calls.

## Firebase Crashlytics

File:

`Assets/CoreRacer/Runtime/Services/Crash/FirebaseCrashlyticsAdapter.cs`

Crashlytics remains disabled until the Firebase Crashlytics Unity package is installed and reflection confirms `Firebase.Crashlytics.Crashlytics`. Do not enable `CORE_RACER_FIREBASE_CRASHLYTICS` until then.

## Unity IAP

File:

`Assets/CoreRacer/Runtime/Monetisation/Iap/UnityPurchasingAdapter.cs`

The adapter subscribes to `IapPurchaseService` request events and uses Unity Purchasing callbacks. Reflection verified `UnityPurchasing.Initialize(IDetailedStoreListener, ConfigurationBuilder)` for the installed package.

Required product ID:

```text
premium_user
```

Rules:

- Successful purchase grants premium.
- Successful restore grants premium if the product is owned.
- Failed restore must not revoke premium.
- Product ID must match Google Play Console exactly.

## Mobile notifications

File:

`Assets/CoreRacer/Runtime/Services/Notifications/MobilePushNotificationService.cs`

The Phase 7 adapter schedules daily local reminders and clears scheduled/displayed notifications behind `CORE_RACER_MOBILE_NOTIFICATIONS`, using the verified Android and iOS notification center APIs.

## Addressables

File:

`Assets/CoreRacer/Runtime/Services/Assets/AddressablesAssetProvider.cs`

Addressables remains disabled because `com.unity.addressables` is not installed. Keep `ResourcesAssetProvider` fallback until the package is installed and `UnityEngine.AddressableAssets.Addressables` is reflected.
