# Core Racer MVP Obstacle Pattern Rework

## Outcome

The core run now uses the preserved authored wedge, fan, and door meshes instead of repeating crossed cube blocks. Obstacles are selected in short pattern groups, snapped to the six-sided tunnel, and unlocked by elapsed run difficulty. The MVP presentation remains a neutral two-tone tunnel with red obstacles and the existing orange gameplay VFX.

## File Manifest

Modified files:

- `Assets/CoreRacer/Generated/Configs/ObstacleGeneration.asset`
- `Assets/CoreRacer/Runtime/Gameplay/Environment/RunZoneManagerV2.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Environment/TunnelWallGeneratorV2.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Obstacles/DoorObstacle.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Obstacles/ObstaclePatternDefinition.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Obstacles/ObstacleRingSpawner.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Obstacles/ObstacleRingView.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Obstacles/ObstacleWorldController.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Vfx/VfxManager.cs`
- `Assets/CoreRacer/Tests/PlayMode/CoreRunPlayModeSmokeTests.cs`
- `DELETE_NOTHING.txt`
- `docs/decision-registry.md`
- `docs/feature-registry.md`
- `docs/implementation-plan.md`
- `docs/script-registry.md`
- `docs/task-registry.md`
- `docs/user-testing/README.md`

New files:

- `Assets/CoreRacer/Editor/Builders/CoreRacerMvpObstacleBuilder.cs` and `.meta`
- `Assets/CoreRacer/Generated/Configs/Obstacles.meta`
- `Assets/CoreRacer/Generated/Configs/Obstacles/ObstaclePattern_door.asset` and `.meta`
- `Assets/CoreRacer/Generated/Configs/Obstacles/ObstaclePattern_fan.asset` and `.meta`
- `Assets/CoreRacer/Generated/Configs/Obstacles/ObstaclePattern_wedge_easy.asset` and `.meta`
- `Assets/CoreRacer/Generated/Configs/Obstacles/ObstaclePattern_wedge_hard.asset` and `.meta`
- `Assets/CoreRacer/Generated/Configs/Obstacles/ObstaclePattern_wedge_medium.asset` and `.meta`
- `Assets/CoreRacer/Generated/Prefabs/ObstacleVariants.meta`
- `Assets/CoreRacer/Generated/Prefabs/ObstacleVariants/Obstacle_Door.prefab` and `.meta`
- `Assets/CoreRacer/Generated/Prefabs/ObstacleVariants/Obstacle_Fan.prefab` and `.meta`
- `Assets/CoreRacer/Generated/Prefabs/ObstacleVariants/Obstacle_WedgeGate_Easy.prefab` and `.meta`
- `Assets/CoreRacer/Generated/Prefabs/ObstacleVariants/Obstacle_WedgeGate_Hard.prefab` and `.meta`
- `Assets/CoreRacer/Generated/Prefabs/ObstacleVariants/Obstacle_WedgeGate_Medium.prefab` and `.meta`
- `Assets/CoreRacer/Tests/EditMode/ObstaclePatternConfigurationTests.cs` and `.meta`
- `_PatchReports/CoreRacer_MVP_Obstacle_Pattern_Rework.md`
- `docs/user-testing/17-core-obstacle-patterns.md`

## Root Cause

The current `ObstacleRingView` only instantiated the generic segment cube, and `ObstacleGeneration.asset` referenced a single starter pattern. The older Blender-derived obstacle prefabs still existed, but their legacy controller scripts and configuration references no longer resolved. Pattern iteration ranges were also present in data but ignored by the current spawner.

## Behaviour Added

- Five clean generated obstacle prefabs: three wedge gates, a rotating fan, and a cycling door.
- Five authored pattern assets with weighted difficulty windows, grouping ranges, spacing, rotation, and fan speed.
- Pattern groups repeat for their configured iteration range before a new weighted selection.
- Pattern placement rotates in 60-degree steps for the always-hexagonal MVP tunnel.
- Generated rigidbodies are kinematic and gravity-free; hazard colliders are triggers.
- The tunnel alternates two neutral white/grey material slots along its length.
- Environment routes no longer tint the MVP tunnel or orange feedback VFX.
- The editor rebuild command is idempotent: `Tools > Core Racer > Playability > Rebuild MVP Obstacles`.

## Validation

- Unity compilation: passed with no new compiler errors.
- EditMode: 34 passed, 0 failed.
- PlayMode: 7 passed, 0 failed.
- Live Unity inspection confirmed `CoreRacer_Main` remained active and an authored red wedge prefab spawned in the run.
- Existing unrelated console debt remains: Unity IAP is initialized before UGS, and two legacy missing-script messages appear during PlayMode domain reload.

## Manual Test

Follow `docs/user-testing/17-core-obstacle-patterns.md`. Run for at least 75 seconds and confirm wedge groups appear first, followed by fan/door patterns, with readable safe routes and a clean difficulty reset after Retry.

## Installation

1. Close Play Mode.
2. Copy the changed-files-only patch over the project root, preserving paths.
3. Open `Assets/CoreRacer/Scenes/CoreRacer_Main.unity`.
4. Allow Unity to import and compile.
5. If generated obstacle assets need rebuilding, run `Tools > Core Racer > Playability > Rebuild MVP Obstacles` once.
6. Run EditMode and PlayMode tests before device testing.

## Deletions

None. Original release obstacle prefabs and the existing scene hierarchy are preserved.

## Recommended Next Slice

Gameplay audio and impact feedback: obstacle pass-by audio, pickup/collision/shield cues, speed-responsive tunnel ambience, and a small mix pass tied to the verified run lifecycle.
