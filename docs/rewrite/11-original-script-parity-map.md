# Original Script Parity Map

The old `Assets/Scripts` tree was intentionally removed. This map shows where the replacement functionality now lives.

| Original area | Replacement area |
|---|---|
| `GameManager` | `RunController`, `RunLifecycleService`, `RunRewardService`, `RunContinueService`, `AdPolicyService` |
| `MainMenuUI` | `MainMenuShell`, `MainMenuPageRouter`, page controllers and item views |
| `MainMenuData` | `PlayerProfileState`, `PlayerProfileService`, `ShopCatalog`, `ShipDatabase`, progression configs |
| `ObstacleRingGenerator` | `ObstacleWorldController`, `ObstacleRingSpawner`, `ObstaclePatternSelector`, pickup systems |
| `PlayerPowerupController` | `PowerupRuntimeController`, `IPowerupEffect`, individual effect classes |
| `AdsConfig` | `PremiumEntitlementService`, `AdPolicyService` |
| `RemoveAdsIAPManager` | `IapPurchaseService`, `PremiumEntitlementService`, `ShopService` |
| `HudController` | `HudController`, `ScoreHudView`, `HealthHudView`, `PowerupStripView` |
| `GameOverUI` | `GameOverController` |
| Debug helpers | `CollisionDebugProbe`, `MusicDebugOverlay`, `RuntimeDiagnosticsOverlay` |
| VFX helpers | `VfxManager`, `VfxPooledInstance`, `SpeedParticlesControllerV2` |
| UI motion/helpers | `UiMotion`, `UiButtonClickEffect`, `UiInteractionHandler`, `SpeedUpFlash` |

If Unity shows a missing script from the old tree, add the replacement component listed here rather than restoring the old script.
