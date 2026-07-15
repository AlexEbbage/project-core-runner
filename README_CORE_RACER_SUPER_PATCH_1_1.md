# Core Racer Super Patch 1.1 — Test and Playability Repair

This is a changed-files-only follow-up to `CoreRacer_Verticals_5_8_Super_Patch`.

It fixes the five reported EditMode test failures and, more importantly, automatically applies the serialized scene wiring that copying C# files alone cannot perform.

## Apply

1. Back up or commit the Unity project.
2. Extract this package into the project root and overwrite matching files.
3. Return to Unity and allow compilation/import to finish.
4. The new auto-installer should run once and log:
   - `Core Racer: applying Super Patch 1.1 playability wiring...`
   - `Core Racer playability integration validation passed.`
   - `Core Racer: Super Patch 1.1 playability wiring completed...`
5. If the automatic pass does not run, use:
   - `Tools > Core Racer > Super Patch > Repair Playability Wiring`
6. Run all EditMode tests again.
7. Open `Assets/CoreRacer/Scenes/CoreRacer_Main.unity` and enter Play Mode.

## Expected first playable flow

1. The Play page should show the first level as unlocked.
2. Press the large Play button in the page, not only the bottom navigation Play tab.
3. The Main Menu should hide and the ship should move forward automatically.
4. Use A/D or Left/Right to move around the tunnel.
5. Obstacles and pickups should spawn ahead.
6. On collision/death, Continue or End Run should lead to Game Over.
7. Retry and Hub should both work.

## What the automatic installer changes in the scene

- Assigns RunConfig, LevelRoadmap, SpeedScaling, camera FOV, run zone and ShipDatabase to RunController.
- Assigns the Play page and Level Select controller to RunController.
- Adds and wires PickupMagnetController, AutoPilotSteeringController and PlayerCosmeticsController.
- Completes PowerupContextBuilder references.
- Assigns PlayerCosmetics back to RunSceneReferences.
- Assigns PickupWorldController stats tracking.
- Creates and wires Retry and Double Rewards buttons if absent.
- Adds persistent fallback listeners to the central Play button and all bottom navigation buttons.
- Saves `CoreRacer_Main.unity` after the changes.
- Ensures the main scene is the enabled build scene through the existing Vertical 8 installer.

## Destructive fallback

The package does **not** automatically rebuild the menu UI because that would delete manual changes beneath `Canvas/MainMenu`.

If the validator passes but the generated menu is still visually corrupted, back up the scene and run:

`Tools > Core Racer > Super Patch > Rebuild Generated UI + Reapply (Destructive)`

This reruns the Phase 5 UI generator, recreates `Canvas/MainMenu`, and reapplies the complete super-patch wiring.

## Test fixes

- SafeSaveStorage no longer hashes a missing primary value as though it were a valid existing save.
- Checksum generation is defensive against null input.
- StartNewRun rejects a second start while a run is already active.
- The achievement test now explicitly establishes its 10-coin starting balance before expecting 110 after a 100-coin reward.
- The Play button resolves dependencies again when enabled and has a public installer-safe action.

## What to send back

If anything fails, send:

- the first compiler error, if present;
- all failed test names and stack traces;
- the full output from `Tools > Core Racer > Super Patch > Validate Integration`;
- a screenshot of the Game view and Hierarchy while the broken menu is visible;
- any Console logs produced when pressing the large Play button.
