# Core Racer — Definition of Done

A vertical is not done when code exists. It is done when the player flow works.

## Universal done criteria

Every vertical must satisfy:

1. Works in `CoreRacer_Main`.
2. Has no obvious console errors during the target flow.
3. Has no visible dead buttons unless intentionally disabled with clear copy.
4. Updates relevant docs.
5. Adds or updates tests for deterministic logic.
6. Has a manual QA checklist.
7. Does not introduce missing script references.
8. Does not reintroduce old scene/menu/source-of-truth drift.

## Player-facing done criteria

A player can complete the intended flow without developer explanation.

For example:

- Vertical 1: player can start a run, collect coins, crash, and retry.
- Vertical 2: player understands each obstacle type.
- Vertical 5: player can navigate all final menus.
- Vertical 6: player can earn and spend rewards.

## Technical done criteria

The implementation should:

- use the clean `Assets/CoreRacer` runtime path,
- avoid reviving stale `Assets/Scripts` code,
- keep data-driven configuration where practical,
- isolate service integrations behind interfaces/fakes,
- support validation/debug tools where useful,
- preserve save/profile compatibility or provide migration.

## No-go criteria

Do not mark a vertical complete if:

- it only works from an editor-only debug path,
- the main scene still shows placeholder/dead UI for that flow,
- the feature works in isolation but not through the real menu/run flow,
- the game becomes less readable or less fun,
- monetisation blocks basic play,
- major behaviour contradicts BDD without updating the BDD decision log.
