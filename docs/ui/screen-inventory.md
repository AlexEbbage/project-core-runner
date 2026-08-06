# Core Racer UI Surface Inventory and Replacement Map

| Surface | Existing behaviour retained | Final UI owner | Main dependencies | Patch state |
| --- | --- | --- | --- | --- |
| Shared shell | Profile level/XP, currencies, settings shortcut, primary navigation | `MainMenuShell.uxml`, `MenuShellView`, `MenuShellPresenter` | `PlayerProfileService`, `IScreenRouter` | Implemented; Unity verification pending |
| Play / Level Select | Core Run selection, MVP lock-down, score/stars/rewards, boosters, Start | `PlayScreen.*` | level roadmap, profile, booster catalog/loadout, `RunController` | Implemented; Unity verification pending |
| Gameplay HUD | score, distance, run credits, health, zone progress, active powerups, pause | `GameplayHud.*` | run trackers, health, powerups, `RunController` | Implemented; PlayMode/device verification pending |
| Pause | Resume and Home | `RunOverlays.*`, `RunOverlayPresenter` | `RunController` | Implemented; PlayMode verification pending |
| Tutorial | Current tutorial instruction and advance | `RunOverlays.*`, `RunOverlayPresenter` | `TutorialService`, localisation | Implemented; pacing/device review pending |
| Continue | Continue or bank/end result | run bottom sheet | `RunController`, rewarded-continue policy behind run logic | Implemented; PlayMode/provider review pending |
| Game Over | result stats, x2 rewards, Retry, Home | run bottom sheet | run settlement and `RunController` | Implemented; PlayMode/provider review pending |
| Shop | category tabs, featured offer, item list, purchase modal/state | `ShopScreen.*` | `ShopCatalog`, `ShopService`, profile/inventory | Implemented; store/runtime verification pending |
| Hangar | ships/cosmetics carousel, unlock/equip state, stats, upgrade | `HangarScreen.*` | `ShipDatabase`, profile/inventory/upgrades | Implemented; 3D/art/runtime review pending |
| Lab | powerup upgrades, ship passives, core experiments | `LabScreen.*` | powerup config, ship upgrades, profile, level roadmap | Implemented; balance/runtime review pending |
| Progress | XP, summary, daily rewards, tasks, achievements and claims | `ProgressionScreen.*` | profile, daily, rotating/progression task and achievement services, ads | Implemented; runtime/provider verification pending |
| Settings | audio, haptics, controls, reduced motion, contrast, privacy/support, tutorial reset | `SettingsScreen.*` | settings, accessibility, support, tutorial | Implemented; device/settings verification pending |
| Generic modal | source-driven confirmation/details/privacy/support | `UiModalService`, `GenericModal` | caller action | Implemented; modal input test pending |
| Toast | success/error feedback | `UiToastService` | semantic animation | Implemented; runtime review pending |
| Loading | deterministic layer and progress placeholder | `LoadingLayer` | future loading owner | Structural contract implemented; no new loading workflow invented |
| Component gallery | standard states and motion previews | `ComponentGallery.*` | animation service | Implemented for Editor/development use |

## Behaviour boundary

The UI invokes existing services and displays their state. It does not own run rules, purchases, reward settlement, persistence, progression evaluation, ad policy, or gameplay movement.
