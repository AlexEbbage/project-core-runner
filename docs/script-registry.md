# Script Registry

## Working Rule

Update this registry whenever code files are added, renamed, or materially changed for an approved feature. This file is a restore map of the current runtime ownership, not an exhaustive source dump.

## Script Registry Table

| File | Directory | Layer | Purpose | Depends On | Feature | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GameManager.cs` | `Assets/Scripts/Gameplay/GameFlow` | Gameplay | Central run-state orchestration, continue flow, reward flow, UI/service coordination | Player systems, run systems, HUD/UI, ad/analytics/audio services | F1, F4, F6, F13, F15 | Active |
| `PlayerController.cs` | `Assets/Scripts/Gameplay/Player` | Gameplay | Core ship movement, input handling, touch control, handling upgrades | Input System, settings/profile hooks | F1 | Active |
| `PlayerHealth.cs` | `Assets/Scripts/Gameplay/Player` | Gameplay | Player survivability, death, shield interaction | GameManager, powerups, collision flow | F1, F4, F5 | Active |
| `ObstacleRingGenerator.cs` | `Assets/Scripts/Gameplay/Obstacles` | Gameplay | Procedural obstacle/pickup ring spawning, pooling, pattern progression, difficulty scaling | Player transform, obstacle configs, pickup/powerup content | F2, F5 | Active |
| `RunScoreManager.cs` | `Assets/Scripts/Gameplay/GameFlow` | Gameplay | Score accumulation, combo multiplier, pickup scoring, best-score persistence | Balance config, UI consumers, powerup modifiers | F3 | Active |
| `RunSpeedController.cs` | `Assets/Scripts/Gameplay/GameFlow` | Gameplay | Base and ramping speed control, config selection, powerup speed modifiers | Speed config, gameplay consumers | F3, F5 | Active |
| `RunStatsTracker.cs` | `Assets/Scripts/Gameplay/GameFlow` | Gameplay | Aggregates run metrics such as distance, combo, hits, and powerup collection | Score/speed/player/powerup systems | F3, F15 | Active |
| `PlayerPowerupController.cs` | `Assets/Scripts/Gameplay/Powerups` | Gameplay | Activates temporary powerups, manages durations, VFX/SFX, and upgrade-scaled tuning | Player systems, speed/score systems, VFX/audio, profile | F5 | Active |
| `Pickup.cs` | `Assets/Scripts/Gameplay/Pickups` | Gameplay | Pickup collection behavior for coins and powerups | Player, VFX, powerup controller, score/currency systems | F2, F5 | Active |
| `HudController.cs` | `Assets/Scripts/UI/HUD` | UI | Displays score, combo, run feedback, active powerups, and HUD popups | Run systems, powerup controller | F6 | Active |
| `RewardedRunPromptUI.cs` | `Assets/Scripts/UI/HUD` | UI | Runtime rewarded side-prompt presentation with timeout/CTA handling | GameManager | F6, F13 | Active |
| `GameOverUI.cs` | `Assets/Scripts/UI/GameOver` | UI | Run-end summary, continue CTA, double rewards CTA, restart/menu actions | GameManager, localization, ad readiness state | F4, F6, F13 | Active |
| `PauseMenuUI.cs` | `Assets/Scripts/UI/Pause` | UI | Pause/resume/menu control surface during runs | GameManager | F6 | Active |
| `MainMenuController.cs` | `Assets/Scripts/UI/MainMenu` | UI | Hub page switching and transitions between play/shop/hangar/challenges/progression pages | Page controllers, bottom nav | F7 | Active |
| `MainMenuUI.cs` | `Assets/Scripts/UI/MainMenu` | UI | Play-page presentation, best score, level selection UI, remove-ads entry points | GameManager, level selection data, monetisation UI | F7, F12, F14 | Active |
| `TopBarController.cs` | `Assets/Scripts/UI/MainMenu` | UI | Displays profile level, XP, soft currency, and premium currency in the hub | Profile/localization/menu routing | F7, F8 | Active |
| `ShopPageController.cs` | `Assets/Scripts/UI/MainMenu` | UI | Builds shop content, opens item modal, handles purchases and unlock state | PlayerProfile, shop database, hangar refresh, analytics | F9, F14, F15 | Active |
| `HangarPageController.cs` | `Assets/Scripts/UI/MainMenu` | UI | Ship stat display, upgrade purchasing, equip flow, cosmetic tab content | PlayerProfile, ship database, powerup upgrade config, analytics | F8, F9, F18 | Active |
| `ProgressionTasksController.cs` | `Assets/Scripts/UI/MainMenu/Progression` | UI | Populates daily/weekly/monthly task UI from config | Progression task config and view prefabs | F11 | Active |
| `ProgressionTasksHubView.cs` | `Assets/Scripts/UI/MainMenu/Progression` | UI | Switches task cadence tabs and hosts daily login preview | Progression views, daily login preview | F11 | Active |
| `DailyLoginRewards.cs` | `Assets/Scripts/Meta` | Meta | Daily login reward config and manager for claim cadence and reward granting | PlayerProfile, PlayerPrefs, daily reward config | F10, F8 | Active |
| `MainMenuData.cs` | `Assets/Scripts/UI/MainMenu` | Meta | Defines profile, ship, shop, and upgrade data models in current repo shape | ScriptableObjects, PlayerPrefs-backed profile state | F8, F9, F18 | Active |
| `RemoveAdsIAPManager.cs` | `Assets/Scripts/Monetisation` | Services | Handles premium/remove-ads purchase initialization and entitlement granting | Unity IAP, ads config, menu UI | F14 | Active |
| `LevelPlayRewardedAdsService.cs` | `Assets/Scripts/Services/Ads` | Services | Rewarded ad adapter for current LevelPlay integration | LevelPlay SDK, game flow consumers | F13 | Active |
| `IRewardedAdService.cs` | `Assets/Scripts/Services/Ads` | Services | Rewarded ad abstraction boundary | Implementations and runtime orchestrators | F13 | Active |
| `IInterstitialAdService.cs` | `Assets/Scripts/Services/Ads` | Services | Interstitial ad abstraction boundary | Implementations and runtime orchestrators | F13 | Active |
| `FirebaseAnalyticsService.cs` | `Assets/Scripts/Services/Analytics` | Services | Concrete analytics adapter used when analytics integration is enabled | Analytics interface/event names | F15 | Active |
| `AnalyticsEventNames.cs` | `Assets/Scripts/Services/Analytics` | Services | Central event-name and parameter-key contract for current telemetry | Analytics service and callers | F15 | Active |
| `AudioManager.cs` | `Assets/Scripts/Services/Audio` | Services | Menu/gameplay music and sound effect playback | Runtime callers across gameplay and UI | F6 | Active |
| `MobilePushNotificationService.cs` | `Assets/Scripts/Services/Notifications` | Services | Local notification adapter for mobile reminders | Notification interface and platform package | F8 | Active |

## Notes

- `Layer` describes ownership in the current repo, not a strict assembly boundary.
- `Status` should remain `Active` until a script is replaced, retired, or split.
- Add new rows only for approved feature work.
