# Core Racer Final Manual Wiring Guide

This package is a clean replacement architecture. The old active `Assets/Scripts` code has been removed, so legacy scenes may show missing script references. Use this guide to wire the replacement systems to your existing art/audio/prefab assets.

## 1. Required first actions

Run these Unity menu commands in order:

1. `Tools/Core Racer/Generate Default Config Assets`
2. `Tools/Core Racer/Create Clean Replacement Scene`
3. `Tools/Core Racer/Validate Project`
4. `Tools/Core Racer/Validate Open Scene Wiring`
5. `Tools/Core Racer/Report Missing Scripts In Open Scene`

## 2. Bootstrap scene object

Create or verify a single `GameBootstrapper` object.

Assign:

- `ShopCatalog`
- `StringTable` if using localised text
- `DummyRewardedAdService` in editor, or `LevelPlayRewardedAdServiceAdapter` after SDK wiring
- `DummyInterstitialAdService` in editor, or your real interstitial adapter
- `DebugAnalyticsService` in editor, or `FirebaseAnalyticsServiceAdapter` after Firebase wiring
- `NoOpPushNotificationService` in editor, or `MobilePushNotificationService` after notification package wiring

The bootstrapper registers all runtime services through `GameServices`.

## 3. Run scene wiring

The clean scene should contain:

- `RunController`
- `RunSceneReferences`
- `PlayerController`
- `PlayerInputReader`
- `PlayerOrbitalMotor`
- `PlayerHealth`
- `PlayerDamageHandler`
- `PlayerCollisionHandler`
- `PlayerRespawnController`
- `ObstacleWorldController`
- `PickupWorldController`
- `PowerupRuntimeController`
- `HudController`
- `GameOverController`
- `PauseMenuController`

Wire all of these into `RunSceneReferences`.

## 4. Prefab swaps

The generated scene uses safe placeholders. Replace them with your existing assets:

| Placeholder | Replace with |
|---|---|
| Player mesh | Final ship prefab/model |
| Obstacle segment | Existing obstacle block/door/wall segment prefab |
| Pickup prefab | Kibble/currency/powerup pickup prefab |
| Tunnel walls | Existing tunnel mesh/material setup |
| HUD text/buttons | Final designed HUD prefabs |
| VFX placeholders | Hit, pickup, speed, warp, shield effects |
| Audio service clips | Existing menu/gameplay music and SFX |

## 5. Premium/ad behaviour

Implemented product rule:

- Premium bypasses continue/respawn ads.
- Premium bypasses run double-reward ads and still receives the double reward.
- Premium bypasses interstitial ads.
- Premium still watches normal rewarded ads such as daily login double reward and mid-run rewarded offers.

This is controlled by `AdPolicyService` and `AdPlacement`.

## 6. SDK integration still manual

The package intentionally avoids hard dependencies on SDK classes so it imports before packages are installed.

Manual SDK work:

- Enable and wire LevelPlay calls inside `LevelPlayRewardedAdServiceAdapter`.
- Enable and wire Firebase calls inside `FirebaseAnalyticsServiceAdapter`.
- Replace `IapPurchaseService` facade with Unity IAP purchase/restore callbacks.
- Install Unity Mobile Notifications and implement guarded notification calls in `MobilePushNotificationService`.

## 7. Final validation checklist

Before considering the replacement complete:

- Fresh install starts with default profile.
- Existing save migrates or resets safely.
- Run starts, pauses, resumes and ends.
- Collision causes damage/death correctly.
- Continue works with ad for non-premium.
- Continue is free for premium.
- Double reward works with ad for non-premium.
- Double reward is granted without ad for premium.
- Mid-run rewarded offers still require ads for premium.
- Daily double reward still requires ads for premium.
- Interstitials do not show for premium.
- Shop unlocks items and spends currency once.
- Restore purchases does not revoke premium on failure.
- Config/project validators pass.
