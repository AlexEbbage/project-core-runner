# Rewarded Offers (Mid-Run) User Test

## Goal

Verify mid-run rewarded offers appear, dismiss cleanly, and do not disrupt the run more than intended.

## Setup

- Open `GameScene`.
- Enter Play Mode.
- Wait for the configured first offer interval or shorten timing values on `RewardedOfferConfig` for faster checks.

## Checklist

- [ ] Confirm the first popout appears only after the configured initial delay.
- [ ] Confirm the popout appears during active gameplay and not during menu, countdown, pause, continue, or game-over states.
- [ ] Confirm the popout includes a visible countdown and reward label.
- [ ] Press `IGNORE` on the popout and confirm it disappears without pausing gameplay.
- [ ] Let the popout expire and confirm it disappears cleanly without leaving orphan UI behind.
- [ ] Tap `VIEW` on the popout and confirm gameplay pauses only at that moment.
- [ ] Confirm the modal clearly shows the reward and rewarded-ad action.
- [ ] Close or ignore the modal and confirm gameplay resumes cleanly.
- [ ] Complete a rewarded ad for a powerup reward and confirm the powerup activates immediately in-run.
- [ ] Complete a rewarded ad for a soft-currency reward and confirm the profile gains soft currency.
- [ ] Complete a rewarded ad for a premium-currency reward and confirm the profile gains premium currency.
- [ ] Confirm the next offer does not appear until the next valid interval window.
- [ ] Confirm only one offer is ever active at a time.

## Expected Result

The offer should feel visible but not annoying, and the accept/ignore/timeout outcomes should all resolve cleanly.

## Test Notes

- Trigger method:
- Pass/Fail:
- Notes:
