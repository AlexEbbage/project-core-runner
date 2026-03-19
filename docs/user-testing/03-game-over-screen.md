# Game Over Screen User Test

## Goal

Verify the end-of-run screen presents the resolved run summary, grants base rewards once, gates continue behind the timer, and resolves the one-shot `x2 Rewards` flow correctly.

## Setup

- Open `GameScene`.
- Start a run, collect at least a few coins, then intentionally fail.
- If possible, keep one continue available so both the continue and `x2 Rewards` paths can be checked in the same run.

## Checklist

- [ ] Confirm the game over screen appears at run end.
- [ ] Confirm score, best score, elapsed time, distance, coins, base rewards, and combo modifier are readable.
- [ ] Confirm rewards are granted at base value immediately when the screen opens.
- [ ] Confirm the continue button stays locked until the visible timer or countdown finishes.
- [ ] Confirm the `x2 Rewards` button is visible before continue unlocks.
- [ ] If available, trigger `x2 Rewards` and confirm bonus rewards are added only after success.
- [ ] Confirm the double-reward button disappears or disables correctly after use.
- [ ] Fail or skip the `x2 Rewards` ad once and confirm no bonus is granted.
- [ ] If continue is available, use it and confirm the player returns to gameplay cleanly.
- [ ] Confirm a continued run resumes with no temporary powerups restored.
- [ ] Confirm the menu and restart actions go to the expected destinations.
- [ ] Confirm no stale HUD or menu panels remain visible under the game over panel.
- [ ] Confirm repeated button presses or repeated ad callbacks do not double-grant rewards.

## Expected Result

The player should clearly understand run outcome, reward outcome, and next-step choices without UI confusion. Base rewards should be granted exactly once on death, and the bonus from `x2 Rewards` should be granted at most once after a successful ad.

## Test Notes

- Continue path tested:
- Continue timer completed:
- Double rewards path tested:
- Pass/Fail:
- Notes:
