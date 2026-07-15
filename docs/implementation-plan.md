# Implementation Plan

## Delivery Strategy

Use the docs set as the durable operating system for the project. Foundation documentation is completed first, then future work proceeds one approved feature at a time. Core-run playability, Level Select/boosters, and run reward settlement are now live-verified. The next slice is human portrait-device review followed by the next approved progression surface.

## Phases

| Phase | Goal | Exit Criteria | Status |
| --- | --- | --- | --- |
| 1 | Foundation | Core docs, architecture, style guide, registries, and decisions reflect repo truth and approved direction | Completed |
| 2 | First approved feature slice | Exactly one chosen feature, or one explicitly approved milestone bundle, is documented, implemented, validated, and reflected in all registries | In progress |
| 3 | Expansion | Additional approved features are implemented in priority order with docs kept current | Not started |
| 4 | Polish and validation | UI polish, tuning, telemetry coverage, testing, and documentation cleanup are complete for the chosen release target | In progress; existing asset wiring pass implemented for the clean scene/configs |

## Implementation Order

1. Read `product-requirements.md`, `architecture.md`, `style-guide.md`, and the registries.
2. Choose one feature from `feature-registry.md`.
3. Move that feature to `In progress` and add or update its tasks in `task-registry.md`.
4. Implement only that feature slice, or the explicitly approved milestone bundle.
5. Update `script-registry.md`, `decision-registry.md`, and validation notes before closing the slice.

## Current Recommended Order

1. Complete the cohesive progression/meta phase: XP and unlocks feeding Environment Select, Lab, Hangar, Tasks, Achievements, and Daily Rewards, with one integrated PlayMode proof pass
2. Add environment-specific obstacle/VFX presentation over the fixed six-sided tunnel, then perform the human portrait-device review and apply only focused clarity fixes
3. Keep production ad SDK verification separate from editor/dev fallback validation

## Current Asset Wiring Status

- `CoreRacer_Main.unity` now uses generated wrappers around safe existing player, obstacle, pickup, tunnel, audio, and VFX assets.
- `CoreRacer_Main.unity` now also contains an authored clean hub flow for `Play`, `Shop`, `Hangar`, `Lab`, `Progression`, and `Settings`, with nested level select, daily login, rotating tasks, achievements, comfort, privacy, and support/debug surfaces.
- `CoreRacer_Main.unity` now has the Phase 6 FTUE tutorial overlay, scene director, deterministic first coin/powerup assistance, support reset action, and save-backed tutorial progress wiring.
- `CoreRacer_Main.unity` now has Phase 7 safe SDK adapter wiring for verified Unity IAP, Firebase Analytics, and Mobile Notifications APIs; LevelPlay, Crashlytics, and Addressables remain disabled/manual setup blockers.
- `CoreRacer_Main.unity` now routes the visible bottom Play button directly through validated level selection into run startup. The development editor command is `Tools > Core Racer > Playability > Start Core Run`.
- `CoreRacer_Main.unity` now owns a `RuntimeTunnel` generator configured by the selected route. The gameplay camera follows the player's orbital position and roll so the craft stays upright in frame while the tunnel appears to rotate.
- `CoreRacer_Main.unity` now exposes five persisted environment variants—Fire, Lightning, Radiation, Ice, and Firestorm—over one stable six-sided MVP tunnel type. Selection changes environment palette/atmosphere data, not tunnel geometry; one booster per family applies only for the active run.
- In Editor and Development Builds, `GameBootstrapper` adds a `DummyRewardedAdService` only when no rewarded provider is assigned, allowing Continue and Double Rewards to be proven without changing production provider wiring.
- Core runs now start on the bottom rail at 270 degrees. The camera-local Y offset is `1.25`, placing the player at portrait viewport Y `0.411`; steering is initially tuned to `140` degrees/second.
- `RuntimeTunnel` uses a renderer-local slate tint (`0.24`, `0.32`, `0.48`) so the shared `WallMaterial` remains unchanged, and player trails are cleared after the run-start teleport.
- Phase 8 final validation and handoff is captured in `docs/rewrite/final-handoff.md`; closed testing remains blocked by placeholder privacy links. Live inspection on 2026-07-15 confirmed `Assets/CoreRacer/Scenes/CoreRacer_Main.unity` is the only enabled Build Settings scene.
- Manual art follow-up is tracked in `docs/rewrite/manual-art-wiring-needed.md`.

## Dependencies and Blockers

- Monetisation expansion depends on an approved catalog and entitlement model.
- UI tooling changes depend on an explicit package adoption decision.
- The progression bundle now depends on a player-facing review pass in `CoreRacer_Main` to confirm navigation clarity, layout polish, content readability, and FTUE pacing across the authored hub shell.
- Level Select and boosters are runtime-proven but still require a human portrait-device clarity review. Shop, ship customisation, lab, tasks, daily login, achievements, and UI polish retain their broader UX review gates.
- DOTween is now the approved UI motion layer, so UI polish work should use the reusable motion helper rather than introducing a second transition stack.
- SDK release blockers remain for LevelPlay C# API installation/verification, Firebase Crashlytics installation, Addressables installation if remote content is required, and replacement of placeholder privacy links.
- Build settings currently target the clean `CoreRacer_Main.unity`; keep this checked during release preparation rather than relying on the stale legacy-scene assumption.
- The runtime tunnel now generates 48 longitudinal mesh sections for the fixed six-sided MVP type and recenters in 40-unit steps while keeping 20 units behind the player. The preserved FBX tunnel is hidden only during Play Mode.
- The Game Over Retry, Double Rewards, and Menu actions now use distinct authored RectTransform positions. Live pointer execution and PlayMode coverage confirm Retry, Menu, and subsequent Play transitions without regenerating the UI.

## Validation Notes

- Cheapest test layer to use:
  - Static repo checks and compile-likely inspection for documentation-only changes
  - Edit Mode or plain C# tests first for logic-heavy future feature slices
  - Play Mode/manual scene verification when inspector wiring or runtime scene behavior is involved
- Manual checks:
  - Confirm documented dependencies and scene flow against current repo state
  - Confirm implemented vs partial vs planned status labels remain accurate
- Asset or inspector dependencies:
  - Treat scene references, prefabs, ScriptableObjects, and UI wiring as part of the feature change surface for all future implementation work
  - Core-run PlayMode smoke test: `CoreRunPlayModeSmokeTests.VisiblePlay_StartsCoreGameplay`
- Latest automated result: 32/32 EditMode tests and 5/5 focused PlayMode tests passed before the current XP rollover regression case was added; the new case covers multi-threshold level advancement and remainder preservation and should be included in the next Unity test run.
