# Game Over Screen User Test

## Status

Behavioural evidence retained. Final presentation and interaction acceptance is deferred until F22 migrates this flow to the replacement UI architecture.

## Goal

Verify the end-of-run screen presents the resolved run summary, grants base rewards once, gates continue behind the timer, and resolves the one-shot `x2 Rewards` flow correctly.

## Setup

- Open `Assets/CoreRacer/Scenes/CoreRacer_Main.unity`.
- Start a run, collect at least a few coins, then intentionally fail.
- If possible, keep one continue available so both the continue and `x2 Rewards` paths can be checked in the same run.

## Verification Result

- EditMode: 32/32 passed on 2026-07-15.
- PlayMode: 5/5 passed, including base settlement, Continue, Double Rewards, Retry, Home, and profile restoration.
- Unity MCP live result: base `+20` coins and `+210` XP settled; Double Rewards added the same bonus and disabled its button; Continue resumed gameplay; Retry created a clean second run; Home returned to Main Menu at `Time.timeScale = 1`.
- Production note: the live proof used the editor/development-only dummy rewarded provider. Production LevelPlay readiness remains a separate release gate.

## Checklist

- [x] Confirm the game over screen appears at run end.
- [ ] Confirm score, best score, elapsed time, distance, coins, base rewards, and combo modifier are readable.
- [x] Confirm rewards are granted at base value immediately when the screen opens.
- [ ] Confirm the continue button stays locked until the visible timer or countdown finishes.
- [ ] Confirm the `x2 Rewards` button is visible before continue unlocks.
- [x] Trigger `x2 Rewards` and confirm bonus rewards are added only after success.
- [x] Confirm the double-reward button disappears or disables correctly after use.
- [ ] Fail or skip the `x2 Rewards` ad once and confirm no bonus is granted.
- [x] If continue is available, use it and confirm the player returns to gameplay cleanly.
- [x] Confirm a continued run resumes with no temporary powerups restored.
- [x] Confirm the menu and restart actions go to the expected destinations.
- [ ] Confirm no stale HUD or menu panels remain visible under the game over panel.
- [x] Confirm repeated button presses or repeated ad callbacks do not double-grant rewards.

## Expected Result

The player should clearly understand run outcome, reward outcome, and next-step choices without UI confusion. Base rewards should be granted exactly once on death, and the bonus from `x2 Rewards` should be granted at most once after a successful ad.

## Test Notes

- Continue path tested:
- Continue timer completed:
- Double rewards path tested:
- Pass/Fail:
- Notes:
