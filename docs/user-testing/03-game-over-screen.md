# Game Over Screen User Test

## Goal

Verify the end-of-run screen presents the right information, supports continue/double-reward flows, and resolves rewards correctly.

## Setup

- Open `GameScene`.
- Start a run and intentionally fail.

## Checklist

- [ ] Confirm the game over screen appears at run end.
- [ ] Confirm score, best score, elapsed time, and continue state are readable.
- [ ] Confirm rewards are granted at base value immediately when the screen opens.
- [ ] If available, trigger `x2 Rewards` and confirm bonus rewards are added only after success.
- [ ] Confirm the double-reward button disappears or disables correctly after use.
- [ ] If continue is available, use it and confirm the player returns to gameplay cleanly.
- [ ] Confirm the menu and restart actions go to the expected destinations.
- [ ] Confirm no stale HUD or menu panels remain visible under the game over panel.

## Expected Result

The player should clearly understand run outcome, reward outcome, and next-step choices without UI confusion.

## Test Notes

- Continue path tested:
- Double rewards path tested:
- Pass/Fail:
- Notes:
