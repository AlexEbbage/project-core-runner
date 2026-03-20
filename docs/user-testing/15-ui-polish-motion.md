# UI Polish and Motion Stack User Test

## Goal

Verify the new UI motion layer improves the feel of the hub and overlays without breaking readability, input, or panel state.

## Setup

- Open `GameScene`.
- Stay in menu state, then trigger the supported UI surfaces one by one.

## Checklist

- [ ] Confirm the hub page transitions feel smooth when switching between `Shop`, `Ship`, `Lab`, `Level Select`, and `Achievements`.
- [ ] Confirm page switches do not leave overlapping pages visible after the motion completes.
- [ ] Open and close the feature panel and confirm the container motion feels deliberate and does not block input after closing.
- [ ] Open and close the game-over panel and confirm the open/close motion still leaves the resolved run summary readable.
- [ ] Open and close the shop item details modal and confirm the modal animates cleanly without losing its content or button state.
- [ ] Open and close the rewarded-run prompt and confirm the motion does not interfere with the countdown or CTA handling.
- [ ] Tap bottom-nav items and confirm the selected destination gets a subtle emphasis without causing layout jitter.
- [ ] Spend currency or gain XP/level progress and confirm the top bar gives light feedback without becoming noisy.
- [ ] Trigger a claimable badge state in the hub and confirm the badge appears with a small emphasis pulse rather than a hard pop.
- [ ] Verify the portrait layout remains readable and that motion never blocks the key CTA or makes text harder to read.
- [ ] Confirm the UI remains usable if a motion reference is absent or a panel is opened from a partially wired scene.

## Expected Result

The UI should feel more premium and responsive while staying predictable. Motion should reinforce state changes, not obscure them.

## Test Notes

- Pass/Fail:
- Notes:
