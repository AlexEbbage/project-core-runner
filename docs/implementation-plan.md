# Implementation Plan

## Delivery Strategy

Use the docs set as the durable operating system for the project. Foundation documentation is completed first, then future work proceeds one approved feature at a time. The current delivery slice is core-run playability: restore the visible Play route, provide a development-only Quick Play route, and prove the run lifecycle in the live `CoreRacer_Main` scene before any broader feature work resumes.

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

1. Human playability signoff for Play, control, crash, Retry, and Home
2. Decide whether dynamic tunnel-section generation is required or whether the current static tunnel mesh is intentional
3. Resume progression and booster review only after the core loop is signed off

## Current Asset Wiring Status

- `CoreRacer_Main.unity` now uses generated wrappers around safe existing player, obstacle, pickup, tunnel, audio, and VFX assets.
- `CoreRacer_Main.unity` now also contains an authored clean hub flow for `Play`, `Shop`, `Hangar`, `Lab`, `Progression`, and `Settings`, with nested level select, daily login, rotating tasks, achievements, comfort, privacy, and support/debug surfaces.
- `CoreRacer_Main.unity` now has the Phase 6 FTUE tutorial overlay, scene director, deterministic first coin/powerup assistance, support reset action, and save-backed tutorial progress wiring.
- `CoreRacer_Main.unity` now has Phase 7 safe SDK adapter wiring for verified Unity IAP, Firebase Analytics, and Mobile Notifications APIs; LevelPlay, Crashlytics, and Addressables remain disabled/manual setup blockers.
- `CoreRacer_Main.unity` now routes the visible bottom Play button directly through validated level selection into run startup. The development editor command is `Tools > Core Racer > Playability > Start Core Run`.
- Phase 8 final validation and handoff is captured in `docs/rewrite/final-handoff.md`; closed testing remains blocked by placeholder privacy links. Live inspection on 2026-07-15 confirmed `Assets/CoreRacer/Scenes/CoreRacer_Main.unity` is the only enabled Build Settings scene.
- Manual art follow-up is tracked in `docs/rewrite/manual-art-wiring-needed.md`.

## Dependencies and Blockers

- Monetisation expansion depends on an approved catalog and entitlement model.
- UI tooling changes depend on an explicit package adoption decision.
- The progression bundle now depends on a player-facing review pass in `CoreRacer_Main` to confirm navigation clarity, layout polish, content readability, and FTUE pacing across the authored hub shell.
- Boosters, shop, ship customisation, lab, tasks, daily login, achievements, and UI polish still require broader UX review even though scene wiring, localization validation, and product-catalog validation now pass in the clean scene.
- DOTween is now the approved UI motion layer, so UI polish work should use the reusable motion helper rather than introducing a second transition stack.
- SDK release blockers remain for LevelPlay C# API installation/verification, Firebase Crashlytics installation, Addressables installation if remote content is required, and replacement of placeholder privacy links.
- Build settings currently target the clean `CoreRacer_Main.unity`; keep this checked during release preparation rather than relying on the stale legacy-scene assumption.
- Live inspection for this slice found active static tunnel geometry, not a runtime tunnel-section generator. Obstacles and pickups do generate during the run; tunnel-section generation must not be claimed until a product decision and implementation/verification slice exists.
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
  - Latest automated result: 29/29 EditMode tests passed and 3/3 focused PlayMode tests passed; coverage includes scene uniqueness, visible listener wiring, duplicate start rejection, movement/camera follow, run session creation, non-overlapping Game Over actions, real Retry/Menu callbacks, Home, and Play after Home
