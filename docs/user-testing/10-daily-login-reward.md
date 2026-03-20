# Daily Login Reward User Test

## Goal

Verify the profile-backed daily login claim flow, streak handling, and manual hub access path for the first progression milestone slice.

## Setup

- Open the flow through the hub `Daily Login` side-entry or through the tasks hub preview.
- If day-skip tools exist, have them ready.

## Checklist

- [ ] Confirm the daily login UI can be opened.
- [ ] Confirm the current day/streak reward is clearly highlighted.
- [ ] Confirm opening the flow does not auto-claim the reward.
- [ ] Use `Claim` and confirm the reward is granted once.
- [ ] Confirm the claim state updates immediately after success.
- [ ] Reopen the screen and confirm the same day cannot be claimed twice.
- [ ] Confirm the tasks hub preview or badge state updates after the claim.
- [ ] If streak simulation exists, move forward a day and confirm the next reward is shown correctly.

## Expected Result

The flow should be quick, satisfying, and clear about current-day reward versus streak progression, with claim state owned by `PlayerProfile`.

## Test Notes

- Claim mode tested:
- Pass/Fail:
- Notes:
