# Core Racer Level Select and Booster Closeout

## Outcome

The active `CoreRacer_Main` hub now exposes five persisted polygon routes and a development-ready pre-run booster loadout. A selected route configures the run's tunnel side count. One booster per family persists in profile v3 and applies only to the active run: Start Shield, x2 coins, and x2 score.

## Live Unity Evidence

- Active scene: `Assets/CoreRacer/Scenes/CoreRacer_Main.unity`.
- Unity MCP runtime trace: `selected=True; started=True; routes=5; options=3; level=deca_sector_05; state=Running; tunnelSides=10; scoreX=2; coinX=2; shield=True; playerVisible=True; timeScale=1`.
- Home/reset trace: `state=MainMenu; scoreX=1; coinX=1; timeScale=1; profileRestored=True`.
- Runtime screenshot: `Screenshots/CoreRacer_Level_Select_Booster_Run.png`.

## Behaviours Completed

- Five route definitions are authored in order from HEXAGON through DECAGON.
- Each route carries three display-only challenge goals.
- Profile-level locks and selected-route persistence remain in the existing Level Select flow.
- The selected route configures tunnel sides when Play starts.
- Three booster families are presented from an authored inactive option template.
- Equipping a booster replaces only the existing choice from the same family.
- Profile migration initializes the persisted loadout at version 3.
- Run startup resolves valid equipped IDs and applies run-only shield, coin, and score effects.
- Home/final run cleanup restores run multipliers to x1.

## Changed Files

- `Assets/CoreRacer/Editor/Builders/CoreRacerDefaultConfigBuilder.cs`
- `Assets/CoreRacer/Generated/Configs/BoosterCatalog.asset`
- `Assets/CoreRacer/Generated/Configs/LevelRoadmap.asset`
- `Assets/CoreRacer/Runtime/Gameplay/Run/RunController.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Run/RunCurrencyTracker.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Run/RunScoreTracker.cs`
- `Assets/CoreRacer/Runtime/Meta/Boosters/BoosterCatalog.cs`
- `Assets/CoreRacer/Runtime/Meta/Levels/LevelRoadmapConfigV2.cs`
- `Assets/CoreRacer/Runtime/Meta/Profile/PlayerProfileState.cs`
- `Assets/CoreRacer/Runtime/Meta/Profile/ProfileMigrationService.cs`
- `Assets/CoreRacer/Runtime/UI/MainMenu/LevelSelectPageController.cs`
- `Assets/CoreRacer/Scenes/CoreRacer_Main.unity`
- `Assets/CoreRacer/Tests/EditMode/ProfileMigrationServiceTests.cs`
- `Assets/CoreRacer/Tests/PlayMode/CoreRunPlayModeSmokeTests.cs`
- `docs/feature-registry.md`
- `docs/implementation-plan.md`
- `docs/script-registry.md`
- `docs/task-registry.md`
- `docs/user-testing/11-boosters.md`
- `docs/user-testing/13-level-select.md`
- `DELETE_NOTHING.txt`

## New Files

- `Assets/CoreRacer/Runtime/Meta/Boosters/BoosterLoadoutService.cs`
- `Assets/CoreRacer/Runtime/Meta/Boosters/BoosterLoadoutService.cs.meta`
- `Assets/CoreRacer/Runtime/UI/MainMenu/BoosterLoadoutController.cs`
- `Assets/CoreRacer/Runtime/UI/MainMenu/BoosterLoadoutController.cs.meta`
- `Assets/CoreRacer/Runtime/UI/MainMenu/BoosterOptionView.cs`
- `Assets/CoreRacer/Runtime/UI/MainMenu/BoosterOptionView.cs.meta`
- `Assets/CoreRacer/Tests/EditMode/BoosterLoadoutServiceTests.cs`
- `Assets/CoreRacer/Tests/EditMode/BoosterLoadoutServiceTests.cs.meta`
- `_PatchReports/CoreRacer_Level_Select_Booster_Closeout.md`
- `_PatchReports/Screenshots/CoreRacer_Level_Select_Booster_Run.png`

## Deletions

None. The legacy roadmap and booster data remain preserved and are not used by the active clean-scene path.

## Validation

- Unity EditMode: 32/32 passed.
- Unity PlayMode: 4/4 passed.
- Added EditMode coverage for same-family replacement, resolver composition, and unequip persistence.
- Added PlayMode coverage for five-route selection, profile persistence, DECAGON tunnel configuration, three run effects, and reset on Home.
- `git diff --check`: passed.

## Installation

1. Back up or commit the target project.
2. Extract the changed-files-only zip at the Unity project root, preserving paths.
3. Open the project in Unity 2022.3 LTS and allow scripts/assets to import.
4. Open `Assets/CoreRacer/Scenes/CoreRacer_Main.unity`.
5. Run EditMode and PlayMode tests before manual review.

## Manual Test

1. Enter Play Mode and open Level Select.
2. Confirm the five routes appear in order and locked routes show their required profile level.
3. Equip Start Shield, Coin Boost, and Score Boost; leave and return to Level Select and confirm they remain equipped.
4. With a level-8 development profile, select DECAGON and press Play.
5. Confirm the player is visible, the tunnel has ten sides, the initial shield is active, and coin/score pickups receive x2 values.
6. End the run and choose Home; confirm the route remains selected but run modifiers no longer apply.
7. Start another run without changing the loadout and confirm the effects apply once, cleanly.

## Logs and Screenshots to Capture

- `[CoreRacer.Run] Selected level: ...`
- `[CoreRacer.Run] Run started successfully ...`
- `[CoreRacer.Boosters] Applied run loadout: ...`
- Portrait Level Select showing all route/lock/challenge states while scrolling.
- Portrait booster section showing one equipped option in each family.
- Gameplay view showing the selected polygon route and active shield state.
- Home return showing the persisted selected route.

## Assumptions and Remaining Review

- Boosters are development selections in this slice; purchasing/consumption is intentionally out of scope.
- Challenge goals are display-only; claim/completion logic is intentionally out of scope.
- Human portrait-device review is still required for card and booster copy/layout clarity.

## Next Slice

Run rewards and Game Over settlement live closeout: prove base reward settlement, one-shot double rewards, continue, Retry/Home, and profile wallet/XP persistence before starting broader shop or progression work.
