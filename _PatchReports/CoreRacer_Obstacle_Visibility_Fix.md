# Core Racer Obstacle Visibility Fix

## Root Cause

Obstacle rings and authored renderers were spawning correctly, but the recovered Blender prefabs retained dimensions intended for the older, larger tunnel. Most red geometry sat outside the current radius-four tunnel and was depth-occluded by the tunnel wall.

## Fix

- Added a validated per-pattern authored-obstacle scale.
- Set every current MVP obstacle pattern to `0.38`, fitting wedges, fans, and doors inside the current hex tunnel.
- Applied the scale whenever a pooled ring is rebuilt, so Retry and prefab swaps remain correct.
- Extended EditMode and PlayMode coverage to lock the fitted-scale contract.

## Live Verification

Unity MCP confirmed a clean run spawned fifteen rings, with the first three wedge groups at Z 30, 40, and 50 using scale `0.38`. A live Game view capture showed the red wedge geometry visible in front of the alternating tunnel walls.

## Automated Validation

- EditMode: 34 passed, 0 failed.
- PlayMode: 7 passed, 0 failed.

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
- `_PatchReports/CoreRacer_Obstacle_Visibility_Fix.md`

Nothing must be deleted.

## Next Slice

Player review and tuning of obstacle readability, collision fairness, pattern gaps, fan speed, and door timing before beginning the gameplay-audio phase.
