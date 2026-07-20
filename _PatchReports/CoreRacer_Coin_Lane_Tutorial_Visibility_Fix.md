# CoreRacer Coin Lane and Tutorial Visibility Fix

## Root Causes

- Coin positions were generated at `side * 60 degrees`, which places them on hex corners. Wall-centre placement is `30 + side * 60 degrees`.
- The earlier correction rotated only the visual and therefore did not change pickup position.
- The tutorial was not missing: the live save was `completed=True` for `core_racer_ftue_v3`. The PlayMode onboarding test had completed the same persistent editor profile and left it completed.

## Changes

- Restored an inspectable coin hierarchy: centre `PickupView` pivot -> radius-offset `PickupBody` -> coin visual.
- Moved the coin trigger onto `PickupBody` and added a thin relay to the pooled root.
- Changed generated coin angles to `30, 90, 150, 210, 270, 330` degrees.
- Updated pickup feedback positions to use the radial body's world position.
- Advanced the corrected tutorial to `core_racer_ftue_v4`, invalidating the accidentally completed v3 state once.
- Updated PlayMode coverage to preserve and restore the editor's tutorial save around every test.

## Live Unity Verification

- Active coin roots reported `(0,0,z)`.
- Every `PickupBody` reported local position `(3,0,0)`.
- Sample root angles were `30, 90, 150, 270, 330` degrees.
- The offset child trigger collected a coin and increased the run coin total.
- Tutorial state reported `completed=False`, current step `welcome`, and an active overlay panel.
- After starting the run, the live overlay displayed `Move left and right` and the drag/input instructions.

## Automated Validation

- EditMode: 39/39 passed.
- PlayMode: 12/12 passed.
- Coin coverage proves centre pivot placement, wall-centre angles, radial child structure, and real trigger collection.
- Tutorial coverage proves crash/Continue completion while save isolation leaves the live editor at `welcome`.

## Files Changed

- `Assets/CoreRacer/Editor/Builders/CoreRacerDefaultConfigBuilder.cs`
- `Assets/CoreRacer/Editor/Builders/CoreRacerPhase4AssetWiring.cs`
- `Assets/CoreRacer/Editor/Builders/CoreRacerPhase5UiBuilder.cs`
- `Assets/CoreRacer/Generated/Configs/TutorialConfig.asset`
- `Assets/CoreRacer/Generated/Prefabs/PickupCoin_AssetWired.prefab`
- `Assets/CoreRacer/Runtime/Gameplay/Pickups/PickupPatternGenerator.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Pickups/PickupView.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Pickups/PickupWorldController.cs`
- `Assets/CoreRacer/Tests/PlayMode/CoreRunPlayModeSmokeTests.cs`
- `DELETE_NOTHING.txt`
- `_PatchReports/CoreRacer_First_Run_Gameplay_Onboarding_Phase.md`
- `docs/feature-registry.md`
- `docs/implementation-plan.md`
- `docs/script-registry.md`
- `docs/task-registry.md`
- `docs/user-testing/20-first-run-gameplay-onboarding.md`

## Files Added

- `Assets/CoreRacer/Runtime/Gameplay/Pickups/PickupTriggerRelay.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Pickups/PickupTriggerRelay.cs.meta`
- `_PatchReports/CoreRacer_Coin_Lane_Tutorial_Visibility_Fix.md`

## Deletions

- None.
