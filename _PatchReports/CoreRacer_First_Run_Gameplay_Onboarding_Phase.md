# CoreRacer First-Run Gameplay Onboarding Phase

## Outcome

- Corrected the coin visual by applying a persistent `-30 degree` local Z rotation in both the prefab and its idempotent asset builder.
- Preserved the user's single-wall collider correction: cleared include/exclude layer overrides and changed collision detection from continuous speculative to continuous.
- Replaced the misleading always-visible `HULL 2/2` label with a contextual damaged-state readout.
- Upgraded the tutorial config to `core_racer_ftue_v3`.
- Removed the broken post-powerup menu redirect. The tutorial now waits for a real crash, shows the real Continue offer, and completes only after Continue restores the run.
- Kept meta/shop/progression teaching outside this gameplay-focused first-run slice.

## Validation

- Unity compilation completed with no new C# errors.
- EditMode: 39/39 passed.
- PlayMode: 12/12 passed.
- Added focused PlayMode coverage for coin alignment and the complete first-run crash/Continue sequence.
- Existing lifecycle coverage still proves visible Play, forward gameplay, crash, Continue, Retry, and Home.

## Known External Logs

- Unity IAP reports that Unity Gaming Services is not initialized.
- Firebase reports that its database URL is not configured.
- These pre-existing service configuration messages are outside the gameplay-onboarding slice.

## Scene and Serialization

- The existing `CoreRacer_Main.unity` hierarchy and references were inspected live through Unity MCP and were not regenerated.
- `PickupCoin_AssetWired.prefab` was saved through the Unity Editor.
- `Obstacle_WedgeGate_Easy.prefab` retains the user's manual serialized physics correction.
- No files must be deleted.

## Exact Changed Files

- `Assets/CoreRacer/Editor/Builders/CoreRacerDefaultConfigBuilder.cs`
- `Assets/CoreRacer/Editor/Builders/CoreRacerPhase4AssetWiring.cs`
- `Assets/CoreRacer/Editor/Builders/CoreRacerPhase5UiBuilder.cs`
- `Assets/CoreRacer/Generated/Configs/StringTable.asset`
- `Assets/CoreRacer/Generated/Configs/TutorialConfig.asset`
- `Assets/CoreRacer/Generated/Prefabs/ObstacleVariants/Obstacle_WedgeGate_Easy.prefab`
- `Assets/CoreRacer/Generated/Prefabs/PickupCoin_AssetWired.prefab`
- `Assets/CoreRacer/Runtime/FTUE/TutorialDirector.cs`
- `Assets/CoreRacer/Runtime/FTUE/TutorialStepKind.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Run/RunController.cs`
- `Assets/CoreRacer/Runtime/UI/Hud/HudController.cs`
- `Assets/CoreRacer/Tests/PlayMode/CoreRunPlayModeSmokeTests.cs`
- `DELETE_NOTHING.txt`
- `docs/feature-registry.md`
- `docs/implementation-plan.md`
- `docs/script-registry.md`
- `docs/task-registry.md`
- `docs/user-testing/19-portrait-gameplay-clarity.md`
- `docs/user-testing/README.md`

## Exact New Files

- `docs/user-testing/20-first-run-gameplay-onboarding.md`
- `_PatchReports/CoreRacer_First_Run_Gameplay_Onboarding_Phase.md`

## Deletions

- None. See `DELETE_NOTHING.txt`.

## Manual Test

Run `docs/user-testing/20-first-run-gameplay-onboarding.md` on a portrait device. Capture the movement prompt, corrected coin, crash/Continue state, lifecycle logs, and device frame rate.

## Next Slice

MVP mobile acceptance and tuning: touch-control feel, collision fairness, frame pacing, pooling stability, and a complete portrait-device lifecycle soak before expanding presentation or monetisation.
