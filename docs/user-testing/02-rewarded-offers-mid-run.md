# Rewarded Offers (Mid-Run) User Test

## Goal

Verify mid-run rewarded offers appear, dismiss cleanly, and do not disrupt the run more than intended.

## Setup

- Open `GameScene`.
- Enter Play Mode.
- If the flow is not naturally wired yet, note the blocker and test any available debug or forced prompt path.

## Checklist

- [ ] Confirm a mid-run offer can appear during active gameplay.
- [ ] Confirm the offer includes a visible countdown or limited response window.
- [ ] Confirm ignoring the offer lets gameplay continue without a harsh interruption.
- [ ] Accept the offer and confirm gameplay pauses only when intended.
- [ ] Confirm the modal clearly shows the reward and the rewarded-ad action.
- [ ] Confirm closing or ignoring the modal returns the player cleanly to the run.
- [ ] Confirm expired offers disappear without leaving orphan UI behind.
- [ ] Confirm the offer does not overlap or break core HUD readability.

## Expected Result

The offer should feel visible but not annoying, and the accept/ignore/timeout outcomes should all resolve cleanly.

## Test Notes

- Trigger method:
- Pass/Fail:
- Notes:
