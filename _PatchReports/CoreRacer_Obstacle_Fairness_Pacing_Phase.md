# Core Racer Obstacle Fairness and Pacing Phase

## Completed

- Rings inside one obstacle group now keep the same hex-side orientation for a readable lane.
- Consecutive groups cannot select the same hex side.
- Early spacing increased from 10 to 13.8 units, with larger safety spacing for fans and doors.
- Medium and hard wedge groups are capped at three repetitions.
- Fans rotate more slowly and doors start open, cycle more slowly, and reset when reused.
- Pattern selection, group-side history, moving obstacle rotation, and difficulty reset cleanly on retry.

## Saved Asset Tuning

- First obstacle distance: 36 units.
- Base ring spacing: 12 units.
- Easy / medium / fan / door / hard spacing multipliers: 1.15 / 1.25 / 1.65 / 1.8 / 1.35.
- Fan rotation range: 22–40 degrees per second.
- Door speed / cycle: 2 units per second / 3.2 seconds.

## Validation

- EditMode: 35 passed, 0 failed.
- PlayMode: 8 passed, 0 failed.
- Live run: early rings held one rotation for 2–3 repetitions, then changed to a different side.
- Live retry: time scale returned to 1, difficulty reset to 0, and a fresh easy group spawned.
- Existing Unity IAP initialization warning remains unrelated to this gameplay slice.

## Known Issue

- The one-piece wall still needs the user's planned manual collider adjustment. This phase does not claim that remaining collision issue is resolved and does not regenerate or replace its mesh.

## Manual Review

1. Play for at least 75 seconds.
2. Confirm each short wedge group establishes one readable lane.
3. Confirm the lane moves only at group boundaries.
4. Check that fans are readable before reaching the player.
5. Check that doors visibly telegraph their open/closed cycle.
6. Crash and retry; confirm the sequence restarts with easy wedges and no retained fan/door pose.

## Deletions

Nothing must be deleted.

## Next Slice

Gameplay audio and feedback: obstacle pass-by, collision impact, pickup confirmation, shield state, speed escalation, and menu/run transition audio.
