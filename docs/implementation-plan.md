# Implementation Plan

## Delivery Strategy

Use the docs set as the durable operating system for the project. Foundation documentation is completed first, then future work proceeds one approved feature at a time. The current delivery slice is the progression expansion bundle: level roadmap, task breadth, achievement breadth, and daily-login x2 support are now in repo, and the clean `CoreRacer_Main` scene now carries the authored Phase 5 hub shell needed for runtime validation.

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

1. Progression expansion runtime review
2. Booster review pass
3. Remaining UI polish and scene-wiring follow-up if needed
4. Next content expansion after runtime signoff

## Current Asset Wiring Status

- `CoreRacer_Main.unity` now uses generated wrappers around safe existing player, obstacle, pickup, tunnel, audio, and VFX assets.
- `CoreRacer_Main.unity` now also contains an authored clean hub flow for `Play`, `Shop`, `Hangar`, `Lab`, `Progression`, and `Settings`, with nested level select, daily login, rotating tasks, achievements, comfort, privacy, and support/debug surfaces.
- `CoreRacer_Main.unity` now has the Phase 6 FTUE tutorial overlay, scene director, deterministic first coin/powerup assistance, support reset action, and save-backed tutorial progress wiring.
- `CoreRacer_Main.unity` now has Phase 7 safe SDK adapter wiring for verified Unity IAP, Firebase Analytics, and Mobile Notifications APIs; LevelPlay, Crashlytics, and Addressables remain disabled/manual setup blockers.
- Phase 8 final validation and handoff is captured in `docs/rewrite/final-handoff.md`; closed testing remains blocked by placeholder privacy links and the build settings still targeting legacy `Assets/Scenes/GameScene.unity`.
- Manual art follow-up is tracked in `docs/rewrite/manual-art-wiring-needed.md`.

## Dependencies and Blockers

- Monetisation expansion depends on an approved catalog and entitlement model.
- UI tooling changes depend on an explicit package adoption decision.
- The progression bundle now depends on a player-facing review pass in `CoreRacer_Main` to confirm navigation clarity, layout polish, content readability, and FTUE pacing across the authored hub shell.
- Boosters, shop, ship customisation, lab, tasks, daily login, achievements, and UI polish still require broader UX review even though scene wiring, localization validation, and product-catalog validation now pass in the clean scene.
- DOTween is now the approved UI motion layer, so UI polish work should use the reusable motion helper rather than introducing a second transition stack.
- SDK release blockers remain for LevelPlay C# API installation/verification, Firebase Crashlytics installation, Addressables installation if remote content is required, and replacement of placeholder privacy links.
- Build settings must be corrected or explicitly approved before closed testing because they currently include `Assets/Scenes/GameScene.unity` instead of the clean `CoreRacer_Main.unity`.

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
