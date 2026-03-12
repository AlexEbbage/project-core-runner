# Architecture

## Project

- Project name: `project-core-racer`
- Architecture mode: `hybrid`

## Architecture Summary

This project uses a hybrid Unity architecture: scene-facing MonoBehaviours remain responsible for lifecycle, inspector references, and orchestration, while gameplay rules, config, and service boundaries are expected to stay explicit and modular. The repo is in a transitional state, but the intended long-term shape is still the repository rule set described in `AGENTS.md`: thin gameplay/UI components, clear service APIs, minimal shared utilities, and opportunistic migration away from legacy top-level folders when those modules are touched.

The current runtime is concentrated into a single gameplay scene with menu, HUD, progression, monetisation, and gameplay systems co-existing in one scene graph. That is acceptable for the current scale, but the documentation must treat scene wiring, ScriptableObjects, and PlayerPrefs keys as part of the project's contract.

## Runtime Module Map

| Module | Responsibility | Current Shape |
| --- | --- | --- |
| `Gameplay` | Player motion, run flow, environment, obstacle generation, pickups, powerups, VFX, scoring, speed, stats | Main runtime gameplay systems live here and own most in-run orchestration |
| `UI` | Main hub, HUD, game over, pause, settings, progression, shop/hangar views, reward prompts | View/controller layer for player-facing screens and overlays |
| `Services` | Ads, analytics, audio, notifications, external integrations | Adapter and integration layer around platform/SDK services |
| Shared/legacy utility areas | Localization, scene helpers, config, settings, monetisation, meta, feedback | Transitional folders; migrate touched modules gradually toward target structure |

## Module Rules

- `Gameplay` must not depend on UI internals.
- `UI` should observe state or call stable public APIs; it should not mutate gameplay internals directly.
- `Services` should expose explicit interfaces or narrow adapters, avoiding hidden global state where possible.
- Shared helpers should remain small and should not become a dumping ground for gameplay-specific logic.
- Serialized fields, scene bindings, prefabs, and ScriptableObjects are treated as part of the architecture contract.

## Scene Structure

- Current gameplay scene: `Assets/Scenes/GameScene.unity`
- Build settings currently include a single enabled scene: `Assets/Scenes/GameScene.unity`
- Current strategy:
  - Gameplay systems, main menu hub, run HUD, pause state, crash/game-over UI, and service objects are layered into one active scene.
  - Menu-to-run transitions are handled in-scene rather than through scene swaps.
- Future options may include bootstrap or additive UI scenes, but no such split is the current contract.

## Key Runtime Flows

### Run Flow

- `GameManager` coordinates menu, playing, paused, and game-over state.
- `PlayerController`, `PlayerHealth`, run score/speed/currency systems, and obstacle generation collaborate during a run.
- Crash flow routes through continue checks, rewarded ad handling, respawn/reset logic, and final game-over reward processing.

### Meta and Hub Flow

- Hub navigation is page-based, driven by the main menu controllers under `UI/MainMenu`.
- Profile state, currencies, unlocks, upgrade levels, and selected cosmetics flow through `PlayerProfile`.
- Shop, hangar, progression, and daily login surfaces read and mutate profile-backed state through explicit page controllers and model accessors.

### Services Flow

- Ads are mediated through rewarded/interstitial service interfaces plus concrete implementations.
- IAP is currently centered on the premium/remove-ads purchase path.
- Analytics currently uses a narrow event-name contract with service implementations behind an interface.
- Notifications and audio are optional but integrated as explicit service references.

## Data Strategy

### ScriptableObjects

- Used for authored configuration and content definitions such as:
  - balance config
  - obstacle ring config
  - speed scaling config
  - ship/shop databases
  - powerup upgrade config
  - progression task config
  - daily login reward config

### Save Data and Runtime State

- `PlayerProfile` is the main player/meta state container and persists via PlayerPrefs-backed serialization.
- Several systems also use direct PlayerPrefs keys for local stats, daily login cadence, and other lightweight persistence.
- Save data remains local-only in the current documented scope.

### Asset and Inspector Contracts

- Scene object references remain inspector-driven for the major orchestration components.
- Any future refactor that changes serialized field names, component requirements, or ScriptableObject shape must document migration impact explicitly.

## Transitional Folder Policy

The repo still contains legacy top-level areas such as `Meta`, `Monetisation`, `Localization`, `SceneManagement`, `Config`, `Feedback`, and `Settings`. These are valid current dependencies and should be documented as such. When a future change materially touches one of those modules, the change should evaluate whether that module can be moved safely toward the target `Gameplay` / `UI` / `Services` / minimal shared-core structure.

## Risks

- Single-scene coupling can hide dependencies between gameplay, hub UI, and services if registries are not maintained.
- PlayerPrefs-backed persistence is simple but can drift without documented ownership and key discipline.
- Transitional folders increase the chance of boundary leakage unless touched modules are deliberately normalized over time.

## Approval

- Architecture approved: Yes, for documentation baseline
- Date: 2026-03-12
