# Core Racer Super Patch 1.1 Playability Report

## Reason for patch

The Verticals 5–8 super patch compiled and ran, but five EditMode tests failed and the copied source files had not caused Unity to serialize the new component/asset references into `CoreRacer_Main.unity`.

Static inspection of the supplied scene confirmed that the earlier Phase 4 and Phase 5 objects already exist. Rebuilding the complete scene is therefore unnecessary and potentially destructive. The missing step is applying the later installers and saving their changes.

## Root causes fixed

### SafeSaveStorage failures

`SafeSaveStorage.Save` called `IsValid` for a key that did not exist. A missing null value with no checksum was considered legacy-valid and was then passed to `ComputeChecksum`, causing the NullReferenceException.

The save now requires the primary key to exist before preserving it as the previous-known-good backup. Checksum generation also treats null defensively as an empty string.

### Duplicate run start failure

`RunStateMachine` deliberately permits same-state transitions, so a second `StartNewRun` while already Running returned true and reset the session.

`RunLifecycleService.StartNewRun` now accepts only MainMenu or Starting as entry states.

### Achievement expectation failure

The achievement reward was correctly granting 100 coins. `RecordRun(... coins: 10 ...)` records lifetime coins collected, but it does not add those coins to the wallet. The test expected an unstated initial wallet balance.

The test now explicitly adds the initial 10 coins before claiming the 100-coin achievement reward.

### Unapplied serialized wiring

The project contained newer fields and components in code, while `CoreRacer_Main.unity` still had the older serialized RunController and GameOver layouts. Copying `.cs` files cannot populate those Unity references.

A one-time `InitializeOnLoad` installer now runs after compilation, applies the existing Vertical 5–8 installers, performs the super-patch runtime wiring, adds fallback menu listeners, saves the scene, and runs the expanded validator.

## Expanded validation

Validation now checks:

- RunController and all essential config/asset references;
- required RunSceneReferences and PlayerCosmetics;
- Level Select roadmap, RunController and central Play listener;
- Game Over RunController, Retry, Hub and Double Rewards references;
- EventSystem and Canvas GraphicRaycaster;
- required menu router pages;
- player magnet, auto pilot and cosmetics components;
- LevelRoadmap, SpeedScaling, RunZoneCatalog and ShipDatabase assets.

## Files changed

- `Assets/CoreRacer/Runtime/Services/Save/SafeSaveStorage.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Run/RunLifecycleService.cs`
- `Assets/CoreRacer/Runtime/UI/MainMenu/LevelSelectPageController.cs`
- `Assets/CoreRacer/Editor/Verticals/CoreRacerSuperPatchInstaller.cs`
- `Assets/CoreRacer/Tests/EditMode/Vertical6ProgressionEconomyTests.cs`

## Files added

- `Assets/CoreRacer/Editor/Verticals/CoreRacerSuperPatchAutoInstaller.cs`
- `Assets/CoreRacer/Editor/Verticals/CoreRacerSuperPatchAutoInstaller.cs.meta`

## Deletions

None.

## Next implementation slice

After this patch is verified, the next slice is a real play-mode integration pass: resolve any remaining scene/runtime errors, confirm the run loop end-to-end, then replace the generated placeholder menu presentation with a deliberate final UI rather than continuing to add systems onto the generated scaffold.
