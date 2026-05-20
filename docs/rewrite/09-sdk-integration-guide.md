# SDK Integration Guide

The replacement package uses dependency-safe adapters so Unity can compile before external packages are installed. Wire SDKs only after the clean scene compiles.

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

Add Firebase parameter conversion behind:

```csharp
#if CORE_RACER_FIREBASE
#endif
```

Keep gameplay/UI logging through `GameAnalytics`; do not scatter raw Firebase calls.

## Unity IAP

File:

`Assets/CoreRacer/Runtime/Monetisation/Iap/IapPurchaseService.cs`

The current service is a safe facade. Replace internals with Unity IAP callbacks when installed.

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

After installing Unity Mobile Notifications, implement Android/iOS scheduling behind:

```csharp
#if CORE_RACER_MOBILE_NOTIFICATIONS
#endif
```
