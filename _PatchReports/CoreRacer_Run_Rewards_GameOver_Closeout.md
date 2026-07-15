# Core Racer Run Rewards and Game Over Closeout

## Outcome

The clean-scene run lifecycle now proves reward settlement end to end. A death offers Continue in Editor/Development Builds through a development-only dummy rewarded provider when no production provider is assigned. Final death settles base rewards once, Double Rewards grants a one-shot bonus, Retry starts a clean run, and Home returns to Main Menu without leaving paused time or stale run state. The saved Game Over Continue button no longer contains a stale persistent `RunController.ContinueRun()` UnityEvent (that method returns `bool`); the runtime controller owns the void-compatible UI route.

## Live Unity MCP Evidence

Runtime trace from `CoreRacer_Main`:

`started=True; offered=True; continued=True; runningAfterContinue=True; gameOver=True; resultCoins=20; resultXp=210; baseSoft=79; baseXp=390; doubledRequest=True; doubleSoft=99; doubleXp=600; doubleButtonAfter=False; retryRunning=True; home=True; timeScale=1`

Relevant logs captured:

- `Development rewarded-ad fallback enabled for Continue and Double Rewards.`
- `Dummy rewarded ad shown: ContinueRun`
- `[CoreRacer.Run] Run ended (... reason=PlayerDeath)`
- `Dummy rewarded ad shown: DoubleRunRewards`

## Behaviours Completed

- Base run rewards settle once into the profile wallet, XP, and run totals.
- Continue resumes the active run at `Time.timeScale = 1` and does not settle a second run.
- Double Rewards grants only the bonus delta and disables its button after success.
- Repeated Double Rewards requests are rejected for the same run.
- Retry creates a new clean run session without an additional reward settlement before that run ends.
- Home returns to Main Menu at normal time scale.
- Profile state is restored by the PlayMode smoke test after validation.
- The dummy rewarded provider is editor/development-only and does not replace production provider wiring.

## Changed Files

- `Assets/CoreRacer/Runtime/Bootstrap/GameBootstrapper.cs`
- `Assets/CoreRacer/Tests/PlayMode/CoreRunPlayModeSmokeTests.cs`
- `docs/feature-registry.md`
- `docs/implementation-plan.md`
- `docs/task-registry.md`
- `docs/user-testing/03-game-over-screen.md`
- `_PatchReports/CoreRacer_Run_Rewards_GameOver_Closeout.md`

## New Files

- `_PatchReports/CoreRacer_Run_Rewards_GameOver_Closeout.md`

## Deletions

None.

## Validation

- Unity EditMode: 32/32 passed.
- Unity PlayMode: 5/5 passed.
- Unity MCP live run: Continue, base settlement, Double Rewards, Retry, Home, wallet/XP deltas, and button retirement verified.
- PlayMode smoke test invokes the actual Continue button and asserts zero stale persistent listeners before routing through `GameOverController`.
- `git diff --check`: required before commit.

## Installation

1. Back up or commit the project.
2. Extract the changed-files-only zip at the Unity project root, preserving paths.
3. Open the project in Unity 2022.3 LTS.
4. Open `Assets/CoreRacer/Scenes/CoreRacer_Main.unity`.
5. Run EditMode and PlayMode tests before manual review.

## Manual Test

1. Enter Play Mode and start a run.
2. Cause a crash and confirm Continue is offered.
3. Complete Continue and confirm gameplay resumes without a paused time scale.
4. Cause a second crash and end the run.
5. Confirm base rewards appear and the profile wallet/XP increase once.
6. Press `x2 Rewards`; confirm the bonus appears once and the button disables.
7. Press Retry; confirm a clean second run starts with reset score, coins, distance, and duration.
8. Press Home; confirm Main Menu is visible and `Time.timeScale` is 1.

## Production Boundary

The fallback provider is guarded by `UNITY_EDITOR || DEVELOPMENT_BUILD` and only activates when no rewarded provider is assigned. Production LevelPlay readiness and real-ad outcome validation remain separate release work.

## Next Slice

Human portrait-device review of Level Select, boosters, Game Over, and reward clarity, followed by the next approved progression surface. Keep shop, IAP, adverts, and broader progression expansion out of scope until that review is complete.
