# Core Racer Core Run Camera and Tunnel Closeout

Date: 2026-07-15

## Outcome

The gameplay camera now stays behind the player and follows the player's orbital roll. The craft remains upright and stable in frame while left/right steering makes the tunnel appear to rotate, making the controls easier to comprehend.

The live scene now has a route-configured `RuntimeTunnel` using the existing `TunnelWallGeneratorV2`. The default route generates a six-sided mesh with 48 longitudinal sections across 240 units and recenters it in 40-unit steps while preserving 20 units behind the player.

## Root Cause

- `PlayerCameraFollow` followed the player's position but forced world rotation to identity.
- The player therefore visibly rotated around the fixed screen/tunnel reference.
- The active FBX tunnel was approximately 10 world units deep and had no runtime generator attached.
- `TunnelWallGeneratorV2` already existed but was not present in `CoreRacer_Main` and was not configured by `RunController`.

## Implementation

- Added opt-in roll-follow behavior to `PlayerCameraFollow`.
- Orbital X/Y position follows exactly; forward Z retains the existing smoothing.
- Added target following and forward recycling to `TunnelWallGeneratorV2`.
- Added a scene-authored `TunnelRoot/RuntimeTunnel` with the existing wall material.
- Wired the selected route side count through `RunController`.
- Kept `TunnelV2Prefab` intact and visible in edit mode; it is hidden only while the runtime tunnel is active in Play Mode.
- Added required-reference validation for the runtime tunnel generator.

## Live Evidence

- New Input System right input read `1` and moved the player from 90 to 135 degrees.
- Player roll: 45 degrees.
- Camera roll: 45 degrees.
- Measured roll delta: 0 degrees.
- Camera/player orbital X and Y deltas: 0.
- Camera remained approximately 11 units behind the player.
- Runtime mesh: `CoreRacer_TunnelWall`, 294 vertices, 6 sides, 48 sections.
- Tunnel start/end at the sustained sample: Z=1640.2 to Z=1880.2 while the player was at Z=1672.1.
- Obstacles and pickups remained visible inside the generated tunnel.

## Tests

- PlayMode job `7c221b04133048c5a003eefd88043414`: 3 passed, 0 failed.
- EditMode job `5e2a70fb0813456e8c0186a094e02b5c`: 29 passed, 0 failed.
- `VisiblePlay_StartsCoreGameplay` now verifies generated mesh presence, side/section configuration, forward recycling, behind-player X/Y alignment, and matching player/camera roll.

## Changed Existing Files

- `Assets/CoreRacer/Runtime/Gameplay/Environment/TunnelWallGeneratorV2.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Player/PlayerCameraFollow.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Run/RunController.cs`
- `Assets/CoreRacer/Scenes/CoreRacer_Main.unity`
- `Assets/CoreRacer/Tests/PlayMode/CoreRunPlayModeSmokeTests.cs`
- `docs/decision-registry.md`
- `docs/feature-registry.md`
- `docs/implementation-plan.md`
- `docs/script-registry.md`
- `docs/task-registry.md`
- `docs/user-testing/04-hub-main-menu.md`

## New Files

- `_PatchReports/CoreRacer_Core_Run_Camera_Tunnel_Closeout.md`
- `_PatchReports/Screenshots/CoreRacer_Camera_Baseline.png`
- `_PatchReports/Screenshots/CoreRacer_Camera_Tunnel_After.png`

## Deletions

No existing project file, scene object, prefab, or authored tunnel asset is deleted.

## Manual Test

1. Open `CoreRacer_Main` and press Play.
2. Start the visible Play route.
3. Hold left and right through several tunnel sides.
4. Confirm the craft remains stable/upright in frame and the tunnel rotates around it.
5. Confirm the camera remains behind the craft without lateral lag.
6. Continue beyond the initial tunnel length and confirm tunnel geometry remains around and ahead of the player.
7. Confirm obstacles, pickups, scoring, collision, Game Over, Retry, and Home still work.
8. Assess motion comfort and tunnel material brightness on a portrait mobile display.

## Risks and Next Slice

The functional camera/tunnel contract is verified, but visual comfort is subjective. The next slice should be a human tuning pass for roll speed, tunnel material brightness, hazard contrast, and portrait framing. Do not expand shop, IAP, adverts, or progression until the core run feel is signed off.
