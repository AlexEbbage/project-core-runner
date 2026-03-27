# Implementation Plan

## Delivery Strategy

Use the docs set as the durable operating system for the project. Foundation documentation is completed first, then future work proceeds one approved feature at a time. The current delivery slice is the progression-shell review closeout: progression config loading is now hardened for runtime builds, and the remaining signoff gate is the live `GameScene` review pass.

## Phases

| Phase | Goal | Exit Criteria | Status |
| --- | --- | --- | --- |
| 1 | Foundation | Core docs, architecture, style guide, registries, and decisions reflect repo truth and approved direction | Completed |
| 2 | First approved feature slice | Exactly one chosen feature, or one explicitly approved milestone bundle, is documented, implemented, validated, and reflected in all registries | In progress |
| 3 | Expansion | Additional approved features are implemented in priority order with docs kept current | Not started |
| 4 | Polish and validation | UI polish, tuning, telemetry coverage, testing, and documentation cleanup are complete for the chosen release target | In progress |

## Implementation Order

1. Read `product-requirements.md`, `architecture.md`, `style-guide.md`, and the registries.
2. Choose one feature from `feature-registry.md`.
3. Move that feature to `In progress` and add or update its tasks in `task-registry.md`.
4. Implement only that feature slice, or the explicitly approved milestone bundle.
5. Update `script-registry.md`, `decision-registry.md`, and validation notes before closing the slice.

## Current Recommended Order

1. Progression-shell review closeout
2. Booster review pass
3. Environment unlock and level-select completion
4. Remaining UI polish follow-up if needed

## Dependencies and Blockers

- Environment unlock work depends on agreed unlock thresholds and content definitions.
- Monetisation expansion depends on an approved catalog and entitlement model.
- UI tooling changes depend on an explicit package adoption decision.
- Progression-shell config assets are now loaded through `Resources`, but the closeout still depends on a live Unity pass to confirm authored wiring, badges, overlays, and menu-to-run-to-menu flow in `GameScene`.
- Boosters, shop, ship customisation, lab, tasks, daily login, achievements, and UI polish all require Unity runtime validation before the broader progression milestone can be considered fully reviewed.
- DOTween is now the approved UI motion layer, so UI polish work should use the reusable motion helper rather than introducing a second transition stack.

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
