# Implementation Plan

## Delivery Strategy

Use the docs set as the durable operating system for the project. Foundation documentation is completed first, then future work proceeds one approved feature at a time. The preferred next delivery slice is to converge progression, rewards, and meta economy rules before broadening content or polishing UI tooling.

## Phases

| Phase | Goal | Exit Criteria | Status |
| --- | --- | --- | --- |
| 1 | Foundation | Core docs, architecture, style guide, registries, and decisions reflect repo truth and approved direction | Completed |
| 2 | First approved feature slice | Exactly one chosen feature, or one explicitly approved milestone bundle, is documented, implemented, validated, and reflected in all registries | In progress |
| 3 | Expansion | Additional approved features are implemented in priority order with docs kept current | Not started |
| 4 | Polish and validation | UI polish, tuning, telemetry coverage, testing, and documentation cleanup are complete for the chosen release target | Not started |

## Implementation Order

1. Read `product-requirements.md`, `architecture.md`, `style-guide.md`, and the registries.
2. Choose one feature from `feature-registry.md`.
3. Move that feature to `In progress` and add or update its tasks in `task-registry.md`.
4. Implement only that feature slice, or the explicitly approved milestone bundle.
5. Update `script-registry.md`, `decision-registry.md`, and validation notes before closing the slice.

## Current Recommended Order

1. Analytics coverage expansion
2. UI tooling and polish improvements
3. Booster and pre-run economy extension
4. Environment unlock and level-select completion
5. Remaining progression-shell polish and tuning

## Dependencies and Blockers

- Environment unlock work depends on agreed unlock thresholds and content definitions.
- Monetisation expansion depends on an approved catalog and entitlement model.
- UI tooling changes depend on an explicit package adoption decision.
- Shop, ship customisation, lab, tasks, daily login, and achievements all require Unity runtime validation before the broader progression milestone can be considered fully reviewed.

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
