# Core Racer Obstacle Visibility Fix

## Root Cause

Obstacle rings and authored renderers were spawning correctly, but the recovered Blender prefabs retained dimensions and cross-section alignment intended for the older tunnel mesh. The provisional `0.38` scale made them visible but placed their radial span inside the player's three-unit orbit, leaving gaps at the tunnel wall and preventing normal gameplay collisions.

## Fix

- Measured the preserved `tunnel_v3.fbx` inner radius as `7` and the current procedural tunnel radius as `4`.
- Set every MVP obstacle pattern to the exact mesh-derived scale `4 / 7` (`0.5714286`). The authored wedge span `5.25–7` therefore maps to `3–4`: player orbit to tunnel wall.
- Measured the old tunnel vertices at 30-degree offsets versus the procedural tunnel vertices at 0-degree offsets, then applied a `-30°` pattern correction.
- Applied scale and rotation whenever a pooled ring is rebuilt, so Retry and prefab swaps remain correct.
- Extended EditMode and PlayMode coverage for exact scale, hex alignment, orbit penetration, and lethal damage routing.

## Live Verification

Unity MCP confirmed active wedges at scale `0.5714286`, aligned at 30-degree modulo-60 rotations. Every inspected active wedge produced physical penetration against the player's three-unit orbit. A live trigger collision reduced health to zero and transitioned the run to `ContinueOffered`; the Game view showed the red wedge endpoints meeting the tunnel walls.

## Automated Validation

- EditMode: 34 passed, 0 failed.
- PlayMode: 8 passed, 0 failed.

## Files

- `Assets/CoreRacer/Editor/Builders/CoreRacerMvpObstacleBuilder.cs`
- `Assets/CoreRacer/Generated/Configs/Obstacles/ObstaclePattern_door.asset`
- `Assets/CoreRacer/Generated/Configs/Obstacles/ObstaclePattern_fan.asset`
- `Assets/CoreRacer/Generated/Configs/Obstacles/ObstaclePattern_wedge_easy.asset`
- `Assets/CoreRacer/Generated/Configs/Obstacles/ObstaclePattern_wedge_hard.asset`
- `Assets/CoreRacer/Generated/Configs/Obstacles/ObstaclePattern_wedge_medium.asset`
- `Assets/CoreRacer/Runtime/Gameplay/Obstacles/ObstaclePatternDefinition.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Obstacles/ObstacleRingView.cs`
- `Assets/CoreRacer/Tests/EditMode/ObstaclePatternConfigurationTests.cs`
- `Assets/CoreRacer/Tests/PlayMode/CoreRunPlayModeSmokeTests.cs`
- `docs/task-registry.md`
- `docs/user-testing/17-core-obstacle-patterns.md`
- `_PatchReports/CoreRacer_Obstacle_Visibility_Fix.md`

Nothing must be deleted.

## Next Slice

Player review and tuning of obstacle readability, collision fairness, pattern gaps, fan speed, and door timing before beginning the gameplay-audio phase.

# Single-wall collision follow-up

- Root cause: the recovered one-piece wall prefab retained `Discrete` collision detection while the player is a fast, transform-driven kinematic Rigidbody. Its thin trigger could therefore pass between physics steps; the three-piece wall happened to use continuous detection.
- Fix: all generated authored obstacle Rigidbodies are now kinematic `ContinuousSpeculative`, and every collider is consistently saved as an `Obstacle` trigger.
- Scope: obstacle meshes, calibrated scale, 30-degree alignment correction, spawn patterns, and the six-sided tunnel are unchanged.
