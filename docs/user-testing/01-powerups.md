# Powerups User Test

## Goal

Verify the five target powerups are understandable, collectible, visible in the HUD, and reset correctly between runs.

## Setup

- Open `GameScene`.
- Enter Play Mode.
- If pickups are hard to reach, use `Tools > Powerups > Activate/...` for each type.

## Checklist

- [ ] Verify only these five powerups are used: `x2 Score`, `x2 Coin Spawn`, `Magnet`, `Autopilot`, `Shield`.
- [ ] Collect or activate `x2 Score` and confirm scoring feels higher immediately.
- [ ] Collect or activate `x2 Coin Spawn` and confirm coin gain feels higher immediately.
- [ ] Collect or activate `Magnet` and confirm nearby pickups pull in more easily.
- [ ] Collect or activate `Autopilot` and confirm steering assistance is obvious.
- [ ] Collect or activate `Shield` and confirm the player gets a protected state.
- [ ] For each powerup, confirm the HUD shows an indicator, timer, and draining progress bar.
- [ ] For each powerup, confirm a clear start state and a clear end state.
- [ ] Die while a powerup is active, continue, and confirm no powerup remains active.
- [ ] Return to menu from a powered run, start again, and confirm no stale powerup state remains.
- [ ] With shield active, take one hit and confirm the hit is blocked and the shield ends.
- [ ] After shield breaks, take another hit and confirm damage/death behaves normally.

## Expected Result

The player can understand what each powerup does within a single run, and no temporary state leaks across death, continue, menu return, or a new run.

## Test Notes

- Build used:
- Scene used:
- Pass/Fail:
- Notes:
