# CoreRacer Super Patch 1.2 — Live UI Quick Play

Patch name: `CoreRacer_SuperPatch_1_2_Live_UI_Quick_Play`

## Outcome

The visible bottom Play button now starts the selected valid core run. A development-only Quick Play path and `Tools > Core Racer > Playability > Start Core Run` editor command start the first valid roadmap level after validating required references and resetting `Time.timeScale`.

This patch preserves the existing scene and authored UI. It does not regenerate UI, add broad features, or delete assets.

## Exact Root Cause

- The active scene and Play Mode scene are both `Assets/CoreRacer/Scenes/CoreRacer_Main.unity`. `EditorSceneManager.playModeStartScene` is unset, so Unity starts the active scene. The same scene is the only enabled Build Settings scene observed during inspection.
- The scene has one active overlay Canvas with an enabled `GraphicRaycaster`, and one active EventSystem with `InputSystemUIInputModule`.
- `ProjectSettings.asset` uses active input handler value `1`: the new Input System only. The installed Input System package version observed was `1.7.0`.
- The visible bottom button at `Canvas/MainMenu/BottomNav/PlayButton` was active, interactable, raycastable, and had no blocking parent CanvasGroup. Its persistent listener called `BottomNavBarController.ShowPlay`.
- The Play page was already active. Pressing the visible button therefore only selected the already-selected page and appeared to do nothing.
- The actual run callback was on a separate CTA at `Canvas/MainMenu/Pages/PlayPage/Viewport/Content/PlayButton`. Its fixed/generated layout was only 100 pixels wide at the far left in the inspected live hierarchy, making it an unreliable player entry point.
- UI raycast diagnostics found no transparent overlay intercepting the visible Play click. The top hit was the button label and `ExecuteHierarchy` correctly resolved to the Play button.
- No duplicate Canvas, EventSystem, MainMenu root, Bootstrapper, or gameplay root caused the fault.
- Programmatically invoking the hidden CTA reached `LevelSelectPageController.PlaySelected`, synchronized the run selection, and started `RunController`, proving the gameplay startup path itself was valid.

The exact fix is to route the existing visible Play button to `BottomNavBarController.StartCoreRun`, which calls the validated selected-level startup path.

## Live Inspection and Runtime Evidence

- Patched pointer diagnostic: `PATCHED_POINTER_CLICK|topHit=Label|handled=PlayButton|state=Running|timeScale=1`.
- Editor command diagnostic: `EDITOR_QUICK_PLAY|playing=True|state=Running|timeScale=1|player=True|menu=False|hud=True`.
- Player was visible. A queued Input System right-steer event changed player angle from 90 to 338 degrees; an earlier live check changed it from 90 to 376 degrees.
- Forward position/distance increased from 762 to 788 during the patched live check. Score reached 1015 and distance reached 788.
- Obstacle and pickup worlds were populated (96 obstacle objects and 98 pickup objects in the sampled run).
- Natural collision reached `ContinueOffered`, health `0`, `Time.timeScale=0`, and displayed Game Over. Declining continued to terminal `GameOver` with `Time.timeScale=1`.
- The live Retry button callback started a second clean run: state `Running`, health `2`, score `0`, distance `0`, menu hidden, Game Over hidden.
- The live Home button callback returned to `MainMenu` with `Time.timeScale=1`, menu visible, HUD and Game Over hidden.
- Fresh EventSystem pointer execution after the structured-log follow-up returned `LIVE_CLICK|handled=True|state=Running|runId=7421bdb86a694d8aa399962b60f49d40|timeScale=1|player=True|menu=False|hud=True`. The previously observed delegate exception did not recur.
- A fresh live content sample found 50 active pickup views and 24 active obstacle rings. Invoking the real pickup trigger handler advanced coins from 46 to 47 and collected powerups from 21 to 22; distance was 1132.3, score was 1775, and camera follow was active.

### Important tunnel finding

The live `TunnelRoot` contains an active `tunnel_v3` mesh (`Tunnel`, 72 vertices) and an inactive `tunnel_v2` mesh. No runtime tunnel-section generator MonoBehaviour was found. Therefore dynamic tunnel-section generation is **not verified and is not claimed by this patch**. The run uses active static tunnel geometry while obstacle and pickup content generates. Product intent for dynamic tunnel sections needs a separate decision/slice.

### Remaining visual issue

The Game Over lifecycle and buttons work, but the captured screen has overlapping visual elements. This patch intentionally does not redesign or regenerate that UI. Human-facing Game Over layout correction is the recommended follow-up after core-loop signoff.

## Changed Existing Files

- `Assets/CoreRacer/Runtime/Gameplay/Run/RunController.cs`
- `Assets/CoreRacer/Runtime/UI/MainMenu/BottomNavBarController.cs`
- `Assets/CoreRacer/Runtime/UI/MainMenu/LevelSelectPageController.cs`
- `Assets/CoreRacer/Scenes/CoreRacer_Main.unity`
- `DELETE_NOTHING.txt`
- `docs/feature-registry.md`
- `docs/task-registry.md`
- `docs/implementation-plan.md`
- `docs/user-testing/04-hub-main-menu.md`

## Exact New Files

- `Assets/CoreRacer/Editor/Playability.meta`
- `Assets/CoreRacer/Editor/Playability/CoreRacerPlayabilityMenu.cs`
- `Assets/CoreRacer/Editor/Playability/CoreRacerPlayabilityMenu.cs.meta`
- `Assets/CoreRacer/Tests/PlayMode.meta`
- `Assets/CoreRacer/Tests/PlayMode/CoreRacer.Tests.PlayMode.asmdef`
- `Assets/CoreRacer/Tests/PlayMode/CoreRacer.Tests.PlayMode.asmdef.meta`
- `Assets/CoreRacer/Tests/PlayMode/CoreRunPlayModeSmokeTests.cs`
- `Assets/CoreRacer/Tests/PlayMode/CoreRunPlayModeSmokeTests.cs.meta`
- `_PatchReports/CoreRacer_SuperPatch_1_2_Live_UI_Quick_Play.md`
- `_PatchReports/Screenshots/CoreRacer_UI_Before.png`
- `_PatchReports/Screenshots/CoreRacer_Run_BeforePatch.png`
- `_PatchReports/Screenshots/CoreRacer_GameOver_BeforePatch.png`
- `_PatchReports/Screenshots/CoreRacer_Run_AfterPatch.png`
- `_PatchReports/Screenshots/CoreRacer_GameOver_AfterPatch.png`

## Deletions

Nothing must be deleted. See `DELETE_NOTHING.txt`.

## Installation

1. Back up or commit local project changes.
2. Extract the changed-files-only zip into the Unity project root, preserving paths and allowing the listed files to overwrite their existing versions.
3. Do not delete or regenerate the scene, Canvas, menu, or gameplay roots.
4. Open the project with its existing Unity 2022 LTS editor.
5. Allow Unity to import scripts and wait for compilation to finish.
6. Open `Assets/CoreRacer/Scenes/CoreRacer_Main.unity` if it is not already active.
7. Run EditMode and PlayMode tests, then perform the manual test below.

Applying the archive more than once is safe: files replace the same paths, the scene contains one serialized listener/reference change, and the command/test additions have stable asset metadata.

## Manual Test

1. Open `CoreRacer_Main` and enter Play Mode.
2. Press the visible bottom Play button. Confirm gameplay appears immediately and `Time.timeScale` is `1`.
3. Confirm the ship is visible. Steer left and right using the configured player controls.
4. Confirm forward movement, score, and distance increase.
5. Confirm obstacles and pickups populate and can be encountered.
6. Collide until the run ends. Confirm the continue/end flow reaches Game Over.
7. Press Retry. Confirm a second clean run starts with reset score/distance and normal time scale.
8. End the run and press Home. Confirm the main menu returns and gameplay/HUD are hidden.
9. Stop Play Mode. Use `Tools > Core Racer > Playability > Start Core Run`; confirm Unity enters Play Mode and starts the known first roadmap run.
10. Capture the Console on failures and capture the menu, active run, Game Over, retried run, and returned menu.

## PlayMode Tests Added

`CoreRunPlayModeSmokeTests.MainScene_HasOneEventSystemBootstrapperAndValidVisiblePlayListener` verifies:

- exactly one active EventSystem;
- exactly one active GameBootstrapper;
- the visible button is active/interactable;
- its persistent listener resolves to `StartCoreRun`.

`CoreRunPlayModeSmokeTests.VisiblePlay_StartsCoreGameplay` invokes the visible-Play controller route and verifies:

- lifecycle state is `Running`;
- a non-empty run session id is created;
- `Time.timeScale` is `1`;
- player is active;
- menu is inactive;
- HUD is active;
- duplicate start is rejected without changing the run id;
- forward movement and camera follow advance;
- coin state and a powerup activation path respond.

`CoreRunPlayModeSmokeTests.RunLifecycle_GameOverRetryHomeAndPlayAgainAreCleanAndIdempotent` verifies:

- run end reaches Game Over and remains idempotent;
- Retry creates a distinct, reset run session;
- Home restores the menu;
- Play starts another distinct run after Home.

## Validation Results

- Expanded PlayMode job `cae1ba85dbae4098a3230bccddaac537`: 3 passed, 0 failed.
- Full EditMode job `574375586fc248df87dff099661b0bbc`: 29 passed, 0 failed.
- Live pointer, editor-command, player input/movement, spawn population, natural collision, Game Over, Retry, and Home checks passed as described above.
- Core run reference validation reported 0 errors and 0 warnings in the sampled live run.

Observed unrelated/pre-existing console warnings include missing-script warnings in existing content, Unity IAP/UGS initialization, and Firebase database URL configuration. They did not fail the focused smoke test but should remain visible in release readiness tracking.

## Logs and Screenshots to Capture

Capture these logs when manually validating:

- `[CoreRacer.UI] Play clicked`
- `[CoreRacer.Run] Start requested (state=...)`
- `[CoreRacer.Run] Selected level: ...`
- `[CoreRacer.Run] Gameplay root activated`
- `[CoreRacer.Run] Run started successfully (runId=..., level=..., ship=...)`
- `[CoreRacer.Run] Run ended (runId=..., reason=...)`
- Any `[CoreRacer.UI] Play failed:`, `Quick Play failed:`, `[CoreRacer.Run] Start failed:`, or `[CoreRacer.Run] Reference error:` message.
- The first exception/error and its full stack trace if a lifecycle step fails.
- Test Runner summary for EditMode and PlayMode.

Included screenshots:

- `_PatchReports/Screenshots/CoreRacer_UI_Before.png`
- `_PatchReports/Screenshots/CoreRacer_Run_BeforePatch.png`
- `_PatchReports/Screenshots/CoreRacer_GameOver_BeforePatch.png`
- `_PatchReports/Screenshots/CoreRacer_Run_AfterPatch.png`
- `_PatchReports/Screenshots/CoreRacer_GameOver_AfterPatch.png`

## Assumptions and Risks

- The first roadmap entry is the known development Quick Play target.
- Quick Play is compiled only for the Unity Editor or development builds.
- Serialized scene fields are preserved; no prefab edits were required.
- Static tunnel geometry currently supplies the run environment; dynamic tunnel section generation remains unresolved.
- Game Over layout needs a focused authored-UI polish slice even though its callbacks work.

## Next Implementation Slice

After a human signs off the full Play/control/crash/Retry/Home loop, take a narrowly scoped playability-closeout slice: decide the required tunnel behavior and correct the authored Game Over layout. Do not start shop, IAP, adverts, visual redesign, or additional progression before that signoff.
