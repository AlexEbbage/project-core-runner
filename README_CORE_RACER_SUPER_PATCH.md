# Core Racer Verticals 5-8 Super Patch

This changed-files-only patch merges the **supplied** Vertical 5, 6, 7 and 8 patches into the current replacement project, then applies the integration and safety corrections identified during review.

## Scope clarification

The supplied patch archive did **not** contain Vertical 1-4 runtime patches. Their generated configs and current project implementations were preserved and integrated where possible, but this package does not pretend to recreate missing patch bundles.

Included:

- Vertical 5: final menus and meta-loop routing
- Vertical 6: progression, economy and retention
- Vertical 7: commercial services and compliance
- Vertical 8: closed-testing hardening and validation
- Super-patch corrections for bootstrap ordering, rewards, saves, purchases, IAP, ads, level/speed integration, powerups, touch input, hangar upgrades and Unity asset/script references
- EditMode tests and an idempotent Unity editor installer

## Install

1. Back up or commit the current Unity project.
2. Close Unity.
3. Extract this zip into the **project root**, preserving paths and allowing matching files to be overwritten.
4. Reopen Unity and allow script compilation/import to finish.
5. Run:

   `Tools > Core Racer > Super Patch > Apply Verticals 5-8 + Corrections`

6. Then run:

   `Tools > Core Racer > Super Patch > Validate Integration`

7. Run the EditMode tests under:

   `Assets/CoreRacer/Tests/EditMode`

8. Run the broader closed-testing validator:

   `Tools > Core Racer > Vertical 8 > Validate Closed Testing Hardening`

The installer is designed to be idempotent. It reuses existing components, buttons and generated assets rather than deliberately duplicating them.

## First smoke test

Check these flows in order:

1. Main menu opens and every bottom-navigation page routes correctly.
2. Level selection reaches the run controller; different levels alter tunnel sides, starting speed, difficulty and zone where configured.
3. Start a run, collect coins and powerups, crash, continue or decline, and reach Game Over.
4. Repeated death/end-run/button presses grant the completed-run reward only once.
5. The x2 reward grants only the bonus currency/XP and does not add another completed run or another powerup count.
6. Retry starts one new run; Hub returns to the menu.
7. Daily, task and achievement claims cannot be duplicated by rapid clicks.
8. Shop unlocks spend and unlock in one save. Currency packs do not grant free currency. Premium purchase stays pending until a store callback.
9. Hangar upgrades spend and advance atomically and affect the next run.
10. Keyboard and mobile touch steering both work for the configured input backend.
11. Magnet, slow motion and auto pilot activate, expire and reset cleanly between runs.

## What to send back when Unity reports errors

Send the **first compiler error in full**, including:

- error code
- complete file path
- line and column
- full Console text/stack trace
- Unity editor version
- Unity IAP package version if the error is purchasing-related
- whether `CORE_RACER_UNITY_IAP` is defined

Fixing the first error first usually prevents cascaded errors from obscuring the root cause.

## Static validation completed before packaging

- no unmatched C# braces in `Assets/CoreRacer`
- no duplicate `.meta` GUIDs
- no `m_Script: {fileID: 0}` references in supplied CoreRacer YAML assets
- no missing referenced UI localisation keys
- no `git diff --check` errors
- changed/new files only; no deletions

A Unity editor/compiler was not available in the analysis environment, so the real Unity compile, asset import, scene save and test runner remain the first verification step on your machine.
