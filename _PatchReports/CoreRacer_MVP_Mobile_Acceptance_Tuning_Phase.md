# CoreRacer MVP Mobile Acceptance Tuning Phase

## Outcome

The default mobile control now responds immediately when the player touches either side of the screen. Players who enable Drag Controls retain continuous analog drag steering. The existing Continue path was verified to move the player behind the crash, restore normal time, and provide a two-second damage grace period.

## Changed Files

- `Assets/CoreRacer/Editor/Builders/CoreRacerPhase5UiBuilder.cs`
- `Assets/CoreRacer/Generated/Configs/StringTable.asset`
- `Assets/CoreRacer/Runtime/Gameplay/Player/PlayerInputReader.cs`
- `Assets/CoreRacer/Tests/PlayMode/CoreRunPlayModeSmokeTests.cs`
- `DELETE_NOTHING.txt`
- `docs/feature-registry.md`
- `docs/implementation-plan.md`
- `docs/script-registry.md`
- `docs/task-registry.md`
- `docs/user-testing/README.md`

## New Files

- `Assets/CoreRacer/Runtime/Gameplay/Player/TouchSteeringInterpreter.cs` and `.meta`
- `Assets/CoreRacer/Tests/EditMode/TouchSteeringInterpreterTests.cs` and `.meta`
- `docs/user-testing/21-mvp-mobile-acceptance.md`
- `_PatchReports/CoreRacer_MVP_Mobile_Acceptance_Tuning_Phase.md`

## Deleted Files

None. `DELETE_NOTHING.txt` is updated accordingly.

## Validation

- Unity compilation: passed; no new compiler errors.
- Focused touch tests: 8/8 passed.
- Focused sustained-run PlayMode smoke test: 1/1 passed.
- Full EditMode suite: 47/47 passed.
- Full PlayMode suite: 13/13 passed.
- Live run: Running at 87 m with score 138 and 14 active obstacle rings.
- Editor profiler sample: 94 batches, 82 SetPass calls, 17,448 triangles, 29,090 vertices, approximately 1.4 ms main-thread render work, and approximately 1.0 ms render-thread work. Editor instrumentation is not device certification.

Known pre-existing console noise remains: Unity IAP requires UGS initialization, Firebase has no database URL, and two scene behaviours report missing scripts. These are not introduced by this patch.

## Installation

1. Close Unity or allow it to finish importing.
2. Extract the changed-files-only zip at the Unity project root, preserving paths.
3. Reopen Unity 2022 LTS and wait for compilation/import.
4. Open `Assets/CoreRacer/Scenes/CoreRacer_Main.unity` and run both test suites.
5. Create a Development Build for the target portrait device and follow `docs/user-testing/21-mvp-mobile-acceptance.md`.

## Next Slice

Create Android/iOS release-candidate Development Builds and complete physical-device signoff for controls, safe areas, collisions, five-minute frame pacing/memory, Continue, Retry, and Home. Do not expand environments or monetisation until that evidence is recorded.
