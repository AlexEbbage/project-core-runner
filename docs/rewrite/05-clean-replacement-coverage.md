# Clean replacement coverage matrix

| Original / planned area | Replacement location | Status |
|---|---|---|
| Bootstrap/services | `Assets/CoreRacer/Runtime/Bootstrap` | Included |
| Save abstraction | `Runtime/Services/Save` | Included |
| Profile state/repository/service | `Runtime/Meta/Profile` | Included |
| Economy/rewards/purchases | `Runtime/Meta/Economy` | Included |
| Shop catalog/service | `Runtime/Meta/Shop` | Included |
| Ships/cosmetics/upgrades | `Runtime/Meta/Ships` | Included |
| Boosters | `Runtime/Meta/Boosters` | Included |
| Levels/roadmap | `Runtime/Meta/Levels` | Included |
| Achievements | `Runtime/Meta/Achievements` | Included |
| Daily rewards | `Runtime/Meta/DailyRewards` | Included |
| Progression tasks | `Runtime/Meta/Progression` | Included |
| Run lifecycle | `Runtime/Gameplay/Run` | Included |
| Player movement/input/health | `Runtime/Gameplay/Player` | Included |
| Obstacles | `Runtime/Gameplay/Obstacles` | Included |
| Pickups | `Runtime/Gameplay/Pickups` | Included |
| Powerups/effects | `Runtime/Gameplay/Powerups` | Included |
| Tunnel/environment | `Runtime/Gameplay/Environment` | Included |
| Camera/VFX | `Runtime/Gameplay/Camera`, `Runtime/Gameplay/Vfx` | Included |
| Ads policy/controllers | `Runtime/Monetisation/Ads` | Included |
| Premium entitlement | `Runtime/Monetisation/Premium` | Included |
| IAP facade/product IDs | `Runtime/Monetisation/Iap` | Included |
| Analytics | `Runtime/Services/Analytics` | Included |
| Audio | `Runtime/Services/Audio` | Included |
| Settings | `Runtime/Services/Settings`, `Runtime/UI/Settings` | Included |
| Haptics | `Runtime/Services/Haptics` | Included |
| Notifications | `Runtime/Services/Notifications` | Included |
| Loading screen | `Runtime/SceneManagement` | Included |
| Localization | `Runtime/Localization` | Included |
| HUD | `Runtime/UI/Hud` | Included |
| Game over | `Runtime/UI/GameOver` | Included |
| Main menu shell/pages | `Runtime/UI/MainMenu` | Included |
| Pause menu | `Runtime/UI/Pause` | Included |
| Config builders | `Editor/Builders/CoreRacerDefaultConfigBuilder.cs` | Included |
| Scene builder | `Editor/Builders/CoreRacerSceneBuilder.cs` | Included |
| Validation tooling | `Editor/Validation/CoreRacerProjectValidator.cs` | Included |
| Edit-mode tests | `Assets/CoreRacer/Tests/EditMode` | Included |

## What still requires Unity-side wiring

- Production scene layout and prefabs.
- Real LevelPlay SDK calls inside the guarded adapter.
- Real Firebase SDK parameter conversion inside the guarded adapter.
- Google Play Console product configuration for `premium_user`.
- Final Android build settings, keystore and package name.
