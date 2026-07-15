# Core Racer Progression XP / Level Closeout

## Outcome

The next progression slice closes the automated proof gap around XP rollover. `PlayerProfileService` now has a focused regression test proving that a grant can cross multiple level thresholds and preserve the remainder for the next level. Existing level-select gating and profile persistence remain unchanged.

## Changed files

- `Assets/CoreRacer/Tests/EditMode/Vertical6ProgressionEconomyTests.cs`
- `Assets/CoreRacer/Tests/PlayMode/CoreRunPlayModeSmokeTests.cs`
- `docs/feature-registry.md`
- `docs/task-registry.md`
- `docs/implementation-plan.md`
- `_PatchReports/CoreRacer_Progression_XP_Level_Closeout.md`

## Validation

- Existing baseline: 32/32 EditMode and 5/5 focused PlayMode tests passed before this regression case was added.
- New EditMode case: 1,250 XP advances level 1 to level 3 with zero remainder; an additional 1,001 XP advances to level 4 with one XP retained.
- New PlayMode case: the authored DECAGON route is locked at level 1, unlocks at level 8, and persists selection through the profile.
- Unity rerun required: include the new EditMode case in the next editor test pass.

## Manual review gate

In portrait orientation, open Progression and Level Select, grant or earn enough XP to cross a level, and confirm the level/XP display, level-up feedback, and route lock copy remain readable and understandable.

## Deletions

Nothing must be deleted. See `DELETE_NOTHING.txt`.

## Next slice

This is now the first proof slice of the larger progression/meta phase. The remaining phase work is an integrated runtime pass across Tasks, Achievements, Daily Rewards, Lab, and Hangar, followed by portrait-device review and only focused copy/layout adjustments discovered during that review.
