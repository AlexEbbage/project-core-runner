# Core Racer Core Run Readability Tuning

Date: 2026-07-15

## Outcome

The inverted start was corrected first and committed separately as `032d83d`. Core runs now begin on the bottom tunnel rail while the ship and camera remain visually upright.

The subsequent readability pass holds the ship in the lower portrait viewport, reduces steering from 180 to 140 degrees/second, replaces the white tunnel wash with a renderer-local slate tint, and clears stale thruster history after the player teleports into a run.

## Live Unity Evidence

- Active scene: `Assets/CoreRacer/Scenes/CoreRacer_Main.unity`.
- Run state: `Running`.
- Start angle: 270 degrees.
- Player position at start: `(0, -3, 0)`.
- Camera position at start: `(0, -1.75, -10)`.
- Camera roll at start: approximately 0 degrees.
- Player viewport position: `(0.500, 0.411, 10.000)`.
- Steering speed: 140 degrees/second.
- Runtime wall tint: `(0.24, 0.32, 0.48, 1)`.
- Thruster trail position count immediately after the final start: 0; the previous diagonal start streak is absent.

## Implementation

- `PlayerOrbitalMotor` resets at 270 degrees and uses the matching upright roll basis.
- The authored camera offset is now `(0, 1.25, -10)` in camera-local roll space.
- `TunnelWallGeneratorV2` applies a `MaterialPropertyBlock` tint, preserving the shared `WallMaterial` asset.
- `PlayerController` caches child trails and clears them after `ResetMotor` moves the player.
- The PlayMode smoke test proves bottom-rail reset, portrait framing before and during steering, comfort speed, tunnel contrast, and stale-trail clearing.

## Validation

- Final PlayMode job `40751fa484854dda8992ed7098843c6b`: 3 passed, 0 failed.
- Final EditMode job `096771789d2d473bbb5b3a03e0e3a62b`: 29 passed, 0 failed.
- Live portrait capture: `_PatchReports/Screenshots/CoreRacer_Readability_Tuned_Final.png`.

## Changed Existing Files

- `Assets/CoreRacer/Runtime/Gameplay/Environment/TunnelWallGeneratorV2.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Player/PlayerController.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Player/PlayerOrbitalMotor.cs`
- `Assets/CoreRacer/Scenes/CoreRacer_Main.unity`
- `Assets/CoreRacer/Tests/PlayMode/CoreRunPlayModeSmokeTests.cs`
- `docs/feature-registry.md`
- `docs/implementation-plan.md`
- `docs/script-registry.md`
- `docs/task-registry.md`
- `docs/user-testing/04-hub-main-menu.md`

## New Files

- `_PatchReports/CoreRacer_Core_Run_Readability_Tuning.md`
- `_PatchReports/Screenshots/CoreRacer_Readability_Tuned_Final.png`

## Deletions

No existing file, scene object, prefab, material, or gameplay asset is deleted.

## Remaining Human Check

Run the complete lifecycle on the target portrait device and judge steering comfort, tunnel edge readability, hazard anticipation distance, and motion comfort. Limit any follow-up to the exposed tuning values unless that review proves a functional defect.

## Next Slice

Obtain human core-loop signoff for Play, steering, collision, Game Over, Retry, and Home. Only after signoff should progression or booster review resume.
