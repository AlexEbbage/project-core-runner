# Core Racer Verticals 5-8 Super Patch Report

## Objective

Integrate the supplied later vertical patches into the current replacement project, reconcile them with the actual source architecture, and correct the most serious compile/readiness, reward, persistence and runtime-integration risks before Unity testing.

## Supplied verticals applied

- Vertical 5: final menu set, navigation and meta-loop wiring
- Vertical 6: progression snapshots, economy and retention
- Vertical 7: commercial services, IAP/ad seams, privacy and compliance
- Vertical 8: closed-testing validation, tests and release gates

Verticals 1-4 were not present as runtime patch bundles. Existing gameplay/config implementations were retained.

## Corrections layered after the vertical merge

### Bootstrap and dependencies

- Deterministic early `GameBootstrapper` execution
- Duplicate bootstrap guard before service creation
- Complete local registry composition before global publication
- Registry-change event for consumers that can enable before services are available
- Safe registry clearing across play-mode/domain lifecycle changes

### Rewards, claims and purchases

- Idempotent run ending and one-run settlement
- One-shot/in-flight continue and x2 requests
- x2 grants bonus currency/XP only, never a second completed run
- Daily, task and achievement claim rewards and markers committed together
- Unlock purchase spend and unlock committed together
- Hangar upgrade spend and level committed together
- Duplicate Play presses rejected before analytics/tutorial start events

### IAP, ads and compliance

- No immediate purchase success
- Every accepted store request must complete through a callback
- No premium entitlement before successful product/receipt handling
- No free currency-pack grants
- Explicit provider/adapter readiness instead of controller-presence false positives
- HTTPS-only production link validation with placeholder rejection
- Privacy/data-control flow from supplied commercial patch retained

### Save/profile safety

- Previous-known-good backup retained rather than overwritten with the new primary
- Batched PlayerPrefs mutations with one durable flush per logical safe-save operation
- Null collection and negative counter repair during migration

### Gameplay/data integration

- Selected level applies tunnel sides, starting speed, difficulty multiplier and zone
- Speed curve and camera FOV intensity used during runs
- Powerup collection progression recorded
- Magnet implemented and reset correctly
- Auto pilot performs local obstacle avoidance instead of holding zero input
- Slow motion modifies player forward speed rather than global time scale
- Touch input supports legacy input and Input System builds
- Ship stats and four upgrade tracks affect subsequent runs with bounded multipliers

### Unity asset/editor integrity

- Split `StringTable`, `RunZoneCatalog` and ship ScriptableObjects into class-matching files
- Repaired generated asset script references
- Added idempotent super-patch installer and integration validator
- Added/merged Vertical 5-8 tests plus super-patch safety tests
- Improved missing-script scan to detect unresolved MonoScript GUIDs, not only literal zero file IDs

## Files to delete

None.

## Validation performed outside Unity

- C# brace sanity
- `.meta` GUID uniqueness
- CoreRacer YAML zero-script scan
- localisation key/reference scan
- patch whitespace validation

## Validation still required in Unity

- Script compile and package API compatibility
- Installer execution and scene serialization
- EditMode test runner
- Play-mode smoke test
- Android/device build
- Real ads/IAP lifecycle and interruption testing

## Next implementation slice after error correction

After the first Unity compile/import correction pass, the next slice should be a focused playability and wiring validation patch: resolve any scene-reference drift, validate level geometry changes visually, tune auto-pilot collision layers, and add PlayMode tests for crash/continue/x2/retry/menu transitions.
